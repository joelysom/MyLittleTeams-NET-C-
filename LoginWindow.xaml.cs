using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls;

namespace MeuApp
{
    public partial class LoginWindow : MetroWindow
    {
        private const string FirebaseApiKey = "AIzaSyA2V4MEzgOoKEEZAAXH49DXbzxUo0_CuWU";
        private const string FirebaseProjectId = "obsseractpi";
        private static readonly HttpClient httpClient = new HttpClient();

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void SwitchTab_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string tag = button.Tag?.ToString() ?? "";

                if (tag == "Login")
                {
                    LoginContent.Visibility = Visibility.Visible;
                    SignupContent.Visibility = Visibility.Collapsed;
                    LoginTabButton.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 120, 212));
                    LoginTabButton.FontWeight = FontWeights.ExtraBold;
                    SignupTabButton.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(153, 153, 153));
                    SignupTabButton.FontWeight = FontWeights.Normal;
                }
                else if (tag == "Signup")
                {
                    LoginContent.Visibility = Visibility.Collapsed;
                    SignupContent.Visibility = Visibility.Visible;
                    SignupTabButton.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 120, 212));
                    SignupTabButton.FontWeight = FontWeights.ExtraBold;
                    LoginTabButton.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(153, 153, 153));
                    LoginTabButton.FontWeight = FontWeights.Normal;
                }
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string email = LoginEmail.Text;
            string password = LoginPassword.Password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Por favor, preencha todos os campos!", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                IsEnabled = false;

                var loginResult = await FirebaseSignInAsync(email, password);
                if (!loginResult.Success)
                {
                    MessageBox.Show($"Falha no login: {loginResult.ErrorMessage}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var bootstrapProfile = CreateBootstrapProfile(loginResult, email);
                var mainWindow = new MainWindow(bootstrapProfile, loginResult.IdToken ?? string.Empty);
                mainWindow.Show();
                this.Close();

                _ = HydrateProfileAfterLoginAsync(mainWindow, loginResult.LocalId!, loginResult.IdToken!, bootstrapProfile);
            }
            finally
            {
                Mouse.OverrideCursor = null;
                IsEnabled = true;
            }
        }

        private static UserProfile CreateBootstrapProfile(AuthResult loginResult, string email)
        {
            var inferredName = email;
            var atIndex = inferredName.IndexOf('@');
            if (atIndex > 0)
            {
                inferredName = inferredName[..atIndex];
            }

            inferredName = inferredName.Replace('.', ' ').Replace('_', ' ').Trim();
            if (string.IsNullOrWhiteSpace(inferredName))
            {
                inferredName = "Usuário";
            }

            return new UserProfile
            {
                UserId = loginResult.LocalId ?? string.Empty,
                Name = inferredName,
                Email = loginResult.Email ?? email,
                Phone = string.Empty,
                Course = string.Empty,
                Registration = string.Empty
            };
        }

        private async Task HydrateProfileAfterLoginAsync(MainWindow mainWindow, string localId, string idToken, UserProfile fallbackProfile)
        {
            try
            {
                var profile = await GetUserProfileAsync(localId, idToken) ?? fallbackProfile;

                await mainWindow.Dispatcher.InvokeAsync(() => mainWindow.UpdateUserProfile(profile));
            }
            catch
            {
                await mainWindow.Dispatcher.InvokeAsync(() => mainWindow.UpdateUserProfile(fallbackProfile));
            }
        }

        private async void SignupButton_Click(object sender, RoutedEventArgs e)
        {
            string name = SignupName.Text;
            string email = SignupEmail.Text;
            string phone = SignupPhone.Text;
            string course = SignupCourse.Text;
            string registration = SignupRegistration.Text;
            string password = SignupPassword.Password;
            string passwordConfirm = SignupPasswordConfirm.Password;

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordConfirm))
            {
                MessageBox.Show("Por favor, preencha todos os campos!", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!email.Contains("@"))
            {
                MessageBox.Show("Email inválido!", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (password.Length < 6)
            {
                MessageBox.Show("Senha deve ter pelo menos 6 caracteres!", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (password != passwordConfirm)
            {
                MessageBox.Show("As senhas não coincidem!", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                IsEnabled = false;

                var signupResult = await FirebaseSignUpAsync(email, password);
                if (!signupResult.Success)
                {
                    MessageBox.Show($"Falha no cadastro: {signupResult.ErrorMessage}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var saveResult = await SaveUserProfileAsync(signupResult.LocalId!, signupResult.IdToken!, name, email, phone, course, registration);
                if (!saveResult.Success)
                {
                    MessageBox.Show($"Não foi possível salvar o perfil: {saveResult.ErrorMessage}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var profile = new UserProfile
                {
                    UserId = signupResult.LocalId!, Name = name, Email = email, Phone = phone, Course = course, Registration = registration
                };

                var mainWindow = new MainWindow(profile, signupResult.IdToken ?? string.Empty);
                mainWindow.Show();
                this.Close();
            }
            finally
            {
                Mouse.OverrideCursor = null;
                IsEnabled = true;
            }
        }

        private async Task<AuthResult> FirebaseSignInAsync(string email, string password)
        {
            var endpoint = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={FirebaseApiKey}";
            var body = new
            {
                email,
                password,
                returnSecureToken = true
            };

            var payload = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var resp = await httpClient.PostAsync(endpoint, payload);
            var content = await resp.Content.ReadAsStringAsync();

            if (resp.IsSuccessStatusCode)
            {
                var json = JsonDocument.Parse(content).RootElement;
                return new AuthResult
                {
                    Success = true,
                    IdToken = json.GetProperty("idToken").GetString(),
                    LocalId = json.GetProperty("localId").GetString(),
                    Email = json.GetProperty("email").GetString()
                };
            }

            try
            {
                var json = JsonDocument.Parse(content).RootElement;
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = json.GetProperty("error").GetProperty("message").GetString() ?? "Erro no login"
                };
            }
            catch
            {
                return new AuthResult { Success = false, ErrorMessage = "Erro desconhecido no login Firebase" };
            }
        }

        private async Task<AuthResult> FirebaseSignUpAsync(string email, string password)
        {
            var endpoint = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={FirebaseApiKey}";
            var body = new
            {
                email,
                password,
                returnSecureToken = true
            };

            var payload = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var resp = await httpClient.PostAsync(endpoint, payload);
            var content = await resp.Content.ReadAsStringAsync();

            if (resp.IsSuccessStatusCode)
            {
                var json = JsonDocument.Parse(content).RootElement;
                return new AuthResult
                {
                    Success = true,
                    IdToken = json.GetProperty("idToken").GetString(),
                    LocalId = json.GetProperty("localId").GetString(),
                    Email = json.GetProperty("email").GetString()
                };
            }

            try
            {
                var json = JsonDocument.Parse(content).RootElement;
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = json.GetProperty("error").GetProperty("message").GetString() ?? "Erro no cadastro"
                };
            }
            catch
            {
                return new AuthResult { Success = false, ErrorMessage = "Erro desconhecido no cadastro Firebase" };
            }
        }

        private async Task<(bool Success, string? ErrorMessage)> SaveUserProfileAsync(string localId, string idToken, string name, string email, string phone, string course, string registration)
        {
            var endpoint = $"https://firestore.googleapis.com/v1/projects/{FirebaseProjectId}/databases/(default)/documents/users?documentId={localId}";
            var body = new
            {
                fields = new
                {
                    name = new { stringValue = name },
                    email = new { stringValue = email },
                    phone = new { stringValue = phone },
                    course = new { stringValue = course },
                    registration = new { stringValue = registration },
                    nickname = new { stringValue = string.Empty },
                    professionalTitle = new { stringValue = string.Empty },
                    bio = new { stringValue = string.Empty },
                    skills = new { stringValue = string.Empty },
                    programmingLanguages = new { stringValue = string.Empty },
                    portfolioLink = new { stringValue = string.Empty },
                    linkedInLink = new { stringValue = string.Empty },
                    avatarBody = new { stringValue = string.Empty },
                    avatarHair = new { stringValue = string.Empty },
                    avatarHat = new { stringValue = string.Empty },
                    avatarAccessory = new { stringValue = string.Empty },
                    avatarClothing = new { stringValue = string.Empty },
                    createdAt = new { timestampValue = DateTime.UtcNow.ToString("o") }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

            var resp = await httpClient.SendAsync(request);
            if (resp.IsSuccessStatusCode)
            {
                return (true, null);
            }

            var error = await resp.Content.ReadAsStringAsync();
            return (false, error);
        }

        private async Task<UserProfile?> GetUserProfileAsync(string localId, string idToken)
        {
            var endpoint = $"https://firestore.googleapis.com/v1/projects/{FirebaseProjectId}/databases/(default)/documents/users/{localId}";
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

            var resp = await httpClient.SendAsync(request);
            if (!resp.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await resp.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(content).RootElement;
            if (!doc.TryGetProperty("fields", out var fields))
                return null;

            return new UserProfile
            {
                UserId = localId,
                Name = TryGetStringField(fields, "name"),
                Email = TryGetStringField(fields, "email"),
                Phone = TryGetStringField(fields, "phone"),
                Course = TryGetStringField(fields, "course"),
                Registration = TryGetStringField(fields, "registration"),
                Nickname = TryGetStringField(fields, "nickname"),
                ProfessionalTitle = TryGetStringField(fields, "professionalTitle"),
                Bio = TryGetStringField(fields, "bio"),
                Skills = TryGetStringField(fields, "skills"),
                ProgrammingLanguages = TryGetStringField(fields, "programmingLanguages"),
                PortfolioLink = TryGetStringField(fields, "portfolioLink"),
                LinkedInLink = TryGetStringField(fields, "linkedInLink"),
                AvatarBody = TryGetStringField(fields, "avatarBody"),
                AvatarHair = TryGetStringField(fields, "avatarHair"),
                AvatarHat = TryGetStringField(fields, "avatarHat"),
                AvatarAccessory = TryGetStringField(fields, "avatarAccessory"),
                AvatarClothing = TryGetStringField(fields, "avatarClothing")
            };
        }

        private string TryGetStringField(JsonElement fields, string fieldName)
        {
            if (fields.TryGetProperty(fieldName, out var field) && field.TryGetProperty("stringValue", out var value))
            {
                return value.GetString() ?? string.Empty;
            }

            return string.Empty;
        }
    }

    public class UserProfile
    {
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Course { get; set; } = string.Empty;
        public string Registration { get; set; } = string.Empty;
        public string Nickname { get; set; } = string.Empty;
        public string ProfessionalTitle { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string Skills { get; set; } = string.Empty;
        public string ProgrammingLanguages { get; set; } = string.Empty;
        public string PortfolioLink { get; set; } = string.Empty;
        public string LinkedInLink { get; set; } = string.Empty;
        public string AvatarBody { get; set; } = string.Empty;
        public string AvatarHair { get; set; } = string.Empty;
        public string AvatarHat { get; set; } = string.Empty;
        public string AvatarAccessory { get; set; } = string.Empty;
        public string AvatarClothing { get; set; } = string.Empty;
    }

    public class AuthResult
    {
        public bool Success { get; set; }
        public string? IdToken { get; set; }
        public string? LocalId { get; set; }
        public string? Email { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
