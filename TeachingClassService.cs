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

        public async Task<TeachingClassOperationResult> SaveClassAsync(TeachingClassInfo teachingClass)
        {
            try
            {
                var normalized = NormalizeClass(teachingClass);
                if (string.IsNullOrWhiteSpace(normalized.ClassName) || string.IsNullOrWhiteSpace(normalized.Course))
                {
                    return TeachingClassOperationResult.Fail("Informe nome da turma e curso antes de salvar.");
                }

                var requestBody = JsonSerializer.Serialize(new
                {
                    fields = new
                    {
                        classId = new { stringValue = normalized.ClassId },
                        className = new { stringValue = normalized.ClassName },
                        course = new { stringValue = normalized.Course },
                        academicTerm = new { stringValue = normalized.AcademicTerm },
                        description = new { stringValue = normalized.Description ?? string.Empty },
                        joinCode = new { stringValue = normalized.JoinCode },
                        createdBy = new { stringValue = normalized.CreatedBy },
                        createdAt = new { timestampValue = ToFirestoreTimestamp(normalized.CreatedAt) },
                        updatedAt = new { timestampValue = ToFirestoreTimestamp(normalized.UpdatedAt) },
                        professorUserIds = new { arrayValue = new { values = ConvertStringsToFirestoreArray(normalized.ProfessorUserIds) } },
                        professorNames = new { arrayValue = new { values = ConvertStringsToFirestoreArray(normalized.ProfessorNames) } },
                        studentIds = new { arrayValue = new { values = ConvertStringsToFirestoreArray(normalized.StudentIds) } },
                        studentSummaries = new { arrayValue = new { values = ConvertMembersToFirestoreArray(normalized.StudentSummaries) } }
                    }
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
                teachingClass.ProfessorUserIds = normalized.ProfessorUserIds;
                teachingClass.ProfessorNames = normalized.ProfessorNames;
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
            normalized.ClassId = string.IsNullOrWhiteSpace(normalized.ClassId)
                ? GenerateClassId(normalized.Course, normalized.ClassName)
                : NormalizeClassCode(normalized.ClassId);
            normalized.JoinCode = string.IsNullOrWhiteSpace(normalized.JoinCode)
                ? GenerateJoinCode()
                : NormalizeJoinCode(normalized.JoinCode);
            normalized.CreatedBy = string.IsNullOrWhiteSpace(normalized.CreatedBy) ? _currentUserId : normalized.CreatedBy;
            normalized.CreatedAt = normalized.CreatedAt == default ? DateTime.Now : normalized.CreatedAt;
            normalized.UpdatedAt = DateTime.Now;

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
                    first.Role = "student";
                    first.JoinedAt = first.JoinedAt == default ? DateTime.Now : first.JoinedAt;
                    return first;
                })
                .OrderBy(item => item.Name)
                .ToList();

            foreach (var student in normalized.StudentSummaries)
            {
                if (!normalized.StudentIds.Contains(student.UserId, StringComparer.OrdinalIgnoreCase))
                {
                    normalized.StudentIds.Add(student.UserId);
                }
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
                    JoinCode = GetString(fields, "joinCode"),
                    CreatedBy = GetString(fields, "createdBy"),
                    CreatedAt = GetTimestamp(fields, "createdAt"),
                    UpdatedAt = GetTimestamp(fields, "updatedAt"),
                    ProfessorUserIds = GetStringArray(fields, "professorUserIds"),
                    ProfessorNames = GetStringArray(fields, "professorNames"),
                    StudentIds = GetStringArray(fields, "studentIds"),
                    StudentSummaries = GetMemberArray(fields, "studentSummaries")
                };
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeachingClassService.ParseClass] Erro: {ex.Message}");
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
                            role = new { stringValue = TeamPermissionService.NormalizeRole(member.Role) },
                            joinedAt = new { timestampValue = ToFirestoreTimestamp(member.JoinedAt == default ? DateTime.Now : member.JoinedAt) }
                        }
                    }
                })
                .Cast<object>()
                .ToArray();
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

        private string GetDocumentId(JsonElement document)
        {
            if (document.TryGetProperty("name", out var nameField))
            {
                return nameField.GetString()?.Split('/').LastOrDefault() ?? string.Empty;
            }

            return string.Empty;
        }

        private sealed class UserClassEnrollmentSnapshot
        {
            public string DocumentId { get; init; } = string.Empty;
            public string UserId { get; init; } = string.Empty;
        }
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