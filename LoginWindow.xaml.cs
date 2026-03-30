using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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

            var loginResult = await FirebaseSignInAsync(email, password);
            if (!loginResult.Success)
            {
                MessageBox.Show($"Falha no login: {loginResult.ErrorMessage}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var profile = await GetUserProfileAsync(loginResult.LocalId!, loginResult.IdToken!);
            if (profile == null)
            {
                MessageBox.Show("Perfil do usuário não encontrado no banco de dados.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                profile = new UserProfile
                {
                    UserId = loginResult.LocalId!,
                    Name = "Usuário", Email = email, Phone = "", Course = "", Registration = ""
                };
            }

            var mainWindow = new MainWindow(profile, loginResult.IdToken);
            mainWindow.Show();
            this.Close();
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

            var mainWindow = new MainWindow(profile, signupResult.IdToken);
            mainWindow.Show();
            this.Close();
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
                Name = fields.GetProperty("name").GetProperty("stringValue").GetString() ?? "",
                Email = fields.GetProperty("email").GetProperty("stringValue").GetString() ?? "",
                Phone = fields.GetProperty("phone").GetProperty("stringValue").GetString() ?? "",
                Course = fields.GetProperty("course").GetProperty("stringValue").GetString() ?? "",
                Registration = fields.GetProperty("registration").GetProperty("stringValue").GetString() ?? "",
                Nickname = TryGetStringField(fields, "nickname"),
                ProfessionalTitle = TryGetStringField(fields, "professionalTitle"),
                Bio = TryGetStringField(fields, "bio"),
                Skills = TryGetStringField(fields, "skills"),
                ProgrammingLanguages = TryGetStringField(fields, "programmingLanguages"),
                PortfolioLink = TryGetStringField(fields, "portfolioLink"),
                LinkedInLink = TryGetStringField(fields, "linkedInLink")
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
