using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IOPath = System.IO.Path;

namespace MeuApp
{
    /// <summary>
    /// Serviço para gerenciar mensagens e conversas no Firebase Firestore
    /// </summary>
    public class ChatService
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private const long MaxChatAttachmentBytes = 26214400;
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

                var normalizedMessageType = NormalizeMessageType(messageType);
                var timestampUtc = DateTime.UtcNow;
                var conversationId = CreateConversationId(_currentUserId, contactId);
                var messageId = Guid.NewGuid().ToString("N");
                var timestampText = timestampUtc.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                var linkPreview = normalizedMessageType == "text"
                    ? await ChatLinkPreviewService.TryBuildPreviewAsync(content)
                    : null;
                var outgoingMessage = new ChatMessage
                {
                    MessageId = messageId,
                    DocumentId = messageId,
                    SenderId = _currentUserId,
                    SenderName = senderName,
                    Content = content,
                    MessageType = normalizedMessageType,
                    StickerAsset = stickerAsset ?? string.Empty,
                    LinkUrl = linkPreview?.Url ?? string.Empty,
                    LinkTitle = linkPreview?.Title ?? string.Empty,
                    LinkDescription = linkPreview?.Description ?? string.Empty,
                    LinkImageUrl = linkPreview?.ImageUrl ?? string.Empty,
                    LinkSiteName = linkPreview?.SiteName ?? string.Empty,
                    Timestamp = timestampUtc,
                    IsOwn = true,
                    IsEdited = false,
                    IsDeleted = false
                };

                var metadataResult = await UpsertConversationMetadataAsync(
                    contactId,
                    contactName,
                    senderName,
                    outgoingMessage.ConversationPreview,
                    timestampUtc,
                    normalizedMessageType
                );

                if (!metadataResult.Success)
                {
                    return metadataResult;
                }

                var messageFields = new Dictionary<string, object>
                {
                    ["messageId"] = new { stringValue = messageId },
                    ["senderId"] = new { stringValue = _currentUserId },
                    ["senderName"] = new { stringValue = senderName },
                    ["content"] = new { stringValue = outgoingMessage.Content },
                    ["messageType"] = new { stringValue = outgoingMessage.MessageType },
                    ["stickerAsset"] = new { stringValue = outgoingMessage.StickerAsset },
                    ["attachmentFileName"] = new { stringValue = string.Empty },
                    ["attachmentContentType"] = new { stringValue = string.Empty },
                    ["attachmentStoragePath"] = new { stringValue = string.Empty },
                    ["attachmentSizeBytes"] = new { integerValue = "0" },
                    ["linkUrl"] = new { stringValue = outgoingMessage.LinkUrl },
                    ["linkTitle"] = new { stringValue = outgoingMessage.LinkTitle },
                    ["linkDescription"] = new { stringValue = outgoingMessage.LinkDescription },
                    ["linkImageUrl"] = new { stringValue = outgoingMessage.LinkImageUrl },
                    ["linkSiteName"] = new { stringValue = outgoingMessage.LinkSiteName },
                    ["isEdited"] = new { booleanValue = false },
                    ["isDeleted"] = new { booleanValue = false },
                    ["timestamp"] = new { timestampValue = timestampText },
                    ["createdAt"] = new { timestampValue = timestampText },
                    ["recipientId"] = new { stringValue = contactId }
                };

                var url = AppConfig.BuildFirestoreDocumentUrl($"conversations/{conversationId}/messages/{messageId}");

                var requestBody = JsonSerializer.Serialize(new { fields = messageFields });
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
                return ChatOperationResult.Ok(outgoingMessage);
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[ChatService.SendMessage] ERRO: {ex.Message}");
                DebugHelper.WriteLine($"[ChatService.SendMessage] Stack: {ex.StackTrace}");
                return ChatOperationResult.Fail($"{ex.GetType().Name}: {ex.Message}");
            }
        }

        public async Task<ChatOperationResult> SendAttachmentMessageAsync(
            string contactId,
            string contactName,
            string senderName,
            string filePath,
            string? caption = null,
            string? attachmentPreviewDataUri = null,
            string? mediaGroupId = null,
            int mediaGroupIndex = 0,
            int mediaGroupCount = 0)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                {
                    return ChatOperationResult.Fail("Arquivo nao encontrado para envio.");
                }

                var fileInfo = new FileInfo(filePath);
                if (!fileInfo.Exists || fileInfo.Length <= 0)
                {
                    return ChatOperationResult.Fail("Arquivo vazio ou indisponivel para envio.");
                }

                if (fileInfo.Length > MaxChatAttachmentBytes)
                {
                    return ChatOperationResult.Fail("O anexo excede o limite atual de 25 MB do chat.");
                }

                var conversationId = CreateConversationId(_currentUserId, contactId);
                var messageId = Guid.NewGuid().ToString("N");
                var messageType = ResolveAttachmentMessageType(fileInfo.Name);
                var contentType = GetMimeTypeFromFileName(fileInfo.Name);
                var storagePath = BuildChatAttachmentStoragePath(conversationId, _currentUserId, messageId, fileInfo.Name);
                var fileBytes = await File.ReadAllBytesAsync(filePath);
                var uploadResult = await UploadChatAttachmentAsync(storagePath, contentType, fileBytes);
                if (!uploadResult.Success)
                {
                    return uploadResult;
                }

                var timestampUtc = DateTime.UtcNow;
                var timestampText = timestampUtc.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                var outgoingMessage = new ChatMessage
                {
                    MessageId = messageId,
                    DocumentId = messageId,
                    SenderId = _currentUserId,
                    SenderName = senderName,
                    Content = string.IsNullOrWhiteSpace(caption)
                        ? BuildAttachmentPreviewText(messageType, fileInfo.Name)
                        : caption.Trim(),
                    MessageType = messageType,
                    StickerAsset = string.Empty,
                    AttachmentFileName = fileInfo.Name,
                    AttachmentContentType = contentType,
                    AttachmentStoragePath = storagePath,
                    AttachmentSizeBytes = fileInfo.Length,
                    AttachmentLocalPath = filePath,
                    AttachmentPreviewDataUri = string.IsNullOrWhiteSpace(attachmentPreviewDataUri) ? string.Empty : attachmentPreviewDataUri.Trim(),
                    MediaGroupId = string.IsNullOrWhiteSpace(mediaGroupId) ? string.Empty : mediaGroupId.Trim(),
                    MediaGroupIndex = Math.Max(0, mediaGroupIndex),
                    MediaGroupCount = Math.Max(0, mediaGroupCount),
                    Timestamp = timestampUtc,
                    IsOwn = true,
                    IsEdited = false,
                    IsDeleted = false
                };

                var metadataResult = await UpsertConversationMetadataAsync(
                    contactId,
                    contactName,
                    senderName,
                    outgoingMessage.ConversationPreview,
                    timestampUtc,
                    messageType);
                if (!metadataResult.Success)
                {
                    await DeleteChatAttachmentIfExistsAsync(storagePath);
                    return metadataResult;
                }

                var messageFields = new Dictionary<string, object>
                {
                    ["messageId"] = new { stringValue = messageId },
                    ["senderId"] = new { stringValue = _currentUserId },
                    ["senderName"] = new { stringValue = senderName },
                    ["content"] = new { stringValue = outgoingMessage.Content },
                    ["messageType"] = new { stringValue = outgoingMessage.MessageType },
                    ["stickerAsset"] = new { stringValue = string.Empty },
                    ["attachmentFileName"] = new { stringValue = outgoingMessage.AttachmentFileName },
                    ["attachmentContentType"] = new { stringValue = outgoingMessage.AttachmentContentType },
                    ["attachmentStoragePath"] = new { stringValue = outgoingMessage.AttachmentStoragePath },
                    ["attachmentSizeBytes"] = new { integerValue = outgoingMessage.AttachmentSizeBytes.ToString() },
                    ["attachmentPreviewDataUri"] = new { stringValue = outgoingMessage.AttachmentPreviewDataUri },
                    ["mediaGroupId"] = new { stringValue = outgoingMessage.MediaGroupId },
                    ["mediaGroupIndex"] = new { integerValue = outgoingMessage.MediaGroupIndex.ToString() },
                    ["mediaGroupCount"] = new { integerValue = outgoingMessage.MediaGroupCount.ToString() },
                    ["linkUrl"] = new { stringValue = string.Empty },
                    ["linkTitle"] = new { stringValue = string.Empty },
                    ["linkDescription"] = new { stringValue = string.Empty },
                    ["linkImageUrl"] = new { stringValue = string.Empty },
                    ["linkSiteName"] = new { stringValue = string.Empty },
                    ["isEdited"] = new { booleanValue = false },
                    ["isDeleted"] = new { booleanValue = false },
                    ["timestamp"] = new { timestampValue = timestampText },
                    ["createdAt"] = new { timestampValue = timestampText },
                    ["recipientId"] = new { stringValue = contactId }
                };

                var url = AppConfig.BuildFirestoreDocumentUrl($"conversations/{conversationId}/messages/{messageId}");
                var requestBody = JsonSerializer.Serialize(new { fields = messageFields });
                var request = new HttpRequestMessage(HttpMethod.Patch, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                LogRequest("SendAttachmentMessage", request.Method.Method, url, requestBody);

                var response = await httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                LogResponse("SendAttachmentMessage", response, responseBody);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = BuildDetailedErrorMessage(response, responseBody);
                    await DeleteChatAttachmentIfExistsAsync(storagePath);
                    DebugHelper.WriteLine($"[ChatService.SendAttachment] Erro ao enviar: {errorMessage}");
                    return ChatOperationResult.Fail(errorMessage);
                }

                return ChatOperationResult.Ok(outgoingMessage);
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[ChatService.SendAttachment] ERRO: {ex.Message}");
                DebugHelper.WriteLine($"[ChatService.SendAttachment] Stack: {ex.StackTrace}");
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
                var linkPreview = await ChatLinkPreviewService.TryBuildPreviewAsync(updatedContent);
                var patchBody = JsonSerializer.Serialize(new
                {
                    fields = new Dictionary<string, object>
                    {
                        ["content"] = new { stringValue = updatedContent },
                        ["isEdited"] = new { booleanValue = true },
                        ["editedAt"] = new { timestampValue = editedAtText },
                        ["linkUrl"] = new { stringValue = linkPreview?.Url ?? string.Empty },
                        ["linkTitle"] = new { stringValue = linkPreview?.Title ?? string.Empty },
                        ["linkDescription"] = new { stringValue = linkPreview?.Description ?? string.Empty },
                        ["linkImageUrl"] = new { stringValue = linkPreview?.ImageUrl ?? string.Empty },
                        ["linkSiteName"] = new { stringValue = linkPreview?.SiteName ?? string.Empty }
                    }
                });

                var url = $"{AppConfig.BuildFirestoreDocumentUrl($"conversations/{conversationId}/messages/{documentId}")}?updateMask.fieldPaths=content&updateMask.fieldPaths=isEdited&updateMask.fieldPaths=editedAt&updateMask.fieldPaths=linkUrl&updateMask.fieldPaths=linkTitle&updateMask.fieldPaths=linkDescription&updateMask.fieldPaths=linkImageUrl&updateMask.fieldPaths=linkSiteName";
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
                if (message.HasAttachment)
                {
                    await DeleteChatAttachmentIfExistsAsync(message.AttachmentStoragePath);
                }

                var patchBody = JsonSerializer.Serialize(new
                {
                    fields = new Dictionary<string, object>
                    {
                        ["content"] = new { stringValue = deletedContent },
                        ["messageType"] = new { stringValue = "deleted" },
                        ["stickerAsset"] = new { stringValue = string.Empty },
                        ["attachmentFileName"] = new { stringValue = string.Empty },
                        ["attachmentContentType"] = new { stringValue = string.Empty },
                        ["attachmentStoragePath"] = new { stringValue = string.Empty },
                        ["attachmentSizeBytes"] = new { integerValue = "0" },
                        ["attachmentPreviewDataUri"] = new { stringValue = string.Empty },
                        ["mediaGroupId"] = new { stringValue = string.Empty },
                        ["mediaGroupIndex"] = new { integerValue = "0" },
                        ["mediaGroupCount"] = new { integerValue = "0" },
                        ["linkUrl"] = new { stringValue = string.Empty },
                        ["linkTitle"] = new { stringValue = string.Empty },
                        ["linkDescription"] = new { stringValue = string.Empty },
                        ["linkImageUrl"] = new { stringValue = string.Empty },
                        ["linkSiteName"] = new { stringValue = string.Empty },
                        ["isDeleted"] = new { booleanValue = true },
                        ["deletedAt"] = new { timestampValue = deletedAtText }
                    }
                });

                var url = $"{AppConfig.BuildFirestoreDocumentUrl($"conversations/{conversationId}/messages/{documentId}")}?updateMask.fieldPaths=content&updateMask.fieldPaths=messageType&updateMask.fieldPaths=stickerAsset&updateMask.fieldPaths=attachmentFileName&updateMask.fieldPaths=attachmentContentType&updateMask.fieldPaths=attachmentStoragePath&updateMask.fieldPaths=attachmentSizeBytes&updateMask.fieldPaths=attachmentPreviewDataUri&updateMask.fieldPaths=mediaGroupId&updateMask.fieldPaths=mediaGroupIndex&updateMask.fieldPaths=mediaGroupCount&updateMask.fieldPaths=linkUrl&updateMask.fieldPaths=linkTitle&updateMask.fieldPaths=linkDescription&updateMask.fieldPaths=linkImageUrl&updateMask.fieldPaths=linkSiteName&updateMask.fieldPaths=isDeleted&updateMask.fieldPaths=deletedAt";
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
                var url = AppConfig.BuildFirestoreDocumentUrl($"conversations/{conversationId}/messages");

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
                messages.Sort((left, right) => left.Timestamp.CompareTo(right.Timestamp));
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

                var url = $"{AppConfig.BuildFirestoreDocumentUrl($"conversations/{conversationId}")}?updateMask.fieldPaths={fieldName}";
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
                string? attachmentFileName = null;
                string? attachmentContentType = null;
                string? attachmentStoragePath = null;
                string? attachmentPreviewDataUri = null;
                string? mediaGroupId = null;
                var mediaGroupIndex = 0;
                var mediaGroupCount = 0;
                long attachmentSizeBytes = 0;
                string? linkUrl = null;
                string? linkTitle = null;
                string? linkDescription = null;
                string? linkImageUrl = null;
                string? linkSiteName = null;
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

                if (fields.TryGetProperty("attachmentFileName", out var attachmentFileNameField) &&
                    attachmentFileNameField.TryGetProperty("stringValue", out var attachmentFileNameValue))
                {
                    attachmentFileName = attachmentFileNameValue.GetString();
                }

                if (fields.TryGetProperty("attachmentContentType", out var attachmentContentTypeField) &&
                    attachmentContentTypeField.TryGetProperty("stringValue", out var attachmentContentTypeValue))
                {
                    attachmentContentType = attachmentContentTypeValue.GetString();
                }

                if (fields.TryGetProperty("attachmentStoragePath", out var attachmentStoragePathField) &&
                    attachmentStoragePathField.TryGetProperty("stringValue", out var attachmentStoragePathValue))
                {
                    attachmentStoragePath = attachmentStoragePathValue.GetString();
                }

                if (fields.TryGetProperty("attachmentPreviewDataUri", out var attachmentPreviewDataUriField) &&
                    attachmentPreviewDataUriField.TryGetProperty("stringValue", out var attachmentPreviewDataUriValue))
                {
                    attachmentPreviewDataUri = attachmentPreviewDataUriValue.GetString();
                }

                if (fields.TryGetProperty("mediaGroupId", out var mediaGroupIdField) &&
                    mediaGroupIdField.TryGetProperty("stringValue", out var mediaGroupIdValue))
                {
                    mediaGroupId = mediaGroupIdValue.GetString();
                }

                mediaGroupIndex = (int)Math.Max(0, GetLongField(fields, "mediaGroupIndex"));
                mediaGroupCount = (int)Math.Max(0, GetLongField(fields, "mediaGroupCount"));

                attachmentSizeBytes = GetLongField(fields, "attachmentSizeBytes");

                if (fields.TryGetProperty("linkUrl", out var linkUrlField) &&
                    linkUrlField.TryGetProperty("stringValue", out var linkUrlValue))
                {
                    linkUrl = linkUrlValue.GetString();
                }

                if (fields.TryGetProperty("linkTitle", out var linkTitleField) &&
                    linkTitleField.TryGetProperty("stringValue", out var linkTitleValue))
                {
                    linkTitle = linkTitleValue.GetString();
                }

                if (fields.TryGetProperty("linkDescription", out var linkDescriptionField) &&
                    linkDescriptionField.TryGetProperty("stringValue", out var linkDescriptionValue))
                {
                    linkDescription = linkDescriptionValue.GetString();
                }

                if (fields.TryGetProperty("linkImageUrl", out var linkImageUrlField) &&
                    linkImageUrlField.TryGetProperty("stringValue", out var linkImageUrlValue))
                {
                    linkImageUrl = linkImageUrlValue.GetString();
                }

                if (fields.TryGetProperty("linkSiteName", out var linkSiteNameField) &&
                    linkSiteNameField.TryGetProperty("stringValue", out var linkSiteNameValue))
                {
                    linkSiteName = linkSiteNameValue.GetString();
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
                    AttachmentFileName = attachmentFileName ?? string.Empty,
                    AttachmentContentType = attachmentContentType ?? string.Empty,
                    AttachmentStoragePath = attachmentStoragePath ?? string.Empty,
                    AttachmentSizeBytes = attachmentSizeBytes,
                    AttachmentPreviewDataUri = attachmentPreviewDataUri ?? string.Empty,
                    MediaGroupId = mediaGroupId ?? string.Empty,
                    MediaGroupIndex = mediaGroupIndex,
                    MediaGroupCount = mediaGroupCount,
                    LinkUrl = linkUrl ?? string.Empty,
                    LinkTitle = linkTitle ?? string.Empty,
                    LinkDescription = linkDescription ?? string.Empty,
                    LinkImageUrl = linkImageUrl ?? string.Empty,
                    LinkSiteName = linkSiteName ?? string.Empty,
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
                var url = AppConfig.BuildFirestoreDocumentUrl($"conversations/{conversationId}");
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

        private static long GetLongField(JsonElement fields, string fieldName)
        {
            if (fields.TryGetProperty(fieldName, out var field) &&
                field.TryGetProperty("integerValue", out var value) &&
                long.TryParse(value.GetString(), out var parsed))
            {
                return parsed;
            }

            return 0;
        }

        public async Task<ChatAttachmentDownloadResult> EnsureAttachmentLocalCopyAsync(ChatMessage message)
        {
            try
            {
                if (message == null || !message.HasAttachment || string.IsNullOrWhiteSpace(message.AttachmentStoragePath))
                {
                    return ChatAttachmentDownloadResult.Fail("A mensagem nao possui anexo remoto disponivel.");
                }

                var cachePath = BuildChatAttachmentCachePath(message);
                if (File.Exists(cachePath) && new FileInfo(cachePath).Length > 0)
                {
                    message.AttachmentLocalPath = cachePath;
                    return ChatAttachmentDownloadResult.Ok(cachePath);
                }

                Directory.CreateDirectory(IOPath.GetDirectoryName(cachePath) ?? GetChatAttachmentCacheRootDirectory());

                string? lastErrorMessage = null;
                foreach (var downloadUrl in AppConfig.BuildFirebaseStorageDownloadUrls(message.AttachmentStoragePath))
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);

                    var response = await httpClient.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        var bytes = await response.Content.ReadAsByteArrayAsync();
                        await File.WriteAllBytesAsync(cachePath, bytes);
                        message.AttachmentLocalPath = cachePath;
                        return ChatAttachmentDownloadResult.Ok(cachePath);
                    }

                    var responseBody = await response.Content.ReadAsStringAsync();
                    lastErrorMessage = BuildDetailedErrorMessage(response, responseBody);
                    if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
                    {
                        break;
                    }
                }

                return ChatAttachmentDownloadResult.Fail(lastErrorMessage ?? "Nao foi possivel baixar o anexo remoto.");
            }
            catch (Exception ex)
            {
                return ChatAttachmentDownloadResult.Fail($"{ex.GetType().Name}: {ex.Message}");
            }
        }

        private static string NormalizeMessageType(string? messageType)
        {
            return (messageType ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "sticker" => "sticker",
                "image" => "image",
                "video" => "video",
                "audio" => "audio",
                "file" => "file",
                "deleted" => "deleted",
                _ => "text"
            };
        }

        private static string ResolveAttachmentMessageType(string fileName)
        {
            var extension = IOPath.GetExtension(fileName)?.Trim().ToLowerInvariant() ?? string.Empty;
            return extension switch
            {
                ".png" or ".jpg" or ".jpeg" or ".gif" or ".webp" or ".bmp" => "image",
                ".mp4" or ".mov" or ".avi" or ".mkv" or ".wmv" or ".webm" => "video",
                ".mp3" or ".wav" or ".m4a" or ".aac" or ".ogg" or ".flac" or ".wma" => "audio",
                _ => "file"
            };
        }

        private static string BuildAttachmentPreviewText(string messageType, string fileName)
        {
            var normalizedName = string.IsNullOrWhiteSpace(fileName) ? "anexo" : fileName;
            return NormalizeMessageType(messageType) switch
            {
                "image" => $"Imagem • {normalizedName}",
                "video" => $"Video • {normalizedName}",
                "audio" => $"Audio • {normalizedName}",
                _ => $"Arquivo • {normalizedName}"
            };
        }

        private static string GetMimeTypeFromFileName(string fileName)
        {
            var extension = IOPath.GetExtension(fileName)?.Trim().ToLowerInvariant() ?? string.Empty;
            return extension switch
            {
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".bmp" => "image/bmp",
                ".mp4" => "video/mp4",
                ".mov" => "video/quicktime",
                ".avi" => "video/x-msvideo",
                ".mkv" => "video/x-matroska",
                ".wmv" => "video/x-ms-wmv",
                ".webm" => "video/webm",
                ".mp3" => "audio/mpeg",
                ".wav" => "audio/wav",
                ".m4a" => "audio/mp4",
                ".aac" => "audio/aac",
                ".ogg" => "audio/ogg",
                ".flac" => "audio/flac",
                ".wma" => "audio/x-ms-wma",
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".txt" => "text/plain",
                ".zip" => "application/zip",
                ".rar" => "application/vnd.rar",
                _ => "application/octet-stream"
            };
        }

        private static string BuildChatAttachmentStoragePath(string conversationId, string ownerUserId, string messageId, string fileName)
        {
            return $"chat-assets/{SanitizeStorageSegment(conversationId)}/{SanitizeStorageSegment(ownerUserId)}/{SanitizeStorageSegment(messageId)}/{SanitizeStorageFileName(fileName)}";
        }

        private static string SanitizeStorageSegment(string? value)
        {
            var normalized = (value ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return "unknown";
            }

            var builder = new StringBuilder(normalized.Length);
            foreach (var character in normalized)
            {
                builder.Append(char.IsLetterOrDigit(character) || character == '-' || character == '_'
                    ? character
                    : '-');
            }

            var sanitized = builder.ToString().Trim('-');
            return string.IsNullOrWhiteSpace(sanitized) ? "unknown" : sanitized;
        }

        private static string SanitizeStorageFileName(string? fileName)
        {
            var candidate = string.IsNullOrWhiteSpace(fileName) ? "arquivo.bin" : fileName.Trim();
            var invalidChars = IOPath.GetInvalidFileNameChars();
            var builder = new StringBuilder(candidate.Length);
            foreach (var character in candidate)
            {
                builder.Append(Array.IndexOf(invalidChars, character) >= 0 || character == '/' || character == '\\'
                    ? '-'
                    : character);
            }

            var sanitized = builder.ToString().Replace(' ', '-');
            return string.IsNullOrWhiteSpace(sanitized) ? "arquivo.bin" : sanitized;
        }

        private async Task<ChatOperationResult> UploadChatAttachmentAsync(string storagePath, string contentType, byte[] fileBytes)
        {
            string? lastErrorMessage = null;
            foreach (var uploadUrl in AppConfig.BuildFirebaseStorageUploadUrls(storagePath))
            {
                var request = new HttpRequestMessage(HttpMethod.Post, uploadUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);
                request.Content = new ByteArrayContent(fileBytes);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

                var response = await httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    return ChatOperationResult.Ok();
                }

                lastErrorMessage = BuildDetailedErrorMessage(response, responseBody);
                if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
                {
                    break;
                }
            }

            return ChatOperationResult.Fail(lastErrorMessage ?? "Nao foi possivel enviar o anexo ao Firebase Storage.");
        }

        private async Task DeleteChatAttachmentIfExistsAsync(string storagePath)
        {
            if (string.IsNullOrWhiteSpace(storagePath))
            {
                return;
            }

            foreach (var metadataUrl in AppConfig.BuildFirebaseStorageMetadataUrls(storagePath))
            {
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Delete, metadataUrl);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);

                    var response = await httpClient.SendAsync(request);
                    if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return;
                    }
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine($"[ChatService.DeleteAttachment] Falha ao remover {storagePath}: {ex.Message}");
                }
            }
        }

        private static string GetChatAttachmentCacheRootDirectory()
        {
            return IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MeuApp", "chat-cache");
        }

        private static string BuildChatAttachmentCachePath(ChatMessage message)
        {
            var safeMessageId = SanitizeStorageSegment(string.IsNullOrWhiteSpace(message.MessageId) ? Guid.NewGuid().ToString("N") : message.MessageId);
            var safeFileName = SanitizeStorageFileName(string.IsNullOrWhiteSpace(message.AttachmentFileName) ? "anexo.bin" : message.AttachmentFileName);
            return IOPath.Combine(GetChatAttachmentCacheRootDirectory(), $"{safeMessageId}-{safeFileName}");
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
            var url = AppConfig.BuildFirestoreRunQueryUrl();
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

            return $"MessageId={message.MessageId}; DocumentId={message.DocumentId}; SenderId={message.SenderId}; SenderName={SanitizeForLog(message.SenderName)}; MessageType={message.MessageType}; IsEdited={message.IsEdited}; IsDeleted={message.IsDeleted}; Timestamp={message.Timestamp:O}; EditedAt={message.EditedAt:O}; DeletedAt={message.DeletedAt:O}; Content={SanitizeForLog(message.Content)}; AttachmentFile={SanitizeForLog(message.AttachmentFileName)}; AttachmentStorage={SanitizeForLog(message.AttachmentStoragePath)}; AttachmentSize={message.AttachmentSizeBytes}; MediaGroupId={SanitizeForLog(message.MediaGroupId)}; MediaIndex={message.MediaGroupIndex}; MediaCount={message.MediaGroupCount}; LinkUrl={SanitizeForLog(message.LinkUrl)}; LinkTitle={SanitizeForLog(message.LinkTitle)}";
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

                var url = $"{AppConfig.BuildFirestoreDocumentUrl($"conversations/{conversationId}")}?updateMask.fieldPaths=lastMessage&updateMask.fieldPaths=lastMessageTime&updateMask.fieldPaths=lastSenderId";
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
        public ChatMessage? Message { get; init; }

        public static ChatOperationResult Ok(ChatMessage? message = null)
        {
            return new ChatOperationResult { Success = true, Message = message };
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

    public class ChatAttachmentDownloadResult
    {
        public bool Success { get; init; }
        public string ErrorMessage { get; init; } = string.Empty;
        public string LocalPath { get; init; } = string.Empty;

        public static ChatAttachmentDownloadResult Ok(string localPath)
        {
            return new ChatAttachmentDownloadResult
            {
                Success = true,
                LocalPath = localPath
            };
        }

        public static ChatAttachmentDownloadResult Fail(string errorMessage)
        {
            return new ChatAttachmentDownloadResult
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }
}
