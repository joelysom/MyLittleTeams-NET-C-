using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MeuApp
{
    /// <summary>
    /// Serviço para gerenciar equipes de projetos no Firebase Firestore
    /// </summary>
    public class TeamService
    {
        private const string FirebaseProjectId = "obsseractpi";
        private static readonly HttpClient httpClient = new HttpClient();
        private readonly string _idToken;
        private readonly string _currentUserId;

        public TeamService(string idToken, string currentUserId)
        {
            _idToken = idToken;
            _currentUserId = currentUserId;
            DebugHelper.InitializeSilent();
        }

        /// <summary>
        /// Salva uma equipe no Firebase (criar ou atualizar)
        /// </summary>
        public async Task<TeamOperationResult> SaveTeamAsync(TeamWorkspaceInfo team)
        {
            try
            {
                DebugHelper.WriteLine($"\n[TeamService.SaveTeam] ===== INICIANDO SALVAMENTO =====");
                DebugHelper.WriteLine($"[TeamService.SaveTeam] Equipe: '{team.TeamName}'");
                DebugHelper.WriteLine($"[TeamService.SaveTeam] ClassId: '{team.ClassId}'");
                DebugHelper.WriteLine($"[TeamService.SaveTeam] Membros: {team.Members.Count}");
                DebugHelper.WriteLine($"[TeamService.SaveTeam] UCs: {team.Ucs.Count}");

                if (string.IsNullOrWhiteSpace(team.ClassId) || string.IsNullOrWhiteSpace(team.TeamName))
                {
                    var error = "ClassId ou TeamName inválidos";
                    DebugHelper.WriteLine($"[TeamService.SaveTeam] ERRO: {error}");
                    return TeamOperationResult.Fail(error);
                }

                // Usar um ID determinístico para a equipe
                var teamId = string.IsNullOrWhiteSpace(team.TeamId)
                    ? GenerateTeamId(team.ClassId, team.TeamName)
                    : NormalizeTeamCode(team.TeamId);
                team.TeamId = teamId;
                team.CreatedBy = string.IsNullOrWhiteSpace(team.CreatedBy) ? _currentUserId : team.CreatedBy;
                DebugHelper.WriteLine($"[TeamService.SaveTeam] TeamId gerado: '{teamId}'");

                // Preparar dados da equipe com timestamp de atualização
                var createdAt = team.CreatedAt == default ? DateTime.UtcNow : team.CreatedAt;
                var updatedAt = DateTime.UtcNow;
                team.CreatedAt = createdAt;
                team.UpdatedAt = updatedAt;
                var createdAtValue = ToFirestoreTimestamp(createdAt);
                var updatedAtValue = ToFirestoreTimestamp(updatedAt);

                var teamData = new
                {
                    fields = new
                    {
                        teamId = new { stringValue = teamId },
                        teamName = new { stringValue = team.TeamName },
                        course = new { stringValue = team.Course ?? "" },
                        className = new { stringValue = team.ClassName ?? "" },
                        classId = new { stringValue = team.ClassId },
                        createdBy = new { stringValue = team.CreatedBy },
                        createdAt = new { timestampValue = createdAtValue },
                        updatedAt = new { timestampValue = updatedAtValue },
                        projectProgress = new { integerValue = team.ProjectProgress.ToString() },
                        projectDeadline = CreateTimestampOrNullValue(team.ProjectDeadline),
                        projectStatus = new { stringValue = team.ProjectStatus ?? "Planejamento" },
                        milestones = new { arrayValue = new { values = ConvertMilestonesToFirestoreArray(team.Milestones) } },
                        members = new { arrayValue = new { values = ConvertMembersToFirestoreArray(team.Members) } },
                        memberIds = new { arrayValue = new { values = ConvertMemberIdsToFirestoreArray(team.Members) } },
                        ucs = new { arrayValue = new { values = ConvertStringsToFirestoreArray(team.Ucs) } },
                        assets = new { arrayValue = new { values = ConvertAssetsToFirestoreArray(team.Assets) } },
                        taskColumns = new { arrayValue = new { values = ConvertTaskColumnsToFirestoreArray(team.TaskColumns) } },
                        notifications = new { arrayValue = new { values = ConvertNotificationsToFirestoreArray(team.Notifications) } },
                        chatMessages = new { arrayValue = new { values = ConvertChatMessagesToFirestoreArray(team.ChatMessages) } },
                        csdBoard = ConvertCsdBoardToFirestoreMap(team.CsdBoard),
                        isActive = new { booleanValue = true }
                    }
                };

                // URL para salvar no Firestore - usando POST com documento específico
                var url = $"https://firestore.googleapis.com/v1/projects/{FirebaseProjectId}/databases/(default)/documents/teams/{teamId}";

                var requestBody = JsonSerializer.Serialize(teamData);
                DebugHelper.WriteLine($"[TeamService.SaveTeam] URL: {url}");

                // Criar o request usando PATCH
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                // Enviar requisição
                DebugHelper.WriteLine($"[TeamService.SaveTeam] Enviando requisição para Firebase...");
                var response = await httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                DebugHelper.WriteLine($"[TeamService.SaveTeam] Status Code: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = $"HTTP {(int)response.StatusCode}: {responseBody.Substring(0, Math.Min(responseBody.Length, 200))}";
                    DebugHelper.WriteLine($"[TeamService.SaveTeam] ❌ ERRO AO SALVAR: {errorMsg}");
                    return TeamOperationResult.Fail(errorMsg);
                }

                // Salvar referência em userTeams para carregamento rápido
                var referenceResult = await SaveTeamReferenceForMembersAsync(team, teamId);
                if (!referenceResult.Success)
                {
                    DebugHelper.WriteLine($"[TeamService.SaveTeam] ❌ ERRO AO SALVAR REFERÊNCIAS: {referenceResult.ErrorMessage}");
                    return referenceResult;
                }

                DebugHelper.WriteLine($"[TeamService.SaveTeam] ✅ Equipe '{team.TeamName}' salva com sucesso!");
                DebugHelper.WriteLine($"[TeamService.SaveTeam] ===== SALVAMENTO CONCLUÍDO =====\n");

                return TeamOperationResult.Ok();
            }
            catch (Exception ex)
            {
                var errorMsg = $"{ex.GetType().Name}: {ex.Message}";
                DebugHelper.WriteLine($"[TeamService.SaveTeam] ❌ EXCEÇÃO: {errorMsg}");
                DebugHelper.WriteLine($"[TeamService.SaveTeam] Stack: {ex.StackTrace}");
                return TeamOperationResult.Fail(errorMsg);
            }
        }

        /// <summary>
        /// Salva referência da equipe na coleção userTeams para cada membro
        /// </summary>
        private async Task<TeamOperationResult> SaveTeamReferenceForMembersAsync(TeamWorkspaceInfo team, string teamId)
        {
            try
            {
                var uniqueUserIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                DebugHelper.WriteLine($"[TeamService.SaveTeamReferences] Salvando referências para {team.Members.Count} membros...");

                // Salvar para o criador
                if (!string.IsNullOrWhiteSpace(_currentUserId))
                {
                    uniqueUserIds.Add(_currentUserId);
                }

                // Salvar para cada membro
                foreach (var member in team.Members)
                {
                    if (!string.IsNullOrWhiteSpace(member.UserId))
                    {
                        uniqueUserIds.Add(member.UserId);
                    }
                }

                foreach (var userId in uniqueUserIds)
                {
                    var referenceResult = await SaveTeamReferenceForUserAsync(userId, teamId, team.TeamName);
                    if (!referenceResult.Success)
                    {
                        return referenceResult;
                    }
                }

                var cleanupResult = await CleanupRemovedMemberReferencesAsync(teamId, uniqueUserIds);
                if (!cleanupResult.Success)
                {
                    return cleanupResult;
                }

                DebugHelper.WriteLine($"[TeamService.SaveTeamReferences] ✅ Referências salvas com sucesso");
                return TeamOperationResult.Ok();
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeamService.SaveTeamReferences] ⚠️ Erro ao salvar referências: {ex.Message}");
                return TeamOperationResult.Fail(ex.Message);
            }
        }

        /// <summary>
        /// Salva referência de uma equipe para um usuário específico
        /// </summary>
        private async Task<TeamOperationResult> SaveTeamReferenceForUserAsync(string userId, string teamId, string teamName)
        {
            try
            {
                // Documento com nome único baseado em userId e teamId
                var docId = $"{userId}_{teamId}";

                var referenceData = new
                {
                    fields = new
                    {
                        userId = new { stringValue = userId },
                        teamId = new { stringValue = teamId },
                        teamName = new { stringValue = teamName },
                        addedAt = new { timestampValue = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") }
                    }
                };

                var url = $"https://firestore.googleapis.com/v1/projects/{FirebaseProjectId}/databases/(default)/documents/userTeams/{docId}";
                var requestBody = JsonSerializer.Serialize(referenceData);

                var request = new HttpRequestMessage(new HttpMethod("PATCH"), url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                var response = await httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    DebugHelper.WriteLine($"[TeamService.SaveTeamReference] ✅ Ref para '{userId}'");
                    return TeamOperationResult.Ok();
                }

                var errorMessage = $"HTTP {(int)response.StatusCode}: {responseBody.Substring(0, Math.Min(responseBody.Length, 200))}";
                DebugHelper.WriteLine($"[TeamService.SaveTeamReference] ⚠️ Erro ref '{userId}': {errorMessage}");
                return TeamOperationResult.Fail(errorMessage);
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeamService.SaveTeamReference] ⚠️ Exceção: {ex.Message}");
                return TeamOperationResult.Fail(ex.Message);
            }
        }

        private async Task<TeamOperationResult> CleanupRemovedMemberReferencesAsync(string teamId, HashSet<string> activeUserIds)
        {
            try
            {
                var existingReferences = await GetTeamReferenceDocumentsAsync(teamId);
                foreach (var reference in existingReferences)
                {
                    if (activeUserIds.Contains(reference.UserId))
                    {
                        continue;
                    }

                    var deleteResult = await DeleteTeamReferenceDocumentAsync(reference.DocumentId);
                    if (!deleteResult.Success)
                    {
                        return deleteResult;
                    }
                }

                return TeamOperationResult.Ok();
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeamService.CleanupReferences] Erro: {ex.Message}");
                return TeamOperationResult.Fail(ex.Message);
            }
        }

        /// <summary>
        /// Carrega todas as equipes do usuário do Firebase
        /// </summary>
        public async Task<List<TeamWorkspaceInfo>> LoadTeamsAsync()
        {
            var teams = new List<TeamWorkspaceInfo>();

            try
            {
                DebugHelper.WriteLine($"\n[TeamService.LoadTeams] ===== INICIANDO CARREGAMENTO =====");
                DebugHelper.WriteLine($"[TeamService.LoadTeams] Usuário: '{_currentUserId}'");

                // Primeiro, obter as referências de equipes do usuário
                var teamIds = await GetUserTeamIdsAsync(_currentUserId);
                DebugHelper.WriteLine($"[TeamService.LoadTeams] Equipes encontradas: {teamIds.Count}");

                var loadTasks = teamIds
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Select(LoadTeamByIdAsync)
                    .ToArray();

                var loadedTeams = loadTasks.Length > 0
                    ? await Task.WhenAll(loadTasks)
                    : Array.Empty<TeamWorkspaceInfo?>();

                foreach (var team in loadedTeams)
                {
                    if (team != null)
                    {
                        teams.Add(team);
                        DebugHelper.WriteLine($"[TeamService.LoadTeams] ✅ Carregada: '{team.TeamName}'");
                    }
                }

                if (teams.Count == 0)
                {
                    DebugHelper.WriteLine("[TeamService.LoadTeams] Nenhuma equipe por referência. Tentando fallback por membros...");
                    teams = await LoadTeamsByMembershipFallbackAsync();
                }

                DebugHelper.WriteLine($"[TeamService.LoadTeams] ===== CONCLUÍDO ({teams.Count} equipes) =====\n");
                return teams;
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeamService.LoadTeams] ❌ ERRO: {ex.Message}");
                DebugHelper.WriteLine($"[TeamService.LoadTeams] Stack: {ex.StackTrace}");
                return teams;
            }
        }

        /// <summary>
        /// Obtém os IDs das equipes de um usuário
        /// </summary>
        private async Task<List<string>> GetUserTeamIdsAsync(string userId)
        {
            var teamIds = new List<string>();

            try
            {
                DebugHelper.WriteLine($"[TeamService.GetUserTeamIds] Buscando equipes para '{userId}'...");

                var url = $"https://firestore.googleapis.com/v1/projects/{FirebaseProjectId}/databases/(default)/documents:runQuery";
                var requestBody = JsonSerializer.Serialize(new
                {
                    structuredQuery = new
                    {
                        select = new
                        {
                            fields = new[]
                            {
                                new { fieldPath = "teamId" }
                            }
                        },
                        from = new[] { new { collectionId = "userTeams" } },
                        where = new
                        {
                            fieldFilter = new
                            {
                                field = new { fieldPath = "userId" },
                                op = "EQUAL",
                                value = new { stringValue = userId }
                            }
                        }
                    }
                });

                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                var response = await httpClient.SendAsync(request);
                var jsonContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    DebugHelper.WriteLine($"[TeamService.GetUserTeamIds] Erro HTTP: {response.StatusCode}");
                    return teamIds;
                }

                using (JsonDocument doc = JsonDocument.Parse(jsonContent))
                {
                    if (doc.RootElement.ValueKind != JsonValueKind.Array)
                    {
                        return teamIds;
                    }

                    foreach (var result in doc.RootElement.EnumerateArray())
                    {
                        if (!result.TryGetProperty("document", out var userTeamDoc) ||
                            !userTeamDoc.TryGetProperty("fields", out var fields))
                        {
                            continue;
                        }

                        if (fields.TryGetProperty("teamId", out var teamIdField) &&
                            teamIdField.TryGetProperty("stringValue", out var teamIdValue))
                        {
                            var teamId = teamIdValue.GetString();
                            if (!string.IsNullOrEmpty(teamId))
                            {
                                teamIds.Add(teamId);
                                DebugHelper.WriteLine($"[TeamService.GetUserTeamIds] TeamId: '{teamId}'");
                            }
                        }
                    }
                }

                DebugHelper.WriteLine($"[TeamService.GetUserTeamIds] Total: {teamIds.Count}");
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeamService.GetUserTeamIds] Erro: {ex.Message}");
            }

            return teamIds;
        }

        /// <summary>
        /// Carrega uma equipe específica pelo ID
        /// </summary>
        private async Task<TeamWorkspaceInfo?> LoadTeamByIdAsync(string teamId)
        {
            try
            {
                DebugHelper.WriteLine($"[TeamService.LoadTeamById] Carregando '{teamId}'...");

                var url = $"https://firestore.googleapis.com/v1/projects/{FirebaseProjectId}/databases/(default)/documents/teams/{teamId}";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);

                var response = await httpClient.SendAsync(request);
                var jsonContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    DebugHelper.WriteLine($"[TeamService.LoadTeamById] Erro: {response.StatusCode}");
                    return null;
                }

                using (JsonDocument doc = JsonDocument.Parse(jsonContent))
                {
                    var root = doc.RootElement;

                    if (root.TryGetProperty("fields", out var fields))
                    {
                        var team = ParseTeamFromFirestore(fields);
                        return team;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeamService.LoadTeamById] Erro: {ex.Message}");
                return null;
            }
        }

        public async Task<TeamJoinResult> JoinTeamByCodeAsync(string joinCode)
        {
            try
            {
                var normalizedTeamId = NormalizeTeamCode(joinCode);
                if (string.IsNullOrWhiteSpace(normalizedTeamId))
                {
                    return TeamJoinResult.Fail("Codigo da equipe invalido.");
                }

                DebugHelper.WriteLine($"[TeamService.JoinTeamByCode] Vinculando equipe '{normalizedTeamId}' ao usuário '{_currentUserId}'...");

                var team = await LoadTeamByIdAsync(normalizedTeamId);
                if (team == null)
                {
                    return TeamJoinResult.Fail("Equipe nao encontrada para o codigo informado.");
                }

                var referenceResult = await SaveTeamReferenceForUserAsync(_currentUserId, normalizedTeamId, team.TeamName);
                if (!referenceResult.Success)
                {
                    return TeamJoinResult.Fail(referenceResult.ErrorMessage ?? "Nao foi possivel vincular a equipe.");
                }

                return TeamJoinResult.Ok(team);
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeamService.JoinTeamByCode] Erro: {ex.Message}");
                return TeamJoinResult.Fail(ex.Message);
            }
        }

        /// <summary>
        /// Deleta uma equipe do Firebase
        /// </summary>
        public async Task<TeamOperationResult> DeleteTeamAsync(TeamWorkspaceInfo team)
        {
            try
            {
                DebugHelper.WriteLine($"[TeamService.DeleteTeam] Deletando '{team.TeamName}'...");

                var teamId = string.IsNullOrWhiteSpace(team.TeamId)
                    ? GenerateTeamId(team.ClassId, team.TeamName)
                    : NormalizeTeamCode(team.TeamId);

                var cleanupResult = await DeleteTeamReferencesByTeamIdAsync(teamId);
                if (!cleanupResult.Success)
                {
                    return cleanupResult;
                }

                var url = $"https://firestore.googleapis.com/v1/projects/{FirebaseProjectId}/databases/(default)/documents/teams/{teamId}";

                var request = new HttpRequestMessage(HttpMethod.Delete, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);

                var response = await httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = $"HTTP {(int)response.StatusCode}";
                    DebugHelper.WriteLine($"[TeamService.DeleteTeam] Erro: {errorMsg}");
                    return TeamOperationResult.Fail(errorMsg);
                }

                DebugHelper.WriteLine($"[TeamService.DeleteTeam] ✅ Deletada");
                return TeamOperationResult.Ok();
            }
            catch (Exception ex)
            {
                var errorMsg = $"{ex.GetType().Name}: {ex.Message}";
                DebugHelper.WriteLine($"[TeamService.DeleteTeam] ERRO: {errorMsg}");
                return TeamOperationResult.Fail(errorMsg);
            }
        }

        public static string GenerateTeamId(string classId, string teamName)
        {
            var combined = $"{classId}_{teamName}".Replace(" ", "_").ToLowerInvariant();
            return combined.Substring(0, Math.Min(combined.Length, 100));
        }

        public static string NormalizeTeamCode(string teamCode)
        {
            return (teamCode ?? string.Empty)
                .Trim()
                .Replace(" ", "_")
                .ToLowerInvariant();
        }

        private object[] ConvertMembersToFirestoreArray(List<UserInfo> members)
        {
            var result = new List<object>();

            foreach (var member in members)
            {
                result.Add(new
                {
                    mapValue = new
                    {
                        fields = new
                        {
                            userId = new { stringValue = member.UserId ?? "" },
                            name = new { stringValue = member.Name ?? "" },
                            email = new { stringValue = member.Email ?? "" },
                            registration = new { stringValue = member.Registration ?? "" },
                            course = new { stringValue = member.Course ?? "" },
                            role = new { stringValue = member.Role ?? "member" },
                            avatarBody = new { stringValue = member.AvatarBody ?? "" },
                            avatarHair = new { stringValue = member.AvatarHair ?? "" },
                            avatarHat = new { stringValue = member.AvatarHat ?? "" },
                            avatarAccessory = new { stringValue = member.AvatarAccessory ?? "" },
                            avatarClothing = new { stringValue = member.AvatarClothing ?? "" }
                        }
                    }
                });
            }

            return result.ToArray();
        }

        private object[] ConvertStringsToFirestoreArray(List<string> strings)
        {
            var result = new List<object>();

            foreach (var str in strings)
            {
                result.Add(new { stringValue = str });
            }

            return result.ToArray();
        }

        private object[] ConvertMemberIdsToFirestoreArray(List<UserInfo> members)
        {
            return members
                .Where(member => !string.IsNullOrWhiteSpace(member.UserId))
                .Select(member => member.UserId)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(userId => new { stringValue = userId })
                .Cast<object>()
                .ToArray();
        }

        private object[] ConvertMilestonesToFirestoreArray(List<TeamMilestoneInfo> milestones)
        {
            return milestones.Select(milestone => new
            {
                mapValue = new
                {
                    fields = new
                    {
                        id = new { stringValue = milestone.Id ?? Guid.NewGuid().ToString() },
                        title = new { stringValue = milestone.Title ?? "" },
                        notes = new { stringValue = milestone.Notes ?? "" },
                        status = new { stringValue = milestone.Status ?? "Planejada" },
                        dueDate = CreateTimestampOrNullValue(milestone.DueDate),
                        createdAt = new { timestampValue = ToFirestoreTimestamp(milestone.CreatedAt == default ? DateTime.UtcNow : milestone.CreatedAt) }
                    }
                }
            }).Cast<object>().ToArray();
        }

        private object[] ConvertAssetsToFirestoreArray(List<TeamAssetInfo> assets)
        {
            return assets.Select(asset => new
            {
                mapValue = new
                {
                    fields = new
                    {
                        assetId = new { stringValue = asset.AssetId ?? Guid.NewGuid().ToString() },
                        category = new { stringValue = asset.Category ?? "" },
                        fileName = new { stringValue = asset.FileName ?? "" },
                        previewImageDataUri = new { stringValue = asset.PreviewImageDataUri ?? "" },
                        addedByUserId = new { stringValue = asset.AddedByUserId ?? "" },
                        addedAt = new { timestampValue = ToFirestoreTimestamp(asset.AddedAt == default ? DateTime.UtcNow : asset.AddedAt) }
                    }
                }
            }).Cast<object>().ToArray();
        }

        private object[] ConvertTaskColumnsToFirestoreArray(List<TeamTaskColumnInfo> columns)
        {
            return columns.Select(column => new
            {
                mapValue = new
                {
                    fields = new
                    {
                        id = new { stringValue = column.Id ?? Guid.NewGuid().ToString() },
                        title = new { stringValue = column.Title ?? "" },
                        accentColor = new { stringValue = ColorToHex(column.AccentColor) },
                        cards = new { arrayValue = new { values = ConvertTaskCardsToFirestoreArray(column.Cards) } }
                    }
                }
            }).Cast<object>().ToArray();
        }

        public Task<TeamWorkspaceInfo?> GetTeamByIdAsync(string teamId)
        {
            return LoadTeamByIdAsync(teamId);
        }

        private object[] ConvertTaskCardsToFirestoreArray(List<TeamTaskCardInfo> cards)
        {
            return cards.Select(card => new
            {
                mapValue = new
                {
                    fields = new
                    {
                        id = new { stringValue = card.Id ?? Guid.NewGuid().ToString() },
                        title = new { stringValue = card.Title ?? "" },
                        description = new { stringValue = card.Description ?? "" },
                        priority = new { stringValue = card.Priority ?? "Media" },
                        dueDate = CreateTimestampOrNullValue(card.DueDate),
                        assignedUserIds = new { arrayValue = new { values = ConvertStringsToFirestoreArray(card.AssignedUserIds) } },
                        createdAt = new { timestampValue = ToFirestoreTimestamp(card.CreatedAt == default ? DateTime.UtcNow : card.CreatedAt) }
                    }
                }
            }).Cast<object>().ToArray();
        }

        private object[] ConvertNotificationsToFirestoreArray(List<TeamNotificationInfo> notifications)
        {
            return notifications.Select(notification => new
            {
                mapValue = new
                {
                    fields = new
                    {
                        message = new { stringValue = notification.Message ?? "" },
                        createdAt = new { timestampValue = ToFirestoreTimestamp(notification.CreatedAt == default ? DateTime.UtcNow : notification.CreatedAt) }
                    }
                }
            }).Cast<object>().ToArray();
        }

        private object[] ConvertChatMessagesToFirestoreArray(List<TeamChatMessageInfo> messages)
        {
            return messages.Select(message => new
            {
                mapValue = new
                {
                    fields = new
                    {
                        id = new { stringValue = message.Id ?? Guid.NewGuid().ToString() },
                        senderId = new { stringValue = message.SenderId ?? "" },
                        senderName = new { stringValue = message.SenderName ?? "" },
                        content = new { stringValue = message.Content ?? "" },
                        sentAt = new { timestampValue = ToFirestoreTimestamp(message.SentAt == default ? DateTime.UtcNow : message.SentAt) }
                    }
                }
            }).Cast<object>().ToArray();
        }

        private object ConvertCsdBoardToFirestoreMap(TeamCsdBoardInfo board)
        {
            board ??= new TeamCsdBoardInfo();
            return new
            {
                mapValue = new
                {
                    fields = new
                    {
                        certainties = new { arrayValue = new { values = ConvertStringsToFirestoreArray(board.Certainties) } },
                        assumptions = new { arrayValue = new { values = ConvertStringsToFirestoreArray(board.Assumptions) } },
                        doubts = new { arrayValue = new { values = ConvertStringsToFirestoreArray(board.Doubts) } }
                    }
                }
            };
        }

        private async Task<List<TeamWorkspaceInfo>> LoadTeamsByMembershipFallbackAsync()
        {
            var teams = new List<TeamWorkspaceInfo>();

            try
            {
                var url = $"https://firestore.googleapis.com/v1/projects/{FirebaseProjectId}/databases/(default)/documents/teams";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);

                var response = await httpClient.SendAsync(request);
                var jsonContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    DebugHelper.WriteLine($"[TeamService.LoadTeamsFallback] Erro HTTP: {response.StatusCode}");
                    return teams;
                }

                using (JsonDocument doc = JsonDocument.Parse(jsonContent))
                {
                    var root = doc.RootElement;
                    if (!root.TryGetProperty("documents", out var documentsArray))
                    {
                        return teams;
                    }

                    foreach (var teamDoc in documentsArray.EnumerateArray())
                    {
                        if (!teamDoc.TryGetProperty("fields", out var fields))
                        {
                            continue;
                        }

                        var team = ParseTeamFromFirestore(fields);
                        if (team == null)
                        {
                            continue;
                        }

                        var isMember = team.Members.Any(member =>
                            string.Equals(member.UserId, _currentUserId, StringComparison.OrdinalIgnoreCase));

                        if (isMember)
                        {
                            teams.Add(team);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeamService.LoadTeamsFallback] Erro: {ex.Message}");
            }

            return teams;
        }

        private TeamWorkspaceInfo? ParseTeamFromFirestore(JsonElement fields)
        {
            try
            {
                var team = new TeamWorkspaceInfo
                {
                    TeamId = GetString(fields, "teamId"),
                    TeamName = GetString(fields, "teamName"),
                    Course = GetString(fields, "course"),
                    ClassName = GetString(fields, "className"),
                    ClassId = GetString(fields, "classId"),
                    CreatedBy = GetString(fields, "createdBy"),
                    CreatedAt = GetTimestamp(fields, "createdAt"),
                    UpdatedAt = GetTimestamp(fields, "updatedAt"),
                    ProjectProgress = GetInt(fields, "projectProgress"),
                    ProjectDeadline = GetNullableTimestamp(fields, "projectDeadline"),
                    ProjectStatus = GetString(fields, "projectStatus"),
                    Milestones = ParseMilestonesFromFirestore(fields),
                    Members = ParseMembersFromFirestore(fields),
                    Ucs = ParseStringsFromFirestore(fields, "ucs"),
                    Assets = ParseAssetsFromFirestore(fields),
                    TaskColumns = ParseTaskColumnsFromFirestore(fields),
                    Notifications = ParseNotificationsFromFirestore(fields),
                    ChatMessages = ParseChatMessagesFromFirestore(fields),
                    CsdBoard = ParseCsdBoardFromFirestore(fields)
                };

                return team;
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeamService.ParseTeam] Erro: {ex.Message}");
                return null;
            }
        }

        private List<UserInfo> ParseMembersFromFirestore(JsonElement fields)
        {
            var members = new List<UserInfo>();

            try
            {
                if (fields.TryGetProperty("members", out var membersField) &&
                    membersField.TryGetProperty("arrayValue", out var arrayValue) &&
                    arrayValue.TryGetProperty("values", out var values))
                {
                    foreach (var value in values.EnumerateArray())
                    {
                        if (value.TryGetProperty("mapValue", out var mapValue) &&
                            mapValue.TryGetProperty("fields", out var memberFields))
                        {
                            var member = new UserInfo
                            {
                                UserId = GetString(memberFields, "userId"),
                                Name = GetString(memberFields, "name"),
                                Email = GetString(memberFields, "email"),
                                Registration = GetString(memberFields, "registration"),
                                Course = GetString(memberFields, "course"),
                                Role = GetString(memberFields, "role"),
                                AvatarBody = GetString(memberFields, "avatarBody"),
                                AvatarHair = GetString(memberFields, "avatarHair"),
                                AvatarHat = GetString(memberFields, "avatarHat"),
                                AvatarAccessory = GetString(memberFields, "avatarAccessory"),
                                AvatarClothing = GetString(memberFields, "avatarClothing")
                            };

                            if (!string.IsNullOrEmpty(member.UserId))
                            {
                                members.Add(member);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeamService.ParseMembers] Erro: {ex.Message}");
            }

            return members;
        }

        private List<string> ParseStringsFromFirestore(JsonElement fields, string fieldName)
        {
            var strings = new List<string>();

            try
            {
                if (fields.TryGetProperty(fieldName, out var field) &&
                    field.TryGetProperty("arrayValue", out var arrayValue) &&
                    arrayValue.TryGetProperty("values", out var values))
                {
                    foreach (var value in values.EnumerateArray())
                    {
                        if (value.TryGetProperty("stringValue", out var stringValue))
                        {
                            var str = stringValue.GetString();
                            if (!string.IsNullOrEmpty(str))
                            {
                                strings.Add(str);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeamService.ParseStrings] Erro: {ex.Message}");
            }

            return strings;
        }

        private List<TeamMilestoneInfo> ParseMilestonesFromFirestore(JsonElement fields)
        {
            var milestones = new List<TeamMilestoneInfo>();

            try
            {
                foreach (var value in EnumerateArrayField(fields, "milestones"))
                {
                    if (!TryGetMapFields(value, out var milestoneFields))
                    {
                        continue;
                    }

                    milestones.Add(new TeamMilestoneInfo
                    {
                        Id = GetString(milestoneFields, "id"),
                        Title = GetString(milestoneFields, "title"),
                        Notes = GetString(milestoneFields, "notes"),
                        Status = GetString(milestoneFields, "status"),
                        DueDate = GetNullableTimestamp(milestoneFields, "dueDate"),
                        CreatedAt = GetTimestamp(milestoneFields, "createdAt")
                    });
                }
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeamService.ParseMilestones] Erro: {ex.Message}");
            }

            return milestones;
        }

        private List<TeamAssetInfo> ParseAssetsFromFirestore(JsonElement fields)
        {
            var assets = new List<TeamAssetInfo>();

            try
            {
                foreach (var value in EnumerateArrayField(fields, "assets"))
                {
                    if (!TryGetMapFields(value, out var assetFields))
                    {
                        continue;
                    }

                    assets.Add(new TeamAssetInfo
                    {
                        AssetId = GetString(assetFields, "assetId"),
                        Category = GetString(assetFields, "category"),
                        FileName = GetString(assetFields, "fileName"),
                        PreviewImageDataUri = GetString(assetFields, "previewImageDataUri"),
                        AddedByUserId = GetString(assetFields, "addedByUserId"),
                        AddedAt = GetTimestamp(assetFields, "addedAt")
                    });
                }
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeamService.ParseAssets] Erro: {ex.Message}");
            }

            return assets;
        }

        private List<TeamTaskColumnInfo> ParseTaskColumnsFromFirestore(JsonElement fields)
        {
            var columns = new List<TeamTaskColumnInfo>();

            try
            {
                foreach (var value in EnumerateArrayField(fields, "taskColumns"))
                {
                    if (!TryGetMapFields(value, out var columnFields))
                    {
                        continue;
                    }

                    columns.Add(new TeamTaskColumnInfo
                    {
                        Id = GetString(columnFields, "id"),
                        Title = GetString(columnFields, "title"),
                        AccentColor = ParseColor(GetString(columnFields, "accentColor")),
                        Cards = ParseTaskCardsFromFirestore(columnFields)
                    });
                }
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeamService.ParseTaskColumns] Erro: {ex.Message}");
            }

            return columns;
        }

        private List<TeamTaskCardInfo> ParseTaskCardsFromFirestore(JsonElement columnFields)
        {
            var cards = new List<TeamTaskCardInfo>();

            foreach (var value in EnumerateArrayField(columnFields, "cards"))
            {
                if (!TryGetMapFields(value, out var cardFields))
                {
                    continue;
                }

                cards.Add(new TeamTaskCardInfo
                {
                    Id = GetString(cardFields, "id"),
                    Title = GetString(cardFields, "title"),
                    Description = GetString(cardFields, "description"),
                    Priority = GetString(cardFields, "priority"),
                    DueDate = GetNullableTimestamp(cardFields, "dueDate"),
                    AssignedUserIds = ParseStringsFromFirestore(cardFields, "assignedUserIds"),
                    CreatedAt = GetTimestamp(cardFields, "createdAt")
                });
            }

            return cards;
        }

        private List<TeamNotificationInfo> ParseNotificationsFromFirestore(JsonElement fields)
        {
            var notifications = new List<TeamNotificationInfo>();

            try
            {
                foreach (var value in EnumerateArrayField(fields, "notifications"))
                {
                    if (!TryGetMapFields(value, out var notificationFields))
                    {
                        continue;
                    }

                    notifications.Add(new TeamNotificationInfo
                    {
                        Message = GetString(notificationFields, "message"),
                        CreatedAt = GetTimestamp(notificationFields, "createdAt")
                    });
                }
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeamService.ParseNotifications] Erro: {ex.Message}");
            }

            return notifications;
        }

        private List<TeamChatMessageInfo> ParseChatMessagesFromFirestore(JsonElement fields)
        {
            var messages = new List<TeamChatMessageInfo>();

            try
            {
                foreach (var value in EnumerateArrayField(fields, "chatMessages"))
                {
                    if (!TryGetMapFields(value, out var messageFields))
                    {
                        continue;
                    }

                    messages.Add(new TeamChatMessageInfo
                    {
                        Id = GetString(messageFields, "id"),
                        SenderId = GetString(messageFields, "senderId"),
                        SenderName = GetString(messageFields, "senderName"),
                        Content = GetString(messageFields, "content"),
                        SentAt = GetTimestamp(messageFields, "sentAt")
                    });
                }
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeamService.ParseChatMessages] Erro: {ex.Message}");
            }

            return messages;
        }

        private TeamCsdBoardInfo ParseCsdBoardFromFirestore(JsonElement fields)
        {
            try
            {
                if (fields.TryGetProperty("csdBoard", out var csdBoardField) &&
                    csdBoardField.TryGetProperty("mapValue", out var mapValue) &&
                    mapValue.TryGetProperty("fields", out var boardFields))
                {
                    return new TeamCsdBoardInfo
                    {
                        Certainties = ParseStringsFromFirestore(boardFields, "certainties"),
                        Assumptions = ParseStringsFromFirestore(boardFields, "assumptions"),
                        Doubts = ParseStringsFromFirestore(boardFields, "doubts")
                    };
                }
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeamService.ParseCsdBoard] Erro: {ex.Message}");
            }

            return new TeamCsdBoardInfo();
        }

        private List<TeamReferenceDocument> ParseTeamReferenceDocuments(string jsonContent, string teamId)
        {
            var references = new List<TeamReferenceDocument>();

            using (JsonDocument doc = JsonDocument.Parse(jsonContent))
            {
                var root = doc.RootElement;
                if (!root.TryGetProperty("documents", out var documentsArray))
                {
                    return references;
                }

                foreach (var userTeamDoc in documentsArray.EnumerateArray())
                {
                    if (!userTeamDoc.TryGetProperty("fields", out var fields))
                    {
                        continue;
                    }

                    var currentTeamId = GetString(fields, "teamId");
                    var userId = GetString(fields, "userId");
                    if (!string.Equals(currentTeamId, teamId, StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(userId))
                    {
                        continue;
                    }

                    references.Add(new TeamReferenceDocument
                    {
                        DocumentId = GetDocumentId(userTeamDoc),
                        UserId = userId
                    });
                }
            }

            return references;
        }

        private async Task<List<TeamReferenceDocument>> GetTeamReferenceDocumentsAsync(string teamId)
        {
            var url = $"https://firestore.googleapis.com/v1/projects/{FirebaseProjectId}/databases/(default)/documents/userTeams";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);

            var response = await httpClient.SendAsync(request);
            var jsonContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                return new List<TeamReferenceDocument>();
            }

            return ParseTeamReferenceDocuments(jsonContent, teamId);
        }

        private async Task<TeamOperationResult> DeleteTeamReferenceDocumentAsync(string documentId)
        {
            if (string.IsNullOrWhiteSpace(documentId))
            {
                return TeamOperationResult.Ok();
            }

            var url = $"https://firestore.googleapis.com/v1/projects/{FirebaseProjectId}/databases/(default)/documents/userTeams/{documentId}";
            var request = new HttpRequestMessage(HttpMethod.Delete, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);

            var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return TeamOperationResult.Ok();
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            return TeamOperationResult.Fail($"HTTP {(int)response.StatusCode}: {responseBody.Substring(0, Math.Min(responseBody.Length, 200))}");
        }

        private async Task<TeamOperationResult> DeleteTeamReferencesByTeamIdAsync(string teamId)
        {
            var references = await GetTeamReferenceDocumentsAsync(teamId);
            foreach (var reference in references)
            {
                var deleteResult = await DeleteTeamReferenceDocumentAsync(reference.DocumentId);
                if (!deleteResult.Success)
                {
                    return deleteResult;
                }
            }

            return TeamOperationResult.Ok();
        }

        private string GetString(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var prop) &&
                prop.TryGetProperty("stringValue", out var stringValue))
            {
                return stringValue.GetString() ?? "";
            }

            return "";
        }

        private DateTime GetTimestamp(JsonElement element, string propertyName)
        {
            var value = GetNullableTimestamp(element, propertyName);
            return value ?? DateTime.UtcNow;
        }

        private int GetInt(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var prop))
            {
                if (prop.TryGetProperty("integerValue", out var integerValue) &&
                    int.TryParse(integerValue.GetString(), out var parsedInt))
                {
                    return parsedInt;
                }

                if (prop.TryGetProperty("stringValue", out var stringValue) &&
                    int.TryParse(stringValue.GetString(), out parsedInt))
                {
                    return parsedInt;
                }
            }

            return 0;
        }

        private DateTime? GetNullableTimestamp(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var prop))
            {
                if (prop.TryGetProperty("timestampValue", out var timestampValue) &&
                    DateTime.TryParse(timestampValue.GetString(), out var parsed))
                {
                    return parsed.ToLocalTime();
                }

                if (prop.TryGetProperty("nullValue", out _))
                {
                    return null;
                }
            }

            return null;
        }

        private IEnumerable<JsonElement> EnumerateArrayField(JsonElement fields, string fieldName)
        {
            if (fields.TryGetProperty(fieldName, out var field) &&
                field.TryGetProperty("arrayValue", out var arrayValue) &&
                arrayValue.TryGetProperty("values", out var values))
            {
                foreach (var value in values.EnumerateArray())
                {
                    yield return value;
                }
            }
        }

        private bool TryGetMapFields(JsonElement value, out JsonElement fields)
        {
            fields = default;
            return value.TryGetProperty("mapValue", out var mapValue) &&
                   mapValue.TryGetProperty("fields", out fields);
        }

        private string GetDocumentId(JsonElement document)
        {
            if (document.TryGetProperty("name", out var nameField))
            {
                return nameField.GetString()?.Split('/').LastOrDefault() ?? string.Empty;
            }

            return string.Empty;
        }

        private string ToFirestoreTimestamp(DateTime dateTime)
        {
            return dateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        }

        private object CreateTimestampOrNullValue(DateTime? dateTime)
        {
            return dateTime.HasValue
                ? new { timestampValue = ToFirestoreTimestamp(dateTime.Value) }
                : new { nullValue = (string?)null };
        }

        private string ColorToHex(System.Windows.Media.Color color)
        {
            return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        private System.Windows.Media.Color ParseColor(string value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return System.Windows.Media.Color.FromRgb(37, 99, 235);
                }

                return (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(value);
            }
            catch
            {
                return System.Windows.Media.Color.FromRgb(37, 99, 235);
            }
        }
    }

    internal sealed class TeamReferenceDocument
    {
        public string DocumentId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Resultado de operações com equipes
    /// </summary>
    public class TeamOperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string? ErrorMessage { get; set; }

        public static TeamOperationResult Ok()
        {
            return new TeamOperationResult { Success = true };
        }

        public static TeamOperationResult Fail(string errorMessage)
        {
            return new TeamOperationResult { Success = false, ErrorMessage = errorMessage };
        }
    }

    public class TeamJoinResult
    {
        public bool Success { get; set; }
        public TeamWorkspaceInfo? Team { get; set; }
        public string? ErrorMessage { get; set; }

        public static TeamJoinResult Ok(TeamWorkspaceInfo team)
        {
            return new TeamJoinResult { Success = true, Team = team };
        }

        public static TeamJoinResult Fail(string errorMessage)
        {
            return new TeamJoinResult { Success = false, ErrorMessage = errorMessage };
        }
    }
}
