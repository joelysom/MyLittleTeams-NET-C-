using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MeuApp
{
    public class ConnectionService
    {
        private const string FirebaseProjectId = "obsseractpi";
        private static readonly HttpClient httpClient = new HttpClient();
        private readonly string _idToken;
        private readonly UserProfile _currentProfile;

        public ConnectionService(string idToken, UserProfile currentProfile)
        {
            _idToken = idToken;
            _currentProfile = currentProfile;
            DebugHelper.InitializeSilent();
        }

        public async Task<List<UserConnectionInfo>> LoadConnectionsAsync()
        {
            var connections = new List<UserConnectionInfo>();

            try
            {
                if (string.IsNullOrWhiteSpace(_currentProfile.UserId) || string.IsNullOrWhiteSpace(_idToken))
                {
                    return connections;
                }

                var url = $"https://firestore.googleapis.com/v1/projects/{FirebaseProjectId}/databases/(default)/documents/userConnections";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);

                var response = await httpClient.SendAsync(request);
                var jsonContent = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    DebugHelper.WriteLine($"[ConnectionService.LoadConnections] Erro HTTP: {response.StatusCode} | {jsonContent}");
                    return connections;
                }

                using var doc = JsonDocument.Parse(jsonContent);
                if (!doc.RootElement.TryGetProperty("documents", out var documentsArray))
                {
                    return connections;
                }

                foreach (var item in documentsArray.EnumerateArray())
                {
                    if (!item.TryGetProperty("fields", out var fields))
                    {
                        continue;
                    }

                    var ownerUserId = GetStringField(fields, "userId");
                    if (!string.Equals(ownerUserId, _currentProfile.UserId, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    connections.Add(new UserConnectionInfo
                    {
                        UserId = ownerUserId,
                        ConnectionId = GetStringField(fields, "connectionId"),
                        ConnectedUserId = GetStringField(fields, "connectedUserId"),
                        ConnectedUserName = GetStringField(fields, "connectedUserName"),
                        ConnectedUserEmail = GetStringField(fields, "connectedUserEmail"),
                        RequestedBy = GetStringField(fields, "requestedBy"),
                        Status = GetStringField(fields, "status"),
                        NotificationType = GetStringField(fields, "notificationType"),
                        NotificationMessage = GetStringField(fields, "notificationMessage"),
                        IsRead = GetBoolField(fields, "isRead"),
                        AddedAt = GetTimestampField(fields, "addedAt") ?? DateTime.UtcNow,
                        UpdatedAt = GetTimestampField(fields, "updatedAt") ?? DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[ConnectionService.LoadConnections] Exceção: {ex.Message}");
            }

            return connections;
        }

        public async Task<ConnectionOperationResult> CreateConnectionRequestAsync(UserInfo targetUser)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_currentProfile.UserId) || string.IsNullOrWhiteSpace(_idToken))
                {
                    return ConnectionOperationResult.Fail("Sessão sem credenciais para salvar conexão.");
                }

                if (string.IsNullOrWhiteSpace(targetUser.UserId))
                {
                    return ConnectionOperationResult.Fail("Usuário de destino inválido.");
                }

                if (string.Equals(targetUser.UserId, _currentProfile.UserId, StringComparison.OrdinalIgnoreCase))
                {
                    return ConnectionOperationResult.Fail("Não é possível conectar com o próprio perfil.");
                }

                var connectionId = CreateConnectionId(_currentProfile.UserId, targetUser.UserId);
                var now = DateTime.UtcNow;
                var nowValue = ToTimestamp(now);
                var userIds = new[] { _currentProfile.UserId, targetUser.UserId };
                Array.Sort(userIds, StringComparer.Ordinal);

                var connectionPayload = new
                {
                    fields = new
                    {
                        connectionId = new { stringValue = connectionId },
                        userAId = new { stringValue = userIds[0] },
                        userBId = new { stringValue = userIds[1] },
                        userIds = new
                        {
                            arrayValue = new
                            {
                                values = new object[]
                                {
                                    new { stringValue = userIds[0] },
                                    new { stringValue = userIds[1] }
                                }
                            }
                        },
                        requestedBy = new { stringValue = _currentProfile.UserId },
                        respondedBy = new { stringValue = string.Empty },
                        status = new { stringValue = "pending" },
                        createdAt = new { timestampValue = nowValue },
                        updatedAt = new { timestampValue = nowValue }
                    }
                };

                var connectionResult = await PatchDocumentAsync($"connections/{connectionId}", connectionPayload);
                if (!connectionResult.Success)
                {
                    return connectionResult;
                }

                var requesterReference = new UserConnectionInfo
                {
                    UserId = _currentProfile.UserId,
                    ConnectionId = connectionId,
                    ConnectedUserId = targetUser.UserId,
                    ConnectedUserName = targetUser.Name,
                    ConnectedUserEmail = targetUser.Email,
                    RequestedBy = _currentProfile.UserId,
                    Status = "pendingOutgoing",
                    NotificationType = string.Empty,
                    NotificationMessage = string.Empty,
                    IsRead = true,
                    AddedAt = now,
                    UpdatedAt = now
                };

                var targetReference = new UserConnectionInfo
                {
                    UserId = targetUser.UserId,
                    ConnectionId = connectionId,
                    ConnectedUserId = _currentProfile.UserId,
                    ConnectedUserName = _currentProfile.Name,
                    ConnectedUserEmail = _currentProfile.Email,
                    RequestedBy = _currentProfile.UserId,
                    Status = "pendingIncoming",
                    NotificationType = "request",
                    NotificationMessage = $"{_currentProfile.Name} quer se conectar com você.",
                    IsRead = false,
                    AddedAt = now,
                    UpdatedAt = now
                };

                var requesterResult = await SaveUserConnectionReferenceAsync(requesterReference);
                if (!requesterResult.Success)
                {
                    return requesterResult;
                }

                return await SaveUserConnectionReferenceAsync(targetReference);
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[ConnectionService.CreateConnectionRequest] Exceção: {ex.Message}");
                return ConnectionOperationResult.Fail($"{ex.GetType().Name}: {ex.Message}");
            }
        }

        public async Task<ConnectionOperationResult> AcceptConnectionAsync(UserConnectionInfo request)
        {
            try
            {
                var now = DateTime.UtcNow;
                var ids = GetConnectionUsers(request.ConnectionId);
                if (ids.Length != 2)
                {
                    return ConnectionOperationResult.Fail("Identificador da conexão inválido.");
                }

                var connectionPayload = new
                {
                    fields = new
                    {
                        connectionId = new { stringValue = request.ConnectionId },
                        userAId = new { stringValue = ids[0] },
                        userBId = new { stringValue = ids[1] },
                        userIds = new
                        {
                            arrayValue = new
                            {
                                values = new object[]
                                {
                                    new { stringValue = ids[0] },
                                    new { stringValue = ids[1] }
                                }
                            }
                        },
                        requestedBy = new { stringValue = request.RequestedBy },
                        respondedBy = new { stringValue = _currentProfile.UserId },
                        status = new { stringValue = "active" },
                        createdAt = new { timestampValue = ToTimestamp(request.AddedAt == default ? now : request.AddedAt.ToUniversalTime()) },
                        updatedAt = new { timestampValue = ToTimestamp(now) }
                    }
                };

                var connectionResult = await PatchDocumentAsync($"connections/{request.ConnectionId}", connectionPayload);
                if (!connectionResult.Success)
                {
                    return connectionResult;
                }

                request.Status = "connected";
                request.NotificationType = string.Empty;
                request.NotificationMessage = string.Empty;
                request.IsRead = true;
                request.UpdatedAt = now;

                var currentUserResult = await SaveUserConnectionReferenceAsync(request);
                if (!currentUserResult.Success)
                {
                    return currentUserResult;
                }

                var requesterReference = new UserConnectionInfo
                {
                    UserId = request.ConnectedUserId,
                    ConnectionId = request.ConnectionId,
                    ConnectedUserId = _currentProfile.UserId,
                    ConnectedUserName = _currentProfile.Name,
                    ConnectedUserEmail = _currentProfile.Email,
                    RequestedBy = request.RequestedBy,
                    Status = "connected",
                    NotificationType = "accepted",
                    NotificationMessage = $"{_currentProfile.Name} aceitou sua conexão.",
                    IsRead = false,
                    AddedAt = request.AddedAt == default ? now : request.AddedAt,
                    UpdatedAt = now
                };

                return await SaveUserConnectionReferenceAsync(requesterReference);
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[ConnectionService.AcceptConnection] Exceção: {ex.Message}");
                return ConnectionOperationResult.Fail($"{ex.GetType().Name}: {ex.Message}");
            }
        }

        public async Task<ConnectionOperationResult> DeclineConnectionAsync(UserConnectionInfo request)
        {
            try
            {
                var now = DateTime.UtcNow;
                var ids = GetConnectionUsers(request.ConnectionId);
                if (ids.Length != 2)
                {
                    return ConnectionOperationResult.Fail("Identificador da conexão inválido.");
                }

                var connectionPayload = new
                {
                    fields = new
                    {
                        connectionId = new { stringValue = request.ConnectionId },
                        userAId = new { stringValue = ids[0] },
                        userBId = new { stringValue = ids[1] },
                        userIds = new
                        {
                            arrayValue = new
                            {
                                values = new object[]
                                {
                                    new { stringValue = ids[0] },
                                    new { stringValue = ids[1] }
                                }
                            }
                        },
                        requestedBy = new { stringValue = request.RequestedBy },
                        respondedBy = new { stringValue = _currentProfile.UserId },
                        status = new { stringValue = "declined" },
                        createdAt = new { timestampValue = ToTimestamp(request.AddedAt == default ? now : request.AddedAt.ToUniversalTime()) },
                        updatedAt = new { timestampValue = ToTimestamp(now) }
                    }
                };

                var patchResult = await PatchDocumentAsync($"connections/{request.ConnectionId}", connectionPayload);
                if (!patchResult.Success)
                {
                    return patchResult;
                }

                var currentDelete = await DeleteDocumentAsync($"userConnections/{request.UserId}_{request.ConnectionId}");
                if (!currentDelete.Success)
                {
                    return currentDelete;
                }

                return await DeleteDocumentAsync($"userConnections/{request.ConnectedUserId}_{request.ConnectionId}");
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[ConnectionService.DeclineConnection] Exceção: {ex.Message}");
                return ConnectionOperationResult.Fail($"{ex.GetType().Name}: {ex.Message}");
            }
        }

        public async Task<ConnectionOperationResult> MarkNotificationAsReadAsync(UserConnectionInfo item)
        {
            item.IsRead = true;
            item.NotificationType = string.Empty;
            item.NotificationMessage = string.Empty;
            item.UpdatedAt = DateTime.UtcNow;
            return await SaveUserConnectionReferenceAsync(item);
        }

        public string GetRelationshipState(string otherUserId, IReadOnlyCollection<UserConnectionInfo> items)
        {
            var item = items
                .Where(entry => string.Equals(entry.ConnectedUserId, otherUserId, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(entry => entry.UpdatedAt)
                .FirstOrDefault();

            return item?.Status ?? "none";
        }

        private async Task<ConnectionOperationResult> SaveUserConnectionReferenceAsync(UserConnectionInfo item)
        {
            var documentId = $"{item.UserId}_{item.ConnectionId}";
            var payload = new
            {
                fields = new
                {
                    userId = new { stringValue = item.UserId },
                    connectionId = new { stringValue = item.ConnectionId },
                    connectedUserId = new { stringValue = item.ConnectedUserId },
                    connectedUserName = new { stringValue = item.ConnectedUserName ?? string.Empty },
                    connectedUserEmail = new { stringValue = item.ConnectedUserEmail ?? string.Empty },
                    requestedBy = new { stringValue = item.RequestedBy ?? string.Empty },
                    status = new { stringValue = item.Status ?? "none" },
                    notificationType = new { stringValue = item.NotificationType ?? string.Empty },
                    notificationMessage = new { stringValue = item.NotificationMessage ?? string.Empty },
                    isRead = new { booleanValue = item.IsRead },
                    addedAt = new { timestampValue = ToTimestamp(item.AddedAt == default ? DateTime.UtcNow : item.AddedAt.ToUniversalTime()) },
                    updatedAt = new { timestampValue = ToTimestamp(item.UpdatedAt == default ? DateTime.UtcNow : item.UpdatedAt.ToUniversalTime()) }
                }
            };

            return await PatchDocumentAsync($"userConnections/{documentId}", payload);
        }

        private async Task<ConnectionOperationResult> PatchDocumentAsync(string relativePath, object payload)
        {
            var url = $"https://firestore.googleapis.com/v1/projects/{FirebaseProjectId}/databases/(default)/documents/{relativePath}";
            var request = new HttpRequestMessage(HttpMethod.Patch, url)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);

            var response = await httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                DebugHelper.WriteLine($"[ConnectionService.PatchDocument] Erro HTTP: {response.StatusCode} | {responseBody}");
                return ConnectionOperationResult.Fail($"HTTP {(int)response.StatusCode}: {responseBody}");
            }

            return ConnectionOperationResult.Ok(string.Empty);
        }

        private async Task<ConnectionOperationResult> DeleteDocumentAsync(string relativePath)
        {
            var url = $"https://firestore.googleapis.com/v1/projects/{FirebaseProjectId}/databases/(default)/documents/{relativePath}";
            var request = new HttpRequestMessage(HttpMethod.Delete, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);

            var response = await httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                DebugHelper.WriteLine($"[ConnectionService.DeleteDocument] Erro HTTP: {response.StatusCode} | {responseBody}");
                return ConnectionOperationResult.Fail($"HTTP {(int)response.StatusCode}: {responseBody}");
            }

            return ConnectionOperationResult.Ok(string.Empty);
        }

        private static string CreateConnectionId(string leftUserId, string rightUserId)
        {
            var ids = new[] { leftUserId.Trim(), rightUserId.Trim() };
            Array.Sort(ids, StringComparer.Ordinal);
            return $"{ids[0]}-{ids[1]}";
        }

        private static string[] GetConnectionUsers(string connectionId)
        {
            return (connectionId ?? string.Empty).Split('-', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        private static string GetStringField(JsonElement fields, string fieldName)
        {
            if (fields.TryGetProperty(fieldName, out var field) && field.TryGetProperty("stringValue", out var value))
            {
                return value.GetString() ?? string.Empty;
            }

            return string.Empty;
        }

        private static bool GetBoolField(JsonElement fields, string fieldName)
        {
            if (fields.TryGetProperty(fieldName, out var field) && field.TryGetProperty("booleanValue", out var value))
            {
                return value.GetBoolean();
            }

            return false;
        }

        private static DateTime? GetTimestampField(JsonElement fields, string fieldName)
        {
            if (!fields.TryGetProperty(fieldName, out var field) || !field.TryGetProperty("timestampValue", out var value))
            {
                return null;
            }

            return DateTime.TryParse(value.GetString(), out var timestamp)
                ? DateTime.SpecifyKind(timestamp, DateTimeKind.Utc).ToLocalTime()
                : null;
        }

        private static string ToTimestamp(DateTime dateTimeUtc)
        {
            return (dateTimeUtc.Kind == DateTimeKind.Utc ? dateTimeUtc : dateTimeUtc.ToUniversalTime()).ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        }
    }

    public class UserConnectionInfo
    {
        public string UserId { get; set; } = string.Empty;
        public string ConnectionId { get; set; } = string.Empty;
        public string ConnectedUserId { get; set; } = string.Empty;
        public string ConnectedUserName { get; set; } = string.Empty;
        public string ConnectedUserEmail { get; set; } = string.Empty;
        public string RequestedBy { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string NotificationType { get; set; } = string.Empty;
        public string NotificationMessage { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime AddedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ConnectionOperationResult
    {
        public bool Success { get; set; }
        public string ConnectionId { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }

        public static ConnectionOperationResult Ok(string connectionId)
        {
            return new ConnectionOperationResult { Success = true, ConnectionId = connectionId };
        }

        public static ConnectionOperationResult Fail(string errorMessage)
        {
            return new ConnectionOperationResult { Success = false, ErrorMessage = errorMessage };
        }
    }
}