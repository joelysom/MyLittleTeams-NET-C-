using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MeuApp
{
    public class UserInfo : INotifyPropertyChanged
    {
        public string UserId { get; set; } = "";
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Registration { get; set; } = "";
        public string Course { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Nickname { get; set; } = "";
        public string ProfessionalTitle { get; set; } = "";
        public string AcademicDepartment { get; set; } = "";
        public string AcademicFocus { get; set; } = "";
        public string OfficeHours { get; set; } = "";
        public string Bio { get; set; } = "";
        public string Skills { get; set; } = "";
        public string ProgrammingLanguages { get; set; } = "";
        public string PortfolioLink { get; set; } = "";
        public string LinkedInLink { get; set; } = "";
        public string ProfilePhotoDataUri { get; set; } = "";
        public string? Role { get; set; } = "student";
        public string AvatarBody { get; set; } = "";
        public string AvatarHair { get; set; } = "";
        public string AvatarHat { get; set; } = "";
        public string AvatarAccessory { get; set; } = "";
        public string AvatarClothing { get; set; } = "";
        public List<TeamWorkspaceInfo> AcademicProjects { get; set; } = new List<TeamWorkspaceInfo>();
        private bool _isConnecting;
        private bool _isCurrentUser;
        private string _connectionState = "none";

        public event PropertyChangedEventHandler? PropertyChanged;

        public string DisplayLabel
        {
            get
            {
                var registration = string.IsNullOrWhiteSpace(Registration) ? "Sem matricula" : Registration;
                var email = string.IsNullOrWhiteSpace(Email) ? "Sem email" : Email;
                var roleLabel = TeamPermissionService.GetRoleLabel(Role);
                return $"{Name} | {roleLabel} | {registration} | {email}";
            }
        }

        public string ConnectionState
        {
            get => _connectionState;
            set
            {
                var normalized = string.IsNullOrWhiteSpace(value) ? "none" : value.Trim();
                if (string.Equals(_connectionState, normalized, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                _connectionState = normalized;
                RaiseConnectionUiPropertiesChanged();
            }
        }

        public bool IsConnecting
        {
            get => _isConnecting;
            set
            {
                if (_isConnecting == value)
                {
                    return;
                }

                _isConnecting = value;
                RaiseConnectionUiPropertiesChanged();
            }
        }

        public bool IsCurrentUser
        {
            get => _isCurrentUser;
            set
            {
                if (_isCurrentUser == value)
                {
                    return;
                }

                _isCurrentUser = value;
                RaiseConnectionUiPropertiesChanged();
            }
        }

        public string ConnectionButtonLabel => IsCurrentUser
            ? "Seu perfil"
            : IsConnecting
                ? "Enviando..."
                : ConnectionState switch
                {
                    "connected" => "Conectado",
                    "pendingOutgoing" => "Pendente",
                    "pendingIncoming" => "Responder na aba",
                    _ => "Conectar"
                };

        public bool CanCreateConnection => !IsCurrentUser
            && !IsConnecting
            && string.Equals(ConnectionState, "none", StringComparison.OrdinalIgnoreCase);

        private void RaiseConnectionUiPropertiesChanged()
        {
            OnPropertyChanged(nameof(ConnectionState));
            OnPropertyChanged(nameof(IsConnecting));
            OnPropertyChanged(nameof(IsCurrentUser));
            OnPropertyChanged(nameof(ConnectionButtonLabel));
            OnPropertyChanged(nameof(CanCreateConnection));
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class UserSearchService
    {
        private readonly string _idToken;
        private static readonly HttpClient httpClient = new HttpClient();
        private static readonly object UserDirectoryCacheLock = new object();
        private static readonly TimeSpan UserDirectoryCacheTtl = TimeSpan.FromMinutes(3);
        private static DateTime _cachedUserDirectoryAt = DateTime.MinValue;
        private static List<UserInfo> _cachedUserDirectory = new List<UserInfo>();
        private static Task<List<UserInfo>>? _cachedUserDirectoryTask;

        public UserSearchService(string idToken)
        {
            _idToken = idToken;
            Debug.WriteLine($"[UserSearchService] Inicializado com token: {(string.IsNullOrEmpty(idToken) ? "VAZIO!" : "OK")}");
        }

        /// <summary>
        /// Busca usuários por nome, matrícula ou email
        /// </summary>
        public async Task<List<UserInfo>> SearchUsersAsync(string query)
        {
            Debug.WriteLine($"[SearchUsersAsync] Iniciando busca por: '{query}'");
            
            if (string.IsNullOrWhiteSpace(query))
            {
                Debug.WriteLine("[SearchUsersAsync] Query vazia, retornando lista vazia");
                return new List<UserInfo>();
            }

            var results = new List<UserInfo>();
            var queryLower = query.ToLower().Trim();
            Debug.WriteLine($"[SearchUsersAsync] Query normalizada: '{queryLower}'");

            try
            {
                if (string.IsNullOrWhiteSpace(_idToken))
                {
                    Debug.WriteLine("[SearchUsersAsync] ERRO: Token está vazio!");
                    return results;
                }

                // Buscar todos os usuários
                results = await SearchAllUsersAsync(queryLower);
                Debug.WriteLine($"[SearchUsersAsync] Total de resultados encontrados: {results.Count}");
                
                return results;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SearchUsersAsync] Exceção capturada: {ex.GetType().Name}: {ex.Message}");
                Debug.WriteLine($"[SearchUsersAsync] Stack trace: {ex.StackTrace}");
                return results;
            }
        }

        /// <summary>
        /// Busca todos os usuários e filtra localmente
        /// </summary>
        private async Task<List<UserInfo>> SearchAllUsersAsync(string query)
        {
            var results = new List<UserInfo>();
            try
            {
                var directory = await LoadUserDirectoryAsync();
                results = directory
                    .Where(user =>
                        user.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        user.Email.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        user.Registration.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        user.Course.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        user.ProfessionalTitle.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        user.AcademicDepartment.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        user.AcademicFocus.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        TeamPermissionService.GetRoleLabel(user.Role).Contains(query, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(user => user.Name)
                    .Take(40)
                    .ToList();

                Debug.WriteLine($"[SearchAllUsersAsync] Retornando {results.Count} resultados filtrados em cache local.");
                return results;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SearchAllUsersAsync] Exceção: {ex.GetType().Name}: {ex.Message}");
                Debug.WriteLine($"[SearchAllUsersAsync] Stack: {ex.StackTrace}");
                return results;
            }
        }

        private Task<List<UserInfo>> LoadUserDirectoryAsync(bool forceRefresh = false)
        {
            lock (UserDirectoryCacheLock)
            {
                if (!forceRefresh &&
                    _cachedUserDirectory.Count > 0 &&
                    DateTime.UtcNow - _cachedUserDirectoryAt < UserDirectoryCacheTtl)
                {
                    Debug.WriteLine("[UserDirectoryCache] Reutilizando diretório de usuários em cache.");
                    return Task.FromResult(_cachedUserDirectory.ToList());
                }

                if (!forceRefresh && _cachedUserDirectoryTask != null)
                {
                    Debug.WriteLine("[UserDirectoryCache] Requisição em andamento; reutilizando task atual.");
                    return _cachedUserDirectoryTask;
                }

                _cachedUserDirectoryTask = FetchUserDirectoryAsync();
                return _cachedUserDirectoryTask;
            }
        }

        private async Task<List<UserInfo>> FetchUserDirectoryAsync()
        {
            var endpoint = AppConfig.BuildFirestoreDocumentUrl("users");
            var users = new List<UserInfo>();

            Debug.WriteLine($"[UserDirectoryCache] Atualizando diretório de usuários via {endpoint}");

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);

                var response = await httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[UserDirectoryCache] HTTP {(int)response.StatusCode}: {content}");
                    return users;
                }

                using var doc = JsonDocument.Parse(content);
                if (!doc.RootElement.TryGetProperty("documents", out var documents))
                {
                    return users;
                }

                var documentIndex = 0;
                foreach (var userDoc in documents.EnumerateArray())
                {
                    documentIndex++;
                    var user = ExtractUserInfo(userDoc, documentIndex);
                    if (user != null && !string.IsNullOrWhiteSpace(user.UserId))
                    {
                        users.Add(user);
                    }
                }

                lock (UserDirectoryCacheLock)
                {
                    _cachedUserDirectory = users
                        .GroupBy(user => user.UserId, StringComparer.OrdinalIgnoreCase)
                        .Select(group => group.First())
                        .OrderBy(user => user.Name)
                        .ToList();
                    _cachedUserDirectoryAt = DateTime.UtcNow;
                    _cachedUserDirectoryTask = null;
                    return _cachedUserDirectory.ToList();
                }
            }
            catch
            {
                lock (UserDirectoryCacheLock)
                {
                    _cachedUserDirectoryTask = null;
                }

                throw;
            }
        }

        /// <summary>
        /// Extrai informações do usuário de um documento Firestore com logging detalhado
        /// </summary>
        private UserInfo? ExtractUserInfo(JsonElement userDoc, int documentIndex)
        {
            var user = new UserInfo();

            try
            {
                // Extrair ID do documento
                if (userDoc.TryGetProperty("name", out var nameField))
                {
                    var nameValue = nameField.GetString();
                    user.UserId = nameValue?.Split('/').LastOrDefault() ?? "";
                    Debug.WriteLine($"  [ExtractUserInfo #{documentIndex}] UserId extraído: {user.UserId}");
                }
                else
                {
                    Debug.WriteLine($"  [ExtractUserInfo #{documentIndex}] Campo 'name' não encontrado");
                    return null;
                }

                if (userDoc.TryGetProperty("fields", out var fields))
                {
                    Debug.WriteLine($"  [ExtractUserInfo #{documentIndex}] Campos encontrados");

                    // Extrair nome - tentar múltiplos formatos
                    user.Name = ExtractField(fields, "name", documentIndex) ?? "";
                    if (!string.IsNullOrEmpty(user.Name))
                        Debug.WriteLine($"  [ExtractUserInfo #{documentIndex}] Nome: {user.Name}");

                    // Extrair email - tentar múltiplos formatos
                    user.Email = ExtractField(fields, "email", documentIndex) ?? "";
                    if (!string.IsNullOrEmpty(user.Email))
                        Debug.WriteLine($"  [ExtractUserInfo #{documentIndex}] Email: {user.Email}");

                    // Extrair matrícula - tentar múltiplos formatos
                    user.Registration = ExtractField(fields, "registration", documentIndex) ?? "";
                    if (!string.IsNullOrEmpty(user.Registration))
                        Debug.WriteLine($"  [ExtractUserInfo #{documentIndex}] Matrícula: {user.Registration}");

                    // Extrair curso - tentar múltiplos formatos
                    user.Course = ExtractField(fields, "course", documentIndex) ?? "";
                    if (!string.IsNullOrEmpty(user.Course))
                        Debug.WriteLine($"  [ExtractUserInfo #{documentIndex}] Curso: {user.Course}");

                    user.Role = TeamPermissionService.NormalizeRole(ExtractField(fields, "role", documentIndex));
                    user.ProfessionalTitle = ExtractField(fields, "professionalTitle", documentIndex) ?? "";
                    user.AcademicDepartment = ExtractField(fields, "academicDepartment", documentIndex) ?? "";
                    user.AcademicFocus = ExtractField(fields, "academicFocus", documentIndex) ?? "";
                    user.OfficeHours = ExtractField(fields, "officeHours", documentIndex) ?? "";

                    user.ProfilePhotoDataUri = ExtractField(fields, "profilePhotoDataUri", documentIndex) ?? "";
                    user.AvatarBody = ExtractField(fields, "avatarBody", documentIndex) ?? "";
                    user.AvatarHair = ExtractField(fields, "avatarHair", documentIndex) ?? "";
                    user.AvatarHat = ExtractField(fields, "avatarHat", documentIndex) ?? "";
                    user.AvatarAccessory = ExtractField(fields, "avatarAccessory", documentIndex) ?? "";
                    user.AvatarClothing = ExtractField(fields, "avatarClothing", documentIndex) ?? "";

                    // Listar campos disponíveis se houver campos não esperados
                    var availableFields = fields.EnumerateObject().Select(p => p.Name).ToList();
                    if (availableFields.Count > 0)
                    {
                        Debug.WriteLine($"  [ExtractUserInfo #{documentIndex}] Campos disponíveis: {string.Join(", ", availableFields)}");
                    }
                }
                else
                {
                    Debug.WriteLine($"  [ExtractUserInfo #{documentIndex}] Campo 'fields' não encontrado");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"  [ExtractUserInfo #{documentIndex}] Exceção: {ex.Message}");
                return null;
            }

            return user;
        }

        /// <summary>
        /// Extrai um campo com suporte a múltiplos formatos (stringValue, doubleValue, etc)
        /// </summary>
        private string? ExtractField(JsonElement fields, string fieldName, int documentIndex)
        {
            try
            {
                if (!fields.TryGetProperty(fieldName, out var field))
                {
                    Debug.WriteLine($"    Campo '{fieldName}' não existe");
                    return null;
                }

                // Formato Firestore padrão: { stringValue: "value" }
                if (field.TryGetProperty("stringValue", out var stringVal))
                {
                    var result = stringVal.GetString();
                    Debug.WriteLine($"    Campo '{fieldName}' (stringValue): {result}");
                    return result;
                }

                // Formato alternativo: valor direto
                if (field.ValueKind == JsonValueKind.String)
                {
                    var result = field.GetString();
                    Debug.WriteLine($"    Campo '{fieldName}' (String direto): {result}");
                    return result;
                }

                // Formato com número
                if (field.TryGetProperty("integerValue", out var intVal))
                {
                    var result = intVal.GetString();
                    Debug.WriteLine($"    Campo '{fieldName}' (integerValue): {result}");
                    return result;
                }

                // Formato com double
                if (field.TryGetProperty("doubleValue", out var doubleVal))
                {
                    var result = doubleVal.ToString();
                    Debug.WriteLine($"    Campo '{fieldName}' (doubleValue): {result}");
                    return result;
                }

                // Se nada funcionou, logar a estrutura real
                Debug.WriteLine($"    Campo '{fieldName}' existe mas em formato desconhecido: {field.ValueKind}");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"    Erro ao extrair campo '{fieldName}': {ex.Message}");
                return null;
            }
        }
    }
}
