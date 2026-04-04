using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MeuApp
{
    /// <summary>
    /// Serviço para gerenciar mensagens e conversas no Firebase Firestore
    /// </summary>
    public class ChatService
    {
        private const string FirebaseProjectId = "obsseractpi";
        private static readonly HttpClient httpClient = new HttpClient();
        private readonly string _idToken;
        private readonly string _currentUserId;

        public ChatService(string idToken, string currentUserId)
        {
            _idToken = idToken;
            _currentUserId = currentUserId;
            DebugHelper.InitializeSilent();
        }

        public string BuildConversationId(string contactId)
        {
            return CreateConversationId(_currentUserId, contactId);
        }

        /// <summary>
        /// Envia uma mensagem para uma conversa no Firebase
        /// </summary>
        public async Task<ChatOperationResult> SendMessageAsync(string contactId, string contactName, string senderName, string content, string messageType = "text", string? stickerAsset = null)
        {
            try
            {
                DebugHelper.WriteLine($"[ChatService.SendMessage] Enviando mensagem para {contactId}");

                var timestampUtc = DateTime.UtcNow;
                var metadataResult = await UpsertConversationMetadataAsync(
                    contactId,
                    contactName,
                    senderName,
                    content,
                    timestampUtc,
                    messageType
                );

                if (!metadataResult.Success)
                {
                    return metadataResult;
                }

                // Criar ID único para a conversa (sempre em ordem alfabética para consistência)
                var conversationId = CreateConversationId(_currentUserId, contactId);
                var messageId = Guid.NewGuid().ToString();
                var timestampText = timestampUtc.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

                // Preparar dados da mensagem
                var messageData = new
                {
                    fields = new
                    {
                        messageId = new { stringValue = messageId },
                        senderId = new { stringValue = _currentUserId },
                        senderName = new { stringValue = senderName },
                        content = new { stringValue = content },
                        messageType = new { stringValue = string.IsNullOrWhiteSpace(messageType) ? "text" : messageType },
                        stickerAsset = new { stringValue = stickerAsset ?? string.Empty },
                        isEdited = new { booleanValue = false },
                        isDeleted = new { booleanValue = false },
                        timestamp = new { timestampValue = timestampText },
                        createdAt = new { timestampValue = timestampText },
                        recipientId = new { stringValue = contactId }
                    }
                };

                // URL para salvar no Firestore
                var url = $"https://firestore.googleapis.com/v1/projects/{FirebaseProjectId}/databases/(default)/documents/conversations/{conversationId}/messages/{messageId}";

                var requestBody = JsonSerializer.Serialize(messageData);
                var request = new HttpRequestMessage(HttpMethod.Patch, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);
                request.Content = new StringContent(
                    requestBody,
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                LogRequest("SendMessage", request.Method.Method, url, requestBody);

                var response = await httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                LogResponse("SendMessage", response, responseBody);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = BuildDetailedErrorMessage(response, responseBody);
                    DebugHelper.WriteLine($"[ChatService.SendMessage] Erro ao enviar: {errorMessage}");
                    return ChatOperationResult.Fail(errorMessage);
                }

                DebugHelper.WriteLine($"[ChatService.SendMessage] Mensagem enviada com sucesso!");
                return ChatOperationResult.Ok();
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[ChatService.SendMessage] ERRO: {ex.Message}");
                DebugHelper.WriteLine($"[ChatService.SendMessage] Stack: {ex.StackTrace}");
                return ChatOperationResult.Fail($"{ex.GetType().Name}: {ex.Message}");
            }
        }

        public async Task<ChatOperationResult> UpdateMessageAsync(string contactId, ChatMessage message, string updatedContent)
        {
            try
            {
                if (message == null || string.IsNullOrWhiteSpace(updatedContent))
                {
                    return ChatOperationResult.Fail("Mensagem inválida para edição.");
                }

                var conversationId = CreateConversationId(_currentUserId, contactId);
                var documentId = ResolveMessageDocumentId(message);
                if (string.IsNullOrWhiteSpace(documentId))
                {
                    return ChatOperationResult.Fail("Não foi possível localizar a mensagem para edição.");
                }

                var editedAt = DateTime.UtcNow;
                var editedAtText = editedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                var patchBody = JsonSerializer.Serialize(new
                {
                    fields = new Dictionary<string, object>
                    {
                        ["content"] = new { stringValue = updatedContent },
                        ["isEdited"] = new { booleanValue = true },
                        ["editedAt"] = new { timestampValue = editedAtText }
                    }
                });

                var url = $"https://firestore.googleapis.com/v1/projects/{FirebaseProjectId}/databases/(default)/documents/conversations/{conversationId}/messages/{documentId}?updateMask.fieldPaths=content&updateMask.fieldPaths=isEdited&updateMask.fieldPaths=editedAt";
                var request = new HttpRequestMessage(HttpMethod.Patch, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);
                request.Content = new StringContent(patchBody, Encoding.UTF8, "application/json");

                LogMessageMutationContext("UpdateMessage", conversationId, documentId, message, patchBody);
                LogRequest("UpdateMessage", request.Method.Method, url, patchBody);

                var response = await httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                LogResponse("UpdateMessage", response, responseBody);

                if (!response.IsSuccessStatusCode)
                {
                    DebugHelper.WriteLine($"[ChatService.UpdateMessage] Falha ao editar {documentId}: {DescribeMessageForLog(message)}");
                    return ChatOperationResult.Fail(AttachLogPath(BuildDetailedErrorMessage(response, responseBody)));
                }

                await SyncConversationSummaryAsync(contactId);
                return ChatOperationResult.Ok();
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[ChatService.UpdateMessage] ERRO: {ex.Message}");
                DebugHelper.WriteLine($"[ChatService.UpdateMessage] Stack: {ex.StackTrace}");
                return ChatOperationResult.Fail(AttachLogPath($"{ex.GetType().Name}: {ex.Message}"));
            }
        }

        public async Task<ChatOperationResult> DeleteMessageAsync(string contactId, ChatMessage message)
        {
            try
            {
                if (message == null)
                {
                    return ChatOperationResult.Fail("Mensagem inválida para exclusão.");
                }

                var conversationId = CreateConversationId(_currentUserId, contactId);
                var documentId = ResolveMessageDocumentId(message);
                if (string.IsNullOrWhiteSpace(documentId))
                {
                    return ChatOperationResult.Fail("Não foi possível localizar a mensagem para exclusão.");
                }

                var deletedAt = DateTime.UtcNow;
                var deletedAtText = deletedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                var deletedContent = BuildDeletedMessageText(message.SenderName);
                var patchBody = JsonSerializer.Serialize(new
                {
                    fields = new Dictionary<string, object>
                    {
                        ["content"] = new { stringValue = deletedContent },
                        ["messageType"] = new { stringValue = "deleted" },
                        ["stickerAsset"] = new { stringValue = string.Empty },
                        ["isDeleted"] = new { booleanValue = true },
                        ["deletedAt"] = new { timestampValue = deletedAtText }
                    }
                });

                var url = $"https://firestore.googleapis.com/v1/projects/{FirebaseProjectId}/databases/(default)/documents/conversations/{conversationId}/messages/{documentId}?updateMask.fieldPaths=content&updateMask.fieldPaths=messageType&updateMask.fieldPaths=stickerAsset&updateMask.fieldPaths=isDeleted&updateMask.fieldPaths=deletedAt";
                var request = new HttpRequestMessage(HttpMethod.Patch, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);
                request.Content = new StringContent(patchBody, Encoding.UTF8, "application/json");

                LogMessageMutationContext("DeleteMessage", conversationId, documentId, message, patchBody);
                LogRequest("DeleteMessage", request.Method.Method, url, patchBody);

                var response = await httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                LogResponse("DeleteMessage", response, responseBody);

                if (!response.IsSuccessStatusCode)
                {
                    DebugHelper.WriteLine($"[ChatService.DeleteMessage] Falha ao apagar {documentId}: {DescribeMessageForLog(message)}");
                    return ChatOperationResult.Fail(AttachLogPath(BuildDetailedErrorMessage(response, responseBody)));
                }

                await SyncConversationSummaryAsync(contactId);
                return ChatOperationResult.Ok();
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[ChatService.DeleteMessage] ERRO: {ex.Message}");
                DebugHelper.WriteLine($"[ChatService.DeleteMessage] Stack: {ex.StackTrace}");
                return ChatOperationResult.Fail(AttachLogPath($"{ex.GetType().Name}: {ex.Message}"));
            }
        }

        /// <summary>
        /// Carrega todas as mensagens de uma conversa
        /// </summary>
        public async Task<List<ChatMessage>> LoadMessagesAsync(string contactId)
        {
            try
            {
                DebugHelper.WriteLine($"[ChatService.LoadMessages] Carregando mensagens de {contactId}");

                var conversationId = CreateConversationId(_currentUserId, contactId);
                var messages = new List<ChatMessage>();

                // URL para obter documentos da conversa
                var url = $"https://firestore.googleapis.com/v1/projects/{FirebaseProjectId}/databases/(default)/documents/conversations/{conversationId}/messages";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);

                LogRequest("LoadMessages", request.Method.Method, url, null);

                var response = await httpClient.SendAsync(request);
                var jsonContent = await response.Content.ReadAsStringAsync();

                LogResponse("LoadMessages", response, jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    DebugHelper.WriteLine($"[ChatService.LoadMessages] Erro ao carregar: {BuildDetailedErrorMessage(response, jsonContent)}");
                    return messages;
                }

                DebugHelper.WriteLine($"[ChatService.LoadMessages] Resposta: {jsonContent}");

                using (JsonDocument doc = JsonDocument.Parse(jsonContent))
                {
                    var root = doc.RootElement;

                    if (root.TryGetProperty("documents", out var documentsArray))
                    {
                        foreach (var msgDoc in documentsArray.EnumerateArray())
                        {
                            if (msgDoc.TryGetProperty("fields", out var fields))
                            {
                                var message = ParseMessageFromFirestore(msgDoc, fields);
                                if (message != null)
                                {
                                    messages.Add(message);
                                }
                            }
                        }
                    }
                }

                // Ordenar por timestamp
                messages.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));

                DebugHelper.WriteLine($"[ChatService.LoadMessages] Carregadas {messages.Count} mensagens");
                return messages;
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[ChatService.LoadMessages] ERRO: {ex.Message}");
                DebugHelper.WriteLine($"[ChatService.LoadMessages] Stack: {ex.StackTrace}");
                return new List<ChatMessage>();
            }
        }

        public async Task<List<Conversation>> LoadConversationsAsync()
        {
            var conversations = new List<Conversation>();

            try
            {
                var seenConversationIds = new HashSet<string>(StringComparer.Ordinal);
                var userAConversationsTask = LoadConversationsByParticipantFieldAsync("userAId");
                var userBConversationsTask = LoadConversationsByParticipantFieldAsync("userBId");
                var results = await Task.WhenAll(userAConversationsTask, userBConversationsTask);

                foreach (var batch in results)
                {
                    foreach (var conversation in batch)
                    {
                        if (conversation == null || string.IsNullOrWhiteSpace(conversation.ConversationId))
                        {
                            continue;
                        }

                        if (seenConversationIds.Add(conversation.ConversationId))
                        {
                            conversations.Add(conversation);
                        }
                    }
                }

                conversations.Sort((left, right) => right.LastMessageTime.CompareTo(left.LastMessageTime));
                return conversations;
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[ChatService.LoadConversations] ERRO: {ex.Message}");
                DebugHelper.WriteLine($"[ChatService.LoadConversations] Stack: {ex.StackTrace}");
                return conversations;
            }
        }

        public async Task<ChatOperationResult> MarkConversationAsReadAsync(string contactId)
        {
            try
            {
                var conversationId = CreateConversationId(_currentUserId, contactId);
                var fieldName = IsCurrentUserFirstParticipant(conversationId) ? "lastReadAtUserA" : "lastReadAtUserB";
                var patchBody = JsonSerializer.Serialize(new
                {
                    fields = new Dictionary<string, object>
                    {
                        [fieldName] = new { timestampValue = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") }
                    }
                });

                var url = $"https://firestore.googleapis.com/v1/projects/{FirebaseProjectId}/databases/(default)/documents/conversations/{conversationId}?updateMask.fieldPaths={fieldName}";
                var request = new HttpRequestMessage(HttpMethod.Patch, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);
                request.Content = new StringContent(patchBody, Encoding.UTF8, "application/json");

                LogRequest("MarkConversationRead", request.Method.Method, url, patchBody);

                var response = await httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                LogResponse("MarkConversationRead", response, responseBody);

                if (!response.IsSuccessStatusCode)
                {
                    return ChatOperationResult.Fail(BuildDetailedErrorMessage(response, responseBody));
                }

                return ChatOperationResult.Ok();
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[ChatService.MarkConversationAsRead] ERRO: {ex.Message}");
                return ChatOperationResult.Fail($"{ex.GetType().Name}: {ex.Message}");
            }
        }

        /// <summary>
        /// Cria um ID único para a conversa entre dois usuários
        /// </summary>
        private string CreateConversationId(string userId1, string userId2)
        {
            // Para garantir que a conversa seja a mesma para ambos os usuários,
            // ordenamos os IDs alfabeticamente
            var ids = new[] { userId1, userId2 };
            Array.Sort(ids);
            return $"{ids[0]}-{ids[1]}";
        }

        /// <summary>
        /// Converte dados do Firestore para ChatMessage
        /// </summary>
        private ChatMessage? ParseMessageFromFirestore(JsonElement messageDocument, JsonElement fields)
        {
            try
            {
                string? messageId = null;
                string? senderId = null;
                string? senderName = null;
                string? content = null;
                var messageType = "text";
                string? stickerAsset = null;
                DateTime timestamp = DateTime.Now;
                DateTime? editedAt = null;
                DateTime? deletedAt = null;
                var isEdited = false;
                var isDeleted = false;

                if (fields.TryGetProperty("messageId", out var messageIdField) &&
                    messageIdField.TryGetProperty("stringValue", out var messageIdValue))
                {
                    messageId = messageIdValue.GetString();
                }

                if (fields.TryGetProperty("senderId", out var senderIdField) && 
                    senderIdField.TryGetProperty("stringValue", out var senderIdValue))
                {
                    senderId = senderIdValue.GetString();
                }

                if (fields.TryGetProperty("senderName", out var senderNameField) && 
                    senderNameField.TryGetProperty("stringValue", out var senderNameValue))
                {
                    senderName = senderNameValue.GetString();
                }

                if (fields.TryGetProperty("content", out var contentField) && 
                    contentField.TryGetProperty("stringValue", out var contentValue))
                {
                    content = contentValue.GetString();
                }

                if (fields.TryGetProperty("messageType", out var messageTypeField) &&
                    messageTypeField.TryGetProperty("stringValue", out var messageTypeValue))
                {
                    messageType = messageTypeValue.GetString() ?? "text";
                }

                if (fields.TryGetProperty("stickerAsset", out var stickerAssetField) &&
                    stickerAssetField.TryGetProperty("stringValue", out var stickerAssetValue))
                {
                    stickerAsset = stickerAssetValue.GetString();
                }

                if (fields.TryGetProperty("timestamp", out var timestampField) && 
                    timestampField.TryGetProperty("timestampValue", out var timestampValue))
                {
                    if (DateTime.TryParse(timestampValue.GetString(), out var parsedTime))
                    {
                        timestamp = parsedTime;
                    }
                }
                else if (fields.TryGetProperty("createdAt", out var createdAtField) &&
                    createdAtField.TryGetProperty("timestampValue", out var createdAtValue))
                {
                    if (DateTime.TryParse(createdAtValue.GetString(), out var parsedCreatedAt))
                    {
                        timestamp = parsedCreatedAt;
                    }
                }

                if (fields.TryGetProperty("editedAt", out var editedAtField) &&
                    editedAtField.TryGetProperty("timestampValue", out var editedAtValue) &&
                    DateTime.TryParse(editedAtValue.GetString(), out var parsedEditedAt))
                {
                    editedAt = parsedEditedAt;
                }

                if (fields.TryGetProperty("deletedAt", out var deletedAtField) &&
                    deletedAtField.TryGetProperty("timestampValue", out var deletedAtValue) &&
                    DateTime.TryParse(deletedAtValue.GetString(), out var parsedDeletedAt))
                {
                    deletedAt = parsedDeletedAt;
                }

                isEdited = GetBoolField(fields, "isEdited");
                isDeleted = GetBoolField(fields, "isDeleted") || string.Equals(messageType, "deleted", StringComparison.OrdinalIgnoreCase);

                if (string.IsNullOrEmpty(senderId) || string.IsNullOrEmpty(content))
                {
                    return null;
                }

                return new ChatMessage
                {
                    MessageId = messageId ?? ExtractDocumentId(messageDocument),
                    DocumentId = ExtractDocumentId(messageDocument),
                    SenderId = senderId,
                    SenderName = senderName ?? "Usuário",
                    Content = content,
                    MessageType = messageType,
                    StickerAsset = stickerAsset ?? string.Empty,
                    Timestamp = timestamp,
                    EditedAt = editedAt,
                    DeletedAt = deletedAt,
                    IsOwn = senderId == _currentUserId
                    ,IsEdited = isEdited
                    ,IsDeleted = isDeleted
                };
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[ChatService.ParseMessage] Erro ao parsear mensagem: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Obtém o resumo da última mensagem de uma conversa
        /// </summary>
        public async Task<(string lastMessage, DateTime lastTime)> GetConversationSummaryAsync(string contactId, string contactName)
        {
            try
            {
                var messages = await LoadMessagesAsync(contactId);
                
                if (messages.Count == 0)
                {
                    return ("Nenhuma mensagem", DateTime.Now);
                }

                var lastMessage = messages[messages.Count - 1];
                return (lastMessage.ConversationPreview, lastMessage.Timestamp);
            }
            catch
            {
                return ("Erro ao carregar", DateTime.Now);
            }
        }

        private static void LogRequest(string operation, string method, string url, string? body)
        {
            DebugHelper.WriteLine($"[HTTP {operation}] {method} {url}");
            if (Debugger.IsAttached && !string.IsNullOrWhiteSpace(body))
            {
                DebugHelper.WriteLine($"[HTTP {operation}] Request Body: {body}");
            }
        }

        private static void LogResponse(string operation, HttpResponseMessage response, string body)
        {
            DebugHelper.WriteLine($"[HTTP {operation}] Status: {(int)response.StatusCode} {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                DebugHelper.WriteLine($"[HTTP {operation}] PayloadLength: {body?.Length ?? 0}");
                return;
            }

            foreach (var header in response.Headers)
            {
                DebugHelper.WriteLine($"[HTTP {operation}] Header {header.Key}: {string.Join(", ", header.Value)}");
            }

            foreach (var header in response.Content.Headers)
            {
                DebugHelper.WriteLine($"[HTTP {operation}] Content-Header {header.Key}: {string.Join(", ", header.Value)}");
            }

            DebugHelper.WriteLine($"[HTTP {operation}] Response Body: {body}");

            foreach (var link in ExtractLinks(body))
            {
                DebugHelper.WriteLine($"[HTTP {operation}] Link detectado: {link}");
            }
        }

        private static string BuildDetailedErrorMessage(HttpResponseMessage response, string responseBody)
        {
            var builder = new StringBuilder();
            builder.Append($"HTTP {(int)response.StatusCode} {response.StatusCode}");

            var firestoreMessage = TryExtractFirestoreError(responseBody);
            if (!string.IsNullOrWhiteSpace(firestoreMessage))
            {
                builder.Append($" | {firestoreMessage}");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden &&
                responseBody.Contains("PERMISSION_DENIED", StringComparison.OrdinalIgnoreCase))
            {
                builder.Append(" | Firestore rules bloquearam acesso a conversations/{conversationId} ou conversations/{conversationId}/messages");
            }

            foreach (var header in response.Headers)
            {
                if (header.Key.Equals("WWW-Authenticate", StringComparison.OrdinalIgnoreCase) ||
                    header.Key.Equals("Location", StringComparison.OrdinalIgnoreCase))
                {
                    builder.Append($" | {header.Key}: {string.Join(", ", header.Value)}");
                }
            }

            var links = ExtractLinks(responseBody);
            if (links.Count > 0)
            {
                builder.Append($" | Links: {string.Join(" | ", links)}");
            }

            return builder.ToString();
        }

        private static string? TryExtractFirestoreError(string responseBody)
        {
            if (string.IsNullOrWhiteSpace(responseBody))
            {
                return null;
            }

            try
            {
                using var doc = JsonDocument.Parse(responseBody);
                if (!doc.RootElement.TryGetProperty("error", out var errorElement))
                {
                    return responseBody;
                }

                string? status = null;
                string? message = null;

                if (errorElement.TryGetProperty("status", out var statusElement))
                {
                    status = statusElement.GetString();
                }

                if (errorElement.TryGetProperty("message", out var messageElement))
                {
                    message = messageElement.GetString();
                }

                if (!string.IsNullOrWhiteSpace(status) && !string.IsNullOrWhiteSpace(message))
                {
                    return $"{status}: {message}";
                }

                return message ?? responseBody;
            }
            catch
            {
                return responseBody;
            }
        }

        private static List<string> ExtractLinks(string text)
        {
            var links = new List<string>();
            if (string.IsNullOrWhiteSpace(text))
            {
                return links;
            }

            var parts = text.Split(new[] { '"', '\'', ' ', '\r', '\n', '\t', '<', '>' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                if (part.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    part.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    links.Add(part.TrimEnd(',', '.', ';'));
                }
            }

            return links;
        }

        private async Task<ChatOperationResult> UpsertConversationMetadataAsync(
            string contactId,
            string contactName,
            string senderName,
            string content,
            DateTime timestampUtc,
            string messageType)
        {
            try
            {
                var conversationId = CreateConversationId(_currentUserId, contactId);
                var isUserA = IsCurrentUserFirstParticipant(conversationId);
                var senderReadField = isUserA ? "lastReadAtUserA" : "lastReadAtUserB";

                var fields = new Dictionary<string, object>
                {
                    ["conversationId"] = new { stringValue = conversationId },
                    ["userAId"] = new { stringValue = isUserA ? _currentUserId : contactId },
                    ["userAName"] = new { stringValue = isUserA ? senderName : contactName },
                    ["userBId"] = new { stringValue = isUserA ? contactId : _currentUserId },
                    ["userBName"] = new { stringValue = isUserA ? contactName : senderName },
                    ["participants"] = new
                    {
                        arrayValue = new
                        {
                            values = new object[]
                            {
                                new { stringValue = _currentUserId },
                                new { stringValue = contactId }
                            }
                        }
                    },
                    ["lastMessage"] = new { stringValue = string.Equals(messageType, "sticker", StringComparison.OrdinalIgnoreCase) ? (string.IsNullOrWhiteSpace(content) ? "Figurinha enviada" : content) : content },
                    ["lastMessageTime"] = new { timestampValue = timestampUtc.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") },
                    ["lastSenderId"] = new { stringValue = _currentUserId },
                    [senderReadField] = new { timestampValue = timestampUtc.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") }
                };

                var requestBody = JsonSerializer.Serialize(new { fields });
                var url = $"https://firestore.googleapis.com/v1/projects/{FirebaseProjectId}/databases/(default)/documents/conversations/{conversationId}";
                var request = new HttpRequestMessage(HttpMethod.Patch, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                LogRequest("UpsertConversation", request.Method.Method, url, requestBody);

                var response = await httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                LogResponse("UpsertConversation", response, responseBody);

                if (!response.IsSuccessStatusCode)
                {
                    return ChatOperationResult.Fail(BuildDetailedErrorMessage(response, responseBody));
                }

                return ChatOperationResult.Ok();
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[ChatService.UpsertConversationMetadata] ERRO: {ex.Message}");
                return ChatOperationResult.Fail($"{ex.GetType().Name}: {ex.Message}");
            }
        }

        private Conversation? ParseConversationFromFirestore(JsonElement conversationDocument)
        {
            try
            {
                if (conversationDocument.TryGetProperty("document", out var wrappedDocument))
                {
                    conversationDocument = wrappedDocument;
                }

                if (!conversationDocument.TryGetProperty("fields", out var fields))
                {
                    return null;
                }

                var userAId = GetStringField(fields, "userAId");
                var userBId = GetStringField(fields, "userBId");

                if (userAId != _currentUserId && userBId != _currentUserId)
                {
                    return null;
                }

                var isCurrentUserA = userAId == _currentUserId;
                var contactId = isCurrentUserA ? userBId : userAId;
                var contactName = isCurrentUserA ? GetStringField(fields, "userBName") : GetStringField(fields, "userAName");
                var lastSenderId = GetStringField(fields, "lastSenderId");
                var lastMessageTime = GetTimestampField(fields, "lastMessageTime") ?? DateTime.Now;
                var lastReadAt = isCurrentUserA ? GetTimestampField(fields, "lastReadAtUserA") : GetTimestampField(fields, "lastReadAtUserB");
                var hasUnread = !string.IsNullOrWhiteSpace(lastSenderId)
                    && lastSenderId != _currentUserId
                    && (!lastReadAt.HasValue || lastReadAt.Value < lastMessageTime);

                return new Conversation
                {
                    ConversationId = GetStringField(fields, "conversationId") ?? ExtractDocumentId(conversationDocument),
                    ContactId = contactId ?? string.Empty,
                    ContactName = contactName ?? "Usuário",
                    LastMessage = GetStringField(fields, "lastMessage") ?? "",
                    LastMessageTime = lastMessageTime,
                    LastSenderId = lastSenderId ?? "",
                    LastReadAt = lastReadAt,
                    HasUnread = hasUnread,
                    Messages = new List<ChatMessage>()
                };
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[ChatService.ParseConversation] ERRO: {ex.Message}");
                return null;
            }
        }

        private bool IsCurrentUserFirstParticipant(string conversationId)
        {
            return conversationId.StartsWith(_currentUserId + "-", StringComparison.Ordinal);
        }

        private static string? GetStringField(JsonElement fields, string fieldName)
        {
            if (fields.TryGetProperty(fieldName, out var field) &&
                field.TryGetProperty("stringValue", out var value))
            {
                return value.GetString();
            }

            return null;
        }

        private static DateTime? GetTimestampField(JsonElement fields, string fieldName)
        {
            if (fields.TryGetProperty(fieldName, out var field) &&
                field.TryGetProperty("timestampValue", out var value) &&
                DateTime.TryParse(value.GetString(), out var parsed))
            {
                return parsed;
            }

            return null;
        }

        private static bool GetBoolField(JsonElement fields, string fieldName)
        {
            if (fields.TryGetProperty(fieldName, out var field) &&
                field.TryGetProperty("booleanValue", out var value))
            {
                return value.GetBoolean();
            }

            return false;
        }

        private static string ExtractDocumentId(JsonElement conversationDocument)
        {
            if (conversationDocument.TryGetProperty("name", out var nameElement))
            {
                var name = nameElement.GetString() ?? string.Empty;
                var index = name.LastIndexOf('/');
                if (index >= 0 && index < name.Length - 1)
                {
                    return name[(index + 1)..];
                }
            }

            return string.Empty;
        }

        private async Task<List<Conversation>> LoadConversationsByParticipantFieldAsync(string fieldName)
        {
            var conversations = new List<Conversation>();
            var url = $"https://firestore.googleapis.com/v1/projects/{FirebaseProjectId}/databases/(default)/documents:runQuery";
            var requestBody = JsonSerializer.Serialize(new
            {
                structuredQuery = new
                {
                    select = new
                    {
                        fields = new[]
                        {
                            new { fieldPath = "conversationId" },
                            new { fieldPath = "userAId" },
                            new { fieldPath = "userAName" },
                            new { fieldPath = "userBId" },
                            new { fieldPath = "userBName" },
                            new { fieldPath = "lastMessage" },
                            new { fieldPath = "lastMessageTime" },
                            new { fieldPath = "lastSenderId" },
                            new { fieldPath = "lastReadAtUserA" },
                            new { fieldPath = "lastReadAtUserB" }
                        }
                    },
                    from = new[] { new { collectionId = "conversations" } },
                    where = new
                    {
                        fieldFilter = new
                        {
                            field = new { fieldPath = fieldName },
                            op = "EQUAL",
                            value = new { stringValue = _currentUserId }
                        }
                    }
                }
            });

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            LogRequest($"LoadConversations-{fieldName}", request.Method.Method, url, requestBody);

            var response = await httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            LogResponse($"LoadConversations-{fieldName}", response, responseBody);

            if (!response.IsSuccessStatusCode)
            {
                DebugHelper.WriteLine($"[ChatService.LoadConversations] Erro ao carregar por {fieldName}: {BuildDetailedErrorMessage(response, responseBody)}");
                return conversations;
            }

            using var doc = JsonDocument.Parse(responseBody);
            if (doc.RootElement.ValueKind != JsonValueKind.Array)
            {
                return conversations;
            }

            foreach (var result in doc.RootElement.EnumerateArray())
            {
                var conversation = ParseConversationFromFirestore(result);
                if (conversation != null && !string.IsNullOrWhiteSpace(conversation.ConversationId))
                {
                    conversations.Add(conversation);
                }
            }

            return conversations;
        }

        private static string BuildDeletedMessageText(string senderName)
        {
            return $"'{(string.IsNullOrWhiteSpace(senderName) ? "Usuário" : senderName)}' apagou essa mensagem (X)";
        }

        private static void LogMessageMutationContext(string operation, string conversationId, string documentId, ChatMessage message, string patchBody)
        {
            DebugHelper.WriteLine($"[ChatService.{operation}] ConversationId={conversationId} | DocumentId={documentId}");
            DebugHelper.WriteLine($"[ChatService.{operation}] MessageSnapshot: {DescribeMessageForLog(message)}");
            DebugHelper.WriteLine($"[ChatService.{operation}] PatchBody: {patchBody}");
        }

        private static string DescribeMessageForLog(ChatMessage? message)
        {
            if (message == null)
            {
                return "<null>";
            }

            return $"MessageId={message.MessageId}; DocumentId={message.DocumentId}; SenderId={message.SenderId}; SenderName={SanitizeForLog(message.SenderName)}; MessageType={message.MessageType}; IsEdited={message.IsEdited}; IsDeleted={message.IsDeleted}; Timestamp={message.Timestamp:O}; EditedAt={message.EditedAt:O}; DeletedAt={message.DeletedAt:O}; Content={SanitizeForLog(message.Content)}";
        }

        private static string SanitizeForLog(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "<empty>";
            }

            return value.Replace("\r", "\\r").Replace("\n", "\\n");
        }

        private static string AttachLogPath(string message)
        {
            return $"{message} | Log: {DebugHelper.GetLogFilePath()}";
        }

        private static string ResolveMessageDocumentId(ChatMessage message)
        {
            if (!string.IsNullOrWhiteSpace(message.DocumentId))
            {
                return message.DocumentId;
            }

            return message.MessageId;
        }

        private async Task SyncConversationSummaryAsync(string contactId)
        {
            try
            {
                var conversationId = CreateConversationId(_currentUserId, contactId);
                var messages = await LoadMessagesAsync(contactId);
                var lastMessage = messages.Count > 0 ? messages[^1] : null;
                var lastMessageText = lastMessage?.ConversationPreview ?? "Nenhuma mensagem";
                var lastMessageTime = lastMessage?.Timestamp ?? DateTime.UtcNow;
                var lastSenderId = lastMessage?.SenderId ?? string.Empty;

                var patchBody = JsonSerializer.Serialize(new
                {
                    fields = new Dictionary<string, object>
                    {
                        ["lastMessage"] = new { stringValue = lastMessageText },
                        ["lastMessageTime"] = new { timestampValue = lastMessageTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ") },
                        ["lastSenderId"] = new { stringValue = lastSenderId }
                    }
                });

                var url = $"https://firestore.googleapis.com/v1/projects/{FirebaseProjectId}/databases/(default)/documents/conversations/{conversationId}?updateMask.fieldPaths=lastMessage&updateMask.fieldPaths=lastMessageTime&updateMask.fieldPaths=lastSenderId";
                var request = new HttpRequestMessage(HttpMethod.Patch, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);
                request.Content = new StringContent(patchBody, Encoding.UTF8, "application/json");

                LogRequest("SyncConversationSummary", request.Method.Method, url, patchBody);

                var response = await httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                LogResponse("SyncConversationSummary", response, responseBody);
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[ChatService.SyncConversationSummary] ERRO: {ex.Message}");
            }
        }
    }

    public class ChatOperationResult
    {
        public bool Success { get; init; }
        public string ErrorMessage { get; init; } = "";

        public static ChatOperationResult Ok()
        {
            return new ChatOperationResult { Success = true };
        }

        public static ChatOperationResult Fail(string errorMessage)
        {
            return new ChatOperationResult
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }
}
