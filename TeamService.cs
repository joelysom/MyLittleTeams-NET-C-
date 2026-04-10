using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;

namespace MeuApp
{
    /// <summary>
    /// Serviço para gerenciar equipes de projetos no Firebase Firestore
    /// </summary>
    public class TeamService
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private readonly string _idToken;
        private readonly string _currentUserId;
        private readonly string _currentUserRole;

        public TeamService(string idToken, string currentUserId, string currentUserRole = "student")
        {
            _idToken = idToken;
            _currentUserId = currentUserId;
            _currentUserRole = TeamPermissionService.NormalizeRole(currentUserRole);
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
                team.LastRealtimeSyncAt = updatedAt;
                NormalizeTeamWorkItems(team);
                var createdAtValue = ToFirestoreTimestamp(createdAt);
                var updatedAtValue = ToFirestoreTimestamp(updatedAt);
                var accessRules = TeamPermissionService.NormalizeAccessRules(team.AccessRules);
                team.AccessRules = accessRules;
                var memberRoleMap = BuildMemberRoleMap(team.Members);

                var teamData = new
                {
                    fields = new
                    {
                        teamId = new { stringValue = teamId },
                        teamName = new { stringValue = team.TeamName },
                        course = new { stringValue = team.Course ?? "" },
                        className = new { stringValue = team.ClassName ?? "" },
                        classId = new { stringValue = team.ClassId },
                        academicTerm = new { stringValue = team.AcademicTerm ?? "" },
                        templateId = new { stringValue = team.TemplateId ?? "" },
                        templateName = new { stringValue = team.TemplateName ?? "" },
                        createdBy = new { stringValue = team.CreatedBy },
                        createdAt = new { timestampValue = createdAtValue },
                        updatedAt = new { timestampValue = updatedAtValue },
                        lastRealtimeSyncAt = CreateTimestampOrNullValue(team.LastRealtimeSyncAt),
                        projectProgress = new { integerValue = team.ProjectProgress.ToString() },
                        projectDeadline = CreateTimestampOrNullValue(team.ProjectDeadline),
                        projectStatus = new { stringValue = team.ProjectStatus ?? "Planejamento" },
                        teacherNotes = new { stringValue = team.TeacherNotes ?? "" },
                        focalProfessorUserId = new { stringValue = team.FocalProfessorUserId ?? "" },
                        focalProfessorName = new { stringValue = team.FocalProfessorName ?? "" },
                        professorSupervisorUserIds = new { arrayValue = new { values = ConvertStringsToFirestoreArray(team.ProfessorSupervisorUserIds) } },
                        professorSupervisorNames = new { arrayValue = new { values = ConvertStringsToFirestoreArray(team.ProfessorSupervisorNames) } },
                        defaultFilePermissionScope = new { stringValue = team.DefaultFilePermissionScope ?? "team" },
                        milestones = new { arrayValue = new { values = ConvertMilestonesToFirestoreArray(team.Milestones ?? new List<TeamMilestoneInfo>()) } },
                        members = new { arrayValue = new { values = ConvertMembersToFirestoreArray(team.Members) } },
                        memberIds = new { arrayValue = new { values = ConvertMemberIdsToFirestoreArray(team.Members) } },
                        memberRolesByUserId = ConvertStringMapToFirestoreMap(memberRoleMap),
                        leaderIds = new { arrayValue = new { values = ConvertRoleMemberIdsToFirestoreArray(team.Members, "leader") } },
                        professorIds = new { arrayValue = new { values = ConvertRoleMemberIdsToFirestoreArray(team.Members, "professor") } },
                        coordinatorIds = new { arrayValue = new { values = ConvertRoleMemberIdsToFirestoreArray(team.Members, "coordinator") } },
                        studentIds = new { arrayValue = new { values = ConvertRoleMemberIdsToFirestoreArray(team.Members, "student") } },
                        ucs = new { arrayValue = new { values = ConvertStringsToFirestoreArray(team.Ucs) } },
                        semesterTimeline = new { arrayValue = new { values = ConvertTimelineToFirestoreArray(team.SemesterTimeline) } },
                        accessRules = new { arrayValue = new { values = ConvertAccessRulesToFirestoreArray(accessRules) } },
                        assets = new { arrayValue = new { values = ConvertAssetsToFirestoreArray(team.Assets) } },
                        taskColumns = new { arrayValue = new { values = ConvertTaskColumnsToFirestoreArray(team.TaskColumns ?? new List<TeamTaskColumnInfo>()) } },
                        notifications = new { arrayValue = new { values = ConvertNotificationsToFirestoreArray(team.Notifications) } },
                        chatMessages = new { arrayValue = new { values = ConvertChatMessagesToFirestoreArray(team.ChatMessages) } },
                        csdBoard = ConvertCsdBoardToFirestoreMap(team.CsdBoard),
                        isActive = new { booleanValue = true }
                    }
                };

                // URL para salvar no Firestore - usando POST com documento específico
                var url = AppConfig.BuildFirestoreDocumentUrl($"teams/{teamId}");

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

                var workItemsResult = await SyncTeamWorkItemsAsync(teamId, team);
                if (!workItemsResult.Success)
                {
                    DebugHelper.WriteLine($"[TeamService.SaveTeam] ❌ ERRO AO SALVAR SUBCOLEÇÕES: {workItemsResult.ErrorMessage}");
                    return workItemsResult;
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

                var url = AppConfig.BuildFirestoreDocumentUrl($"userTeams/{docId}");
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

        public async Task<List<TeamWorkspaceInfo>> LoadTeamsForUserAsync(string userId, int? maxTeams = null)
        {
            var teams = new List<TeamWorkspaceInfo>();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return teams;
            }

            try
            {
                var teamIds = await GetUserTeamIdsAsync(userId);
                var normalizedIds = teamIds
                    .Where(teamId => !string.IsNullOrWhiteSpace(teamId))
                    .Distinct(StringComparer.OrdinalIgnoreCase);

                if (maxTeams.HasValue && maxTeams.Value > 0)
                {
                    normalizedIds = normalizedIds.Take(maxTeams.Value);
                }

                var loadTasks = normalizedIds
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
                    }
                }

                return teams
                    .GroupBy(team => team.TeamId, StringComparer.OrdinalIgnoreCase)
                    .Select(group => group.First())
                    .OrderByDescending(team => team.UpdatedAt)
                    .ThenBy(team => team.TeamName)
                    .ToList();
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeamService.LoadTeamsForUser] Erro para '{userId}': {ex.Message}");
                return teams;
            }
        }

        public async Task<List<TeamWorkspaceInfo>> SearchTeamsAsync(string query, int maxResults = 12)
        {
            var results = new List<TeamWorkspaceInfo>();
            var normalizedQuery = (query ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedQuery))
            {
                return results;
            }

            try
            {
                DebugHelper.WriteLine($"[TeamService.SearchTeams] Buscando equipes com query '{normalizedQuery}'...");

                var request = new HttpRequestMessage(HttpMethod.Get, AppConfig.BuildFirestoreDocumentUrl("teams"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);

                var response = await httpClient.SendAsync(request);
                var jsonContent = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    DebugHelper.WriteLine($"[TeamService.SearchTeams] Erro HTTP: {response.StatusCode}");
                    return results;
                }

                using var doc = JsonDocument.Parse(jsonContent);
                if (!doc.RootElement.TryGetProperty("documents", out var documentsArray))
                {
                    return results;
                }

                foreach (var teamDoc in documentsArray.EnumerateArray())
                {
                    if (!teamDoc.TryGetProperty("fields", out var fields))
                    {
                        continue;
                    }

                    var team = ParseTeamFromFirestore(fields);
                    if (team == null || !TeamMatchesQuery(team, normalizedQuery))
                    {
                        continue;
                    }

                    results.Add(team);
                }

                return results
                    .GroupBy(team => team.TeamId, StringComparer.OrdinalIgnoreCase)
                    .Select(group => group.First())
                    .OrderByDescending(team => team.UpdatedAt)
                    .ThenBy(team => team.TeamName)
                    .Take(Math.Max(1, maxResults))
                    .ToList();
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeamService.SearchTeams] Erro: {ex.Message}");
                return results;
            }
        }

        public async Task<List<TeamWorkspaceInfo>> LoadAllVisibleTeamsAsync(int maxResults = 80)
        {
            var results = new List<TeamWorkspaceInfo>();

            try
            {
                DebugHelper.WriteLine($"[TeamService.LoadAllVisibleTeams] Carregando até {maxResults} equipes visíveis...");

                var request = new HttpRequestMessage(HttpMethod.Get, AppConfig.BuildFirestoreDocumentUrl("teams"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);

                var response = await httpClient.SendAsync(request);
                var jsonContent = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    DebugHelper.WriteLine($"[TeamService.LoadAllVisibleTeams] Erro HTTP: {response.StatusCode}");
                    return results;
                }

                using var doc = JsonDocument.Parse(jsonContent);
                if (!doc.RootElement.TryGetProperty("documents", out var documentsArray))
                {
                    return results;
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

                    results.Add(team);
                }

                var orderedTeams = results
                    .GroupBy(team => team.TeamId, StringComparer.OrdinalIgnoreCase)
                    .Select(group => group.OrderByDescending(team => team.UpdatedAt).First())
                    .OrderByDescending(team => team.UpdatedAt)
                    .ThenBy(team => team.TeamName)
                    .Take(Math.Max(1, maxResults))
                    .ToList();

                foreach (var team in orderedTeams)
                {
                    await PopulateTeamWorkItemsFromSubcollectionsAsync(team);
                }

                return orderedTeams;
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeamService.LoadAllVisibleTeams] Erro: {ex.Message}");
                return results;
            }
        }

        public async Task<TeamOperationResult> UpdateProfessorFocusAsync(TeamWorkspaceInfo team)
        {
            try
            {
                DebugHelper.WriteLine($"\n[TeamService.UpdateProfessorFocus] ===== INICIANDO =====");

                if (team == null || string.IsNullOrWhiteSpace(team.ClassId) || string.IsNullOrWhiteSpace(team.TeamName))
                {
                    return TeamOperationResult.Fail("Equipe inválida para atualizar supervisão focal.");
                }

                var teamId = string.IsNullOrWhiteSpace(team.TeamId)
                    ? GenerateTeamId(team.ClassId, team.TeamName)
                    : NormalizeTeamCode(team.TeamId);
                team.TeamId = teamId;
                team.CreatedBy = string.IsNullOrWhiteSpace(team.CreatedBy) ? _currentUserId : team.CreatedBy;

                var updatedAt = DateTime.UtcNow;
                team.UpdatedAt = updatedAt;
                team.LastRealtimeSyncAt = updatedAt;
                NormalizeTeamWorkItems(team);

                var memberRoleMap = BuildMemberRoleMap(team.Members);
                var requestBody = JsonSerializer.Serialize(new
                {
                    fields = new
                    {
                        updatedAt = new { timestampValue = ToFirestoreTimestamp(updatedAt) },
                        lastRealtimeSyncAt = CreateTimestampOrNullValue(team.LastRealtimeSyncAt),
                        focalProfessorUserId = new { stringValue = team.FocalProfessorUserId ?? string.Empty },
                        focalProfessorName = new { stringValue = team.FocalProfessorName ?? string.Empty },
                        professorSupervisorUserIds = new { arrayValue = new { values = ConvertStringsToFirestoreArray(team.ProfessorSupervisorUserIds) } },
                        professorSupervisorNames = new { arrayValue = new { values = ConvertStringsToFirestoreArray(team.ProfessorSupervisorNames) } },
                        members = new { arrayValue = new { values = ConvertMembersToFirestoreArray(team.Members) } },
                        memberIds = new { arrayValue = new { values = ConvertMemberIdsToFirestoreArray(team.Members) } },
                        memberRolesByUserId = ConvertStringMapToFirestoreMap(memberRoleMap),
                        leaderIds = new { arrayValue = new { values = ConvertRoleMemberIdsToFirestoreArray(team.Members, "leader") } },
                        professorIds = new { arrayValue = new { values = ConvertRoleMemberIdsToFirestoreArray(team.Members, "professor") } },
                        coordinatorIds = new { arrayValue = new { values = ConvertRoleMemberIdsToFirestoreArray(team.Members, "coordinator") } },
                        studentIds = new { arrayValue = new { values = ConvertRoleMemberIdsToFirestoreArray(team.Members, "student") } },
                        notifications = new { arrayValue = new { values = ConvertNotificationsToFirestoreArray(team.Notifications) } }
                    }
                });

                var request = new HttpRequestMessage(new HttpMethod("PATCH"), AppConfig.BuildFirestoreDocumentUrl($"teams/{teamId}"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                var response = await httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                DebugHelper.WriteLine($"[TeamService.UpdateProfessorFocus] Status Code: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = $"HTTP {(int)response.StatusCode}: {responseBody.Substring(0, Math.Min(responseBody.Length, 200))}";
                    DebugHelper.WriteLine($"[TeamService.UpdateProfessorFocus] ❌ ERRO AO SALVAR DOC PRINCIPAL: {errorMsg}");
                    return TeamOperationResult.Fail(errorMsg);
                }

                var referenceResult = await SaveTeamReferenceForUserAsync(_currentUserId, teamId, team.TeamName);
                if (!referenceResult.Success)
                {
                    DebugHelper.WriteLine($"[TeamService.UpdateProfessorFocus] ⚠️ Supervisão focal salva, mas a referência rápida falhou: {referenceResult.ErrorMessage}");
                    return new TeamOperationResult
                    {
                        Success = true,
                        Message = referenceResult.ErrorMessage ?? string.Empty
                    };
                }

                DebugHelper.WriteLine($"[TeamService.UpdateProfessorFocus] ✅ Supervisão focal atualizada para '{team.TeamName}'.");
                return TeamOperationResult.Ok();
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeamService.UpdateProfessorFocus] ❌ EXCEÇÃO: {ex.Message}");
                return TeamOperationResult.Fail(ex.Message);
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

                var url = AppConfig.BuildFirestoreRunQueryUrl();
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

                var url = AppConfig.BuildFirestoreDocumentUrl($"teams/{teamId}");

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
                        if (team != null)
                        {
                            await PopulateTeamWorkItemsFromSubcollectionsAsync(team);
                        }
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

                var workItemCleanupResult = await DeleteTeamWorkItemDocumentsAsync(teamId);
                if (!workItemCleanupResult.Success)
                {
                    return workItemCleanupResult;
                }

                var assetCleanupResult = await DeleteAssetContentDocumentsAsync(team);
                if (!assetCleanupResult.Success)
                {
                    return assetCleanupResult;
                }

                var cleanupResult = await DeleteTeamReferencesByTeamIdAsync(teamId);
                if (!cleanupResult.Success)
                {
                    return cleanupResult;
                }

                var url = AppConfig.BuildFirestoreDocumentUrl($"teams/{teamId}");

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
                            phone = new { stringValue = member.Phone ?? "" },
                            registration = new { stringValue = member.Registration ?? "" },
                            course = new { stringValue = member.Course ?? "" },
                            role = new { stringValue = TeamPermissionService.NormalizeRole(member.Role) },
                            nickname = new { stringValue = member.Nickname ?? "" },
                            professionalTitle = new { stringValue = member.ProfessionalTitle ?? "" },
                            academicDepartment = new { stringValue = member.AcademicDepartment ?? "" },
                            academicFocus = new { stringValue = member.AcademicFocus ?? "" },
                            officeHours = new { stringValue = member.OfficeHours ?? "" },
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

        private Dictionary<string, string> BuildMemberRoleMap(List<UserInfo> members)
        {
            return (members ?? new List<UserInfo>())
                .Where(member => !string.IsNullOrWhiteSpace(member.UserId))
                .GroupBy(member => member.UserId, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => TeamPermissionService.NormalizeRole(group.First().Role),
                    StringComparer.OrdinalIgnoreCase);
        }

        private void NormalizeTeamWorkItems(TeamWorkspaceInfo team)
        {
            var fallbackOwnerId = string.IsNullOrWhiteSpace(team.CreatedBy) ? _currentUserId : team.CreatedBy;

            team.FocalProfessorUserId = team.FocalProfessorUserId?.Trim() ?? string.Empty;
            team.FocalProfessorName = team.FocalProfessorName?.Trim() ?? string.Empty;
            team.ProfessorSupervisorUserIds = (team.ProfessorSupervisorUserIds ?? new List<string>())
                .Where(userId => !string.IsNullOrWhiteSpace(userId))
                .Select(userId => userId.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            team.ProfessorSupervisorNames = (team.ProfessorSupervisorNames ?? new List<string>())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name)
                .ToList();

            if (!string.IsNullOrWhiteSpace(team.FocalProfessorUserId) &&
                !team.ProfessorSupervisorUserIds.Contains(team.FocalProfessorUserId, StringComparer.OrdinalIgnoreCase))
            {
                team.ProfessorSupervisorUserIds.Add(team.FocalProfessorUserId);
            }

            if (!string.IsNullOrWhiteSpace(team.FocalProfessorName) &&
                !team.ProfessorSupervisorNames.Contains(team.FocalProfessorName, StringComparer.OrdinalIgnoreCase))
            {
                team.ProfessorSupervisorNames.Add(team.FocalProfessorName);
            }

            foreach (var milestone in team.Milestones ?? new List<TeamMilestoneInfo>())
            {
                milestone.Id = string.IsNullOrWhiteSpace(milestone.Id) ? Guid.NewGuid().ToString("N") : milestone.Id;
                milestone.CreatedByUserId = string.IsNullOrWhiteSpace(milestone.CreatedByUserId) ? fallbackOwnerId : milestone.CreatedByUserId;
                milestone.OwnerUserId = string.IsNullOrWhiteSpace(milestone.OwnerUserId) ? milestone.CreatedByUserId : milestone.OwnerUserId;
                milestone.CreatedAt = milestone.CreatedAt == default ? DateTime.UtcNow : milestone.CreatedAt;
                milestone.UpdatedAt = milestone.UpdatedAt == default ? DateTime.UtcNow : milestone.UpdatedAt;
                milestone.UpdatedByUserId = string.IsNullOrWhiteSpace(milestone.UpdatedByUserId) ? _currentUserId : milestone.UpdatedByUserId;
                milestone.MentionedUserIds ??= new List<string>();
                milestone.Comments ??= new List<TeamCommentInfo>();
                milestone.Attachments ??= new List<TeamAttachmentInfo>();
            }

            foreach (var column in team.TaskColumns ?? new List<TeamTaskColumnInfo>())
            {
                column.Id = string.IsNullOrWhiteSpace(column.Id) ? Guid.NewGuid().ToString("N") : column.Id;
                foreach (var card in column.Cards ?? new List<TeamTaskCardInfo>())
                {
                    card.Id = string.IsNullOrWhiteSpace(card.Id) ? Guid.NewGuid().ToString("N") : card.Id;
                    card.ColumnId = string.IsNullOrWhiteSpace(card.ColumnId) ? column.Id : card.ColumnId;
                    card.RequiredRole = TeamPermissionService.NormalizeExecutionRole(card.RequiredRole);
                    card.CreatedByUserId = string.IsNullOrWhiteSpace(card.CreatedByUserId) ? fallbackOwnerId : card.CreatedByUserId;
                    card.CreatedAt = card.CreatedAt == default ? DateTime.UtcNow : card.CreatedAt;
                    card.UpdatedAt = card.UpdatedAt == default ? DateTime.UtcNow : card.UpdatedAt;
                    card.UpdatedByUserId = string.IsNullOrWhiteSpace(card.UpdatedByUserId) ? _currentUserId : card.UpdatedByUserId;
                    card.MentionedUserIds ??= new List<string>();
                    card.Comments ??= new List<TeamCommentInfo>();
                    card.Attachments ??= new List<TeamAttachmentInfo>();
                }
            }
        }

        private object ConvertStringMapToFirestoreMap(Dictionary<string, string> values)
        {
            var fields = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in values)
            {
                fields[pair.Key] = new { stringValue = pair.Value ?? string.Empty };
            }

            return new { mapValue = new { fields } };
        }

        private object[] ConvertRoleMemberIdsToFirestoreArray(List<UserInfo> members, string role)
        {
            return (members ?? new List<UserInfo>())
                .Where(member => !string.IsNullOrWhiteSpace(member.UserId))
                .Where(member => string.Equals(TeamPermissionService.NormalizeRole(member.Role), role, StringComparison.OrdinalIgnoreCase))
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
                        createdByUserId = new { stringValue = milestone.CreatedByUserId ?? "" },
                        ownerUserId = new { stringValue = milestone.OwnerUserId ?? "" },
                        requiresProfessorReview = new { booleanValue = milestone.RequiresProfessorReview },
                        mentionedUserIds = new { arrayValue = new { values = ConvertStringsToFirestoreArray(milestone.MentionedUserIds) } },
                        comments = new { arrayValue = new { values = ConvertCommentsToFirestoreArray(milestone.Comments) } },
                        attachments = new { arrayValue = new { values = ConvertAttachmentsToFirestoreArray(milestone.Attachments) } },
                        dueDate = CreateTimestampOrNullValue(milestone.DueDate),
                        createdAt = new { timestampValue = ToFirestoreTimestamp(milestone.CreatedAt == default ? DateTime.UtcNow : milestone.CreatedAt) },
                        updatedAt = new { timestampValue = ToFirestoreTimestamp(milestone.UpdatedAt == default ? DateTime.UtcNow : milestone.UpdatedAt) },
                        updatedByUserId = new { stringValue = milestone.UpdatedByUserId ?? "" }
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
                        description = new { stringValue = asset.Description ?? "" },
                        mimeType = new { stringValue = asset.MimeType ?? "" },
                        folderPath = new { stringValue = asset.FolderPath ?? "" },
                        permissionScope = new { stringValue = asset.PermissionScope ?? "team" },
                        storageKind = new { stringValue = asset.StorageKind ?? "firebase-storage" },
                        storageReference = new { stringValue = asset.StorageReference ?? "" },
                        sizeBytes = new { integerValue = asset.SizeBytes.ToString() },
                        version = new { integerValue = asset.Version.ToString() },
                        addedByUserId = new { stringValue = asset.AddedByUserId ?? "" },
                        addedAt = new { timestampValue = ToFirestoreTimestamp(asset.AddedAt == default ? DateTime.UtcNow : asset.AddedAt) },
                        lastSyncedAt = CreateTimestampOrNullValue(asset.LastSyncedAt),
                        versionHistory = new { arrayValue = new { values = ConvertAssetVersionsToFirestoreArray(asset.VersionHistory) } }
                    }
                }
            }).Cast<object>().ToArray();
        }

        private object[] ConvertTaskColumnsToFirestoreArray(List<TeamTaskColumnInfo> columns, bool includeCards = true)
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
                        cards = new { arrayValue = new { values = includeCards ? ConvertTaskCardsToFirestoreArray(column.Cards) : Array.Empty<object>() } }
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
                        columnId = new { stringValue = card.ColumnId ?? "" },
                        title = new { stringValue = card.Title ?? "" },
                        description = new { stringValue = card.Description ?? "" },
                        priority = new { stringValue = card.Priority ?? "Media" },
                        estimatedHours = new { integerValue = card.EstimatedHours.ToString() },
                        workloadPoints = new { integerValue = card.WorkloadPoints.ToString() },
                        requiredRole = new { stringValue = TeamPermissionService.NormalizeRole(card.RequiredRole) },
                        requiresProfessorReview = new { booleanValue = card.RequiresProfessorReview },
                        dueDate = CreateTimestampOrNullValue(card.DueDate),
                        assignedUserIds = new { arrayValue = new { values = ConvertStringsToFirestoreArray(card.AssignedUserIds) } },
                        mentionedUserIds = new { arrayValue = new { values = ConvertStringsToFirestoreArray(card.MentionedUserIds) } },
                        comments = new { arrayValue = new { values = ConvertCommentsToFirestoreArray(card.Comments) } },
                        attachments = new { arrayValue = new { values = ConvertAttachmentsToFirestoreArray(card.Attachments) } },
                        createdAt = new { timestampValue = ToFirestoreTimestamp(card.CreatedAt == default ? DateTime.UtcNow : card.CreatedAt) },
                        createdByUserId = new { stringValue = card.CreatedByUserId ?? "" },
                        updatedAt = new { timestampValue = ToFirestoreTimestamp(card.UpdatedAt == default ? DateTime.UtcNow : card.UpdatedAt) },
                        updatedByUserId = new { stringValue = card.UpdatedByUserId ?? "" }
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
                        id = new { stringValue = notification.Id ?? Guid.NewGuid().ToString() },
                        message = new { stringValue = notification.Message ?? "" },
                        type = new { stringValue = notification.Type ?? "info" },
                        audience = new { stringValue = notification.Audience ?? "team" },
                        relatedEntityId = new { stringValue = notification.RelatedEntityId ?? "" },
                        createdAt = new { timestampValue = ToFirestoreTimestamp(notification.CreatedAt == default ? DateTime.UtcNow : notification.CreatedAt) }
                    }
                }
            }).Cast<object>().ToArray();
        }

        private object[] ConvertTimelineToFirestoreArray(List<TeamTimelineItemInfo> timeline)
        {
            return (timeline ?? new List<TeamTimelineItemInfo>())
                .Select(item => new
                {
                    mapValue = new
                    {
                        fields = new
                        {
                            id = new { stringValue = item.Id ?? Guid.NewGuid().ToString() },
                            title = new { stringValue = item.Title ?? "" },
                            description = new { stringValue = item.Description ?? "" },
                            category = new { stringValue = item.Category ?? "" },
                            status = new { stringValue = item.Status ?? "Planejado" },
                            ownerUserId = new { stringValue = item.OwnerUserId ?? "" },
                            startsAt = CreateTimestampOrNullValue(item.StartsAt),
                            endsAt = CreateTimestampOrNullValue(item.EndsAt)
                        }
                    }
                })
                .Cast<object>()
                .ToArray();
        }

        private object[] ConvertAccessRulesToFirestoreArray(List<TeamAccessRuleInfo> accessRules)
        {
            return (accessRules ?? new List<TeamAccessRuleInfo>())
                .Select(rule => new
                {
                    mapValue = new
                    {
                        fields = new
                        {
                            role = new { stringValue = TeamPermissionService.NormalizeRole(rule.Role) },
                            canAddMembers = new { booleanValue = rule.CanAddMembers },
                            canManageMembers = new { booleanValue = rule.CanManageMembers },
                            canAssignLeadership = new { booleanValue = rule.CanAssignLeadership },
                            canEditProjectSettings = new { booleanValue = rule.CanEditProjectSettings },
                            canDeleteTeam = new { booleanValue = rule.CanDeleteTeam },
                            canComment = new { booleanValue = rule.CanComment },
                            canUploadFiles = new { booleanValue = rule.CanUploadFiles },
                            canExportAgenda = new { booleanValue = rule.CanExportAgenda },
                            canViewProfessorDashboard = new { booleanValue = rule.CanViewProfessorDashboard },
                            canReviewDeliverables = new { booleanValue = rule.CanReviewDeliverables }
                        }
                    }
                })
                .Cast<object>()
                .ToArray();
        }

        private object[] ConvertCommentsToFirestoreArray(List<TeamCommentInfo> comments)
        {
            return (comments ?? new List<TeamCommentInfo>())
                .Select(comment => new
                {
                    mapValue = new
                    {
                        fields = new
                        {
                            commentId = new { stringValue = comment.CommentId ?? Guid.NewGuid().ToString() },
                            authorUserId = new { stringValue = comment.AuthorUserId ?? "" },
                            authorName = new { stringValue = comment.AuthorName ?? "" },
                            content = new { stringValue = comment.Content ?? "" },
                            mentionedUserIds = new { arrayValue = new { values = ConvertStringsToFirestoreArray(comment.MentionedUserIds) } },
                            attachmentFileNames = new { arrayValue = new { values = ConvertStringsToFirestoreArray(comment.AttachmentFileNames) } },
                            createdAt = new { timestampValue = ToFirestoreTimestamp(comment.CreatedAt == default ? DateTime.UtcNow : comment.CreatedAt) }
                        }
                    }
                })
                .Cast<object>()
                .ToArray();
        }

        private object[] ConvertAttachmentsToFirestoreArray(List<TeamAttachmentInfo> attachments)
        {
            return (attachments ?? new List<TeamAttachmentInfo>())
                .Select(attachment => new
                {
                    mapValue = new
                    {
                        fields = new
                        {
                            attachmentId = new { stringValue = attachment.AttachmentId ?? Guid.NewGuid().ToString() },
                            assetId = new { stringValue = attachment.AssetId ?? "" },
                            fileName = new { stringValue = attachment.FileName ?? "" },
                            previewImageDataUri = new { stringValue = attachment.PreviewImageDataUri ?? "" },
                            permissionScope = new { stringValue = attachment.PermissionScope ?? "team" },
                            version = new { integerValue = attachment.Version.ToString() },
                            addedByUserId = new { stringValue = attachment.AddedByUserId ?? "" },
                            addedAt = new { timestampValue = ToFirestoreTimestamp(attachment.AddedAt == default ? DateTime.UtcNow : attachment.AddedAt) }
                        }
                    }
                })
                .Cast<object>()
                .ToArray();
        }

        private object[] ConvertAssetVersionsToFirestoreArray(List<TeamAssetVersionInfo> versions)
        {
            return (versions ?? new List<TeamAssetVersionInfo>())
                .Select(version => new
                {
                    mapValue = new
                    {
                        fields = new
                        {
                            versionNumber = new { integerValue = version.VersionNumber.ToString() },
                            changedByUserId = new { stringValue = version.ChangedByUserId ?? "" },
                            changeSummary = new { stringValue = version.ChangeSummary ?? "" },
                            fileName = new { stringValue = version.FileName ?? "" },
                            mimeType = new { stringValue = version.MimeType ?? "" },
                            permissionScope = new { stringValue = version.PermissionScope ?? "team" },
                            storageKind = new { stringValue = version.StorageKind ?? "firebase-storage" },
                            storageReference = new { stringValue = version.StorageReference ?? "" },
                            sizeBytes = new { integerValue = version.SizeBytes.ToString() },
                            changedAt = new { timestampValue = ToFirestoreTimestamp(version.ChangedAt == default ? DateTime.UtcNow : version.ChangedAt) }
                        }
                    }
                })
                .Cast<object>()
                .ToArray();
        }

        private async Task<TeamOperationResult> SyncTeamWorkItemsAsync(string teamId, TeamWorkspaceInfo team)
        {
            DebugHelper.WriteLine($"[TeamService.SyncWorkItems] Equipe '{teamId}' com {team.Milestones?.Count ?? 0} milestone(s) e {team.TaskColumns?.Sum(column => column.Cards?.Count ?? 0) ?? 0} card(s).");
            var milestoneResult = await SyncMilestonesSubcollectionAsync(teamId, team, team.Milestones ?? new List<TeamMilestoneInfo>());
            if (!milestoneResult.Success)
            {
                return milestoneResult;
            }

            return await SyncTaskCardsSubcollectionAsync(teamId, team, team.TaskColumns ?? new List<TeamTaskColumnInfo>());
        }

        private async Task<TeamOperationResult> SyncMilestonesSubcollectionAsync(string teamId, TeamWorkspaceInfo team, List<TeamMilestoneInfo> milestones)
        {
            var collectionPath = $"teams/{teamId}/milestones";
            var existingDocuments = await GetSubcollectionDocumentsAsync(collectionPath);
            if (existingDocuments.Count == 0 && !CanCurrentUserManageStructuredWorkItems(team))
            {
                DebugHelper.WriteLine($"[TeamService.SyncMilestones] Subcoleção vazia em '{collectionPath}', mantendo fallback inline porque o usuário atual não gerencia marcos estruturados.");
                return TeamOperationResult.Ok();
            }

            var existingIds = new HashSet<string>(existingDocuments.Select(document => document.DocumentId), StringComparer.OrdinalIgnoreCase);
            var currentIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var milestone in milestones ?? new List<TeamMilestoneInfo>())
            {
                var documentId = string.IsNullOrWhiteSpace(milestone.Id) ? Guid.NewGuid().ToString("N") : milestone.Id;
                milestone.Id = documentId;
                currentIds.Add(documentId);

                var payload = BuildMilestoneDocumentPayload(teamId, milestone);
                var saveResult = await UpsertSubcollectionDocumentAsync($"{collectionPath}/{documentId}", payload);
                if (!saveResult.Success)
                {
                    return saveResult;
                }
            }

            foreach (var staleId in existingIds.Except(currentIds, StringComparer.OrdinalIgnoreCase))
            {
                var deleteResult = await DeleteSubcollectionDocumentAsync($"{collectionPath}/{staleId}");
                if (!deleteResult.Success)
                {
                    return deleteResult;
                }
            }

            return TeamOperationResult.Ok();
        }

        private async Task<TeamOperationResult> SyncTaskCardsSubcollectionAsync(string teamId, TeamWorkspaceInfo team, List<TeamTaskColumnInfo> columns)
        {
            var collectionPath = $"teams/{teamId}/taskCards";
            var existingDocuments = await GetSubcollectionDocumentsAsync(collectionPath);
            if (existingDocuments.Count == 0 && !CanCurrentUserManageStructuredWorkItems(team))
            {
                DebugHelper.WriteLine($"[TeamService.SyncTaskCards] Subcoleção vazia em '{collectionPath}', mantendo fallback inline porque o usuário atual não gerencia cards estruturados.");
                return TeamOperationResult.Ok();
            }

            var existingIds = new HashSet<string>(existingDocuments.Select(document => document.DocumentId), StringComparer.OrdinalIgnoreCase);
            var currentIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var column in columns ?? new List<TeamTaskColumnInfo>())
            {
                foreach (var card in column.Cards ?? new List<TeamTaskCardInfo>())
                {
                    var documentId = string.IsNullOrWhiteSpace(card.Id) ? Guid.NewGuid().ToString("N") : card.Id;
                    card.Id = documentId;
                    card.ColumnId = string.IsNullOrWhiteSpace(card.ColumnId) ? column.Id : card.ColumnId;
                    currentIds.Add(documentId);

                    var payload = BuildTaskCardDocumentPayload(teamId, card);
                    var saveResult = await UpsertSubcollectionDocumentAsync($"{collectionPath}/{documentId}", payload);
                    if (!saveResult.Success)
                    {
                        return saveResult;
                    }
                }
            }

            foreach (var staleId in existingIds.Except(currentIds, StringComparer.OrdinalIgnoreCase))
            {
                var deleteResult = await DeleteSubcollectionDocumentAsync($"{collectionPath}/{staleId}");
                if (!deleteResult.Success)
                {
                    return deleteResult;
                }
            }

            return TeamOperationResult.Ok();
        }

        private bool CanCurrentUserManageStructuredWorkItems(TeamWorkspaceInfo team)
        {
            if (team == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(team.CreatedBy) && string.Equals(team.CreatedBy, _currentUserId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var currentRole = team.Members
                .FirstOrDefault(member => string.Equals(member.UserId, _currentUserId, StringComparison.OrdinalIgnoreCase))
                ?.Role;

            if (string.IsNullOrWhiteSpace(currentRole))
            {
                if (string.Equals(team.FocalProfessorUserId, _currentUserId, StringComparison.OrdinalIgnoreCase)
                    || (team.ProfessorSupervisorUserIds?.Contains(_currentUserId, StringComparer.OrdinalIgnoreCase) ?? false))
                {
                    currentRole = "professor";
                }
                else
                {
                    currentRole = _currentUserRole;
                }
            }

            currentRole = TeamPermissionService.NormalizeRole(currentRole);

            return TeamPermissionService.CanEditProjectSettings(team, currentRole)
                || TeamPermissionService.CanReviewDeliverables(team, currentRole);
        }

        private object BuildMilestoneDocumentPayload(string teamId, TeamMilestoneInfo milestone)
        {
            return new
            {
                fields = new
                {
                    teamId = new { stringValue = teamId },
                    id = new { stringValue = milestone.Id ?? string.Empty },
                    title = new { stringValue = milestone.Title ?? string.Empty },
                    notes = new { stringValue = milestone.Notes ?? string.Empty },
                    status = new { stringValue = milestone.Status ?? "Planejada" },
                    createdByUserId = new { stringValue = milestone.CreatedByUserId ?? string.Empty },
                    ownerUserId = new { stringValue = milestone.OwnerUserId ?? string.Empty },
                    requiresProfessorReview = new { booleanValue = milestone.RequiresProfessorReview },
                    mentionedUserIds = new { arrayValue = new { values = ConvertStringsToFirestoreArray(milestone.MentionedUserIds) } },
                    comments = new { arrayValue = new { values = ConvertCommentsToFirestoreArray(milestone.Comments) } },
                    attachments = new { arrayValue = new { values = ConvertAttachmentsToFirestoreArray(milestone.Attachments) } },
                    dueDate = CreateTimestampOrNullValue(milestone.DueDate),
                    createdAt = new { timestampValue = ToFirestoreTimestamp(milestone.CreatedAt == default ? DateTime.UtcNow : milestone.CreatedAt) },
                    updatedAt = new { timestampValue = ToFirestoreTimestamp(milestone.UpdatedAt == default ? DateTime.UtcNow : milestone.UpdatedAt) },
                    updatedByUserId = new { stringValue = milestone.UpdatedByUserId ?? string.Empty }
                }
            };
        }

        private object BuildTaskCardDocumentPayload(string teamId, TeamTaskCardInfo card)
        {
            return new
            {
                fields = new
                {
                    teamId = new { stringValue = teamId },
                    id = new { stringValue = card.Id ?? string.Empty },
                    columnId = new { stringValue = card.ColumnId ?? string.Empty },
                    title = new { stringValue = card.Title ?? string.Empty },
                    description = new { stringValue = card.Description ?? string.Empty },
                    priority = new { stringValue = card.Priority ?? "Media" },
                    dueDate = CreateTimestampOrNullValue(card.DueDate),
                    estimatedHours = new { integerValue = card.EstimatedHours.ToString() },
                    workloadPoints = new { integerValue = card.WorkloadPoints.ToString() },
                    requiredRole = new { stringValue = TeamPermissionService.NormalizeRole(card.RequiredRole) },
                    requiresProfessorReview = new { booleanValue = card.RequiresProfessorReview },
                    assignedUserIds = new { arrayValue = new { values = ConvertStringsToFirestoreArray(card.AssignedUserIds) } },
                    mentionedUserIds = new { arrayValue = new { values = ConvertStringsToFirestoreArray(card.MentionedUserIds) } },
                    comments = new { arrayValue = new { values = ConvertCommentsToFirestoreArray(card.Comments) } },
                    attachments = new { arrayValue = new { values = ConvertAttachmentsToFirestoreArray(card.Attachments) } },
                    createdAt = new { timestampValue = ToFirestoreTimestamp(card.CreatedAt == default ? DateTime.UtcNow : card.CreatedAt) },
                    createdByUserId = new { stringValue = card.CreatedByUserId ?? string.Empty },
                    updatedAt = new { timestampValue = ToFirestoreTimestamp(card.UpdatedAt == default ? DateTime.UtcNow : card.UpdatedAt) },
                    updatedByUserId = new { stringValue = card.UpdatedByUserId ?? string.Empty }
                }
            };
        }

        private async Task<TeamOperationResult> UpsertSubcollectionDocumentAsync(string documentPath, object payload)
        {
            try
            {
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), AppConfig.BuildFirestoreDocumentUrl(documentPath));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);
                request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return TeamOperationResult.Ok();
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                DebugHelper.WriteLine($"[TeamService.UpsertSubcollection] ❌ {documentPath} -> HTTP {(int)response.StatusCode}: {responseBody.Substring(0, Math.Min(responseBody.Length, 200))}");
                return TeamOperationResult.Fail($"HTTP {(int)response.StatusCode}: {responseBody.Substring(0, Math.Min(responseBody.Length, 200))}");
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeamService.UpsertSubcollection] ❌ {documentPath} -> {ex.Message}");
                return TeamOperationResult.Fail(ex.Message);
            }
        }

        private async Task<TeamOperationResult> DeleteSubcollectionDocumentAsync(string documentPath)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, AppConfig.BuildFirestoreDocumentUrl(documentPath));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);

                var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return TeamOperationResult.Ok();
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                return TeamOperationResult.Fail($"HTTP {(int)response.StatusCode}: {responseBody.Substring(0, Math.Min(responseBody.Length, 200))}");
            }
            catch (Exception ex)
            {
                return TeamOperationResult.Fail(ex.Message);
            }
        }

        private async Task<List<FirestoreDocumentSnapshot>> GetSubcollectionDocumentsAsync(string collectionPath)
        {
            var snapshots = new List<FirestoreDocumentSnapshot>();

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, AppConfig.BuildFirestoreDocumentUrl(collectionPath));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);

                var response = await httpClient.SendAsync(request);
                var jsonContent = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    DebugHelper.WriteLine($"[TeamService.GetSubcollectionDocuments] {collectionPath} -> HTTP {(int)response.StatusCode}: {jsonContent.Substring(0, Math.Min(jsonContent.Length, 200))}");
                    return snapshots;
                }

                using var doc = JsonDocument.Parse(jsonContent);
                if (!doc.RootElement.TryGetProperty("documents", out var documentsArray))
                {
                    return snapshots;
                }

                foreach (var document in documentsArray.EnumerateArray())
                {
                    if (!document.TryGetProperty("fields", out var fields))
                    {
                        continue;
                    }

                    snapshots.Add(new FirestoreDocumentSnapshot
                    {
                        DocumentId = GetDocumentId(document),
                        Fields = fields.Clone()
                    });
                }

                DebugHelper.WriteLine($"[TeamService.GetSubcollectionDocuments] {collectionPath} -> {snapshots.Count} documento(s).");
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeamService.GetSubcollectionDocuments] Erro em {collectionPath}: {ex.Message}");
            }

            return snapshots;
        }

        public async Task<TeamAssetStorageResult> SaveTeamAssetContentAsync(string teamId, string assetId, TeamAssetVersionInfo version, byte[] fileBytes)
        {
            try
            {
                if (fileBytes == null || fileBytes.Length == 0)
                {
                    return TeamAssetStorageResult.Fail("Arquivo vazio ou indisponível para sincronização remota.");
                }

                var normalizedTeamId = NormalizeTeamCode(teamId);
                var safeAssetId = string.IsNullOrWhiteSpace(assetId) ? Guid.NewGuid().ToString("N") : assetId.Trim();
                var versionNumber = Math.Max(1, version.VersionNumber);
                var permissionScope = TeamPermissionService.NormalizePermissionScope(version.PermissionScope);
                var ownerUserId = string.IsNullOrWhiteSpace(version.ChangedByUserId) ? _currentUserId : version.ChangedByUserId;
                var objectPath = BuildTeamAssetStorageObjectPath(normalizedTeamId, permissionScope, ownerUserId, safeAssetId, versionNumber, version.FileName);

                var uploadResult = await UploadTeamAssetToStorageAsync(objectPath, version.MimeType, fileBytes);
                if (!uploadResult.Success)
                {
                    return uploadResult;
                }

                return TeamAssetStorageResult.Ok(objectPath, objectPath);
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeamService.SaveTeamAssetContent] Exceção: {ex.Message}");
                return TeamAssetStorageResult.Fail(ex.Message);
            }
        }

        public async Task<TeamAssetDownloadResult> LoadTeamAssetContentAsync(string storageReference)
        {
            try
            {
                return IsLegacyFirestoreAssetReference(storageReference)
                    ? await LoadTeamAssetContentFromFirestoreAsync(storageReference)
                    : await LoadTeamAssetContentFromStorageAsync(storageReference);
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeamService.LoadTeamAssetContent] Exceção: {ex.Message}");
                return TeamAssetDownloadResult.Fail(ex.Message);
            }
        }

        public async Task<TeamOperationResult> DeleteTeamAssetContentAsync(string storageReference)
        {
            try
            {
                return IsLegacyFirestoreAssetReference(storageReference)
                    ? await DeleteTeamAssetContentFromFirestoreAsync(storageReference)
                    : await DeleteTeamAssetContentFromStorageAsync(storageReference);
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeamService.DeleteTeamAssetContent] Exceção: {ex.Message}");
                return TeamOperationResult.Fail(ex.Message);
            }
        }

        private async Task<TeamOperationResult> DeleteAssetContentDocumentsAsync(TeamWorkspaceInfo team)
        {
            var storageReferences = (team.Assets ?? new List<TeamAssetInfo>())
                .SelectMany(asset => new[] { asset.StorageReference }
                    .Concat((asset.VersionHistory ?? new List<TeamAssetVersionInfo>()).Select(version => version.StorageReference)))
                .Where(reference => !string.IsNullOrWhiteSpace(reference))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var storageReference in storageReferences)
            {
                var deleteResult = await DeleteTeamAssetContentAsync(storageReference);
                if (!deleteResult.Success)
                {
                    return deleteResult;
                }
            }

            return TeamOperationResult.Ok();
        }

        private static string NormalizeFirestoreDocumentPath(string? documentPath)
        {
            var normalized = (documentPath ?? string.Empty).Trim().Trim('/');
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return string.Empty;
            }

            var documentsMarker = "/documents/";
            var markerIndex = normalized.IndexOf(documentsMarker, StringComparison.OrdinalIgnoreCase);
            if (markerIndex >= 0)
            {
                normalized = normalized.Substring(markerIndex + documentsMarker.Length);
            }

            if (normalized.StartsWith("documents/", StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized.Substring("documents/".Length);
            }

            return normalized.Trim('/');
        }

        private static bool IsLegacyFirestoreAssetReference(string? storageReference)
        {
            var normalized = NormalizeFirestoreDocumentPath(storageReference);
            return normalized.StartsWith("teamAssetFiles/", StringComparison.OrdinalIgnoreCase);
        }

        private string BuildTeamAssetStorageObjectPath(string teamId, string permissionScope, string ownerUserId, string assetId, int versionNumber, string? fileName)
        {
            var normalizedTeamId = SanitizeStorageSegment(teamId);
            var normalizedScope = TeamPermissionService.NormalizePermissionScope(permissionScope);
            var normalizedOwner = SanitizeStorageSegment(string.IsNullOrWhiteSpace(ownerUserId) ? _currentUserId : ownerUserId);
            var normalizedAssetId = SanitizeStorageSegment(assetId);
            var normalizedFileName = SanitizeStorageFileName(fileName);
            return $"team-assets/{normalizedTeamId}/{normalizedScope}/{normalizedOwner}/{normalizedAssetId}/v{versionNumber:D4}/{normalizedFileName}";
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
                if (char.IsLetterOrDigit(character) || character == '-' || character == '_')
                {
                    builder.Append(char.ToLowerInvariant(character));
                }
                else
                {
                    builder.Append('-');
                }
            }

            return builder.ToString().Trim('-');
        }

        private static string SanitizeStorageFileName(string? fileName)
        {
            var candidate = string.IsNullOrWhiteSpace(fileName) ? "arquivo.bin" : fileName.Trim();
            var invalidChars = Path.GetInvalidFileNameChars();
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

        private async Task<TeamAssetStorageResult> UploadTeamAssetToStorageAsync(string objectPath, string? mimeType, byte[] fileBytes)
        {
            string? lastErrorMessage = null;
            foreach (var uploadUrl in AppConfig.BuildFirebaseStorageUploadUrls(objectPath))
            {
                var request = new HttpRequestMessage(HttpMethod.Post, uploadUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);
                request.Content = new ByteArrayContent(fileBytes);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrWhiteSpace(mimeType) ? "application/octet-stream" : mimeType);

                var response = await httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    return TeamAssetStorageResult.Ok(objectPath, objectPath);
                }

                lastErrorMessage = $"HTTP {(int)response.StatusCode}: {responseBody.Substring(0, Math.Min(responseBody.Length, 200))}";
                if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
                {
                    DebugHelper.WriteLine($"[TeamService.UploadTeamAssetToStorage] Erro: {lastErrorMessage}");
                    return TeamAssetStorageResult.Fail(lastErrorMessage);
                }
            }

            DebugHelper.WriteLine($"[TeamService.UploadTeamAssetToStorage] Erro: {lastErrorMessage}");
            return TeamAssetStorageResult.Fail(lastErrorMessage ?? "HTTP 404: bucket do Firebase Storage não encontrado.");
        }

        private async Task<TeamAssetDownloadResult> LoadTeamAssetContentFromStorageAsync(string storageReference)
        {
            var normalizedReference = (storageReference ?? string.Empty).Trim().Trim('/');
            if (string.IsNullOrWhiteSpace(normalizedReference))
            {
                return TeamAssetDownloadResult.Fail("Referência remota do arquivo está vazia.");
            }

            HttpResponseMessage? response = null;
            string? errorMessage = null;
            foreach (var downloadUrl in AppConfig.BuildFirebaseStorageDownloadUrls(normalizedReference))
            {
                var request = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);

                response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    break;
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                errorMessage = $"HTTP {(int)response.StatusCode}: {responseBody.Substring(0, Math.Min(responseBody.Length, 200))}";
                if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
                {
                    DebugHelper.WriteLine($"[TeamService.LoadTeamAssetContentFromStorage] Erro: {errorMessage}");
                    return TeamAssetDownloadResult.Fail(errorMessage);
                }
            }

            if (response == null || !response.IsSuccessStatusCode)
            {
                DebugHelper.WriteLine($"[TeamService.LoadTeamAssetContentFromStorage] Erro: {errorMessage}");
                return TeamAssetDownloadResult.Fail(errorMessage ?? "HTTP 404: bucket do Firebase Storage não encontrado.");
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();
            var segments = normalizedReference.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var versionSegment = segments.Length >= 2 ? segments[^2] : "v0001";
            var fileName = segments.Length >= 1 ? segments[^1] : "arquivo.bin";
            var versionNumber = versionSegment.StartsWith("v", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(versionSegment.Substring(1), out var parsedVersion)
                    ? parsedVersion
                    : 1;
            var teamId = segments.Length >= 6 ? segments[1] : string.Empty;
            var permissionScope = segments.Length >= 6 ? segments[2] : "team";
            var ownerUserId = segments.Length >= 6 ? segments[3] : string.Empty;
            var assetId = segments.Length >= 6 ? segments[4] : string.Empty;

            return TeamAssetDownloadResult.Ok(new TeamAssetContentPayload
            {
                TeamId = teamId,
                AssetId = assetId,
                FileName = fileName,
                MimeType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream",
                PermissionScope = permissionScope,
                StorageKind = "firebase-storage",
                StorageReference = normalizedReference,
                VersionNumber = Math.Max(1, versionNumber),
                SizeBytes = bytes.LongLength,
                UploadedByUserId = ownerUserId,
                UploadedAt = DateTime.Now,
                Bytes = bytes
            });
        }

        private async Task<TeamAssetDownloadResult> LoadTeamAssetContentFromFirestoreAsync(string storageReference)
        {
            var documentPath = NormalizeFirestoreDocumentPath(storageReference);
            if (string.IsNullOrWhiteSpace(documentPath))
            {
                return TeamAssetDownloadResult.Fail("Referência remota do arquivo está vazia.");
            }

            var request = new HttpRequestMessage(HttpMethod.Get, AppConfig.BuildFirestoreDocumentUrl(documentPath));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);

            var response = await httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = $"HTTP {(int)response.StatusCode}: {responseBody.Substring(0, Math.Min(responseBody.Length, 200))}";
                DebugHelper.WriteLine($"[TeamService.LoadTeamAssetContentFromFirestore] Erro: {errorMessage}");
                return TeamAssetDownloadResult.Fail(errorMessage);
            }

            using var doc = JsonDocument.Parse(responseBody);
            if (!doc.RootElement.TryGetProperty("fields", out var fields))
            {
                return TeamAssetDownloadResult.Fail("Documento remoto do arquivo retornou sem campos legíveis.");
            }

            var contentBase64 = GetString(fields, "contentBase64");
            if (string.IsNullOrWhiteSpace(contentBase64))
            {
                return TeamAssetDownloadResult.Fail("Conteúdo remoto do arquivo está vazio.");
            }

            return TeamAssetDownloadResult.Ok(new TeamAssetContentPayload
            {
                TeamId = GetString(fields, "teamId"),
                AssetId = GetString(fields, "assetId"),
                FileName = GetString(fields, "fileName"),
                MimeType = GetString(fields, "mimeType"),
                PermissionScope = GetString(fields, "permissionScope"),
                StorageKind = GetString(fields, "storageKind"),
                VersionNumber = Math.Max(1, GetInt(fields, "versionNumber")),
                SizeBytes = GetLong(fields, "sizeBytes"),
                UploadedByUserId = GetString(fields, "uploadedByUserId"),
                UploadedAt = GetTimestamp(fields, "uploadedAt"),
                Bytes = Convert.FromBase64String(contentBase64),
                StorageReference = documentPath
            });
        }

        private async Task<TeamOperationResult> DeleteTeamAssetContentFromStorageAsync(string storageReference)
        {
            var normalizedReference = (storageReference ?? string.Empty).Trim().Trim('/');
            if (string.IsNullOrWhiteSpace(normalizedReference))
            {
                return TeamOperationResult.Ok();
            }

            string? errorMessage = null;
            foreach (var metadataUrl in AppConfig.BuildFirebaseStorageMetadataUrls(normalizedReference))
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, metadataUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);

                var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return TeamOperationResult.Ok();
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                errorMessage = $"HTTP {(int)response.StatusCode}: {responseBody.Substring(0, Math.Min(responseBody.Length, 200))}";
                if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
                {
                    DebugHelper.WriteLine($"[TeamService.DeleteTeamAssetContentFromStorage] Erro: {errorMessage}");
                    return TeamOperationResult.Fail(errorMessage);
                }
            }

            DebugHelper.WriteLine($"[TeamService.DeleteTeamAssetContentFromStorage] Erro: {errorMessage}");
            return TeamOperationResult.Fail(errorMessage ?? "HTTP 404: bucket do Firebase Storage não encontrado.");
        }

        private async Task<TeamOperationResult> DeleteTeamAssetContentFromFirestoreAsync(string storageReference)
        {
            var documentPath = NormalizeFirestoreDocumentPath(storageReference);
            if (string.IsNullOrWhiteSpace(documentPath))
            {
                return TeamOperationResult.Ok();
            }

            var request = new HttpRequestMessage(HttpMethod.Delete, AppConfig.BuildFirestoreDocumentUrl(documentPath));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);

            var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return TeamOperationResult.Ok();
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var errorMessage = $"HTTP {(int)response.StatusCode}: {responseBody.Substring(0, Math.Min(responseBody.Length, 200))}";
            DebugHelper.WriteLine($"[TeamService.DeleteTeamAssetContentFromFirestore] Erro: {errorMessage}");
            return TeamOperationResult.Fail(errorMessage);
        }

        private static bool TeamMatchesQuery(TeamWorkspaceInfo team, string query)
        {
            return (team.TeamName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false)
                || (team.Course?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false)
                || (team.ClassName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false)
                || (team.ClassId?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false)
                || (team.TeamId?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false)
                || (team.TemplateName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false)
                || (team.FocalProfessorName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false)
                || (team.ProfessorSupervisorNames?.Any(name => name.Contains(query, StringComparison.OrdinalIgnoreCase)) ?? false)
                || (team.Ucs?.Any(uc => uc.Contains(query, StringComparison.OrdinalIgnoreCase)) ?? false);
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
                var url = AppConfig.BuildFirestoreDocumentUrl("teams");
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
                    AcademicTerm = GetString(fields, "academicTerm"),
                    TemplateId = GetString(fields, "templateId"),
                    TemplateName = GetString(fields, "templateName"),
                    CreatedBy = GetString(fields, "createdBy"),
                    CreatedAt = GetTimestamp(fields, "createdAt"),
                    UpdatedAt = GetTimestamp(fields, "updatedAt"),
                    LastRealtimeSyncAt = GetNullableTimestamp(fields, "lastRealtimeSyncAt"),
                    ProjectProgress = GetInt(fields, "projectProgress"),
                    ProjectDeadline = GetNullableTimestamp(fields, "projectDeadline"),
                    ProjectStatus = GetString(fields, "projectStatus"),
                    TeacherNotes = GetString(fields, "teacherNotes"),
                    FocalProfessorUserId = GetString(fields, "focalProfessorUserId"),
                    FocalProfessorName = GetString(fields, "focalProfessorName"),
                    ProfessorSupervisorUserIds = ParseStringsFromFirestore(fields, "professorSupervisorUserIds"),
                    ProfessorSupervisorNames = ParseStringsFromFirestore(fields, "professorSupervisorNames"),
                    DefaultFilePermissionScope = GetString(fields, "defaultFilePermissionScope"),
                    Milestones = ParseMilestonesFromFirestore(fields),
                    Members = ParseMembersFromFirestore(fields),
                    Ucs = ParseStringsFromFirestore(fields, "ucs"),
                    SemesterTimeline = ParseTimelineFromFirestore(fields),
                    AccessRules = ParseAccessRulesFromFirestore(fields),
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
                                Phone = GetString(memberFields, "phone"),
                                Registration = GetString(memberFields, "registration"),
                                Course = GetString(memberFields, "course"),
                                Role = TeamPermissionService.NormalizeRole(GetString(memberFields, "role")),
                                Nickname = GetString(memberFields, "nickname"),
                                ProfessionalTitle = GetString(memberFields, "professionalTitle"),
                                AcademicDepartment = GetString(memberFields, "academicDepartment"),
                                AcademicFocus = GetString(memberFields, "academicFocus"),
                                OfficeHours = GetString(memberFields, "officeHours"),
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
                        CreatedByUserId = GetString(milestoneFields, "createdByUserId"),
                        OwnerUserId = GetString(milestoneFields, "ownerUserId"),
                        RequiresProfessorReview = GetBool(milestoneFields, "requiresProfessorReview"),
                        MentionedUserIds = ParseStringsFromFirestore(milestoneFields, "mentionedUserIds"),
                        Comments = ParseCommentsFromFirestore(milestoneFields),
                        Attachments = ParseAttachmentsFromFirestore(milestoneFields),
                        DueDate = GetNullableTimestamp(milestoneFields, "dueDate"),
                        CreatedAt = GetTimestamp(milestoneFields, "createdAt"),
                        UpdatedAt = GetTimestamp(milestoneFields, "updatedAt"),
                        UpdatedByUserId = GetString(milestoneFields, "updatedByUserId")
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
                        Description = GetString(assetFields, "description"),
                        MimeType = GetString(assetFields, "mimeType"),
                        FolderPath = GetString(assetFields, "folderPath"),
                        PermissionScope = GetString(assetFields, "permissionScope"),
                        StorageKind = GetString(assetFields, "storageKind"),
                        StorageReference = GetString(assetFields, "storageReference"),
                        SizeBytes = GetLong(assetFields, "sizeBytes"),
                        Version = Math.Max(1, GetInt(assetFields, "version")),
                        AddedByUserId = GetString(assetFields, "addedByUserId"),
                        AddedAt = GetTimestamp(assetFields, "addedAt"),
                        LastSyncedAt = GetNullableTimestamp(assetFields, "lastSyncedAt"),
                        VersionHistory = ParseAssetVersionsFromFirestore(assetFields)
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
                    ColumnId = GetString(cardFields, "columnId"),
                    Title = GetString(cardFields, "title"),
                    Description = GetString(cardFields, "description"),
                    Priority = GetString(cardFields, "priority"),
                    EstimatedHours = GetInt(cardFields, "estimatedHours"),
                    WorkloadPoints = GetInt(cardFields, "workloadPoints"),
                    RequiredRole = TeamPermissionService.NormalizeRole(GetString(cardFields, "requiredRole")),
                    RequiresProfessorReview = GetBool(cardFields, "requiresProfessorReview"),
                    DueDate = GetNullableTimestamp(cardFields, "dueDate"),
                    AssignedUserIds = ParseStringsFromFirestore(cardFields, "assignedUserIds"),
                    MentionedUserIds = ParseStringsFromFirestore(cardFields, "mentionedUserIds"),
                    Comments = ParseCommentsFromFirestore(cardFields),
                    Attachments = ParseAttachmentsFromFirestore(cardFields),
                    CreatedAt = GetTimestamp(cardFields, "createdAt"),
                    CreatedByUserId = GetString(cardFields, "createdByUserId"),
                    UpdatedAt = GetTimestamp(cardFields, "updatedAt"),
                    UpdatedByUserId = GetString(cardFields, "updatedByUserId")
                });
            }

            return cards;
        }

        private async Task PopulateTeamWorkItemsFromSubcollectionsAsync(TeamWorkspaceInfo team)
        {
            if (team == null || string.IsNullOrWhiteSpace(team.TeamId))
            {
                return;
            }

            var milestoneDocuments = await GetSubcollectionDocumentsAsync($"teams/{team.TeamId}/milestones");
            if (milestoneDocuments.Count > 0)
            {
                team.Milestones = ParseMilestonesFromSubcollectionDocuments(milestoneDocuments);
            }

            var taskCardDocuments = await GetSubcollectionDocumentsAsync($"teams/{team.TeamId}/taskCards");
            if (taskCardDocuments.Count > 0)
            {
                ApplyTaskCardsFromSubcollectionDocuments(team, taskCardDocuments);
            }
        }

        private List<TeamMilestoneInfo> ParseMilestonesFromSubcollectionDocuments(List<FirestoreDocumentSnapshot> documents)
        {
            var milestones = new List<TeamMilestoneInfo>();

            foreach (var document in documents)
            {
                var milestoneFields = document.Fields;
                milestones.Add(new TeamMilestoneInfo
                {
                    Id = string.IsNullOrWhiteSpace(GetString(milestoneFields, "id")) ? document.DocumentId : GetString(milestoneFields, "id"),
                    Title = GetString(milestoneFields, "title"),
                    Notes = GetString(milestoneFields, "notes"),
                    Status = GetString(milestoneFields, "status"),
                    DueDate = GetNullableTimestamp(milestoneFields, "dueDate"),
                    CreatedByUserId = GetString(milestoneFields, "createdByUserId"),
                    OwnerUserId = GetString(milestoneFields, "ownerUserId"),
                    RequiresProfessorReview = GetBool(milestoneFields, "requiresProfessorReview"),
                    MentionedUserIds = ParseStringsFromFirestore(milestoneFields, "mentionedUserIds"),
                    Comments = ParseCommentsFromFirestore(milestoneFields),
                    Attachments = ParseAttachmentsFromFirestore(milestoneFields),
                    CreatedAt = GetTimestamp(milestoneFields, "createdAt"),
                    UpdatedAt = GetTimestamp(milestoneFields, "updatedAt"),
                    UpdatedByUserId = GetString(milestoneFields, "updatedByUserId")
                });
            }

            return milestones;
        }

        private void ApplyTaskCardsFromSubcollectionDocuments(TeamWorkspaceInfo team, List<FirestoreDocumentSnapshot> documents)
        {
            foreach (var column in team.TaskColumns ?? new List<TeamTaskColumnInfo>())
            {
                column.Cards = new List<TeamTaskCardInfo>();
            }

            if (team.TaskColumns == null)
            {
                team.TaskColumns = new List<TeamTaskColumnInfo>();
            }

            foreach (var document in documents)
            {
                var cardFields = document.Fields;
                var card = new TeamTaskCardInfo
                {
                    Id = string.IsNullOrWhiteSpace(GetString(cardFields, "id")) ? document.DocumentId : GetString(cardFields, "id"),
                    ColumnId = GetString(cardFields, "columnId"),
                    Title = GetString(cardFields, "title"),
                    Description = GetString(cardFields, "description"),
                    Priority = GetString(cardFields, "priority"),
                    DueDate = GetNullableTimestamp(cardFields, "dueDate"),
                    EstimatedHours = GetInt(cardFields, "estimatedHours"),
                    WorkloadPoints = GetInt(cardFields, "workloadPoints"),
                    RequiredRole = TeamPermissionService.NormalizeRole(GetString(cardFields, "requiredRole")),
                    RequiresProfessorReview = GetBool(cardFields, "requiresProfessorReview"),
                    AssignedUserIds = ParseStringsFromFirestore(cardFields, "assignedUserIds"),
                    MentionedUserIds = ParseStringsFromFirestore(cardFields, "mentionedUserIds"),
                    Comments = ParseCommentsFromFirestore(cardFields),
                    Attachments = ParseAttachmentsFromFirestore(cardFields),
                    CreatedAt = GetTimestamp(cardFields, "createdAt"),
                    CreatedByUserId = GetString(cardFields, "createdByUserId"),
                    UpdatedAt = GetTimestamp(cardFields, "updatedAt"),
                    UpdatedByUserId = GetString(cardFields, "updatedByUserId")
                };

                var targetColumn = team.TaskColumns.FirstOrDefault(column => string.Equals(column.Id, card.ColumnId, StringComparison.OrdinalIgnoreCase));
                if (targetColumn == null)
                {
                    targetColumn = new TeamTaskColumnInfo
                    {
                        Id = string.IsNullOrWhiteSpace(card.ColumnId) ? Guid.NewGuid().ToString("N") : card.ColumnId,
                        Title = "Coluna migrada",
                        AccentColor = System.Windows.Media.Color.FromRgb(37, 99, 235),
                        Cards = new List<TeamTaskCardInfo>()
                    };
                    team.TaskColumns.Add(targetColumn);
                }

                targetColumn.Cards.Add(card);
            }
        }

        private async Task<TeamOperationResult> DeleteTeamWorkItemDocumentsAsync(string teamId)
        {
            var milestoneDeleteResult = await DeleteSubcollectionDocumentsAsync($"teams/{teamId}/milestones");
            if (!milestoneDeleteResult.Success)
            {
                return milestoneDeleteResult;
            }

            return await DeleteSubcollectionDocumentsAsync($"teams/{teamId}/taskCards");
        }

        private async Task<TeamOperationResult> DeleteSubcollectionDocumentsAsync(string collectionPath)
        {
            var documents = await GetSubcollectionDocumentsAsync(collectionPath);
            foreach (var document in documents)
            {
                var deleteResult = await DeleteSubcollectionDocumentAsync($"{collectionPath}/{document.DocumentId}");
                if (!deleteResult.Success)
                {
                    return deleteResult;
                }
            }

            return TeamOperationResult.Ok();
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
                        Id = GetString(notificationFields, "id"),
                        Message = GetString(notificationFields, "message"),
                        Type = GetString(notificationFields, "type"),
                        Audience = GetString(notificationFields, "audience"),
                        RelatedEntityId = GetString(notificationFields, "relatedEntityId"),
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

        private List<TeamTimelineItemInfo> ParseTimelineFromFirestore(JsonElement fields)
        {
            var timeline = new List<TeamTimelineItemInfo>();

            try
            {
                foreach (var value in EnumerateArrayField(fields, "semesterTimeline"))
                {
                    if (!TryGetMapFields(value, out var timelineFields))
                    {
                        continue;
                    }

                    timeline.Add(new TeamTimelineItemInfo
                    {
                        Id = GetString(timelineFields, "id"),
                        Title = GetString(timelineFields, "title"),
                        Description = GetString(timelineFields, "description"),
                        Category = GetString(timelineFields, "category"),
                        Status = GetString(timelineFields, "status"),
                        OwnerUserId = GetString(timelineFields, "ownerUserId"),
                        StartsAt = GetNullableTimestamp(timelineFields, "startsAt"),
                        EndsAt = GetNullableTimestamp(timelineFields, "endsAt")
                    });
                }
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeamService.ParseTimeline] Erro: {ex.Message}");
            }

            return timeline;
        }

        private List<TeamAccessRuleInfo> ParseAccessRulesFromFirestore(JsonElement fields)
        {
            var accessRules = new List<TeamAccessRuleInfo>();

            try
            {
                foreach (var value in EnumerateArrayField(fields, "accessRules"))
                {
                    if (!TryGetMapFields(value, out var ruleFields))
                    {
                        continue;
                    }

                    accessRules.Add(new TeamAccessRuleInfo
                    {
                        Role = TeamPermissionService.NormalizeRole(GetString(ruleFields, "role")),
                        CanAddMembers = !ruleFields.TryGetProperty("canAddMembers", out _)
                            ? GetBool(ruleFields, "canManageMembers")
                            : GetBool(ruleFields, "canAddMembers"),
                        CanManageMembers = GetBool(ruleFields, "canManageMembers"),
                        CanAssignLeadership = GetBool(ruleFields, "canAssignLeadership"),
                        CanEditProjectSettings = GetBool(ruleFields, "canEditProjectSettings"),
                        CanDeleteTeam = GetBool(ruleFields, "canDeleteTeam"),
                        CanComment = !ruleFields.TryGetProperty("canComment", out _) || GetBool(ruleFields, "canComment"),
                        CanUploadFiles = !ruleFields.TryGetProperty("canUploadFiles", out _) || GetBool(ruleFields, "canUploadFiles"),
                        CanExportAgenda = GetBool(ruleFields, "canExportAgenda"),
                        CanViewProfessorDashboard = GetBool(ruleFields, "canViewProfessorDashboard"),
                        CanReviewDeliverables = GetBool(ruleFields, "canReviewDeliverables")
                    });
                }
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeamService.ParseAccessRules] Erro: {ex.Message}");
            }

            return TeamPermissionService.NormalizeAccessRules(accessRules.Count == 0 ? TeamPermissionService.CreateDefaultAccessRules() : accessRules);
        }

        private List<TeamCommentInfo> ParseCommentsFromFirestore(JsonElement fields)
        {
            var comments = new List<TeamCommentInfo>();

            try
            {
                foreach (var value in EnumerateArrayField(fields, "comments"))
                {
                    if (!TryGetMapFields(value, out var commentFields))
                    {
                        continue;
                    }

                    comments.Add(new TeamCommentInfo
                    {
                        CommentId = GetString(commentFields, "commentId"),
                        AuthorUserId = GetString(commentFields, "authorUserId"),
                        AuthorName = GetString(commentFields, "authorName"),
                        Content = GetString(commentFields, "content"),
                        MentionedUserIds = ParseStringsFromFirestore(commentFields, "mentionedUserIds"),
                        AttachmentFileNames = ParseStringsFromFirestore(commentFields, "attachmentFileNames"),
                        CreatedAt = GetTimestamp(commentFields, "createdAt")
                    });
                }
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeamService.ParseComments] Erro: {ex.Message}");
            }

            return comments;
        }

        private List<TeamAttachmentInfo> ParseAttachmentsFromFirestore(JsonElement fields)
        {
            var attachments = new List<TeamAttachmentInfo>();

            try
            {
                foreach (var value in EnumerateArrayField(fields, "attachments"))
                {
                    if (!TryGetMapFields(value, out var attachmentFields))
                    {
                        continue;
                    }

                    attachments.Add(new TeamAttachmentInfo
                    {
                        AttachmentId = GetString(attachmentFields, "attachmentId"),
                        AssetId = GetString(attachmentFields, "assetId"),
                        FileName = GetString(attachmentFields, "fileName"),
                        PreviewImageDataUri = GetString(attachmentFields, "previewImageDataUri"),
                        PermissionScope = GetString(attachmentFields, "permissionScope"),
                        Version = Math.Max(1, GetInt(attachmentFields, "version")),
                        AddedByUserId = GetString(attachmentFields, "addedByUserId"),
                        AddedAt = GetTimestamp(attachmentFields, "addedAt")
                    });
                }
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeamService.ParseAttachments] Erro: {ex.Message}");
            }

            return attachments;
        }

        private List<TeamAssetVersionInfo> ParseAssetVersionsFromFirestore(JsonElement fields)
        {
            var versions = new List<TeamAssetVersionInfo>();

            try
            {
                foreach (var value in EnumerateArrayField(fields, "versionHistory"))
                {
                    if (!TryGetMapFields(value, out var versionFields))
                    {
                        continue;
                    }

                    versions.Add(new TeamAssetVersionInfo
                    {
                        VersionNumber = Math.Max(1, GetInt(versionFields, "versionNumber")),
                        ChangedByUserId = GetString(versionFields, "changedByUserId"),
                        ChangeSummary = GetString(versionFields, "changeSummary"),
                        FileName = GetString(versionFields, "fileName"),
                        MimeType = GetString(versionFields, "mimeType"),
                        PermissionScope = GetString(versionFields, "permissionScope"),
                        StorageKind = GetString(versionFields, "storageKind"),
                        StorageReference = GetString(versionFields, "storageReference"),
                        SizeBytes = GetLong(versionFields, "sizeBytes"),
                        ChangedAt = GetTimestamp(versionFields, "changedAt")
                    });
                }
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeamService.ParseAssetVersions] Erro: {ex.Message}");
            }

            return versions;
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
            var url = AppConfig.BuildFirestoreDocumentUrl("userTeams");
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

            var url = AppConfig.BuildFirestoreDocumentUrl($"userTeams/{documentId}");
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

        private long GetLong(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var prop))
            {
                if (prop.TryGetProperty("integerValue", out var integerValue) &&
                    long.TryParse(integerValue.GetString(), out var parsedLong))
                {
                    return parsedLong;
                }

                if (prop.TryGetProperty("stringValue", out var stringValue) &&
                    long.TryParse(stringValue.GetString(), out parsedLong))
                {
                    return parsedLong;
                }
            }

            return 0;
        }

        private bool GetBool(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var prop) &&
                prop.TryGetProperty("booleanValue", out var booleanValue) &&
                booleanValue.GetBoolean();
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

    internal sealed class FirestoreDocumentSnapshot
    {
        public string DocumentId { get; set; } = string.Empty;
        public JsonElement Fields { get; set; }
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

    public class TeamAssetStorageResult
    {
        public bool Success { get; set; }
        public string StorageReference { get; set; } = string.Empty;
        public string DocumentId { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }

        public static TeamAssetStorageResult Ok(string storageReference, string documentId)
        {
            return new TeamAssetStorageResult
            {
                Success = true,
                StorageReference = storageReference,
                DocumentId = documentId
            };
        }

        public static TeamAssetStorageResult Fail(string errorMessage)
        {
            return new TeamAssetStorageResult { Success = false, ErrorMessage = errorMessage };
        }
    }

    public class TeamAssetDownloadResult
    {
        public bool Success { get; set; }
        public TeamAssetContentPayload? Payload { get; set; }
        public string? ErrorMessage { get; set; }

        public static TeamAssetDownloadResult Ok(TeamAssetContentPayload payload)
        {
            return new TeamAssetDownloadResult { Success = true, Payload = payload };
        }

        public static TeamAssetDownloadResult Fail(string errorMessage)
        {
            return new TeamAssetDownloadResult { Success = false, ErrorMessage = errorMessage };
        }
    }

    public class TeamAssetContentPayload
    {
        public string TeamId { get; set; } = string.Empty;
        public string AssetId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public string PermissionScope { get; set; } = "team";
        public string StorageKind { get; set; } = "firebase-storage";
        public string StorageReference { get; set; } = string.Empty;
        public int VersionNumber { get; set; } = 1;
        public long SizeBytes { get; set; }
        public string UploadedByUserId { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public byte[] Bytes { get; set; } = Array.Empty<byte>();
    }
}
