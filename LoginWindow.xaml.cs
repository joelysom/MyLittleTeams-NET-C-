using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls;

namespace MeuApp
{
    public partial class LoginWindow : ChromeWindow
    {
        private enum LoginDialogAction
        {
            Dismiss,
            Retry,
            ForgotPassword,
            CloseWindow
        }

        private enum LoginFeedbackKind
        {
            LoginError,
            ConnectionError,
            Info
        }

        private static readonly HttpClient httpClient = new HttpClient();
        private bool _loginDarkModeEnabled;

        public LoginWindow()
        {
            InitializeComponent();
            _loginDarkModeEnabled = AccessibilityPreferences.DarkModeEnabled;
            ApplyLoginTheme();
            UpdateProfessorSignupFieldsVisibility();
            UpdateTabVisualState();
            UpdateLoginAccessibilityPanelState();
        }

        protected override void OnAccessibilitySettingsChanged(AccessibilitySettings settings)
        {
            _loginDarkModeEnabled = settings.DarkModeEnabled;
            ApplyLoginTheme();
            UpdateLoginAccessibilityPanelState();
        }

        private void ApplyLoginTheme()
        {
            if (AccessibilityPreferences.HighContrastEnabled)
            {
                SetBrushResource("LoginAccentBrush", Color.FromRgb(125, 211, 252));
                SetBrushResource("LoginAccentHoverBrush", Color.FromRgb(56, 189, 248));
                SetBrushResource("LoginAccentPressedBrush", Color.FromRgb(14, 165, 233));
                SetBrushResource("LoginWindowBackgroundBrush", Color.FromRgb(2, 6, 23));
                SetBrushResource("LoginSurfaceBrush", Color.FromRgb(4, 9, 24));
                SetBrushResource("LoginSurfaceAltBrush", Color.FromRgb(11, 18, 32));
                SetBrushResource("LoginInputBackgroundBrush", Color.FromRgb(0, 0, 0));
                SetBrushResource("LoginBorderBrush", Color.FromRgb(56, 189, 248));
                SetBrushResource("LoginPrimaryTextBrush", Color.FromRgb(248, 250, 252));
                SetBrushResource("LoginSecondaryTextBrush", Color.FromRgb(203, 213, 225));
                SetBrushResource("LoginMutedTextBrush", Color.FromRgb(226, 232, 240));
                SetBrushResource("LoginDividerBrush", Color.FromRgb(56, 189, 248));
                SetBrushResource("LoginInfoCardBrush", Color.FromRgb(10, 33, 58));
                SetBrushResource("LoginInfoCardBorderBrush", Color.FromRgb(125, 211, 252));
                SetBrushResource("LoginInfoCardTextBrush", Color.FromRgb(224, 242, 254));
                SetBrushResource("LoginAccessibilityCardBrush", Color.FromRgb(4, 9, 24));
                SetBrushResource("LoginAccessibilityCardBorderBrush", Color.FromRgb(56, 189, 248));
                SetBrushResource("LoginAccessibilityMutedBrush", Color.FromRgb(11, 18, 32));
                Resources["LoginHeroBackgroundBrush"] = CreateHeroBrush(
                    Color.FromRgb(0, 0, 0),
                    Color.FromRgb(9, 30, 66),
                    Color.FromRgb(8, 145, 178));
            }
            else if (_loginDarkModeEnabled)
            {
                SetBrushResource("LoginAccentBrush", Color.FromRgb(96, 165, 250));
                SetBrushResource("LoginAccentHoverBrush", Color.FromRgb(59, 130, 246));
                SetBrushResource("LoginAccentPressedBrush", Color.FromRgb(37, 99, 235));
                SetBrushResource("LoginWindowBackgroundBrush", Color.FromRgb(8, 17, 31));
                SetBrushResource("LoginSurfaceBrush", Color.FromRgb(15, 23, 42));
                SetBrushResource("LoginSurfaceAltBrush", Color.FromRgb(22, 32, 51));
                SetBrushResource("LoginInputBackgroundBrush", Color.FromRgb(17, 28, 46));
                SetBrushResource("LoginBorderBrush", Color.FromRgb(51, 65, 85));
                SetBrushResource("LoginPrimaryTextBrush", Color.FromRgb(226, 232, 240));
                SetBrushResource("LoginSecondaryTextBrush", Color.FromRgb(148, 163, 184));
                SetBrushResource("LoginMutedTextBrush", Color.FromRgb(203, 213, 225));
                SetBrushResource("LoginDividerBrush", Color.FromRgb(36, 50, 71));
                SetBrushResource("LoginInfoCardBrush", Color.FromRgb(17, 35, 58));
                SetBrushResource("LoginInfoCardBorderBrush", Color.FromRgb(30, 58, 95));
                SetBrushResource("LoginInfoCardTextBrush", Color.FromRgb(191, 219, 254));
                SetBrushResource("LoginAccessibilityCardBrush", Color.FromRgb(15, 23, 42));
                SetBrushResource("LoginAccessibilityCardBorderBrush", Color.FromRgb(51, 65, 85));
                SetBrushResource("LoginAccessibilityMutedBrush", Color.FromRgb(17, 28, 46));
                Resources["LoginHeroBackgroundBrush"] = CreateHeroBrush(
                    Color.FromRgb(4, 11, 24),
                    Color.FromRgb(10, 29, 62),
                    Color.FromRgb(29, 78, 216));
            }
            else
            {
                SetBrushResource("LoginAccentBrush", Color.FromRgb(0, 120, 212));
                SetBrushResource("LoginAccentHoverBrush", Color.FromRgb(16, 110, 190));
                SetBrushResource("LoginAccentPressedBrush", Color.FromRgb(0, 99, 177));
                SetBrushResource("LoginWindowBackgroundBrush", Color.FromRgb(255, 255, 255));
                SetBrushResource("LoginSurfaceBrush", Color.FromRgb(255, 255, 255));
                SetBrushResource("LoginSurfaceAltBrush", Color.FromRgb(248, 250, 252));
                SetBrushResource("LoginInputBackgroundBrush", Color.FromRgb(255, 255, 255));
                SetBrushResource("LoginBorderBrush", Color.FromRgb(229, 231, 235));
                SetBrushResource("LoginPrimaryTextBrush", Color.FromRgb(31, 31, 31));
                SetBrushResource("LoginSecondaryTextBrush", Color.FromRgb(113, 113, 113));
                SetBrushResource("LoginMutedTextBrush", Color.FromRgb(102, 102, 102));
                SetBrushResource("LoginDividerBrush", Color.FromRgb(224, 224, 224));
                SetBrushResource("LoginInfoCardBrush", Color.FromRgb(247, 250, 255));
                SetBrushResource("LoginInfoCardBorderBrush", Color.FromRgb(215, 231, 255));
                SetBrushResource("LoginInfoCardTextBrush", Color.FromRgb(36, 80, 122));
                SetBrushResource("LoginAccessibilityCardBrush", Color.FromRgb(255, 255, 255));
                SetBrushResource("LoginAccessibilityCardBorderBrush", Color.FromRgb(229, 231, 235));
                SetBrushResource("LoginAccessibilityMutedBrush", Color.FromRgb(248, 250, 252));
                Resources["LoginHeroBackgroundBrush"] = CreateHeroBrush(
                    Color.FromRgb(8, 18, 35),
                    Color.FromRgb(13, 42, 82),
                    Color.FromRgb(29, 78, 216));
            }

            Background = GetThemeBrushFromResources("LoginWindowBackgroundBrush", Color.FromRgb(255, 255, 255));
            UpdateTabVisualState();
        }

        private void SetBrushResource(string key, Color color)
        {
            if (Resources[key] is SolidColorBrush brush)
            {
                if (brush.IsFrozen)
                {
                    var clonedBrush = brush.CloneCurrentValue();
                    clonedBrush.Color = color;
                    Resources[key] = clonedBrush;
                    return;
                }

                brush.Color = color;
                return;
            }

            Resources[key] = new SolidColorBrush(color);
        }

        private Brush GetThemeBrushFromResources(string key, Color fallback)
        {
            return TryFindResource(key) as Brush ?? new SolidColorBrush(fallback);
        }

        private static LinearGradientBrush CreateHeroBrush(Color start, Color middle, Color end)
        {
            return new LinearGradientBrush(
                new GradientStopCollection
                {
                    new GradientStop(start, 0),
                    new GradientStop(middle, 0.45),
                    new GradientStop(end, 1)
                },
                new Point(0, 0),
                new Point(1, 1));
        }

        private void UpdateTabVisualState()
        {
            var accentBrush = GetThemeBrushFromResources("LoginAccentBrush", Color.FromRgb(0, 120, 212));
            var secondaryBrush = GetThemeBrushFromResources("LoginSecondaryTextBrush", Color.FromRgb(113, 113, 113));

            if (LoginContent.Visibility == Visibility.Visible)
            {
                LoginTabButton.Foreground = accentBrush;
                LoginTabButton.FontWeight = FontWeights.ExtraBold;
                SignupTabButton.Foreground = secondaryBrush;
                SignupTabButton.FontWeight = FontWeights.Normal;
            }
            else
            {
                SignupTabButton.Foreground = accentBrush;
                SignupTabButton.FontWeight = FontWeights.ExtraBold;
                LoginTabButton.Foreground = secondaryBrush;
                LoginTabButton.FontWeight = FontWeights.Normal;
            }

            UpdateHeroCopyState();
        }

        private void UpdateHeroCopyState()
        {
            if (LoginHeroBadgeText == null || LoginHeroTitleText == null || LoginHeroBodyText == null)
            {
                return;
            }

            if (LoginContent.Visibility == Visibility.Visible)
            {
                LoginHeroBadgeText.Text = "ACESSO";
                LoginHeroTitleText.Text = "Entre no observatório";
                LoginHeroBodyText.Text = "Acompanhe turmas, equipes, arquivos e conversas em um workspace acadêmico mais organizado.";
                return;
            }

            LoginHeroBadgeText.Text = "CADASTRO";
            LoginHeroTitleText.Text = "Monte seu perfil acadêmico";
            LoginHeroBodyText.Text = "Defina seu papel, curso e contexto docente para abrir a experiência certa já no primeiro acesso.";
        }

        private void UpdateLoginAccessibilityPanelState()
        {
            if (LoginDarkModeToggle != null)
            {
                LoginDarkModeToggle.IsChecked = _loginDarkModeEnabled;
            }

            if (LoginHighContrastToggle != null)
            {
                LoginHighContrastToggle.IsChecked = AccessibilityPreferences.HighContrastEnabled;
            }

            if (LoginReduceAnimationsToggle != null)
            {
                LoginReduceAnimationsToggle.IsChecked = AccessibilityPreferences.ReduceAnimations;
            }

            if (LoginTextScalePercentText != null)
            {
                LoginTextScalePercentText.Text = $"({AccessibilityPreferences.TextScalePercent})%";
            }

            if (DecreaseTextScaleButton != null)
            {
                DecreaseTextScaleButton.IsEnabled = AccessibilityPreferences.TextScalePercent > AccessibilityPreferences.MinTextScalePercent;
            }

            if (IncreaseTextScaleButton != null)
            {
                IncreaseTextScaleButton.IsEnabled = AccessibilityPreferences.TextScalePercent < AccessibilityPreferences.MaxTextScalePercent;
            }
        }

        private void AccessibilityButton_Click(object sender, RoutedEventArgs e)
        {
            LoginAccessibilityPanel.Visibility = LoginAccessibilityPanel.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;

            UpdateLoginAccessibilityPanelState();
        }

        private void LoginDarkModeToggle_Changed(object sender, RoutedEventArgs e)
        {
            AccessibilityPreferences.SetDarkModeEnabled(LoginDarkModeToggle.IsChecked == true);
        }

        private void LoginHighContrastToggle_Changed(object sender, RoutedEventArgs e)
        {
            AccessibilityPreferences.SetHighContrastEnabled(LoginHighContrastToggle.IsChecked == true);
        }

        private void LoginReduceAnimationsToggle_Changed(object sender, RoutedEventArgs e)
        {
            AccessibilityPreferences.SetReduceAnimations(LoginReduceAnimationsToggle.IsChecked == true);
        }

        private void DecreaseTextScaleButton_Click(object sender, RoutedEventArgs e)
        {
            AccessibilityPreferences.AdjustTextScale(-AccessibilityPreferences.TextScaleStepPercent);
            UpdateLoginAccessibilityPanelState();
        }

        private void IncreaseTextScaleButton_Click(object sender, RoutedEventArgs e)
        {
            AccessibilityPreferences.AdjustTextScale(AccessibilityPreferences.TextScaleStepPercent);
            UpdateLoginAccessibilityPanelState();
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
                }
                else if (tag == "Signup")
                {
                    LoginContent.Visibility = Visibility.Collapsed;
                    SignupContent.Visibility = Visibility.Visible;
                }

                UpdateTabVisualState();
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

            var shouldRetry = true;
            while (shouldRetry)
            {
                shouldRetry = false;

                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    IsEnabled = false;

                    var loginResult = await FirebaseSignInAsync(email, password);
                    if (!loginResult.Success)
                    {
                        var isConnectionError = IsConnectionError(loginResult.ErrorMessage);
                        var action = ShowLoginFeedbackDialog(
                            isConnectionError ? LoginFeedbackKind.ConnectionError : LoginFeedbackKind.LoginError,
                            isConnectionError ? "Sem conexão com a internet" : "Não foi possível entrar",
                            BuildLoginFeedbackMessage(loginResult.ErrorMessage, isConnectionError),
                            isConnectionError ? "Verifique sua conexão e tente novamente. Se o problema persistir, feche a janela e abra o app novamente." : "Confira suas credenciais ou recupere sua senha para tentar novamente.");

                        if (action == LoginDialogAction.Retry)
                        {
                            if (isConnectionError)
                            {
                                shouldRetry = true;
                            }
                            else
                            {
                                LoginPassword.Focus();
                                LoginPassword.SelectAll();
                            }
                        }
                        else if (action == LoginDialogAction.ForgotPassword)
                        {
                            await HandleForgotPasswordAsync();
                        }
                        else if (action == LoginDialogAction.CloseWindow)
                        {
                            Close();
                        }

                        continue;
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
        }

        private async void ForgotPasswordLink_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            await HandleForgotPasswordAsync();
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
                Registration = string.Empty,
                Role = "student"
            };
        }

        private void SignupRoleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateProfessorSignupFieldsVisibility();
        }

        private void UpdateProfessorSignupFieldsVisibility()
        {
            if (ProfessorSignupFieldsPanel is null || SignupRoleComboBox is null)
            {
                return;
            }

            var selectedRole = GetSelectedSignupRole();
            ProfessorSignupFieldsPanel.Visibility = TeamPermissionService.IsProfessorLike(selectedRole)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private string GetSelectedSignupRole()
        {
            return SignupRoleComboBox?.SelectedItem is ComboBoxItem selectedRole
                ? TeamPermissionService.NormalizeRole(selectedRole.Tag?.ToString())
                : "student";
        }

        private string BuildDefaultProfessionalTitle(string role)
        {
            return TeamPermissionService.IsProfessorLike(role)
                ? "Professor orientador"
                : "Aluno";
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
            string role = GetSelectedSignupRole();
            string academicDepartment = SignupAcademicDepartment.Text;
            string academicFocus = SignupAcademicFocus.Text;
            string officeHours = SignupOfficeHours.Text;
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

                var profile = new UserProfile
                {
                    UserId = signupResult.LocalId!,
                    Name = name,
                    Email = email,
                    Phone = phone,
                    Course = course,
                    Registration = registration,
                    Role = role,
                    AcademicDepartment = academicDepartment,
                    AcademicFocus = academicFocus,
                    OfficeHours = officeHours,
                    ProfessorAccessLevel = TeamPermissionService.IsProfessorLike(role) ? "faculty-dashboard" : "student-workspace",
                    ProfessionalTitle = BuildDefaultProfessionalTitle(role)
                };

                var saveResult = await SaveUserProfileAsync(signupResult.LocalId!, signupResult.IdToken!, profile);
                if (!saveResult.Success)
                {
                    MessageBox.Show($"Não foi possível salvar o perfil: {saveResult.ErrorMessage}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

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
            try
            {
                var endpoint = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={AppConfig.FirebaseApiKey}";
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
            catch (HttpRequestException)
            {
                return new AuthResult { Success = false, ErrorMessage = "NETWORK_ERROR" };
            }
            catch (TaskCanceledException)
            {
                return new AuthResult { Success = false, ErrorMessage = "NETWORK_TIMEOUT" };
            }
        }

        private async Task<(bool Success, string? ErrorMessage)> SendPasswordResetEmailAsync(string email)
        {
            try
            {
                var endpoint = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={AppConfig.FirebaseApiKey}";
                var body = new
                {
                    requestType = "PASSWORD_RESET",
                    email
                };

                var payload = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(endpoint, payload);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                try
                {
                    var json = JsonDocument.Parse(content).RootElement;
                    return (false, json.GetProperty("error").GetProperty("message").GetString() ?? "Não foi possível enviar o email de recuperação.");
                }
                catch
                {
                    return (false, "Não foi possível enviar o email de recuperação.");
                }
            }
            catch (HttpRequestException)
            {
                return (false, "NETWORK_ERROR");
            }
            catch (TaskCanceledException)
            {
                return (false, "NETWORK_TIMEOUT");
            }
        }

        private async Task HandleForgotPasswordAsync()
        {
            var email = LoginEmail.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@", StringComparison.Ordinal))
            {
                ShowLoginFeedbackDialog(
                    LoginFeedbackKind.LoginError,
                    "Informe um email válido",
                    "Para recuperar a senha, digite primeiro o email usado no login.",
                    "Depois disso, escolha novamente a opção de recuperação.");

                LoginEmail.Focus();
                LoginEmail.SelectAll();
                return;
            }

            Mouse.OverrideCursor = Cursors.Wait;
            IsEnabled = false;

            try
            {
                var result = await SendPasswordResetEmailAsync(email);
                if (result.Success)
                {
                    ShowLoginFeedbackDialog(
                        LoginFeedbackKind.Info,
                        "Email de recuperação enviado",
                        $"Enviamos as instruções de redefinição para {email}.",
                        "Verifique sua caixa de entrada e também a pasta de spam.");
                    return;
                }

                ShowLoginFeedbackDialog(
                    IsConnectionError(result.ErrorMessage) ? LoginFeedbackKind.ConnectionError : LoginFeedbackKind.LoginError,
                    IsConnectionError(result.ErrorMessage) ? "Não foi possível conectar" : "Não foi possível recuperar a senha",
                    BuildPasswordResetFeedbackMessage(result.ErrorMessage),
                    IsConnectionError(result.ErrorMessage)
                        ? "Confira sua internet e tente novamente."
                        : "Revise o email informado e tente novamente.");
            }
            finally
            {
                Mouse.OverrideCursor = null;
                IsEnabled = true;
            }
        }

        private LoginDialogAction ShowLoginFeedbackDialog(LoginFeedbackKind kind, string title, string message, string helperText)
        {
            var dialog = new Window
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Width = 700,
                Height = 680,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false,
                Title = title,
                Background = new SolidColorBrush(Color.FromRgb(245, 247, 250)),
                BorderThickness = new Thickness(0),
                BorderBrush = Brushes.Transparent
            };

            var result = LoginDialogAction.Dismiss;
            var isConnection = kind == LoginFeedbackKind.ConnectionError;
            var isInfo = kind == LoginFeedbackKind.Info;
            var imagePath = kind switch
            {
                LoginFeedbackKind.ConnectionError => "pack://application:,,,/img/Error/ConectionError.png",
                _ => "pack://application:,,,/img/Error/LoginError.png"
            };

            var shell = new Border
            {
                Margin = new Thickness(14),
                CornerRadius = new CornerRadius(30),
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                BorderThickness = new Thickness(1),
                SnapsToDevicePixels = true
            };
            shell.Effect = new DropShadowEffect
            {
                BlurRadius = 32,
                ShadowDepth = 10,
                Opacity = 0.22,
                Color = Color.FromRgb(15, 23, 42)
            };

            var root = new Grid
            {
                Margin = new Thickness(30, 28, 30, 30)
            };
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var imageCard = new Border
            {
                Height = 210,
                Background = Brushes.Transparent,
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(0),
                Child = new Image
                {
                    Source = TryCreateDialogImageSource(imagePath),
                    Stretch = Stretch.Uniform,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
            imageCard.Effect = new DropShadowEffect
            {
                BlurRadius = 34,
                ShadowDepth = 8,
                Opacity = 0.16,
                Color = Color.FromRgb(15, 23, 42)
            };
            root.Children.Add(imageCard);

            var contentStack = new StackPanel
            {
                Margin = new Thickness(0, 22, 0, 0)
            };
            contentStack.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 26,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(15, 23, 42)),
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 620,
                LineHeight = 32
            });
            contentStack.Children.Add(new TextBlock
            {
                Text = message,
                Margin = new Thickness(0, 14, 0, 0),
                FontSize = 15,
                Foreground = new SolidColorBrush(Color.FromRgb(51, 65, 85)),
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 620,
                LineHeight = 24
            });
            contentStack.Children.Add(new Border
            {
                Margin = new Thickness(0, 18, 0, 0),
                Padding = new Thickness(16, 14, 16, 14),
                CornerRadius = new CornerRadius(16),
                Background = new SolidColorBrush(Color.FromRgb(248, 250, 252)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                BorderThickness = new Thickness(1),
                Child = new TextBlock
                {
                    Text = helperText,
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.FromRgb(71, 85, 105)),
                    TextWrapping = TextWrapping.Wrap,
                    MaxWidth = 588,
                    LineHeight = 21
                }
            });
            Grid.SetRow(contentStack, 1);
            root.Children.Add(contentStack);

            var buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 30, 0, 6)
            };

            Button CreateDialogButton(string label, Brush background, Brush foreground, Brush borderBrush, LoginDialogAction action)
            {
                var button = new Button
                {
                    Content = label,
                    MinWidth = 112,
                    Height = 42,
                    Margin = new Thickness(12, 0, 0, 12),
                    Padding = new Thickness(16, 0, 16, 0),
                    Background = background,
                    Foreground = foreground,
                    BorderBrush = borderBrush,
                    BorderThickness = borderBrush == Brushes.Transparent ? new Thickness(0) : new Thickness(1),
                    FontWeight = FontWeights.SemiBold,
                    Cursor = Cursors.Hand
                };
                button.Click += (_, __) =>
                {
                    result = action;
                    dialog.Close();
                };
                return button;
            }

            if (isConnection)
            {
                buttons.Children.Add(CreateDialogButton("OK", Brushes.White, new SolidColorBrush(Color.FromRgb(51, 65, 85)), new SolidColorBrush(Color.FromRgb(203, 213, 225)), LoginDialogAction.Dismiss));
                buttons.Children.Add(CreateDialogButton("Tentar novamente", new SolidColorBrush(Color.FromRgb(37, 99, 235)), Brushes.White, Brushes.Transparent, LoginDialogAction.Retry));
                buttons.Children.Add(CreateDialogButton("Fechar", new SolidColorBrush(Color.FromRgb(15, 23, 42)), Brushes.White, Brushes.Transparent, LoginDialogAction.CloseWindow));
            }
            else if (isInfo)
            {
                buttons.Children.Add(CreateDialogButton("OK", new SolidColorBrush(Color.FromRgb(37, 99, 235)), Brushes.White, Brushes.Transparent, LoginDialogAction.Dismiss));
            }
            else
            {
                buttons.Children.Add(CreateDialogButton("OK", Brushes.White, new SolidColorBrush(Color.FromRgb(51, 65, 85)), new SolidColorBrush(Color.FromRgb(203, 213, 225)), LoginDialogAction.Dismiss));
                buttons.Children.Add(CreateDialogButton("Tentar novamente", new SolidColorBrush(Color.FromRgb(37, 99, 235)), Brushes.White, Brushes.Transparent, LoginDialogAction.Retry));
                buttons.Children.Add(CreateDialogButton("Esqueci senha", new SolidColorBrush(Color.FromRgb(15, 23, 42)), Brushes.White, Brushes.Transparent, LoginDialogAction.ForgotPassword));
            }

            Grid.SetRow(buttons, 2);
            root.Children.Add(buttons);

            shell.Child = root;
            dialog.Content = shell;
            dialog.ShowDialog();
            return result;
        }

        private ImageSource? TryCreateDialogImageSource(string imagePath)
        {
            try
            {
                return new BitmapImage(new Uri(imagePath, UriKind.Absolute));
            }
            catch
            {
                return new BitmapImage(new Uri("pack://application:,,,/img/tesseractICO.png", UriKind.Absolute));
            }
        }

        private static bool IsConnectionError(string? errorMessage)
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                return false;
            }

            return errorMessage.Contains("NETWORK", StringComparison.OrdinalIgnoreCase)
                || errorMessage.Contains("TIMEOUT", StringComparison.OrdinalIgnoreCase)
                || errorMessage.Contains("UNAVAILABLE", StringComparison.OrdinalIgnoreCase)
                || errorMessage.Contains("socket", StringComparison.OrdinalIgnoreCase)
                || errorMessage.Contains("conex", StringComparison.OrdinalIgnoreCase)
                || errorMessage.Contains("internet", StringComparison.OrdinalIgnoreCase);
        }

        private static string BuildLoginFeedbackMessage(string? errorMessage, bool isConnectionError)
        {
            if (isConnectionError)
            {
                return "A autenticação não conseguiu conversar com o Firebase neste momento.";
            }

            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                return "Seu email ou senha não foram aceitos.";
            }

            return errorMessage switch
            {
                var message when message.Contains("INVALID_LOGIN_CREDENTIALS", StringComparison.OrdinalIgnoreCase) => "Email ou senha incorretos. Confira os dados e tente novamente.",
                var message when message.Contains("INVALID_PASSWORD", StringComparison.OrdinalIgnoreCase) => "A senha informada está incorreta.",
                var message when message.Contains("EMAIL_NOT_FOUND", StringComparison.OrdinalIgnoreCase) => "Não encontramos uma conta com esse email.",
                var message when message.Contains("USER_DISABLED", StringComparison.OrdinalIgnoreCase) => "Essa conta foi desativada e não pode entrar agora.",
                _ => $"Falha de autenticação: {errorMessage}."
            };
        }

        private static string BuildPasswordResetFeedbackMessage(string? errorMessage)
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                return "Não foi possível iniciar a recuperação de senha.";
            }

            if (IsConnectionError(errorMessage))
            {
                return "A solicitação de recuperação não conseguiu chegar ao servidor.";
            }

            return errorMessage switch
            {
                var message when message.Contains("EMAIL_NOT_FOUND", StringComparison.OrdinalIgnoreCase) => "Esse email não está cadastrado na plataforma.",
                var message when message.Contains("INVALID_EMAIL", StringComparison.OrdinalIgnoreCase) => "O email informado não é válido para recuperação.",
                _ => $"Falha na recuperação: {errorMessage}."
            };
        }

        private async Task<AuthResult> FirebaseSignUpAsync(string email, string password)
        {
            var endpoint = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={AppConfig.FirebaseApiKey}";
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

        private async Task<(bool Success, string? ErrorMessage)> SaveUserProfileAsync(string localId, string idToken, UserProfile profile)
        {
            profile.Role = TeamPermissionService.NormalizeRole(profile.Role);

            var endpoint = AppConfig.BuildFirestoreDocumentUrl($"users?documentId={localId}");
            var body = new
            {
                fields = new
                {
                    name = new { stringValue = profile.Name },
                    email = new { stringValue = profile.Email },
                    phone = new { stringValue = profile.Phone },
                    course = new { stringValue = profile.Course },
                    registration = new { stringValue = profile.Registration },
                    role = new { stringValue = profile.Role },
                    academicDepartment = new { stringValue = profile.AcademicDepartment ?? string.Empty },
                    academicFocus = new { stringValue = profile.AcademicFocus ?? string.Empty },
                    officeHours = new { stringValue = profile.OfficeHours ?? string.Empty },
                    professorAccessLevel = new { stringValue = profile.ProfessorAccessLevel ?? string.Empty },
                    nickname = new { stringValue = profile.Nickname ?? string.Empty },
                    professionalTitle = new { stringValue = profile.ProfessionalTitle ?? string.Empty },
                    bio = new { stringValue = profile.Bio ?? string.Empty },
                    skills = new { stringValue = profile.Skills ?? string.Empty },
                    programmingLanguages = new { stringValue = profile.ProgrammingLanguages ?? string.Empty },
                    portfolioLink = new { stringValue = profile.PortfolioLink ?? string.Empty },
                    linkedInLink = new { stringValue = profile.LinkedInLink ?? string.Empty },
                    profilePhotoDataUri = new { stringValue = profile.ProfilePhotoDataUri ?? string.Empty },
                    avatarBody = new { stringValue = profile.AvatarBody ?? string.Empty },
                    avatarHair = new { stringValue = profile.AvatarHair ?? string.Empty },
                    avatarHat = new { stringValue = profile.AvatarHat ?? string.Empty },
                    avatarAccessory = new { stringValue = profile.AvatarAccessory ?? string.Empty },
                    avatarClothing = new { stringValue = profile.AvatarClothing ?? string.Empty },
                    galleryImages = new { arrayValue = new { } },
                    featuredProjectIds = new { arrayValue = new { } },
                    calendarEntries = new { arrayValue = new { values = ConvertProfileCalendarEntriesToFirestoreArray(profile.CalendarEntries) } },
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
            var endpoint = AppConfig.BuildFirestoreDocumentUrl($"users/{localId}");
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
                Role = TeamPermissionService.NormalizeRole(TryGetStringField(fields, "role")),
                AcademicDepartment = TryGetStringField(fields, "academicDepartment"),
                AcademicFocus = TryGetStringField(fields, "academicFocus"),
                OfficeHours = TryGetStringField(fields, "officeHours"),
                ProfessorAccessLevel = TryGetStringField(fields, "professorAccessLevel"),
                Nickname = TryGetStringField(fields, "nickname"),
                ProfessionalTitle = TryGetStringField(fields, "professionalTitle"),
                Bio = TryGetStringField(fields, "bio"),
                Skills = TryGetStringField(fields, "skills"),
                ProgrammingLanguages = TryGetStringField(fields, "programmingLanguages"),
                PortfolioLink = TryGetStringField(fields, "portfolioLink"),
                LinkedInLink = TryGetStringField(fields, "linkedInLink"),
                ProfilePhotoDataUri = TryGetStringField(fields, "profilePhotoDataUri"),
                AvatarBody = TryGetStringField(fields, "avatarBody"),
                AvatarHair = TryGetStringField(fields, "avatarHair"),
                AvatarHat = TryGetStringField(fields, "avatarHat"),
                AvatarAccessory = TryGetStringField(fields, "avatarAccessory"),
                AvatarClothing = TryGetStringField(fields, "avatarClothing"),
                GalleryImages = TryGetProfileGalleryField(fields, "galleryImages"),
                FeaturedProjectIds = TryGetStringListField(fields, "featuredProjectIds"),
                CalendarEntries = TryGetProfileCalendarField(fields, "calendarEntries")
            };
        }

        private object[] ConvertProfileCalendarEntriesToFirestoreArray(IEnumerable<ProfileCalendarEntry> calendarEntries)
        {
            return (calendarEntries ?? Enumerable.Empty<ProfileCalendarEntry>())
                .Where(item => item != null && !string.IsNullOrWhiteSpace(item.Title))
                .Select(item => new
                {
                    mapValue = new
                    {
                        fields = new
                        {
                            entryId = new { stringValue = string.IsNullOrWhiteSpace(item.EntryId) ? Guid.NewGuid().ToString("N") : item.EntryId },
                            date = new { timestampValue = (item.Date == default ? DateTime.UtcNow : item.Date.ToUniversalTime()).ToString("o") },
                            entryType = new { stringValue = item.EntryType ?? string.Empty },
                            contextLabel = new { stringValue = item.ContextLabel ?? string.Empty },
                            title = new { stringValue = item.Title ?? string.Empty },
                            notes = new { stringValue = item.Notes ?? string.Empty },
                            createdAt = new { timestampValue = (item.CreatedAt == default ? DateTime.UtcNow : item.CreatedAt.ToUniversalTime()).ToString("o") }
                        }
                    }
                })
                .Cast<object>()
                .ToArray();
        }

        private string TryGetStringField(JsonElement fields, string fieldName)
        {
            if (fields.TryGetProperty(fieldName, out var field) && field.TryGetProperty("stringValue", out var value))
            {
                return value.GetString() ?? string.Empty;
            }

            return string.Empty;
        }

        private List<string> TryGetStringListField(JsonElement fields, string fieldName)
        {
            var items = new List<string>();

            if (!fields.TryGetProperty(fieldName, out var field) ||
                !field.TryGetProperty("arrayValue", out var arrayValue) ||
                !arrayValue.TryGetProperty("values", out var values))
            {
                return items;
            }

            foreach (var value in values.EnumerateArray())
            {
                if (value.TryGetProperty("stringValue", out var stringValue))
                {
                    var item = stringValue.GetString();
                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        items.Add(item);
                    }
                }
            }

            return items;
        }

        private List<ProfileGalleryImage> TryGetProfileGalleryField(JsonElement fields, string fieldName)
        {
            var images = new List<ProfileGalleryImage>();

            if (!fields.TryGetProperty(fieldName, out var field) ||
                !field.TryGetProperty("arrayValue", out var arrayValue) ||
                !arrayValue.TryGetProperty("values", out var values))
            {
                return images;
            }

            foreach (var value in values.EnumerateArray())
            {
                if (!value.TryGetProperty("mapValue", out var mapValue) ||
                    !mapValue.TryGetProperty("fields", out var imageFields))
                {
                    continue;
                }

                images.Add(new ProfileGalleryImage
                {
                    ImageId = TryGetStringField(imageFields, "imageId"),
                    Title = TryGetStringField(imageFields, "title"),
                    Description = TryGetStringField(imageFields, "description"),
                    GalleryAlbumId = TryGetStringField(imageFields, "galleryAlbumId"),
                    GalleryAlbumTitle = TryGetStringField(imageFields, "galleryAlbumTitle"),
                    GalleryAlbumDescription = TryGetStringField(imageFields, "galleryAlbumDescription"),
                    ImageDataUri = TryGetStringField(imageFields, "imageDataUri"),
                    AddedAt = TryGetTimestampField(imageFields, "addedAt")
                });
            }

            return images;
        }

        private List<ProfileCalendarEntry> TryGetProfileCalendarField(JsonElement fields, string fieldName)
        {
            var entries = new List<ProfileCalendarEntry>();

            if (!fields.TryGetProperty(fieldName, out var field) ||
                !field.TryGetProperty("arrayValue", out var arrayValue) ||
                !arrayValue.TryGetProperty("values", out var values))
            {
                return entries;
            }

            foreach (var value in values.EnumerateArray())
            {
                if (!value.TryGetProperty("mapValue", out var mapValue) ||
                    !mapValue.TryGetProperty("fields", out var entryFields))
                {
                    continue;
                }

                entries.Add(new ProfileCalendarEntry
                {
                    EntryId = TryGetStringField(entryFields, "entryId"),
                    Date = TryGetTimestampField(entryFields, "date"),
                    EntryType = TryGetStringField(entryFields, "entryType"),
                    ContextLabel = TryGetStringField(entryFields, "contextLabel"),
                    Title = TryGetStringField(entryFields, "title"),
                    Notes = TryGetStringField(entryFields, "notes"),
                    CreatedAt = TryGetTimestampField(entryFields, "createdAt")
                });
            }

            return entries;
        }

        private DateTime TryGetTimestampField(JsonElement fields, string fieldName)
        {
            if (fields.TryGetProperty(fieldName, out var field) &&
                field.TryGetProperty("timestampValue", out var value) &&
                DateTime.TryParse(value.GetString(), out var parsed))
            {
                return parsed.ToLocalTime();
            }

            return DateTime.MinValue;
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
        public string Role { get; set; } = "student";
        public string AcademicDepartment { get; set; } = string.Empty;
        public string AcademicFocus { get; set; } = string.Empty;
        public string OfficeHours { get; set; } = string.Empty;
        public string ProfessorAccessLevel { get; set; } = string.Empty;
        public string Nickname { get; set; } = string.Empty;
        public string ProfessionalTitle { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string Skills { get; set; } = string.Empty;
        public string ProgrammingLanguages { get; set; } = string.Empty;
        public string PortfolioLink { get; set; } = string.Empty;
        public string LinkedInLink { get; set; } = string.Empty;
        public string ProfilePhotoDataUri { get; set; } = string.Empty;
        public string AvatarBody { get; set; } = string.Empty;
        public string AvatarHair { get; set; } = string.Empty;
        public string AvatarHat { get; set; } = string.Empty;
        public string AvatarAccessory { get; set; } = string.Empty;
        public string AvatarClothing { get; set; } = string.Empty;
        public List<ProfileGalleryImage> GalleryImages { get; set; } = new List<ProfileGalleryImage>();
        public List<string> FeaturedProjectIds { get; set; } = new List<string>();
        public List<ProfileCalendarEntry> CalendarEntries { get; set; } = new List<ProfileCalendarEntry>();
    }

    public class ProfileCalendarEntry
    {
        public string EntryId { get; set; } = Guid.NewGuid().ToString("N");
        public DateTime Date { get; set; } = DateTime.Today;
        public string EntryType { get; set; } = "Aviso";
        public string ContextLabel { get; set; } = "Professor";
        public string Title { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class ProfileGalleryImage
    {
        public string ImageId { get; set; } = Guid.NewGuid().ToString("N");
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string GalleryAlbumId { get; set; } = string.Empty;
        public string GalleryAlbumTitle { get; set; } = string.Empty;
        public string GalleryAlbumDescription { get; set; } = string.Empty;
        public string ImageDataUri { get; set; } = string.Empty;
        public DateTime AddedAt { get; set; } = DateTime.Now;
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
