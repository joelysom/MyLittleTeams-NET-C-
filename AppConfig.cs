using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MeuApp
{
    public sealed class FirebaseSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public string StorageBucket { get; set; } = string.Empty;
    }

    public sealed class UpdaterSettings
    {
        public bool Enabled { get; set; } = true;
        public string GitHubOwner { get; set; } = string.Empty;
        public string GitHubRepository { get; set; } = string.Empty;
        public bool IncludePrereleases { get; set; }
    }

    public sealed class AiAssistantSettings
    {
        public bool Enabled { get; set; } = true;
        public string BaseUrl { get; set; } = string.Empty;
        public string EndpointPath { get; set; } = string.Empty;
        public string EndpointUrl { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; } = 45;
    }

    internal sealed class AppConfigDocument
    {
        public FirebaseSettings Firebase { get; set; } = new FirebaseSettings();
        public UpdaterSettings Updater { get; set; } = new UpdaterSettings();
        public AiAssistantSettings AiAssistant { get; set; } = new AiAssistantSettings();
    }

    public static class AppConfig
    {
        private const string DefaultFirebaseApiKey = "AIzaSyA2V4MEzgOoKEEZAAXH49DXbzxUo0_CuWU";
        private const string DefaultFirebaseProjectId = "obsseractpi";
        private const string DefaultFirebaseStorageBucket = "obsseractpi.firebasestorage.app";
        private const string DefaultGitHubOwner = "joelysom";
        private const string DefaultGitHubRepository = "MyLittleTeams-NET-C-";
        private const string DefaultAiAssistantBaseUrl = "https://choas-web-app.vercel.app";
        private const string DefaultAiAssistantEndpointPath = "/api/ai/chat";
        private static readonly Lazy<FirebaseSettings> FirebaseSettingsLazy = new Lazy<FirebaseSettings>(LoadFirebaseSettings);
        private static readonly Lazy<UpdaterSettings> UpdaterSettingsLazy = new Lazy<UpdaterSettings>(LoadUpdaterSettings);
        private static readonly Lazy<AiAssistantSettings> AiAssistantSettingsLazy = new Lazy<AiAssistantSettings>(LoadAiAssistantSettings);

        public static FirebaseSettings Firebase => FirebaseSettingsLazy.Value;

        public static UpdaterSettings Updater => UpdaterSettingsLazy.Value;

        public static AiAssistantSettings AiAssistant => AiAssistantSettingsLazy.Value;

        public static string FirebaseApiKey => Firebase.ApiKey;

        public static string FirebaseProjectId => Firebase.ProjectId;

        public static string FirebaseStorageBucket => Firebase.StorageBucket;

        public static string FirestoreBaseUrl => $"https://firestore.googleapis.com/v1/projects/{FirebaseProjectId}/databases/(default)";

        public static string FirebaseStorageBaseUrl => $"https://firebasestorage.googleapis.com/v0/b/{FirebaseStorageBucket}";

        public static IReadOnlyList<string> FirebaseStorageBucketCandidates => BuildFirebaseStorageBucketCandidates(FirebaseStorageBucket, FirebaseProjectId);

        public static string BuildFirestoreDocumentUrl(string relativePath)
        {
            return $"{FirestoreBaseUrl}/documents/{relativePath}";
        }

        public static string BuildFirestoreRunQueryUrl()
        {
            return $"{FirestoreBaseUrl}/documents:runQuery";
        }

        public static string BuildFirebaseStorageUploadUrl(string objectPath)
        {
            var escapedObjectPath = Uri.EscapeDataString((objectPath ?? string.Empty).Trim('/'));
            return $"{FirebaseStorageBaseUrl}/o?uploadType=media&name={escapedObjectPath}";
        }

        public static IReadOnlyList<string> BuildFirebaseStorageUploadUrls(string objectPath)
        {
            var escapedObjectPath = Uri.EscapeDataString((objectPath ?? string.Empty).Trim('/'));
            return FirebaseStorageBucketCandidates
                .Select(bucket => $"https://firebasestorage.googleapis.com/v0/b/{bucket}/o?uploadType=media&name={escapedObjectPath}")
                .ToList();
        }

        public static string BuildFirebaseStorageMetadataUrl(string objectPath)
        {
            var escapedObjectPath = Uri.EscapeDataString((objectPath ?? string.Empty).Trim('/'));
            return $"{FirebaseStorageBaseUrl}/o/{escapedObjectPath}";
        }

        public static IReadOnlyList<string> BuildFirebaseStorageMetadataUrls(string objectPath)
        {
            var escapedObjectPath = Uri.EscapeDataString((objectPath ?? string.Empty).Trim('/'));
            return FirebaseStorageBucketCandidates
                .Select(bucket => $"https://firebasestorage.googleapis.com/v0/b/{bucket}/o/{escapedObjectPath}")
                .ToList();
        }

        public static string BuildFirebaseStorageDownloadUrl(string objectPath)
        {
            return $"{BuildFirebaseStorageMetadataUrl(objectPath)}?alt=media";
        }

        public static IReadOnlyList<string> BuildFirebaseStorageDownloadUrls(string objectPath)
        {
            return BuildFirebaseStorageMetadataUrls(objectPath)
                .Select(url => $"{url}?alt=media")
                .ToList();
        }

        private static FirebaseSettings LoadFirebaseSettings()
        {
            var settings = new FirebaseSettings
            {
                ApiKey = DefaultFirebaseApiKey,
                ProjectId = DefaultFirebaseProjectId,
                StorageBucket = DefaultFirebaseStorageBucket
            };

            foreach (var configPath in GetLocalConfigPaths())
            {
                if (!File.Exists(configPath))
                {
                    continue;
                }

                try
                {
                    var json = File.ReadAllText(configPath);
                    var document = JsonSerializer.Deserialize<AppConfigDocument>(json);
                    if (document?.Firebase == null)
                    {
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(document.Firebase.ApiKey))
                    {
                        settings.ApiKey = document.Firebase.ApiKey.Trim();
                    }

                    if (!string.IsNullOrWhiteSpace(document.Firebase.ProjectId))
                    {
                        settings.ProjectId = document.Firebase.ProjectId.Trim();
                    }

                    if (!string.IsNullOrWhiteSpace(document.Firebase.StorageBucket))
                    {
                        settings.StorageBucket = document.Firebase.StorageBucket.Trim();
                    }
                }
                catch
                {
                    // Usa fallback interno quando a configuração local estiver inválida.
                }
            }

            var apiKeyOverride = Environment.GetEnvironmentVariable("CHOAS_FIREBASE_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKeyOverride))
            {
                apiKeyOverride = Environment.GetEnvironmentVariable("OBSSERACT_FIREBASE_API_KEY");
            }
            if (!string.IsNullOrWhiteSpace(apiKeyOverride))
            {
                settings.ApiKey = apiKeyOverride.Trim();
            }

            var projectIdOverride = Environment.GetEnvironmentVariable("CHOAS_FIREBASE_PROJECT_ID");
            if (string.IsNullOrWhiteSpace(projectIdOverride))
            {
                projectIdOverride = Environment.GetEnvironmentVariable("OBSSERACT_FIREBASE_PROJECT_ID");
            }
            if (!string.IsNullOrWhiteSpace(projectIdOverride))
            {
                settings.ProjectId = projectIdOverride.Trim();
            }

            var storageBucketOverride = Environment.GetEnvironmentVariable("CHOAS_FIREBASE_STORAGE_BUCKET");
            if (string.IsNullOrWhiteSpace(storageBucketOverride))
            {
                storageBucketOverride = Environment.GetEnvironmentVariable("OBSSERACT_FIREBASE_STORAGE_BUCKET");
            }
            if (!string.IsNullOrWhiteSpace(storageBucketOverride))
            {
                settings.StorageBucket = storageBucketOverride.Trim();
            }

            if (string.IsNullOrWhiteSpace(settings.StorageBucket))
            {
                settings.StorageBucket = $"{settings.ProjectId}.firebasestorage.app";
            }

            return settings;
        }

        private static UpdaterSettings LoadUpdaterSettings()
        {
            var settings = new UpdaterSettings
            {
                Enabled = true,
                GitHubOwner = DefaultGitHubOwner,
                GitHubRepository = DefaultGitHubRepository,
                IncludePrereleases = false
            };

            foreach (var configPath in GetLocalConfigPaths())
            {
                if (!File.Exists(configPath))
                {
                    continue;
                }

                try
                {
                    var json = File.ReadAllText(configPath);
                    var document = JsonSerializer.Deserialize<AppConfigDocument>(json);
                    if (document?.Updater == null)
                    {
                        continue;
                    }

                    settings.Enabled = document.Updater.Enabled;

                    if (!string.IsNullOrWhiteSpace(document.Updater.GitHubOwner))
                    {
                        settings.GitHubOwner = document.Updater.GitHubOwner.Trim();
                    }

                    if (!string.IsNullOrWhiteSpace(document.Updater.GitHubRepository))
                    {
                        settings.GitHubRepository = document.Updater.GitHubRepository.Trim();
                    }

                    settings.IncludePrereleases = document.Updater.IncludePrereleases;
                }
                catch
                {
                    // Usa fallback interno quando a configuração local estiver inválida.
                }
            }

            var updaterEnabledOverride = Environment.GetEnvironmentVariable("CHOAS_UPDATER_ENABLED");
            if (TryParseBoolean(updaterEnabledOverride, out var updaterEnabled))
            {
                settings.Enabled = updaterEnabled;
            }

            var githubOwnerOverride = Environment.GetEnvironmentVariable("CHOAS_GITHUB_OWNER");
            if (!string.IsNullOrWhiteSpace(githubOwnerOverride))
            {
                settings.GitHubOwner = githubOwnerOverride.Trim();
            }

            var githubRepositoryOverride = Environment.GetEnvironmentVariable("CHOAS_GITHUB_REPOSITORY");
            if (!string.IsNullOrWhiteSpace(githubRepositoryOverride))
            {
                settings.GitHubRepository = githubRepositoryOverride.Trim();
            }

            var includePrereleasesOverride = Environment.GetEnvironmentVariable("CHOAS_UPDATER_INCLUDE_PRERELEASES");
            if (TryParseBoolean(includePrereleasesOverride, out var includePrereleases))
            {
                settings.IncludePrereleases = includePrereleases;
            }

            return settings;
        }

        private static AiAssistantSettings LoadAiAssistantSettings()
        {
            var settings = new AiAssistantSettings
            {
                Enabled = true,
                BaseUrl = DefaultAiAssistantBaseUrl,
                EndpointPath = DefaultAiAssistantEndpointPath,
                EndpointUrl = string.Empty,
                TimeoutSeconds = 45
            };

            foreach (var configPath in GetLocalConfigPaths())
            {
                if (!File.Exists(configPath))
                {
                    continue;
                }

                try
                {
                    var json = File.ReadAllText(configPath);
                    var document = JsonSerializer.Deserialize<AppConfigDocument>(json);
                    if (document?.AiAssistant == null)
                    {
                        continue;
                    }

                    settings.Enabled = document.AiAssistant.Enabled;

                    if (!string.IsNullOrWhiteSpace(document.AiAssistant.BaseUrl))
                    {
                        settings.BaseUrl = document.AiAssistant.BaseUrl.Trim();
                    }

                    if (!string.IsNullOrWhiteSpace(document.AiAssistant.EndpointPath))
                    {
                        settings.EndpointPath = NormalizeEndpointPath(document.AiAssistant.EndpointPath);
                    }

                    if (!string.IsNullOrWhiteSpace(document.AiAssistant.EndpointUrl))
                    {
                        settings.EndpointUrl = document.AiAssistant.EndpointUrl.Trim();
                    }

                    if (document.AiAssistant.TimeoutSeconds > 0)
                    {
                        settings.TimeoutSeconds = document.AiAssistant.TimeoutSeconds;
                    }
                }
                catch
                {
                    // Usa fallback interno quando a configuracao local estiver invalida.
                }
            }

            var enabledOverride = Environment.GetEnvironmentVariable("CHOAS_AI_ENABLED");
            if (TryParseBoolean(enabledOverride, out var enabled))
            {
                settings.Enabled = enabled;
            }

            var endpointOverride = Environment.GetEnvironmentVariable("CHOAS_AI_ENDPOINT_URL");
            if (!string.IsNullOrWhiteSpace(endpointOverride))
            {
                settings.EndpointUrl = endpointOverride.Trim();
            }

            var baseUrlOverride = Environment.GetEnvironmentVariable("CHOAS_AI_BASE_URL");
            if (!string.IsNullOrWhiteSpace(baseUrlOverride))
            {
                settings.BaseUrl = baseUrlOverride.Trim();
            }

            var endpointPathOverride = Environment.GetEnvironmentVariable("CHOAS_AI_ENDPOINT_PATH");
            if (!string.IsNullOrWhiteSpace(endpointPathOverride))
            {
                settings.EndpointPath = NormalizeEndpointPath(endpointPathOverride);
            }

            var timeoutOverride = Environment.GetEnvironmentVariable("CHOAS_AI_TIMEOUT_SECONDS");
            if (int.TryParse(timeoutOverride, out var timeoutSeconds) && timeoutSeconds > 0)
            {
                settings.TimeoutSeconds = timeoutSeconds;
            }

            settings.EndpointPath = NormalizeEndpointPath(settings.EndpointPath);
            settings.TimeoutSeconds = Math.Clamp(settings.TimeoutSeconds, 10, 120);

            return settings;
        }

        private static string NormalizeEndpointPath(string? endpointPath)
        {
            var normalized = (endpointPath ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return DefaultAiAssistantEndpointPath;
            }

            return normalized.StartsWith("/", StringComparison.Ordinal) ? normalized : "/" + normalized;
        }

        private static IReadOnlyList<string> GetLocalConfigPaths()
        {
            return new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.local.json"),
                Path.Combine(Environment.CurrentDirectory, "appsettings.local.json")
            };
        }

        private static bool TryParseBoolean(string? value, out bool result)
        {
            if (bool.TryParse(value, out result))
            {
                return true;
            }

            var normalized = (value ?? string.Empty).Trim();
            if (string.Equals(normalized, "1", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "yes", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "sim", StringComparison.OrdinalIgnoreCase))
            {
                result = true;
                return true;
            }

            if (string.Equals(normalized, "0", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "no", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "nao", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "não", StringComparison.OrdinalIgnoreCase))
            {
                result = false;
                return true;
            }

            result = false;
            return false;
        }

        private static IReadOnlyList<string> BuildFirebaseStorageBucketCandidates(string configuredBucket, string projectId)
        {
            var candidates = new List<string>();

            void AddCandidate(string? bucket)
            {
                var normalized = (bucket ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(normalized) || candidates.Contains(normalized, StringComparer.OrdinalIgnoreCase))
                {
                    return;
                }

                candidates.Add(normalized);
            }

            var normalizedConfiguredBucket = (configuredBucket ?? string.Empty).Trim();
            AddCandidate(normalizedConfiguredBucket);

            if (normalizedConfiguredBucket.EndsWith(".appspot.com", StringComparison.OrdinalIgnoreCase))
            {
                AddCandidate(normalizedConfiguredBucket[..^".appspot.com".Length] + ".firebasestorage.app");
            }
            else if (normalizedConfiguredBucket.EndsWith(".firebasestorage.app", StringComparison.OrdinalIgnoreCase))
            {
                AddCandidate(normalizedConfiguredBucket[..^".firebasestorage.app".Length] + ".appspot.com");
            }

            if (!string.IsNullOrWhiteSpace(projectId))
            {
                AddCandidate($"{projectId}.firebasestorage.app");
                AddCandidate($"{projectId}.appspot.com");
            }

            return candidates;
        }
    }
}
