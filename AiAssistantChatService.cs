using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MeuApp
{
    public sealed class AiAssistantChatMessage
    {
        public string Role { get; set; } = "user";
        public string Content { get; set; } = string.Empty;
    }

    public sealed class AiAssistantUserContext
    {
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string RoleLabel { get; set; } = string.Empty;
        public string Course { get; set; } = string.Empty;
        public string Registration { get; set; } = string.Empty;
    }

    public sealed class AiAssistantProjectContext
    {
        public string TeamId { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;
        public string Course { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string AcademicTerm { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int Progress { get; set; }
        public string Deadline { get; set; } = string.Empty;
        public string AcademicBrief { get; set; } = string.Empty;
        public List<string> UpcomingMilestones { get; set; } = new List<string>();
        public List<string> OpenTasks { get; set; } = new List<string>();
        public List<string> Doubts { get; set; } = new List<string>();
    }

    public sealed class AiAssistantContext
    {
        public string AppName { get; set; } = "Choas";
        public string Locale { get; set; } = "pt-BR";
        public string CurrentDate { get; set; } = string.Empty;
        public string ScopePolicy { get; set; } = string.Empty;
        public AiAssistantUserContext User { get; set; } = new AiAssistantUserContext();
        public List<AiAssistantProjectContext> Projects { get; set; } = new List<AiAssistantProjectContext>();
        public List<string> AppCapabilities { get; set; } = new List<string>();
    }

    public sealed class AiAssistantChatRequest
    {
        public string Message { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public List<AiAssistantChatMessage> Messages { get; set; } = new List<AiAssistantChatMessage>();
        public AiAssistantContext Context { get; set; } = new AiAssistantContext();
    }

    public sealed class AiAssistantChatResult
    {
        public bool Success { get; init; }
        public string Content { get; init; } = string.Empty;
        public string ErrorMessage { get; init; } = string.Empty;
        public string EndpointUrl { get; init; } = string.Empty;

        public static AiAssistantChatResult Ok(string content, string endpointUrl)
        {
            return new AiAssistantChatResult
            {
                Success = true,
                Content = content,
                EndpointUrl = endpointUrl
            };
        }

        public static AiAssistantChatResult Fail(string errorMessage, string endpointUrl = "")
        {
            return new AiAssistantChatResult
            {
                Success = false,
                ErrorMessage = errorMessage,
                EndpointUrl = endpointUrl
            };
        }
    }

    public sealed class AiAssistantChatService
    {
        private static readonly HttpClient Client = new HttpClient();
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

        public async Task<AiAssistantChatResult> SendAsync(AiAssistantChatRequest request, CancellationToken cancellationToken = default)
        {
            var settings = AppConfig.AiAssistant;
            if (!settings.Enabled)
            {
                return AiAssistantChatResult.Fail("O assistente de IA esta desativado nas configuracoes locais.");
            }

            var endpoints = BuildEndpointCandidates(settings);
            if (endpoints.Count == 0)
            {
                return AiAssistantChatResult.Fail("Configure a URL da API da IA em AiAssistant.EndpointUrl ou CHOAS_AI_ENDPOINT_URL.");
            }

            var payload = JsonSerializer.Serialize(request, JsonOptions);
            var lastError = string.Empty;
            var lastEndpoint = string.Empty;

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(settings.TimeoutSeconds, 10, 120)));

            foreach (var endpoint in endpoints)
            {
                lastEndpoint = endpoint.ToString();
                try
                {
                    using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
                    {
                        Content = new StringContent(payload, Encoding.UTF8, "application/json")
                    };
                    httpRequest.Headers.TryAddWithoutValidation("X-Choas-Client", "desktop-chat-ai");

                    using var response = await Client.SendAsync(httpRequest, timeoutCts.Token);
                    var body = await response.Content.ReadAsStringAsync(timeoutCts.Token);

                    if (!response.IsSuccessStatusCode)
                    {
                        lastError = $"{(int)response.StatusCode} {response.ReasonPhrase}: {TrimForDiagnostics(body)}";
                        continue;
                    }

                    var content = ExtractAssistantText(body);
                    if (string.IsNullOrWhiteSpace(content))
                    {
                        lastError = "A API respondeu, mas nao retornou texto reconhecivel.";
                        continue;
                    }

                    return AiAssistantChatResult.Ok(content.Trim(), lastEndpoint);
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    lastError = "Tempo limite atingido ao chamar a API da IA.";
                }
                catch (Exception ex)
                {
                    lastError = $"{ex.GetType().Name}: {ex.Message}";
                }
            }

            return AiAssistantChatResult.Fail(
                string.IsNullOrWhiteSpace(lastError)
                    ? "Nao foi possivel chamar a API da IA."
                    : lastError,
                lastEndpoint);
        }

        private static IReadOnlyList<Uri> BuildEndpointCandidates(AiAssistantSettings settings)
        {
            var candidates = new List<Uri>();

            void Add(string? value)
            {
                var normalized = (value ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(normalized))
                {
                    return;
                }

                if (!Uri.TryCreate(normalized, UriKind.Absolute, out var uri))
                {
                    return;
                }

                if (candidates.Any(existing => string.Equals(existing.ToString(), uri.ToString(), StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }

                candidates.Add(uri);
            }

            Add(settings.EndpointUrl);

            var baseUrl = (settings.BaseUrl ?? string.Empty).Trim().TrimEnd('/');
            if (!string.IsNullOrWhiteSpace(baseUrl))
            {
                var endpointPath = string.IsNullOrWhiteSpace(settings.EndpointPath)
                    ? "/api/ai/chat"
                    : (settings.EndpointPath.StartsWith("/", StringComparison.Ordinal) ? settings.EndpointPath : "/" + settings.EndpointPath);

                Add(baseUrl + endpointPath);
                Add(baseUrl + "/api/chat");
                Add(baseUrl + "/api/openai/chat");
                Add(baseUrl + "/api/ai");
            }

            return candidates;
        }

        private static string ExtractAssistantText(string responseBody)
        {
            if (string.IsNullOrWhiteSpace(responseBody))
            {
                return string.Empty;
            }

            try
            {
                using var document = JsonDocument.Parse(responseBody);
                var root = document.RootElement;

                var direct = TryGetFirstString(root, "reply", "answer", "content", "message", "text", "output_text", "outputText", "response");
                if (!string.IsNullOrWhiteSpace(direct))
                {
                    return direct;
                }

                if (root.TryGetProperty("message", out var messageElement) && messageElement.ValueKind == JsonValueKind.Object)
                {
                    var messageContent = TryGetFirstString(messageElement, "content", "text", "reply");
                    if (!string.IsNullOrWhiteSpace(messageContent))
                    {
                        return messageContent;
                    }
                }

                if (root.TryGetProperty("choices", out var choicesElement) && choicesElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var choice in choicesElement.EnumerateArray())
                    {
                        if (choice.TryGetProperty("message", out var choiceMessage))
                        {
                            var choiceContent = TryGetFirstString(choiceMessage, "content", "text");
                            if (!string.IsNullOrWhiteSpace(choiceContent))
                            {
                                return choiceContent;
                            }
                        }
                    }
                }

                if (root.TryGetProperty("output", out var outputElement) && outputElement.ValueKind == JsonValueKind.Array)
                {
                    var parts = new List<string>();
                    foreach (var outputItem in outputElement.EnumerateArray())
                    {
                        CollectResponseOutputText(outputItem, parts);
                    }

                    return string.Join(Environment.NewLine + Environment.NewLine, parts.Where(part => !string.IsNullOrWhiteSpace(part)));
                }
            }
            catch
            {
                return responseBody.Trim();
            }

            return string.Empty;
        }

        private static void CollectResponseOutputText(JsonElement element, List<string> parts)
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                parts.Add(element.GetString() ?? string.Empty);
                return;
            }

            if (element.ValueKind != JsonValueKind.Object)
            {
                return;
            }

            var direct = TryGetFirstString(element, "text", "output_text", "content");
            if (!string.IsNullOrWhiteSpace(direct))
            {
                parts.Add(direct);
            }

            if (element.TryGetProperty("content", out var contentElement) && contentElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var contentItem in contentElement.EnumerateArray())
                {
                    CollectResponseOutputText(contentItem, parts);
                }
            }
        }

        private static string TryGetFirstString(JsonElement element, params string[] propertyNames)
        {
            if (element.ValueKind != JsonValueKind.Object)
            {
                return string.Empty;
            }

            foreach (var propertyName in propertyNames)
            {
                if (!element.TryGetProperty(propertyName, out var property))
                {
                    continue;
                }

                if (property.ValueKind == JsonValueKind.String)
                {
                    return property.GetString() ?? string.Empty;
                }

                if (property.ValueKind == JsonValueKind.Object)
                {
                    var nested = TryGetFirstString(property, "content", "text", "reply", "answer");
                    if (!string.IsNullOrWhiteSpace(nested))
                    {
                        return nested;
                    }
                }
            }

            return string.Empty;
        }

        private static string TrimForDiagnostics(string value)
        {
            var normalized = (value ?? string.Empty).Trim();
            if (normalized.Length <= 240)
            {
                return normalized;
            }

            return normalized[..240] + "...";
        }
    }
}
