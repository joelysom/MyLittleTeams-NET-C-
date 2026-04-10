using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MeuApp
{
    public class TeachingClassService
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private const string JoinCodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        private readonly string _idToken;
        private readonly string _currentUserId;
        private readonly string _currentUserRole;

        public TeachingClassService(string idToken, string currentUserId, string currentUserRole = "student")
        {
            _idToken = idToken;
            _currentUserId = currentUserId;
            _currentUserRole = TeamPermissionService.NormalizeRole(currentUserRole);
        }

        public async Task<List<TeachingClassInfo>> LoadClassesAsync()
        {
            var classes = new List<TeachingClassInfo>();
            if (string.IsNullOrWhiteSpace(_currentUserId))
            {
                return classes;
            }

            try
            {
                var classIds = await GetUserClassIdsAsync(_currentUserId);
                var loadTasks = classIds
                    .Where(classId => !string.IsNullOrWhiteSpace(classId))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Select(GetClassByIdAsync)
                    .ToArray();

                var loaded = loadTasks.Length == 0
                    ? Array.Empty<TeachingClassInfo?>()
                    : await Task.WhenAll(loadTasks);

                foreach (var item in loaded)
                {
                    if (item != null)
                    {
                        classes.Add(item);
                    }
                }

                return classes
                    .GroupBy(item => item.ClassId, StringComparer.OrdinalIgnoreCase)
                    .Select(group => group.First())
                    .OrderBy(item => item.Course)
                    .ThenBy(item => item.ClassName)
                    .ToList();
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeachingClassService.LoadClasses] Erro: {ex.Message}");
                return classes;
            }
        }

        public async Task<TeachingClassInfo?> GetClassByIdAsync(string classId)
        {
            try
            {
                var normalizedClassId = NormalizeClassCode(classId);
                if (string.IsNullOrWhiteSpace(normalizedClassId))
                {
                    return null;
                }

                var request = new HttpRequestMessage(HttpMethod.Get, AppConfig.BuildFirestoreDocumentUrl($"teachingClasses/{normalizedClassId}"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);

                var response = await httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    DebugHelper.WriteLine($"[TeachingClassService.GetClassById] HTTP {(int)response.StatusCode}: {responseBody}");
                    return null;
                }

                using var doc = JsonDocument.Parse(responseBody);
                return doc.RootElement.TryGetProperty("fields", out var fields)
                    ? ParseClassFromFirestore(fields)
                    : null;
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeachingClassService.GetClassById] Erro: {ex.Message}");
                return null;
            }
        }

        public async Task<List<TeachingClassHomePostInfo>> LoadHomePostsAsync(string classId)
        {
            var posts = new List<TeachingClassHomePostInfo>();
            try
            {
                var normalizedClassId = NormalizeClassCode(classId);
                if (string.IsNullOrWhiteSpace(normalizedClassId))
                {
                    return posts;
                }

                var snapshots = await ListCollectionDocumentsAsync($"teachingClasses/{normalizedClassId}/homePosts");
                posts = snapshots
                    .Select(snapshot => ParseHomePostFromFirestore(snapshot.DocumentId, snapshot.Fields))
                    .Where(post => post != null)
                    .Cast<TeachingClassHomePostInfo>()
                    .OrderByDescending(post => post.PublishedAt)
                    .ToList();

                var enrichmentTasks = posts.Select(async post =>
                {
                    post.Comments = await LoadHomeCommentsAsync(normalizedClassId, post.PostId);
                    post.Reactions = await LoadHomeReactionsAsync(normalizedClassId, post.PostId);
                    return post;
                }).ToArray();

                if (enrichmentTasks.Length > 0)
                {
                    posts = (await Task.WhenAll(enrichmentTasks))
                        .OrderByDescending(post => post.PublishedAt)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeachingClassService.LoadHomePosts] Erro: {ex.Message}");
            }

            return posts;
        }

        public async Task<TeachingClassHomePostResult> SaveHomePostAsync(string classId, TeachingClassHomePostInfo post)
        {
            try
            {
                var normalizedClassId = NormalizeClassCode(classId);
                if (string.IsNullOrWhiteSpace(normalizedClassId))
                {
                    return TeachingClassHomePostResult.Fail("Turma docente inválida para publicar no mural.");
                }

                var normalizedPost = NormalizeHomePost(post);
                var requestBody = JsonSerializer.Serialize(new Dictionary<string, object?>
                {
                    ["fields"] = BuildHomePostFirestoreFields(normalizedPost)
                });

                var request = new HttpRequestMessage(new HttpMethod("PATCH"), AppConfig.BuildFirestoreDocumentUrl($"teachingClasses/{normalizedClassId}/homePosts/{normalizedPost.PostId}"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                var response = await httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    return TeachingClassHomePostResult.Fail($"HTTP {(int)response.StatusCode}: {responseBody.Substring(0, Math.Min(responseBody.Length, 200))}");
                }

                return TeachingClassHomePostResult.Ok(normalizedPost);
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeachingClassService.SaveHomePost] Erro: {ex.Message}");
                return TeachingClassHomePostResult.Fail(ex.Message);
            }
        }

        public async Task<TeachingClassOperationResult> SaveHomeCommentAsync(string classId, string postId, TeachingClassPostCommentInfo comment)
        {
            try
            {
                var normalizedClassId = NormalizeClassCode(classId);
                var normalizedPostId = NormalizeDocumentId(postId);
                if (string.IsNullOrWhiteSpace(normalizedClassId) || string.IsNullOrWhiteSpace(normalizedPostId))
                {
                    return TeachingClassOperationResult.Fail("Publicação da turma inválida para receber comentário.");
                }

                var normalizedComment = NormalizeHomeComment(comment);
                var requestBody = JsonSerializer.Serialize(new Dictionary<string, object?>
                {
                    ["fields"] = BuildHomeCommentFirestoreFields(normalizedComment)
                });

                var request = new HttpRequestMessage(new HttpMethod("PATCH"), AppConfig.BuildFirestoreDocumentUrl($"teachingClasses/{normalizedClassId}/homePosts/{normalizedPostId}/comments/{normalizedComment.CommentId}"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                var response = await httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    return TeachingClassOperationResult.Fail($"HTTP {(int)response.StatusCode}: {responseBody.Substring(0, Math.Min(responseBody.Length, 200))}");
                }

                return TeachingClassOperationResult.Ok();
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeachingClassService.SaveHomeComment] Erro: {ex.Message}");
                return TeachingClassOperationResult.Fail(ex.Message);
            }
        }

        public async Task<TeachingClassOperationResult> SaveHomeReactionAsync(string classId, string postId, TeachingClassPostReactionInfo reaction)
        {
            try
            {
                var normalizedClassId = NormalizeClassCode(classId);
                var normalizedPostId = NormalizeDocumentId(postId);
                if (string.IsNullOrWhiteSpace(normalizedClassId) || string.IsNullOrWhiteSpace(normalizedPostId))
                {
                    return TeachingClassOperationResult.Fail("Publicação da turma inválida para registrar reação.");
                }

                var normalizedReaction = NormalizeHomeReaction(reaction);
                var requestBody = JsonSerializer.Serialize(new Dictionary<string, object?>
                {
                    ["fields"] = BuildHomeReactionFirestoreFields(normalizedReaction)
                });

                var request = new HttpRequestMessage(new HttpMethod("PATCH"), AppConfig.BuildFirestoreDocumentUrl($"teachingClasses/{normalizedClassId}/homePosts/{normalizedPostId}/reactions/{normalizedReaction.UserId}"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                var response = await httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    return TeachingClassOperationResult.Fail($"HTTP {(int)response.StatusCode}: {responseBody.Substring(0, Math.Min(responseBody.Length, 200))}");
                }

                return TeachingClassOperationResult.Ok();
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeachingClassService.SaveHomeReaction] Erro: {ex.Message}");
                return TeachingClassOperationResult.Fail(ex.Message);
            }
        }

        public async Task<TeachingClassOperationResult> DeleteHomeReactionAsync(string classId, string postId, string userId)
        {
            try
            {
                var normalizedClassId = NormalizeClassCode(classId);
                var normalizedPostId = NormalizeDocumentId(postId);
                var normalizedUserId = (userId ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(normalizedClassId) || string.IsNullOrWhiteSpace(normalizedPostId) || string.IsNullOrWhiteSpace(normalizedUserId))
                {
                    return TeachingClassOperationResult.Fail("Reação inválida para remoção.");
                }

                var request = new HttpRequestMessage(HttpMethod.Delete, AppConfig.BuildFirestoreDocumentUrl($"teachingClasses/{normalizedClassId}/homePosts/{normalizedPostId}/reactions/{normalizedUserId}"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);

                var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return TeachingClassOperationResult.Ok();
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                return TeachingClassOperationResult.Fail($"HTTP {(int)response.StatusCode}: {responseBody.Substring(0, Math.Min(responseBody.Length, 200))}");
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeachingClassService.DeleteHomeReaction] Erro: {ex.Message}");
                return TeachingClassOperationResult.Fail(ex.Message);
            }
        }

        public async Task<TeachingClassOperationResult> DeleteHomeCommentAsync(string classId, string postId, string commentId)
        {
            try
            {
                var normalizedClassId = NormalizeClassCode(classId);
                var normalizedPostId = NormalizeDocumentId(postId);
                var normalizedCommentId = NormalizeDocumentId(commentId);
                if (string.IsNullOrWhiteSpace(normalizedClassId) || string.IsNullOrWhiteSpace(normalizedPostId) || string.IsNullOrWhiteSpace(normalizedCommentId))
                {
                    return TeachingClassOperationResult.Fail("Comentário inválido para remoção.");
                }

                var commentSnapshots = await ListCollectionDocumentsAsync($"teachingClasses/{normalizedClassId}/homePosts/{normalizedPostId}/comments");
                var commentIds = commentSnapshots
                    .Select(snapshot => ParseHomeCommentFromFirestore(snapshot.DocumentId, snapshot.Fields))
                    .Where(comment => comment != null)
                    .Cast<TeachingClassPostCommentInfo>()
                    .Where(comment => string.Equals(comment.CommentId, normalizedCommentId, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(comment.ParentCommentId, normalizedCommentId, StringComparison.OrdinalIgnoreCase))
                    .Select(comment => comment.CommentId)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (!commentIds.Contains(normalizedCommentId, StringComparer.OrdinalIgnoreCase))
                {
                    commentIds.Add(normalizedCommentId);
                }

                foreach (var id in commentIds)
                {
                    var deleteResult = await DeleteFirestoreDocumentAsync($"teachingClasses/{normalizedClassId}/homePosts/{normalizedPostId}/comments/{id}");
                    if (!deleteResult.Success)
                    {
                        return deleteResult;
                    }
                }

                return TeachingClassOperationResult.Ok();
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeachingClassService.DeleteHomeComment] Erro: {ex.Message}");
                return TeachingClassOperationResult.Fail(ex.Message);
            }
        }

        public async Task<TeachingClassOperationResult> DeleteHomePostAsync(string classId, string postId)
        {
            try
            {
                var normalizedClassId = NormalizeClassCode(classId);
                var normalizedPostId = NormalizeDocumentId(postId);
                if (string.IsNullOrWhiteSpace(normalizedClassId) || string.IsNullOrWhiteSpace(normalizedPostId))
                {
                    return TeachingClassOperationResult.Fail("Publicação inválida para remoção.");
                }

                var reactionSnapshots = await ListCollectionDocumentsAsync($"teachingClasses/{normalizedClassId}/homePosts/{normalizedPostId}/reactions");
                foreach (var snapshot in reactionSnapshots)
                {
                    var deleteReaction = await DeleteFirestoreDocumentAsync($"teachingClasses/{normalizedClassId}/homePosts/{normalizedPostId}/reactions/{snapshot.DocumentId}");
                    if (!deleteReaction.Success)
                    {
                        return deleteReaction;
                    }
                }

                var commentSnapshots = await ListCollectionDocumentsAsync($"teachingClasses/{normalizedClassId}/homePosts/{normalizedPostId}/comments");
                foreach (var snapshot in commentSnapshots)
                {
                    var deleteComment = await DeleteFirestoreDocumentAsync($"teachingClasses/{normalizedClassId}/homePosts/{normalizedPostId}/comments/{snapshot.DocumentId}");
                    if (!deleteComment.Success)
                    {
                        return deleteComment;
                    }
                }

                return await DeleteFirestoreDocumentAsync($"teachingClasses/{normalizedClassId}/homePosts/{normalizedPostId}");
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeachingClassService.DeleteHomePost] Erro: {ex.Message}");
                return TeachingClassOperationResult.Fail(ex.Message);
            }
        }

        public async Task<TeachingClassOperationResult> DeleteClassAsync(string classId)
        {
            try
            {
                var normalizedClassId = NormalizeClassCode(classId);
                if (string.IsNullOrWhiteSpace(normalizedClassId))
                {
                    return TeachingClassOperationResult.Fail("Turma docente inválida para remoção.");
                }

                var postSnapshots = await ListCollectionDocumentsAsync($"teachingClasses/{normalizedClassId}/homePosts");
                foreach (var snapshot in postSnapshots)
                {
                    var deletePostResult = await DeleteHomePostAsync(normalizedClassId, snapshot.DocumentId);
                    if (!deletePostResult.Success)
                    {
                        return deletePostResult;
                    }
                }

                var cleanupResult = await CleanupRemovedUserClassEnrollmentsAsync(normalizedClassId, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
                if (!cleanupResult.Success)
                {
                    return cleanupResult;
                }

                return await DeleteFirestoreDocumentAsync($"teachingClasses/{normalizedClassId}");
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeachingClassService.DeleteClass] Erro: {ex.Message}");
                return TeachingClassOperationResult.Fail(ex.Message);
            }
        }

        public async Task<TeachingClassAssetStorageResult> SaveTeachingClassAssetContentAsync(string classId, string assetId, TeachingClassPostAttachmentInfo attachment, byte[] fileBytes)
        {
            try
            {
                if (fileBytes == null || fileBytes.Length == 0)
                {
                    return TeachingClassAssetStorageResult.Fail("Arquivo vazio ou indisponível para sincronização remota.");
                }

                var normalizedClassId = NormalizeClassCode(classId);
                if (string.IsNullOrWhiteSpace(normalizedClassId))
                {
                    return TeachingClassAssetStorageResult.Fail("Turma docente inválida para upload do anexo.");
                }

                var safeAssetId = string.IsNullOrWhiteSpace(assetId) ? Guid.NewGuid().ToString("N") : assetId.Trim();
                var versionNumber = Math.Max(1, attachment.Version);
                var permissionScope = NormalizeTeachingClassPermissionScope(attachment.PermissionScope);
                var ownerUserId = string.IsNullOrWhiteSpace(attachment.AddedByUserId) ? _currentUserId : attachment.AddedByUserId;
                var objectPath = BuildTeachingClassAssetStorageObjectPath(normalizedClassId, permissionScope, ownerUserId, safeAssetId, versionNumber, attachment.FileName);

                var uploadResult = await UploadTeachingClassAssetToStorageAsync(objectPath, attachment.MimeType, fileBytes);
                if (!uploadResult.Success)
                {
                    return uploadResult;
                }

                return TeachingClassAssetStorageResult.Ok(objectPath, objectPath);
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeachingClassService.SaveTeachingClassAssetContent] Erro: {ex.Message}");
                return TeachingClassAssetStorageResult.Fail(ex.Message);
            }
        }

        public async Task<TeachingClassAssetStorageResult> SaveTeachingClassIconContentAsync(string classId, string fileName, string mimeType, byte[] fileBytes, int versionNumber = 1)
        {
            try
            {
                if (fileBytes == null || fileBytes.Length == 0)
                {
                    return TeachingClassAssetStorageResult.Fail("Imagem da turma vazia ou indisponível para upload.");
                }

                var normalizedClassId = NormalizeClassCode(classId);
                if (string.IsNullOrWhiteSpace(normalizedClassId))
                {
                    return TeachingClassAssetStorageResult.Fail("Turma docente inválida para upload da foto/ícone.");
                }

                var safeFileName = string.IsNullOrWhiteSpace(fileName) ? "turma.jpg" : fileName.Trim();
                var objectPath = BuildTeachingClassIconStorageObjectPath(normalizedClassId, _currentUserId, Guid.NewGuid().ToString("N"), Math.Max(1, versionNumber), safeFileName);
                var uploadResult = await UploadTeachingClassAssetToStorageAsync(objectPath, mimeType, fileBytes);
                if (!uploadResult.Success)
                {
                    return uploadResult;
                }

                return TeachingClassAssetStorageResult.Ok(objectPath, objectPath);
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeachingClassService.SaveTeachingClassIconContent] Erro: {ex.Message}");
                return TeachingClassAssetStorageResult.Fail(ex.Message);
            }
        }

        public async Task<TeachingClassAssetDownloadResult> LoadTeachingClassAssetContentAsync(string storageReference)
        {
            try
            {
                return await LoadTeachingClassAssetContentFromStorageAsync(storageReference);
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeachingClassService.LoadTeachingClassAssetContent] Erro: {ex.Message}");
                return TeachingClassAssetDownloadResult.Fail(ex.Message);
            }
        }

        public async Task<TeachingClassOperationResult> SaveClassAsync(TeachingClassInfo teachingClass)
        {
            try
            {
                var normalized = NormalizeClass(teachingClass);
                if (string.IsNullOrWhiteSpace(normalized.ClassName) || string.IsNullOrWhiteSpace(normalized.Course))
                {
                    return TeachingClassOperationResult.Fail("Informe nome da turma e curso antes de salvar.");
                }

                var requestBody = JsonSerializer.Serialize(new Dictionary<string, object?>
                {
                    ["fields"] = BuildTeachingClassFirestoreFields(normalized)
                });

                var request = new HttpRequestMessage(new HttpMethod("PATCH"), AppConfig.BuildFirestoreDocumentUrl($"teachingClasses/{normalized.ClassId}"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                var response = await httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    return TeachingClassOperationResult.Fail($"HTTP {(int)response.StatusCode}: {responseBody.Substring(0, Math.Min(responseBody.Length, 200))}");
                }

                var activeUsers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var professorId in normalized.ProfessorUserIds)
                {
                    if (string.IsNullOrWhiteSpace(professorId))
                    {
                        continue;
                    }

                    activeUsers.Add(professorId);
                    var professorRefResult = await SaveUserClassEnrollmentAsync(professorId, normalized.ClassId, normalized.ClassName, "professor");
                    if (!professorRefResult.Success)
                    {
                        return professorRefResult;
                    }
                }

                foreach (var studentId in normalized.StudentIds)
                {
                    if (string.IsNullOrWhiteSpace(studentId))
                    {
                        continue;
                    }

                    activeUsers.Add(studentId);
                    var studentRefResult = await SaveUserClassEnrollmentAsync(studentId, normalized.ClassId, normalized.ClassName, "student");
                    if (!studentRefResult.Success)
                    {
                        return studentRefResult;
                    }
                }

                var cleanupResult = await CleanupRemovedUserClassEnrollmentsAsync(normalized.ClassId, activeUsers);
                if (!cleanupResult.Success)
                {
                    return cleanupResult;
                }

                teachingClass.ClassId = normalized.ClassId;
                teachingClass.JoinCode = normalized.JoinCode;
                teachingClass.CreatedBy = normalized.CreatedBy;
                teachingClass.CreatedAt = normalized.CreatedAt;
                teachingClass.UpdatedAt = normalized.UpdatedAt;
                teachingClass.IconPreviewImageDataUri = normalized.IconPreviewImageDataUri;
                teachingClass.IconStorageReference = normalized.IconStorageReference;
                teachingClass.IconFileName = normalized.IconFileName;
                teachingClass.IconMimeType = normalized.IconMimeType;
                teachingClass.IconVersion = normalized.IconVersion;
                teachingClass.IconUpdatedAt = normalized.IconUpdatedAt;
                teachingClass.ProfessorUserIds = normalized.ProfessorUserIds;
                teachingClass.ProfessorNames = normalized.ProfessorNames;
                teachingClass.RepresentativeUserId = normalized.RepresentativeUserId;
                teachingClass.RepresentativeName = normalized.RepresentativeName;
                teachingClass.ViceRepresentativeUserId = normalized.ViceRepresentativeUserId;
                teachingClass.ViceRepresentativeName = normalized.ViceRepresentativeName;
                teachingClass.StudentIds = normalized.StudentIds;
                teachingClass.StudentSummaries = normalized.StudentSummaries;
                return TeachingClassOperationResult.Ok();
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeachingClassService.SaveClass] Erro: {ex.Message}");
                return TeachingClassOperationResult.Fail(ex.Message);
            }
        }

        public async Task<TeachingClassJoinResult> JoinClassByCodeAsync(string joinCode, UserInfo? currentUser)
        {
            try
            {
                var normalizedJoinCode = NormalizeJoinCode(joinCode);
                if (string.IsNullOrWhiteSpace(normalizedJoinCode))
                {
                    return TeachingClassJoinResult.Fail("Código da turma inválido.");
                }

                if (currentUser == null || string.IsNullOrWhiteSpace(currentUser.UserId))
                {
                    return TeachingClassJoinResult.Fail("Perfil do aluno indisponível para vincular a turma.");
                }

                var teachingClass = await FindClassByJoinCodeAsync(normalizedJoinCode);
                if (teachingClass == null)
                {
                    return TeachingClassJoinResult.Fail("Turma docente não encontrada para o código informado.");
                }

                if (!teachingClass.StudentIds.Contains(currentUser.UserId, StringComparer.OrdinalIgnoreCase))
                {
                    teachingClass.StudentIds.Add(currentUser.UserId);
                }

                teachingClass.StudentSummaries.RemoveAll(member => string.Equals(member.UserId, currentUser.UserId, StringComparison.OrdinalIgnoreCase));
                teachingClass.StudentSummaries.Add(new TeachingClassMemberInfo
                {
                    UserId = currentUser.UserId,
                    Name = currentUser.Name ?? string.Empty,
                    Email = currentUser.Email ?? string.Empty,
                    Registration = currentUser.Registration ?? string.Empty,
                    Role = "student",
                    JoinedAt = DateTime.Now
                });
                teachingClass.UpdatedAt = DateTime.Now;

                var saveResult = await SaveClassAsync(teachingClass);
                if (!saveResult.Success)
                {
                    return TeachingClassJoinResult.Fail(saveResult.ErrorMessage ?? "Não foi possível vincular a turma.");
                }

                return TeachingClassJoinResult.Ok(teachingClass);
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeachingClassService.JoinClassByCode] Erro: {ex.Message}");
                return TeachingClassJoinResult.Fail(ex.Message);
            }
        }

        public static string GenerateClassId(string course, string className)
        {
            var baseId = NormalizeIdPart($"{course}-{className}");
            if (string.IsNullOrWhiteSpace(baseId))
            {
                baseId = "turma-docente";
            }

            if (baseId.Length > 52)
            {
                baseId = baseId.Substring(0, 52);
            }

            return $"{baseId}-{Guid.NewGuid():N}".Substring(0, Math.Min(64, baseId.Length + 9));
        }

        public static string GenerateJoinCode()
        {
            var buffer = new char[8];
            for (var index = 0; index < buffer.Length; index++)
            {
                buffer[index] = JoinCodeAlphabet[Random.Shared.Next(JoinCodeAlphabet.Length)];
            }

            return new string(buffer);
        }

        public static string NormalizeJoinCode(string joinCode)
        {
            return (joinCode ?? string.Empty)
                .Trim()
                .Replace(" ", string.Empty)
                .ToUpperInvariant();
        }

        public static string NormalizeClassCode(string classCode)
        {
            return NormalizeIdPart(classCode);
        }

        private static string NormalizeIdPart(string? value)
        {
            var normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(normalized.Length);
            var lastWasSeparator = false;

            foreach (var character in normalized)
            {
                if (char.IsLetterOrDigit(character) || character == '.' || character == '(' || character == ')')
                {
                    builder.Append(character);
                    lastWasSeparator = false;
                    continue;
                }

                if (char.IsWhiteSpace(character) || character == '_' || character == '-' || character == '/' || character == '\\')
                {
                    if (!lastWasSeparator)
                    {
                        builder.Append('-');
                        lastWasSeparator = true;
                    }
                }
            }

            var sanitized = builder.ToString().Trim('-', '.');
            return sanitized == "." || sanitized == ".."
                ? string.Empty
                : sanitized;
        }

        private TeachingClassInfo NormalizeClass(TeachingClassInfo teachingClass)
        {
            var normalized = teachingClass ?? new TeachingClassInfo();
            normalized.ClassName = (normalized.ClassName ?? string.Empty).Trim();
            normalized.Course = (normalized.Course ?? string.Empty).Trim();
            normalized.AcademicTerm = string.IsNullOrWhiteSpace(normalized.AcademicTerm)
                ? $"{DateTime.Now.Year}.{(DateTime.Now.Month <= 6 ? 1 : 2)}"
                : normalized.AcademicTerm.Trim();
            normalized.Description = (normalized.Description ?? string.Empty).Trim();
            normalized.IconPreviewImageDataUri = (normalized.IconPreviewImageDataUri ?? string.Empty).Trim();
            normalized.IconStorageReference = (normalized.IconStorageReference ?? string.Empty).Trim();
            normalized.IconFileName = (normalized.IconFileName ?? string.Empty).Trim();
            normalized.IconMimeType = (normalized.IconMimeType ?? string.Empty).Trim();
            normalized.IconVersion = Math.Max(0, normalized.IconVersion);
            normalized.ClassId = string.IsNullOrWhiteSpace(normalized.ClassId)
                ? GenerateClassId(normalized.Course, normalized.ClassName)
                : NormalizeClassCode(normalized.ClassId);
            normalized.JoinCode = string.IsNullOrWhiteSpace(normalized.JoinCode)
                ? GenerateJoinCode()
                : NormalizeJoinCode(normalized.JoinCode);
            normalized.CreatedBy = string.IsNullOrWhiteSpace(normalized.CreatedBy) ? _currentUserId : normalized.CreatedBy;
            normalized.RepresentativeUserId = (normalized.RepresentativeUserId ?? string.Empty).Trim();
            normalized.RepresentativeName = (normalized.RepresentativeName ?? string.Empty).Trim();
            normalized.ViceRepresentativeUserId = (normalized.ViceRepresentativeUserId ?? string.Empty).Trim();
            normalized.ViceRepresentativeName = (normalized.ViceRepresentativeName ?? string.Empty).Trim();
            normalized.CreatedAt = normalized.CreatedAt == default ? DateTime.Now : normalized.CreatedAt;
            normalized.UpdatedAt = DateTime.Now;

            if (string.IsNullOrWhiteSpace(normalized.IconPreviewImageDataUri) || string.IsNullOrWhiteSpace(normalized.IconStorageReference))
            {
                normalized.IconPreviewImageDataUri = string.Empty;
                normalized.IconStorageReference = string.Empty;
                normalized.IconFileName = string.Empty;
                normalized.IconMimeType = string.Empty;
                normalized.IconVersion = 0;
                normalized.IconUpdatedAt = null;
            }
            else
            {
                normalized.IconVersion = Math.Max(1, normalized.IconVersion);
                normalized.IconUpdatedAt ??= DateTime.Now;
            }

            normalized.ProfessorUserIds = (normalized.ProfessorUserIds ?? new List<string>())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Select(item => item.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            normalized.ProfessorNames = (normalized.ProfessorNames ?? new List<string>())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Select(item => item.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            normalized.StudentIds = (normalized.StudentIds ?? new List<string>())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Select(item => item.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            normalized.StudentSummaries = (normalized.StudentSummaries ?? new List<TeachingClassMemberInfo>())
                .Where(item => item != null && !string.IsNullOrWhiteSpace(item.UserId))
                .GroupBy(item => item.UserId, StringComparer.OrdinalIgnoreCase)
                .Select(group =>
                {
                    var first = group.First();
                    first.Name = (first.Name ?? string.Empty).Trim();
                    first.Email = (first.Email ?? string.Empty).Trim();
                    first.Registration = (first.Registration ?? string.Empty).Trim();
                    first.Role = NormalizeTeachingClassMemberRole(first.Role);
                    first.JoinedAt = first.JoinedAt == default ? DateTime.Now : first.JoinedAt;
                    return first;
                })
                .OrderBy(item => item.Name)
                .ToList();
            normalized.HomePosts ??= new List<TeachingClassHomePostInfo>();

            foreach (var student in normalized.StudentSummaries)
            {
                if (!normalized.StudentIds.Contains(student.UserId, StringComparer.OrdinalIgnoreCase))
                {
                    normalized.StudentIds.Add(student.UserId);
                }
            }

            if (!string.IsNullOrWhiteSpace(normalized.RepresentativeUserId)
                && string.Equals(normalized.RepresentativeUserId, normalized.ViceRepresentativeUserId, StringComparison.OrdinalIgnoreCase))
            {
                normalized.ViceRepresentativeUserId = string.Empty;
                normalized.ViceRepresentativeName = string.Empty;
            }

            EnsureTeachingClassStudentSummary(normalized, normalized.RepresentativeUserId, normalized.RepresentativeName, "leader");
            EnsureTeachingClassStudentSummary(normalized, normalized.ViceRepresentativeUserId, normalized.ViceRepresentativeName, "vice");

            foreach (var student in normalized.StudentSummaries)
            {
                if (string.Equals(student.UserId, normalized.RepresentativeUserId, StringComparison.OrdinalIgnoreCase))
                {
                    student.Role = "leader";
                    if (string.IsNullOrWhiteSpace(normalized.RepresentativeName))
                    {
                        normalized.RepresentativeName = student.Name;
                    }
                }
                else if (string.Equals(student.UserId, normalized.ViceRepresentativeUserId, StringComparison.OrdinalIgnoreCase))
                {
                    student.Role = "vice";
                    if (string.IsNullOrWhiteSpace(normalized.ViceRepresentativeName))
                    {
                        normalized.ViceRepresentativeName = student.Name;
                    }
                }
                else
                {
                    student.Role = "student";
                }
            }

            if (!normalized.StudentSummaries.Any(student => string.Equals(student.UserId, normalized.RepresentativeUserId, StringComparison.OrdinalIgnoreCase)))
            {
                normalized.RepresentativeUserId = string.Empty;
                normalized.RepresentativeName = string.Empty;
            }

            if (!normalized.StudentSummaries.Any(student => string.Equals(student.UserId, normalized.ViceRepresentativeUserId, StringComparison.OrdinalIgnoreCase)))
            {
                normalized.ViceRepresentativeUserId = string.Empty;
                normalized.ViceRepresentativeName = string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(normalized.CreatedBy)
                && !normalized.ProfessorUserIds.Contains(normalized.CreatedBy, StringComparer.OrdinalIgnoreCase))
            {
                normalized.ProfessorUserIds.Add(normalized.CreatedBy);
            }

            if (TeamPermissionService.IsFacultyRole(_currentUserRole) && !string.IsNullOrWhiteSpace(_currentUserId))
            {
                if (!normalized.ProfessorUserIds.Contains(_currentUserId, StringComparer.OrdinalIgnoreCase))
                {
                    normalized.ProfessorUserIds.Add(_currentUserId);
                }
            }

            return normalized;
        }

        private TeachingClassHomePostInfo NormalizeHomePost(TeachingClassHomePostInfo post)
        {
            var normalized = post ?? new TeachingClassHomePostInfo();
            normalized.PostId = string.IsNullOrWhiteSpace(normalized.PostId) ? Guid.NewGuid().ToString("N") : NormalizeDocumentId(normalized.PostId);
            normalized.PostType = NormalizeHomePostType(normalized.PostType);
            normalized.AuthorUserId = string.IsNullOrWhiteSpace(normalized.AuthorUserId) ? _currentUserId : normalized.AuthorUserId.Trim();
            normalized.AuthorName = (normalized.AuthorName ?? string.Empty).Trim();
            normalized.Title = (normalized.Title ?? string.Empty).Trim();
            normalized.Content = (normalized.Content ?? string.Empty).Trim();
            normalized.LinkUrl = NormalizeOptionalLink(normalized.LinkUrl);
            normalized.ActivityLabel = (normalized.ActivityLabel ?? string.Empty).Trim();
            normalized.PublishedAt = normalized.PublishedAt == default ? DateTime.Now : normalized.PublishedAt;
            normalized.UpdatedAt = DateTime.Now;
            normalized.Attachments = (normalized.Attachments ?? new List<TeachingClassPostAttachmentInfo>())
                .Where(item => item != null)
                .Select(item =>
                {
                    item.AttachmentId = string.IsNullOrWhiteSpace(item.AttachmentId) ? Guid.NewGuid().ToString("N") : NormalizeDocumentId(item.AttachmentId);
                    item.FileName = (item.FileName ?? string.Empty).Trim();
                    item.PermissionScope = NormalizeTeachingClassPermissionScope(item.PermissionScope);
                    item.StorageKind = string.IsNullOrWhiteSpace(item.StorageKind) ? "firebase-storage" : item.StorageKind.Trim();
                    item.StorageReference = (item.StorageReference ?? string.Empty).Trim();
                    item.MimeType = (item.MimeType ?? string.Empty).Trim();
                    item.PreviewImageDataUri = (item.PreviewImageDataUri ?? string.Empty).Trim();
                    item.Version = Math.Max(1, item.Version);
                    item.AddedByUserId = string.IsNullOrWhiteSpace(item.AddedByUserId) ? _currentUserId : item.AddedByUserId.Trim();
                    item.AddedAt = item.AddedAt == default ? DateTime.Now : item.AddedAt;
                    return item;
                })
                .GroupBy(item => item.AttachmentId, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();
            normalized.Comments ??= new List<TeachingClassPostCommentInfo>();
            normalized.Reactions ??= new List<TeachingClassPostReactionInfo>();
            return normalized;
        }

        private Dictionary<string, object?> BuildTeachingClassFirestoreFields(TeachingClassInfo teachingClass)
        {
            var fields = new Dictionary<string, object?>
            {
                ["classId"] = new Dictionary<string, object?> { ["stringValue"] = teachingClass.ClassId },
                ["className"] = new Dictionary<string, object?> { ["stringValue"] = teachingClass.ClassName },
                ["course"] = new Dictionary<string, object?> { ["stringValue"] = teachingClass.Course },
                ["academicTerm"] = new Dictionary<string, object?> { ["stringValue"] = teachingClass.AcademicTerm },
                ["description"] = new Dictionary<string, object?> { ["stringValue"] = teachingClass.Description ?? string.Empty },
                ["iconPreviewImageDataUri"] = new Dictionary<string, object?> { ["stringValue"] = teachingClass.IconPreviewImageDataUri ?? string.Empty },
                ["iconStorageReference"] = new Dictionary<string, object?> { ["stringValue"] = teachingClass.IconStorageReference ?? string.Empty },
                ["iconFileName"] = new Dictionary<string, object?> { ["stringValue"] = teachingClass.IconFileName ?? string.Empty },
                ["iconMimeType"] = new Dictionary<string, object?> { ["stringValue"] = teachingClass.IconMimeType ?? string.Empty },
                ["iconVersion"] = new Dictionary<string, object?> { ["integerValue"] = Math.Max(0, teachingClass.IconVersion).ToString() },
                ["joinCode"] = new Dictionary<string, object?> { ["stringValue"] = teachingClass.JoinCode },
                ["createdBy"] = new Dictionary<string, object?> { ["stringValue"] = teachingClass.CreatedBy },
                ["createdAt"] = new Dictionary<string, object?> { ["timestampValue"] = ToFirestoreTimestamp(teachingClass.CreatedAt) },
                ["updatedAt"] = new Dictionary<string, object?> { ["timestampValue"] = ToFirestoreTimestamp(teachingClass.UpdatedAt) },
                ["professorUserIds"] = new Dictionary<string, object?>
                {
                    ["arrayValue"] = new Dictionary<string, object?>
                    {
                        ["values"] = ConvertStringsToFirestoreArray(teachingClass.ProfessorUserIds)
                    }
                },
                ["professorNames"] = new Dictionary<string, object?>
                {
                    ["arrayValue"] = new Dictionary<string, object?>
                    {
                        ["values"] = ConvertStringsToFirestoreArray(teachingClass.ProfessorNames)
                    }
                },
                ["representativeUserId"] = new Dictionary<string, object?> { ["stringValue"] = teachingClass.RepresentativeUserId ?? string.Empty },
                ["representativeName"] = new Dictionary<string, object?> { ["stringValue"] = teachingClass.RepresentativeName ?? string.Empty },
                ["viceRepresentativeUserId"] = new Dictionary<string, object?> { ["stringValue"] = teachingClass.ViceRepresentativeUserId ?? string.Empty },
                ["viceRepresentativeName"] = new Dictionary<string, object?> { ["stringValue"] = teachingClass.ViceRepresentativeName ?? string.Empty },
                ["studentIds"] = new Dictionary<string, object?>
                {
                    ["arrayValue"] = new Dictionary<string, object?>
                    {
                        ["values"] = ConvertStringsToFirestoreArray(teachingClass.StudentIds)
                    }
                },
                ["studentSummaries"] = new Dictionary<string, object?>
                {
                    ["arrayValue"] = new Dictionary<string, object?>
                    {
                        ["values"] = ConvertMembersToFirestoreArray(teachingClass.StudentSummaries)
                    }
                }
            };

            if (teachingClass.IconUpdatedAt.HasValue)
            {
                fields["iconUpdatedAt"] = new Dictionary<string, object?> { ["timestampValue"] = ToFirestoreTimestamp(teachingClass.IconUpdatedAt.Value) };
            }

            return fields;
        }

        private TeachingClassPostCommentInfo NormalizeHomeComment(TeachingClassPostCommentInfo comment)
        {
            var normalized = comment ?? new TeachingClassPostCommentInfo();
            normalized.CommentId = string.IsNullOrWhiteSpace(normalized.CommentId) ? Guid.NewGuid().ToString("N") : NormalizeDocumentId(normalized.CommentId);
            normalized.ParentCommentId = string.IsNullOrWhiteSpace(normalized.ParentCommentId) ? string.Empty : NormalizeDocumentId(normalized.ParentCommentId);
            normalized.AuthorUserId = string.IsNullOrWhiteSpace(normalized.AuthorUserId) ? _currentUserId : normalized.AuthorUserId.Trim();
            normalized.AuthorName = (normalized.AuthorName ?? string.Empty).Trim();
            normalized.Content = (normalized.Content ?? string.Empty).Trim();
            normalized.CreatedAt = normalized.CreatedAt == default ? DateTime.Now : normalized.CreatedAt;
            return normalized;
        }

        private TeachingClassPostReactionInfo NormalizeHomeReaction(TeachingClassPostReactionInfo reaction)
        {
            var normalized = reaction ?? new TeachingClassPostReactionInfo();
            normalized.UserId = string.IsNullOrWhiteSpace(normalized.UserId) ? _currentUserId : normalized.UserId.Trim();
            normalized.UserName = (normalized.UserName ?? string.Empty).Trim();
            normalized.Emoji = (normalized.Emoji ?? string.Empty).Trim();
            normalized.CreatedAt = normalized.CreatedAt == default ? DateTime.Now : normalized.CreatedAt;
            normalized.UpdatedAt = DateTime.Now;
            return normalized;
        }

        private async Task<List<string>> GetUserClassIdsAsync(string userId)
        {
            var classIds = new List<string>();

            var requestBody = JsonSerializer.Serialize(new
            {
                structuredQuery = new
                {
                    select = new
                    {
                        fields = new[] { new { fieldPath = "classId" } }
                    },
                    from = new[] { new { collectionId = "userClassEnrollments" } },
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

            var request = new HttpRequestMessage(HttpMethod.Post, AppConfig.BuildFirestoreRunQueryUrl());
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            var response = await httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                DebugHelper.WriteLine($"[TeachingClassService.GetUserClassIds] HTTP {(int)response.StatusCode}: {responseBody}");
                return classIds;
            }

            using var doc = JsonDocument.Parse(responseBody);
            foreach (var result in doc.RootElement.EnumerateArray())
            {
                if (!result.TryGetProperty("document", out var document) ||
                    !document.TryGetProperty("fields", out var fields))
                {
                    continue;
                }

                var classId = GetString(fields, "classId");
                if (!string.IsNullOrWhiteSpace(classId))
                {
                    classIds.Add(classId);
                }
            }

            return classIds;
        }

        private async Task<TeachingClassInfo?> FindClassByJoinCodeAsync(string joinCode)
        {
            var requestBody = JsonSerializer.Serialize(new
            {
                structuredQuery = new
                {
                    from = new[] { new { collectionId = "teachingClasses" } },
                    where = new
                    {
                        fieldFilter = new
                        {
                            field = new { fieldPath = "joinCode" },
                            op = "EQUAL",
                            value = new { stringValue = joinCode }
                        }
                    },
                    limit = 1
                }
            });

            var request = new HttpRequestMessage(HttpMethod.Post, AppConfig.BuildFirestoreRunQueryUrl());
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            var response = await httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                DebugHelper.WriteLine($"[TeachingClassService.FindClassByJoinCode] HTTP {(int)response.StatusCode}: {responseBody}");
                return null;
            }

            using var doc = JsonDocument.Parse(responseBody);
            foreach (var result in doc.RootElement.EnumerateArray())
            {
                if (result.TryGetProperty("document", out var document) &&
                    document.TryGetProperty("fields", out var fields))
                {
                    return ParseClassFromFirestore(fields);
                }
            }

            return null;
        }

        private async Task<TeachingClassOperationResult> SaveUserClassEnrollmentAsync(string userId, string classId, string className, string role)
        {
            var documentId = $"{userId}_{classId}";
            var requestBody = JsonSerializer.Serialize(new
            {
                fields = new
                {
                    userId = new { stringValue = userId },
                    classId = new { stringValue = classId },
                    className = new { stringValue = className },
                    role = new { stringValue = TeamPermissionService.NormalizeRole(role) },
                    joinedAt = new { timestampValue = ToFirestoreTimestamp(DateTime.Now) }
                }
            });

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), AppConfig.BuildFirestoreDocumentUrl($"userClassEnrollments/{documentId}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            var response = await httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                return TeachingClassOperationResult.Fail($"HTTP {(int)response.StatusCode}: {responseBody.Substring(0, Math.Min(responseBody.Length, 200))}");
            }

            return TeachingClassOperationResult.Ok();
        }

        private async Task<TeachingClassOperationResult> CleanupRemovedUserClassEnrollmentsAsync(string classId, HashSet<string> activeUserIds)
        {
            var existing = await GetClassEnrollmentDocumentsAsync(classId);
            foreach (var snapshot in existing)
            {
                if (activeUserIds.Contains(snapshot.UserId))
                {
                    continue;
                }

                var deleteResult = await DeleteClassEnrollmentDocumentAsync(snapshot.DocumentId);
                if (!deleteResult.Success)
                {
                    return deleteResult;
                }
            }

            return TeachingClassOperationResult.Ok();
        }

        private async Task<List<UserClassEnrollmentSnapshot>> GetClassEnrollmentDocumentsAsync(string classId)
        {
            var result = new List<UserClassEnrollmentSnapshot>();

            var requestBody = JsonSerializer.Serialize(new
            {
                structuredQuery = new
                {
                    from = new[] { new { collectionId = "userClassEnrollments" } },
                    where = new
                    {
                        fieldFilter = new
                        {
                            field = new { fieldPath = "classId" },
                            op = "EQUAL",
                            value = new { stringValue = classId }
                        }
                    }
                }
            });

            var request = new HttpRequestMessage(HttpMethod.Post, AppConfig.BuildFirestoreRunQueryUrl());
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            var response = await httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                DebugHelper.WriteLine($"[TeachingClassService.GetClassEnrollments] HTTP {(int)response.StatusCode}: {responseBody}");
                return result;
            }

            using var doc = JsonDocument.Parse(responseBody);
            foreach (var item in doc.RootElement.EnumerateArray())
            {
                if (!item.TryGetProperty("document", out var document) ||
                    !document.TryGetProperty("fields", out var fields))
                {
                    continue;
                }

                result.Add(new UserClassEnrollmentSnapshot
                {
                    DocumentId = GetDocumentId(document),
                    UserId = GetString(fields, "userId")
                });
            }

            return result;
        }

        private async Task<TeachingClassOperationResult> DeleteClassEnrollmentDocumentAsync(string documentId)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, AppConfig.BuildFirestoreDocumentUrl($"userClassEnrollments/{documentId}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);

            var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return TeachingClassOperationResult.Ok();
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            return TeachingClassOperationResult.Fail($"HTTP {(int)response.StatusCode}: {responseBody.Substring(0, Math.Min(responseBody.Length, 200))}");
        }

        private async Task<TeachingClassOperationResult> DeleteFirestoreDocumentAsync(string relativeDocumentPath)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, AppConfig.BuildFirestoreDocumentUrl(relativeDocumentPath));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);

            var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return TeachingClassOperationResult.Ok();
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            return TeachingClassOperationResult.Fail($"HTTP {(int)response.StatusCode}: {responseBody.Substring(0, Math.Min(responseBody.Length, 200))}");
        }

        private async Task<List<TeachingClassDocumentSnapshot>> ListCollectionDocumentsAsync(string relativeCollectionPath)
        {
            var result = new List<TeachingClassDocumentSnapshot>();
            var request = new HttpRequestMessage(HttpMethod.Get, AppConfig.BuildFirestoreDocumentUrl(relativeCollectionPath));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);

            var response = await httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return result;
            }

            if (!response.IsSuccessStatusCode)
            {
                DebugHelper.WriteLine($"[TeachingClassService.ListCollectionDocuments] HTTP {(int)response.StatusCode}: {responseBody}");
                return result;
            }

            using var doc = JsonDocument.Parse(responseBody);
            if (!doc.RootElement.TryGetProperty("documents", out var documents))
            {
                return result;
            }

            foreach (var document in documents.EnumerateArray())
            {
                if (!document.TryGetProperty("fields", out var fields))
                {
                    continue;
                }

                result.Add(new TeachingClassDocumentSnapshot
                {
                    DocumentId = GetDocumentId(document),
                    Fields = fields.Clone()
                });
            }

            return result;
        }

        private async Task<List<TeachingClassPostCommentInfo>> LoadHomeCommentsAsync(string classId, string postId)
        {
            var snapshots = await ListCollectionDocumentsAsync($"teachingClasses/{classId}/homePosts/{postId}/comments");
            return snapshots
                .Select(snapshot => ParseHomeCommentFromFirestore(snapshot.DocumentId, snapshot.Fields))
                .Where(comment => comment != null)
                .Cast<TeachingClassPostCommentInfo>()
                .OrderBy(comment => comment.CreatedAt)
                .ToList();
        }

        private async Task<List<TeachingClassPostReactionInfo>> LoadHomeReactionsAsync(string classId, string postId)
        {
            var snapshots = await ListCollectionDocumentsAsync($"teachingClasses/{classId}/homePosts/{postId}/reactions");
            return snapshots
                .Select(snapshot => ParseHomeReactionFromFirestore(snapshot.DocumentId, snapshot.Fields))
                .Where(reaction => reaction != null)
                .Cast<TeachingClassPostReactionInfo>()
                .OrderBy(reaction => reaction.CreatedAt)
                .ToList();
        }

        private TeachingClassInfo? ParseClassFromFirestore(JsonElement fields)
        {
            try
            {
                return new TeachingClassInfo
                {
                    ClassId = GetString(fields, "classId"),
                    ClassName = GetString(fields, "className"),
                    Course = GetString(fields, "course"),
                    AcademicTerm = GetString(fields, "academicTerm"),
                    Description = GetString(fields, "description"),
                    IconPreviewImageDataUri = GetString(fields, "iconPreviewImageDataUri"),
                    IconStorageReference = GetString(fields, "iconStorageReference"),
                    IconFileName = GetString(fields, "iconFileName"),
                    IconMimeType = GetString(fields, "iconMimeType"),
                    IconVersion = GetInt(fields, "iconVersion"),
                    IconUpdatedAt = GetOptionalTimestamp(fields, "iconUpdatedAt"),
                    JoinCode = GetString(fields, "joinCode"),
                    CreatedBy = GetString(fields, "createdBy"),
                    CreatedAt = GetTimestamp(fields, "createdAt"),
                    UpdatedAt = GetTimestamp(fields, "updatedAt"),
                    ProfessorUserIds = GetStringArray(fields, "professorUserIds"),
                    ProfessorNames = GetStringArray(fields, "professorNames"),
                    RepresentativeUserId = GetString(fields, "representativeUserId"),
                    RepresentativeName = GetString(fields, "representativeName"),
                    ViceRepresentativeUserId = GetString(fields, "viceRepresentativeUserId"),
                    ViceRepresentativeName = GetString(fields, "viceRepresentativeName"),
                    StudentIds = GetStringArray(fields, "studentIds"),
                    StudentSummaries = GetMemberArray(fields, "studentSummaries"),
                    HomePosts = new List<TeachingClassHomePostInfo>(),
                    HomeFeedReady = false
                };
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeachingClassService.ParseClass] Erro: {ex.Message}");
                return null;
            }
        }

        private TeachingClassHomePostInfo? ParseHomePostFromFirestore(string documentId, JsonElement fields)
        {
            try
            {
                return new TeachingClassHomePostInfo
                {
                    PostId = string.IsNullOrWhiteSpace(GetString(fields, "postId")) ? documentId : GetString(fields, "postId"),
                    PostType = NormalizeHomePostType(GetString(fields, "postType")),
                    AuthorUserId = GetString(fields, "authorUserId"),
                    AuthorName = GetString(fields, "authorName"),
                    Title = GetString(fields, "title"),
                    Content = GetString(fields, "content"),
                    LinkUrl = GetString(fields, "linkUrl"),
                    ActivityLabel = GetString(fields, "activityLabel"),
                    ActivityDueAt = GetOptionalTimestamp(fields, "activityDueAt"),
                    PublishedAt = GetTimestamp(fields, "publishedAt"),
                    UpdatedAt = GetTimestamp(fields, "updatedAt"),
                    Attachments = GetPostAttachmentArray(fields, "attachments"),
                    Comments = new List<TeachingClassPostCommentInfo>(),
                    Reactions = new List<TeachingClassPostReactionInfo>()
                };
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeachingClassService.ParseHomePost] Erro: {ex.Message}");
                return null;
            }
        }

        private TeachingClassPostCommentInfo? ParseHomeCommentFromFirestore(string documentId, JsonElement fields)
        {
            try
            {
                return new TeachingClassPostCommentInfo
                {
                    CommentId = string.IsNullOrWhiteSpace(GetString(fields, "commentId")) ? documentId : GetString(fields, "commentId"),
                    ParentCommentId = GetString(fields, "parentCommentId"),
                    AuthorUserId = GetString(fields, "authorUserId"),
                    AuthorName = GetString(fields, "authorName"),
                    Content = GetString(fields, "content"),
                    CreatedAt = GetTimestamp(fields, "createdAt")
                };
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeachingClassService.ParseHomeComment] Erro: {ex.Message}");
                return null;
            }
        }

        private TeachingClassPostReactionInfo? ParseHomeReactionFromFirestore(string documentId, JsonElement fields)
        {
            try
            {
                return new TeachingClassPostReactionInfo
                {
                    UserId = string.IsNullOrWhiteSpace(GetString(fields, "userId")) ? documentId : GetString(fields, "userId"),
                    UserName = GetString(fields, "userName"),
                    Emoji = GetString(fields, "emoji"),
                    CreatedAt = GetTimestamp(fields, "createdAt"),
                    UpdatedAt = GetTimestamp(fields, "updatedAt")
                };
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeachingClassService.ParseHomeReaction] Erro: {ex.Message}");
                return null;
            }
        }

        private static string ToFirestoreTimestamp(DateTime value)
        {
            return value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        }

        private object[] ConvertStringsToFirestoreArray(IEnumerable<string> items)
        {
            return (items ?? Enumerable.Empty<string>())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Select(item => new { stringValue = item })
                .Cast<object>()
                .ToArray();
        }

        private object[] ConvertMembersToFirestoreArray(IEnumerable<TeachingClassMemberInfo> members)
        {
            return (members ?? Enumerable.Empty<TeachingClassMemberInfo>())
                .Where(member => member != null && !string.IsNullOrWhiteSpace(member.UserId))
                .Select(member => new
                {
                    mapValue = new
                    {
                        fields = new
                        {
                            userId = new { stringValue = member.UserId ?? string.Empty },
                            name = new { stringValue = member.Name ?? string.Empty },
                            email = new { stringValue = member.Email ?? string.Empty },
                            registration = new { stringValue = member.Registration ?? string.Empty },
                            role = new { stringValue = NormalizeTeachingClassMemberRole(member.Role) },
                            joinedAt = new { timestampValue = ToFirestoreTimestamp(member.JoinedAt == default ? DateTime.Now : member.JoinedAt) }
                        }
                    }
                })
                .Cast<object>()
                .ToArray();
        }

        private object[] ConvertAttachmentsToFirestoreArray(IEnumerable<TeachingClassPostAttachmentInfo> attachments)
        {
            return (attachments ?? Enumerable.Empty<TeachingClassPostAttachmentInfo>())
                .Where(attachment => attachment != null)
                .Select(attachment => new
                {
                    mapValue = new
                    {
                        fields = new
                        {
                            attachmentId = new { stringValue = attachment.AttachmentId ?? Guid.NewGuid().ToString("N") },
                            fileName = new { stringValue = attachment.FileName ?? string.Empty },
                            previewImageDataUri = new { stringValue = attachment.PreviewImageDataUri ?? string.Empty },
                            permissionScope = new { stringValue = NormalizeTeachingClassPermissionScope(attachment.PermissionScope) },
                            storageKind = new { stringValue = string.IsNullOrWhiteSpace(attachment.StorageKind) ? "firebase-storage" : attachment.StorageKind },
                            storageReference = new { stringValue = attachment.StorageReference ?? string.Empty },
                            mimeType = new { stringValue = attachment.MimeType ?? string.Empty },
                            sizeBytes = new { integerValue = Math.Max(0, attachment.SizeBytes).ToString() },
                            version = new { integerValue = Math.Max(1, attachment.Version).ToString() },
                            addedByUserId = new { stringValue = attachment.AddedByUserId ?? string.Empty },
                            addedAt = new { timestampValue = ToFirestoreTimestamp(attachment.AddedAt == default ? DateTime.Now : attachment.AddedAt) }
                        }
                    }
                })
                .Cast<object>()
                .ToArray();
        }

        private Dictionary<string, object?> BuildHomePostFirestoreFields(TeachingClassHomePostInfo post)
        {
            var fields = new Dictionary<string, object?>
            {
                ["postId"] = new Dictionary<string, object?> { ["stringValue"] = post.PostId },
                ["postType"] = new Dictionary<string, object?> { ["stringValue"] = NormalizeHomePostType(post.PostType) },
                ["authorUserId"] = new Dictionary<string, object?> { ["stringValue"] = post.AuthorUserId ?? string.Empty },
                ["authorName"] = new Dictionary<string, object?> { ["stringValue"] = post.AuthorName ?? string.Empty },
                ["title"] = new Dictionary<string, object?> { ["stringValue"] = post.Title ?? string.Empty },
                ["content"] = new Dictionary<string, object?> { ["stringValue"] = post.Content ?? string.Empty },
                ["linkUrl"] = new Dictionary<string, object?> { ["stringValue"] = post.LinkUrl ?? string.Empty },
                ["activityLabel"] = new Dictionary<string, object?> { ["stringValue"] = post.ActivityLabel ?? string.Empty },
                ["publishedAt"] = new Dictionary<string, object?> { ["timestampValue"] = ToFirestoreTimestamp(post.PublishedAt == default ? DateTime.Now : post.PublishedAt) },
                ["updatedAt"] = new Dictionary<string, object?> { ["timestampValue"] = ToFirestoreTimestamp(post.UpdatedAt == default ? DateTime.Now : post.UpdatedAt) },
                ["attachments"] = new Dictionary<string, object?>
                {
                    ["arrayValue"] = new Dictionary<string, object?>
                    {
                        ["values"] = ConvertAttachmentsToFirestoreArray(post.Attachments)
                    }
                }
            };

            if (post.ActivityDueAt.HasValue)
            {
                fields["activityDueAt"] = new Dictionary<string, object?> { ["timestampValue"] = ToFirestoreTimestamp(post.ActivityDueAt.Value) };
            }

            return fields;
        }

        private Dictionary<string, object?> BuildHomeCommentFirestoreFields(TeachingClassPostCommentInfo comment)
        {
            return new Dictionary<string, object?>
            {
                ["commentId"] = new Dictionary<string, object?> { ["stringValue"] = comment.CommentId },
                ["parentCommentId"] = new Dictionary<string, object?> { ["stringValue"] = comment.ParentCommentId ?? string.Empty },
                ["authorUserId"] = new Dictionary<string, object?> { ["stringValue"] = comment.AuthorUserId ?? string.Empty },
                ["authorName"] = new Dictionary<string, object?> { ["stringValue"] = comment.AuthorName ?? string.Empty },
                ["content"] = new Dictionary<string, object?> { ["stringValue"] = comment.Content ?? string.Empty },
                ["createdAt"] = new Dictionary<string, object?> { ["timestampValue"] = ToFirestoreTimestamp(comment.CreatedAt == default ? DateTime.Now : comment.CreatedAt) }
            };
        }

        private Dictionary<string, object?> BuildHomeReactionFirestoreFields(TeachingClassPostReactionInfo reaction)
        {
            return new Dictionary<string, object?>
            {
                ["userId"] = new Dictionary<string, object?> { ["stringValue"] = reaction.UserId ?? string.Empty },
                ["userName"] = new Dictionary<string, object?> { ["stringValue"] = reaction.UserName ?? string.Empty },
                ["emoji"] = new Dictionary<string, object?> { ["stringValue"] = reaction.Emoji ?? string.Empty },
                ["createdAt"] = new Dictionary<string, object?> { ["timestampValue"] = ToFirestoreTimestamp(reaction.CreatedAt == default ? DateTime.Now : reaction.CreatedAt) },
                ["updatedAt"] = new Dictionary<string, object?> { ["timestampValue"] = ToFirestoreTimestamp(reaction.UpdatedAt == default ? DateTime.Now : reaction.UpdatedAt) }
            };
        }

        private List<string> GetStringArray(JsonElement fields, string fieldName)
        {
            var result = new List<string>();
            if (!fields.TryGetProperty(fieldName, out var field) ||
                !field.TryGetProperty("arrayValue", out var arrayValue) ||
                !arrayValue.TryGetProperty("values", out var values))
            {
                return result;
            }

            foreach (var value in values.EnumerateArray())
            {
                if (value.TryGetProperty("stringValue", out var stringValue))
                {
                    var item = stringValue.GetString();
                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        result.Add(item);
                    }
                }
            }

            return result;
        }

        private List<TeachingClassMemberInfo> GetMemberArray(JsonElement fields, string fieldName)
        {
            var result = new List<TeachingClassMemberInfo>();
            if (!fields.TryGetProperty(fieldName, out var field) ||
                !field.TryGetProperty("arrayValue", out var arrayValue) ||
                !arrayValue.TryGetProperty("values", out var values))
            {
                return result;
            }

            foreach (var value in values.EnumerateArray())
            {
                if (!value.TryGetProperty("mapValue", out var mapValue) ||
                    !mapValue.TryGetProperty("fields", out var memberFields))
                {
                    continue;
                }

                result.Add(new TeachingClassMemberInfo
                {
                    UserId = GetString(memberFields, "userId"),
                    Name = GetString(memberFields, "name"),
                    Email = GetString(memberFields, "email"),
                    Registration = GetString(memberFields, "registration"),
                    Role = GetString(memberFields, "role"),
                    JoinedAt = GetTimestamp(memberFields, "joinedAt")
                });
            }

            return result;
        }

        private List<TeachingClassPostAttachmentInfo> GetPostAttachmentArray(JsonElement fields, string fieldName)
        {
            var result = new List<TeachingClassPostAttachmentInfo>();
            if (!fields.TryGetProperty(fieldName, out var field) ||
                !field.TryGetProperty("arrayValue", out var arrayValue) ||
                !arrayValue.TryGetProperty("values", out var values))
            {
                return result;
            }

            foreach (var value in values.EnumerateArray())
            {
                if (!value.TryGetProperty("mapValue", out var mapValue) ||
                    !mapValue.TryGetProperty("fields", out var attachmentFields))
                {
                    continue;
                }

                result.Add(new TeachingClassPostAttachmentInfo
                {
                    AttachmentId = GetString(attachmentFields, "attachmentId"),
                    FileName = GetString(attachmentFields, "fileName"),
                    PreviewImageDataUri = GetString(attachmentFields, "previewImageDataUri"),
                    PermissionScope = NormalizeTeachingClassPermissionScope(GetString(attachmentFields, "permissionScope")),
                    StorageKind = GetString(attachmentFields, "storageKind"),
                    StorageReference = GetString(attachmentFields, "storageReference"),
                    MimeType = GetString(attachmentFields, "mimeType"),
                    SizeBytes = GetLong(attachmentFields, "sizeBytes"),
                    Version = Math.Max(1, GetInt(attachmentFields, "version")),
                    AddedByUserId = GetString(attachmentFields, "addedByUserId"),
                    AddedAt = GetTimestamp(attachmentFields, "addedAt")
                });
            }

            return result;
        }

        private string GetString(JsonElement fields, string fieldName)
        {
            if (fields.TryGetProperty(fieldName, out var field) &&
                field.TryGetProperty("stringValue", out var stringValue))
            {
                return stringValue.GetString() ?? string.Empty;
            }

            return string.Empty;
        }

        private DateTime GetTimestamp(JsonElement fields, string fieldName)
        {
            if (fields.TryGetProperty(fieldName, out var field) &&
                field.TryGetProperty("timestampValue", out var timestampValue) &&
                DateTime.TryParse(timestampValue.GetString(), out var parsed))
            {
                return parsed.ToLocalTime();
            }

            return DateTime.Now;
        }

        private DateTime? GetOptionalTimestamp(JsonElement fields, string fieldName)
        {
            if (fields.TryGetProperty(fieldName, out var field) &&
                field.TryGetProperty("timestampValue", out var timestampValue) &&
                DateTime.TryParse(timestampValue.GetString(), out var parsed))
            {
                return parsed.ToLocalTime();
            }

            return null;
        }

        private int GetInt(JsonElement fields, string fieldName)
        {
            if (fields.TryGetProperty(fieldName, out var field) &&
                field.TryGetProperty("integerValue", out var integerValue) &&
                int.TryParse(integerValue.GetString(), out var parsed))
            {
                return parsed;
            }

            return 0;
        }

        private long GetLong(JsonElement fields, string fieldName)
        {
            if (fields.TryGetProperty(fieldName, out var field) &&
                field.TryGetProperty("integerValue", out var integerValue) &&
                long.TryParse(integerValue.GetString(), out var parsed))
            {
                return parsed;
            }

            return 0;
        }

        private string GetDocumentId(JsonElement document)
        {
            if (document.TryGetProperty("name", out var nameField))
            {
                return nameField.GetString()?.Split('/').LastOrDefault() ?? string.Empty;
            }

            return string.Empty;
        }

        private static string NormalizeHomePostType(string? postType)
        {
            var normalized = (postType ?? string.Empty).Trim().ToLowerInvariant();
            return normalized == "activity" ? "activity" : "announcement";
        }

        private static string NormalizeTeachingClassPermissionScope(string? scope)
        {
            var normalized = (scope ?? string.Empty).Trim().ToLowerInvariant();
            return normalized switch
            {
                "course" => "course",
                "staff" => "staff",
                "private" => "private",
                _ => "class"
            };
        }


                    private static string NormalizeTeachingClassMemberRole(string? role)
                    {
                        var normalized = (role ?? string.Empty).Trim().ToLowerInvariant();
                        return normalized switch
                        {
                            "leader" => "leader",
                            "lider" => "leader",
                            "representative" => "leader",
                            "representante" => "leader",
                            "vice" => "vice",
                            "vice-representative" => "vice",
                            "vice_representative" => "vice",
                            "vice-representante" => "vice",
                            "vice_representante" => "vice",
                            "professor" => "professor",
                            _ => TeamPermissionService.IsFacultyRole(normalized) ? "professor" : "student"
                        };
                    }

                    private static void EnsureTeachingClassStudentSummary(TeachingClassInfo teachingClass, string userId, string userName, string role)
                    {
                        if (teachingClass == null || string.IsNullOrWhiteSpace(userId))
                        {
                            return;
                        }

                        if (!teachingClass.StudentIds.Contains(userId, StringComparer.OrdinalIgnoreCase))
                        {
                            teachingClass.StudentIds.Add(userId);
                        }

                        var existing = teachingClass.StudentSummaries.FirstOrDefault(member => string.Equals(member.UserId, userId, StringComparison.OrdinalIgnoreCase));
                        if (existing == null)
                        {
                            teachingClass.StudentSummaries.Add(new TeachingClassMemberInfo
                            {
                                UserId = userId,
                                Name = userName ?? string.Empty,
                                Role = role,
                                JoinedAt = DateTime.Now
                            });
                            return;
                        }

                        if (string.IsNullOrWhiteSpace(existing.Name) && !string.IsNullOrWhiteSpace(userName))
                        {
                            existing.Name = userName;
                        }

                        existing.Role = role;
                    }
        private static string NormalizeOptionalLink(string? url)
        {
            var normalized = (url ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return string.Empty;
            }

            if (normalized.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                normalized.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return normalized;
            }

            return normalized.Contains(' ') ? string.Empty : $"https://{normalized}";
        }

        private static string NormalizeDocumentId(string? value)
        {
            var normalized = (value ?? string.Empty).Trim().Replace("/", string.Empty).Replace("\\", string.Empty);
            return string.IsNullOrWhiteSpace(normalized) ? Guid.NewGuid().ToString("N") : normalized;
        }

        private string BuildTeachingClassAssetStorageObjectPath(string classId, string permissionScope, string ownerUserId, string assetId, int versionNumber, string? fileName)
        {
            var normalizedClassId = SanitizeStorageSegment(classId, "class");
            var normalizedScope = NormalizeTeachingClassPermissionScope(permissionScope);
            var normalizedOwner = SanitizeStorageSegment(string.IsNullOrWhiteSpace(ownerUserId) ? _currentUserId : ownerUserId, "user");
            var normalizedAssetId = SanitizeStorageSegment(assetId, "asset");
            var normalizedFileName = SanitizeStorageFileName(fileName);
            return $"teaching-class-assets/{normalizedClassId}/{normalizedScope}/{normalizedOwner}/{normalizedAssetId}/v{versionNumber:D4}/{normalizedFileName}";
        }

        private string BuildTeachingClassIconStorageObjectPath(string classId, string ownerUserId, string assetId, int versionNumber, string? fileName)
        {
            var normalizedClassId = SanitizeStorageSegment(classId, "class");
            var normalizedOwner = SanitizeStorageSegment(string.IsNullOrWhiteSpace(ownerUserId) ? _currentUserId : ownerUserId, "user");
            var normalizedAssetId = SanitizeStorageSegment(assetId, "asset");
            var normalizedFileName = SanitizeStorageFileName(fileName);
            return $"teaching-class-icons/{normalizedClassId}/{normalizedOwner}/{normalizedAssetId}/v{versionNumber:D4}/{normalizedFileName}";
        }

        private static string SanitizeStorageSegment(string? value, string fallbackSegment = "unknown")
        {
            var normalized = (value ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return fallbackSegment;
            }

            var builder = new StringBuilder(normalized.Length);
            foreach (var character in normalized)
            {
                if (character == '/' || character == '\\' || char.IsControl(character))
                {
                    builder.Append('-');
                }
                else
                {
                    builder.Append(character);
                }
            }

            var sanitized = builder.ToString().Trim();
            return string.IsNullOrWhiteSpace(sanitized)
                ? fallbackSegment
                : sanitized;
        }

        private static string SanitizeStorageFileName(string? fileName)
        {
            var candidate = string.IsNullOrWhiteSpace(fileName) ? "arquivo.bin" : fileName.Trim();
            var invalidChars = System.IO.Path.GetInvalidFileNameChars();
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

        private async Task<TeachingClassAssetStorageResult> UploadTeachingClassAssetToStorageAsync(string objectPath, string? mimeType, byte[] fileBytes)
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
                    return TeachingClassAssetStorageResult.Ok(objectPath, objectPath);
                }

                lastErrorMessage = $"HTTP {(int)response.StatusCode}: {responseBody.Substring(0, Math.Min(responseBody.Length, 200))}";
                if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
                {
                    DebugHelper.WriteLine($"[TeachingClassService.UploadTeachingClassAssetToStorage] Erro: {lastErrorMessage}");
                    return TeachingClassAssetStorageResult.Fail(lastErrorMessage);
                }
            }

            DebugHelper.WriteLine($"[TeachingClassService.UploadTeachingClassAssetToStorage] Erro: {lastErrorMessage}");
            return TeachingClassAssetStorageResult.Fail(lastErrorMessage ?? "HTTP 404: bucket do Firebase Storage não encontrado.");
        }

        private async Task<TeachingClassAssetDownloadResult> LoadTeachingClassAssetContentFromStorageAsync(string storageReference)
        {
            var normalizedReference = (storageReference ?? string.Empty).Trim().Trim('/');
            if (string.IsNullOrWhiteSpace(normalizedReference))
            {
                return TeachingClassAssetDownloadResult.Fail("Referência remota do arquivo está vazia.");
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
                    DebugHelper.WriteLine($"[TeachingClassService.LoadTeachingClassAssetContentFromStorage] Erro: {errorMessage}");
                    return TeachingClassAssetDownloadResult.Fail(errorMessage);
                }
            }

            if (response == null || !response.IsSuccessStatusCode)
            {
                DebugHelper.WriteLine($"[TeachingClassService.LoadTeachingClassAssetContentFromStorage] Erro: {errorMessage}");
                return TeachingClassAssetDownloadResult.Fail(errorMessage ?? "HTTP 404: bucket do Firebase Storage não encontrado.");
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();
            var segments = normalizedReference.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var versionSegment = segments.Length >= 2 ? segments[^2] : "v0001";
            var fileName = segments.Length >= 1 ? segments[^1] : "arquivo.bin";
            var versionNumber = versionSegment.StartsWith("v", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(versionSegment.Substring(1), out var parsedVersion)
                    ? parsedVersion
                    : 1;
            var classId = segments.Length >= 6 ? segments[1] : string.Empty;
            var permissionScope = segments.Length >= 6 ? segments[2] : "class";
            var ownerUserId = segments.Length >= 6 ? segments[3] : string.Empty;
            var assetId = segments.Length >= 6 ? segments[4] : string.Empty;

            return TeachingClassAssetDownloadResult.Ok(new TeachingClassAssetContentPayload
            {
                ClassId = classId,
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

        private sealed class UserClassEnrollmentSnapshot
        {
            public string DocumentId { get; init; } = string.Empty;
            public string UserId { get; init; } = string.Empty;
        }

        private sealed class TeachingClassDocumentSnapshot
        {
            public string DocumentId { get; init; } = string.Empty;
            public JsonElement Fields { get; init; }
        }
    }

    public class TeachingClassHomePostResult
    {
        public bool Success { get; set; }
        public TeachingClassHomePostInfo? Post { get; set; }
        public string? ErrorMessage { get; set; }

        public static TeachingClassHomePostResult Ok(TeachingClassHomePostInfo post)
        {
            return new TeachingClassHomePostResult { Success = true, Post = post };
        }

        public static TeachingClassHomePostResult Fail(string errorMessage)
        {
            return new TeachingClassHomePostResult { Success = false, ErrorMessage = errorMessage };
        }
    }

    public class TeachingClassAssetStorageResult
    {
        public bool Success { get; set; }
        public string StorageReference { get; set; } = string.Empty;
        public string DocumentId { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }

        public static TeachingClassAssetStorageResult Ok(string storageReference, string documentId)
        {
            return new TeachingClassAssetStorageResult
            {
                Success = true,
                StorageReference = storageReference,
                DocumentId = documentId
            };
        }

        public static TeachingClassAssetStorageResult Fail(string errorMessage)
        {
            return new TeachingClassAssetStorageResult { Success = false, ErrorMessage = errorMessage };
        }
    }

    public class TeachingClassAssetDownloadResult
    {
        public bool Success { get; set; }
        public TeachingClassAssetContentPayload? Payload { get; set; }
        public string? ErrorMessage { get; set; }

        public static TeachingClassAssetDownloadResult Ok(TeachingClassAssetContentPayload payload)
        {
            return new TeachingClassAssetDownloadResult { Success = true, Payload = payload };
        }

        public static TeachingClassAssetDownloadResult Fail(string errorMessage)
        {
            return new TeachingClassAssetDownloadResult { Success = false, ErrorMessage = errorMessage };
        }
    }

    public class TeachingClassAssetContentPayload
    {
        public string ClassId { get; set; } = string.Empty;
        public string AssetId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public string PermissionScope { get; set; } = "class";
        public string StorageKind { get; set; } = "firebase-storage";
        public string StorageReference { get; set; } = string.Empty;
        public int VersionNumber { get; set; } = 1;
        public long SizeBytes { get; set; }
        public string UploadedByUserId { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public byte[] Bytes { get; set; } = Array.Empty<byte>();
    }

    public class TeachingClassOperationResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }

        public static TeachingClassOperationResult Ok()
        {
            return new TeachingClassOperationResult { Success = true };
        }

        public static TeachingClassOperationResult Fail(string errorMessage)
        {
            return new TeachingClassOperationResult { Success = false, ErrorMessage = errorMessage };
        }
    }

    public class TeachingClassJoinResult
    {
        public bool Success { get; set; }
        public TeachingClassInfo? TeachingClass { get; set; }
        public string? ErrorMessage { get; set; }

        public static TeachingClassJoinResult Ok(TeachingClassInfo teachingClass)
        {
            return new TeachingClassJoinResult { Success = true, TeachingClass = teachingClass };
        }

        public static TeachingClassJoinResult Fail(string errorMessage)
        {
            return new TeachingClassJoinResult { Success = false, ErrorMessage = errorMessage };
        }
    }
}