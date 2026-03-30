using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using MahApps.Metro.Controls;

namespace MeuApp
{
    public partial class MainWindow : MetroWindow
    {
        private const string FirebaseProjectId = "obsseractpi";
        private static readonly HttpClient httpClient = new HttpClient();
        private static readonly string[] KnownProgrammingLanguages =
        {
            "ABAP", "Ada", "Apex", "APL", "Assembly", "Awk", "Ballerina", "Bash", "Basic", "C", "C#", "C++",
            "Clojure", "COBOL", "Crystal", "CSS", "Cuda", "D", "Dart", "Delphi", "Elixir", "Elm", "Erlang",
            "F#", "Fortran", "GDScript", "Gherkin", "Go", "Groovy", "Hack", "Haskell", "HCL", "HTML", "IDL",
            "Inform", "Io", "J", "Java", "JavaScript", "Julia", "Kotlin", "LabVIEW", "Ladder Logic", "Lisp",
            "Logo", "Lua", "MATLAB", "MDX", "MEL", "Nim", "Nix", "Objective-C", "OCaml", "Pascal", "Perl",
            "PHP", "PL/SQL", "PostScript", "PowerBuilder", "PowerFX", "Power Query M", "PowerShell", "Processing",
            "Prolog", "PureScript", "Python", "Q#", "R", "Racket", "Razor", "ReasonML", "Red", "REXX", "Ring",
            "Ruby", "Rust", "SAS", "Scala", "Scheme", "Scratch", "Shell", "Simula", "Smalltalk", "Solidity",
            "SPARQL", "SQL", "Squirrel", "Standard ML", "Stata", "Structured Text", "Swift", "T-SQL", "Tcl",
            "Tex", "TypeScript", "V", "Vala", "VB.NET", "VBA", "Verilog", "VHDL", "Visual Basic", "WAT",
            "WebAssembly", "Wolfram", "X++", "XAML", "XML", "XQuery", "YAML", "Zig", "1C:Enterprise", "4D",
            "ActionScript", "AutoHotkey", "AutoIt", "BBC BASIC", "BlitzMax", "Boo", "Chapel", "Clipper", "ColdFusion",
            "Common Lisp", "Datalog", "Dhall", "Eiffel", "Factor", "Forth", "FoxPro", "Genie", "Harbour", "Icon",
            "Janus", "Jovial", "Monkey C", "MUMPS", "OpenCL", "Oz", "Pony", "QML", "RPG", "SML", "SNOBOL",
            "SuperCollider", "Turing", "UnrealScript", "Velocity", "Vim Script", "xBase", "Xtend", "Yorick", "ZPL",
            "Cassandra CQL", "Cypher", "DAX", "EdgeQL", "GraphQL", "Gremlin", "HiveQL", "KQL", "LINQ", "MQL",
            "Pig Latin", "PQL", "PromQL", "SOQL", "Splunk SPL", "Transact-SQL"
        };
        private UserProfile? _currentProfile;
        private string _idToken = "";
        private List<Conversation> _conversations = new List<Conversation>();
        private readonly List<string> _selectedProgrammingLanguages = new List<string>();
        private Conversation? _selectedConversation = null;
        private bool _appDarkModeEnabled = false;

        public MainWindow()
        {
            InitializeComponent();
            ProgrammingLanguageInput.ItemsSource = KnownProgrammingLanguages.OrderBy(language => language).ToList();
            ApplyAppTheme();
            this.KeyDown += MainWindow_KeyDown;
            this.Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DebugHelper.WriteLine("MainWindow carregada");

            if (!string.IsNullOrWhiteSpace(_idToken) && _currentProfile != null)
            {
                await LoadActiveConversationsAsync();
            }
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl+D para ativar modo debug
            if (e.Key == Key.D && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (!DebugHelper.IsInitialized)
                {
                    DebugHelper.Initialize();
                }
                else
                {
                    DebugHelper.OpenLogFile();
                }
                e.Handled = true;
            }
        }

        public MainWindow(UserProfile profile, string idToken = "") : this()
        {
            _currentProfile = profile;
            _idToken = idToken;
            ApplyUserProfile(profile);
        }

        public MainWindow(UserProfile profile) : this()
        {
            _currentProfile = profile;
            ApplyUserProfile(profile);
        }

        private void ApplyUserProfile(UserProfile profile)
        {
            WelcomeText.Text = $"Bem-vindo, {profile.Name}";
            SidebarUserName.Text = profile.Name;
            SidebarUserEmail.Text = profile.Email;
            SidebarUserRole.Text = $"{profile.Course} | Matrícula: {profile.Registration}";
            PopulateProfessionalProfileFields(profile);
        }

        private void PopulateProfessionalProfileFields(UserProfile profile)
        {
            AccountNameText.Text = profile.Name;
            NicknameTextBox.Text = profile.Nickname;
            ProfessionalTitleTextBox.Text = profile.ProfessionalTitle;
            BioTextBox.Text = profile.Bio;
            SkillsTextBox.Text = profile.Skills;
            PortfolioLinkTextBox.Text = profile.PortfolioLink;
            LinkedInLinkTextBox.Text = profile.LinkedInLink;
            SetProgrammingLanguages(profile.ProgrammingLanguages);
            ProfessionalProfileStatusText.Text = string.Empty;
        }

        private void SetProgrammingLanguages(string serializedLanguages)
        {
            _selectedProgrammingLanguages.Clear();

            var languages = serializedLanguages
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(language => !string.IsNullOrWhiteSpace(language))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(language => language)
                .ToList();

            foreach (var language in languages)
            {
                _selectedProgrammingLanguages.Add(NormalizeProgrammingLanguage(language));
            }

            RenderProgrammingLanguageTags();
            ProgrammingLanguageInput.Text = string.Empty;
        }

        private string NormalizeProgrammingLanguage(string language)
        {
            var trimmed = language.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                return string.Empty;
            }

            var known = KnownProgrammingLanguages.FirstOrDefault(item => string.Equals(item, trimmed, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(known))
            {
                return known;
            }

            return trimmed;
        }

        private void RenderProgrammingLanguageTags()
        {
            ProgrammingLanguagesTagsPanel.Children.Clear();

            foreach (var language in _selectedProgrammingLanguages.OrderBy(item => item))
            {
                ProgrammingLanguagesTagsPanel.Children.Add(CreateProgrammingLanguageTag(language));
            }
        }

        private Border CreateProgrammingLanguageTag(string language)
        {
            var background = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(8, 47, 73))
                : new SolidColorBrush(Color.FromRgb(234, 244, 255));
            var foreground = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(186, 230, 253))
                : new SolidColorBrush(Color.FromRgb(3, 105, 161));

            var removeButton = new Button
            {
                Content = "X",
                Background = Brushes.Transparent,
                Foreground = foreground,
                BorderThickness = new Thickness(0),
                FontWeight = FontWeights.Bold,
                FontSize = 11,
                Padding = new Thickness(4, 0, 0, 0),
                Margin = new Thickness(6, 0, 0, 0),
                Cursor = Cursors.Hand,
                VerticalAlignment = VerticalAlignment.Center
            };
            removeButton.Click += (sender, args) =>
            {
                _selectedProgrammingLanguages.RemoveAll(item => string.Equals(item, language, StringComparison.OrdinalIgnoreCase));
                RenderProgrammingLanguageTags();
            };

            var content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };
            content.Children.Add(new TextBlock
            {
                Text = language,
                Foreground = foreground,
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center
            });
            content.Children.Add(removeButton);

            return new Border
            {
                Background = background,
                CornerRadius = new CornerRadius(16),
                Padding = new Thickness(12, 7, 10, 7),
                Margin = new Thickness(0, 0, 8, 8),
                Child = content
            };
        }

        private void AddProgrammingLanguage_Click(object sender, RoutedEventArgs e)
        {
            AddProgrammingLanguageFromInput();
        }

        private void ProgrammingLanguageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddProgrammingLanguageFromInput();
                e.Handled = true;
            }
        }

        private void AddProgrammingLanguageFromInput()
        {
            var language = NormalizeProgrammingLanguage(ProgrammingLanguageInput.Text ?? string.Empty);
            if (string.IsNullOrWhiteSpace(language))
            {
                return;
            }

            if (_selectedProgrammingLanguages.Any(item => string.Equals(item, language, StringComparison.OrdinalIgnoreCase)))
            {
                ProgrammingLanguageInput.Text = string.Empty;
                return;
            }

            _selectedProgrammingLanguages.Add(language);
            RenderProgrammingLanguageTags();
            ProgrammingLanguageInput.Text = string.Empty;
            ProfessionalProfileStatusText.Text = string.Empty;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Funcionou!");
        }

        private async void SearchFriends_Click(object sender, RoutedEventArgs e)
        {
            string query = SearchFriendsBox.Text?.Trim();

            DebugHelper.WriteLine($"=== BUSCA INICIADA ===");
            DebugHelper.WriteLine($"Query do usuário: '{query}'");
            DebugHelper.WriteLine($"ID Token disponível: {!string.IsNullOrEmpty(_idToken)}");
            DebugHelper.WriteLine($"Perfil do usuário: {_currentProfile?.Name}");

            if (string.IsNullOrWhiteSpace(query))
            {
                MessageBox.Show("Digite algo para pesquisar!", "Pesquisa", MessageBoxButton.OK, MessageBoxImage.Information);
                DebugHelper.WriteLine("Busca cancelada: query vazia");
                return;
            }

            if (string.IsNullOrWhiteSpace(_idToken))
            {
                MessageBox.Show("Token de autenticação não disponível. Faça login novamente.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                DebugHelper.WriteLine("Busca cancelada: token vazio");
                return;
            }

            // Abrir janela de resultados
            var searchWindow = new SearchResultsWindow(query, _currentProfile?.UserId ?? "", _currentProfile);
            searchWindow.Owner = this;
            
            // REGISTRAR HANDLER para quando conversa for iniciada
            searchWindow.OnConversationStarted += (selectedUser) =>
            {
                DebugHelper.WriteLine($"[MainWindow] Conversa iniciada com: {selectedUser.Name}");
                ShowConversationInMainWindow(selectedUser);
            };
            
            searchWindow.ShowLoading();
            searchWindow.Show();

            DebugHelper.WriteLine("Janela de resultados aberta, iniciando busca...");

            // Executar busca assincronamente
            try
            {
                var searchService = new UserSearchService(_idToken);
                DebugHelper.WriteLine("UserSearchService criado");

                var results = await searchService.SearchUsersAsync(query);

                DebugHelper.WriteLine($"Busca concluída. Resultados: {results?.Count ?? 0}");

                if (results == null || results.Count == 0)
                {
                    DebugHelper.WriteLine("AVISO: Nenhum resultado encontrado via Firebase, usando dados simulados...");
                    // Usar dados simulados para teste (remover quando Firebase permissions for resolvido)
                    results = MockData.SearchMockUsers(query);
                    DebugHelper.WriteLine($"Dados simulados: {results.Count} resultados");
                }
                else
                {
                    foreach (var result in results)
                    {
                        DebugHelper.WriteLine($"  - {result.Name} ({result.Registration}): {result.Email}");
                    }
                }

                searchWindow.SetResults(results ?? new System.Collections.Generic.List<UserInfo>());
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"EXCEÇÃO na busca: {ex.GetType().Name}");
                DebugHelper.WriteLine($"Mensagem: {ex.Message}");
                DebugHelper.WriteLine($"Stack: {ex.StackTrace}");
                
                DebugHelper.WriteLine("Usando dados simulados como fallback...");
                var mockResults = MockData.SearchMockUsers(query);
                searchWindow.SetResults(mockResults);

                MessageBox.Show(
                    $"Firebase indisponível. Usando dados simulados para demonstração.\n\nErro: {ex.Message}",
                    "Busca com Dados Simulados",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }

            DebugHelper.WriteLine("=== BUSCA FINALIZADA ===\n");
        }

        private async void TestFirebase_Click(object sender, RoutedEventArgs e)
        {
            DebugHelper.WriteLine("\n========== TESTE DE CONEXÃO FIREBASE INICIADO ==========\n");

            TestFirebaseButton.IsEnabled = false;
            TestFirebaseButton.Content = "⏳";
            TestFirebaseButton.ToolTip = "Testando conexão...";

            try
            {
                if (string.IsNullOrWhiteSpace(_idToken))
                {
                    MessageBox.Show("Token não disponível. Faça login novamente.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    DebugHelper.WriteLine("TESTE CANCELADO: Token não disponível");
                    TestFirebaseButton.IsEnabled = true;
                    TestFirebaseButton.Content = "🧪";
                    TestFirebaseButton.ToolTip = "Teste de conexão Firebase";
                    return;
                }

                var tester = new FirebaseConnectionTester(_idToken);
                var result = await tester.RunFullTestAsync();

                // Mostrar resultado
                MessageBox.Show(
                    result.GetSummary(),
                    "Resultado do Teste Firebase",
                    MessageBoxButton.OK,
                    result.Errors.Count == 0 ? MessageBoxImage.Information : MessageBoxImage.Warning
                );

                DebugHelper.WriteLine(result.GetSummary());
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"EXCEÇÃO no teste: {ex.Message}");
                DebugHelper.WriteLine($"Stack: {ex.StackTrace}");

                MessageBox.Show(
                    $"Erro ao testar Firebase: {ex.Message}",
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
            finally
            {
                TestFirebaseButton.IsEnabled = true;
                TestFirebaseButton.Content = "🧪";
                TestFirebaseButton.ToolTip = "Teste de conexão Firebase";
            }
        }

        private async void NavButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string tag = button.Tag?.ToString() ?? "";
                ResetNavigation();

                switch (tag)
                {
                    case "Chats":
                        _selectedConversation = null;
                        ChatsContent.Visibility = Visibility.Visible;
                        RenderChatsLoadingState();

                        if (!string.IsNullOrWhiteSpace(_idToken) && _currentProfile != null)
                        {
                            await LoadActiveConversationsAsync();
                        }
                        else
                        {
                            RefreshChatsUI();
                        }
                        break;
                    case "Equipes":
                        TeamsContent.Visibility = Visibility.Visible;
                        break;
                    case "Calendario":
                        CalendarContent.Visibility = Visibility.Visible;
                        break;
                    case "Arquivos":
                        FilesContent.Visibility = Visibility.Visible;
                        break;
                    case "Configuracoes":
                        SettingsContent.Visibility = Visibility.Visible;
                        PopulateProfessionalProfileFields(_currentProfile ?? new UserProfile());
                        break;
                }
            }
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            ResetNavigation();
            SettingsContent.Visibility = Visibility.Visible;
            PopulateProfessionalProfileFields(_currentProfile ?? new UserProfile());
        }

        private void ResetNavigation()
        {
            ChatsContent.Visibility = Visibility.Collapsed;
            TeamsContent.Visibility = Visibility.Collapsed;
            CalendarContent.Visibility = Visibility.Collapsed;
            FilesContent.Visibility = Visibility.Collapsed;
            SettingsContent.Visibility = Visibility.Collapsed;
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Deseja encerrar a sessão atual e voltar para a tela de login?",
                "Sair da conta",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }

        private void ChatDarkModeToggle_Changed(object sender, RoutedEventArgs e)
        {
            _appDarkModeEnabled = ChatDarkModeToggle.IsChecked == true;
            ApplyAppTheme();

            if (ChatsContent.Visibility == Visibility.Visible)
            {
                RefreshChatsUI();
            }
        }

        private async void SaveProfessionalProfile_Click(object sender, RoutedEventArgs e)
        {
            if (_currentProfile == null)
            {
                MessageBox.Show("Nenhum perfil carregado para atualizar.", "Perfil", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveProfessionalProfileButton.IsEnabled = false;
            ProfessionalProfileStatusText.Text = "Salvando perfil...";

            _currentProfile.Nickname = NicknameTextBox.Text?.Trim() ?? string.Empty;
            _currentProfile.ProfessionalTitle = ProfessionalTitleTextBox.Text?.Trim() ?? string.Empty;
            _currentProfile.Bio = BioTextBox.Text?.Trim() ?? string.Empty;
            _currentProfile.Skills = SkillsTextBox.Text?.Trim() ?? string.Empty;
            _currentProfile.ProgrammingLanguages = string.Join(", ", _selectedProgrammingLanguages.OrderBy(language => language));
            _currentProfile.PortfolioLink = PortfolioLinkTextBox.Text?.Trim() ?? string.Empty;
            _currentProfile.LinkedInLink = LinkedInLinkTextBox.Text?.Trim() ?? string.Empty;

            try
            {
                if (string.IsNullOrWhiteSpace(_idToken) || string.IsNullOrWhiteSpace(_currentProfile.UserId))
                {
                    ProfessionalProfileStatusText.Text = "Sessão sem token para salvar no Firebase.";
                    return;
                }

                var result = await SaveProfessionalProfileAsync(_currentProfile, _idToken);
                if (result.Success)
                {
                    ProfessionalProfileStatusText.Text = "Perfil profissional salvo com sucesso.";
                }
                else
                {
                    ProfessionalProfileStatusText.Text = "Falha ao salvar perfil.";
                    MessageBox.Show(
                        $"Não foi possível salvar o perfil profissional.\n\n{result.ErrorMessage}",
                        "Erro ao salvar",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                ProfessionalProfileStatusText.Text = "Erro ao salvar perfil.";
                MessageBox.Show(
                    $"Erro inesperado ao salvar perfil: {ex.Message}",
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                SaveProfessionalProfileButton.IsEnabled = true;
            }
        }

        private async Task<(bool Success, string? ErrorMessage)> SaveProfessionalProfileAsync(UserProfile profile, string idToken)
        {
            var endpoint = $"https://firestore.googleapis.com/v1/projects/{FirebaseProjectId}/databases/(default)/documents/users/{profile.UserId}";
            var body = new
            {
                fields = new
                {
                    name = new { stringValue = profile.Name },
                    email = new { stringValue = profile.Email },
                    phone = new { stringValue = profile.Phone },
                    course = new { stringValue = profile.Course },
                    registration = new { stringValue = profile.Registration },
                    nickname = new { stringValue = profile.Nickname },
                    professionalTitle = new { stringValue = profile.ProfessionalTitle },
                    bio = new { stringValue = profile.Bio },
                    skills = new { stringValue = profile.Skills },
                    programmingLanguages = new { stringValue = profile.ProgrammingLanguages },
                    portfolioLink = new { stringValue = profile.PortfolioLink },
                    linkedInLink = new { stringValue = profile.LinkedInLink },
                    updatedAt = new { timestampValue = DateTime.UtcNow.ToString("o") }
                }
            };

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), endpoint)
            {
                Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

            var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }

            var error = await response.Content.ReadAsStringAsync();
            return (false, error);
        }

        private void ApplyAppTheme()
        {
            SetThemeBrush("WindowBackgroundBrush", _appDarkModeEnabled ? Color.FromRgb(9, 14, 18) : Color.FromRgb(247, 249, 252));
            SetThemeBrush("SurfaceBrush", _appDarkModeEnabled ? Color.FromRgb(17, 24, 39) : Colors.White);
            SetThemeBrush("SidebarBackgroundBrush", _appDarkModeEnabled ? Color.FromRgb(15, 23, 42) : Colors.White);
            SetThemeBrush("SidebarBorderBrush", _appDarkModeEnabled ? Color.FromRgb(31, 41, 55) : Color.FromRgb(232, 232, 232));
            SetThemeBrush("TopBarBackgroundBrush", _appDarkModeEnabled ? Color.FromRgb(17, 24, 39) : Colors.White);
            SetThemeBrush("MainContentBackgroundBrush", _appDarkModeEnabled ? Color.FromRgb(11, 18, 32) : Colors.White);
            SetThemeBrush("SearchBackgroundBrush", _appDarkModeEnabled ? Color.FromRgb(30, 41, 59) : Color.FromRgb(245, 245, 245));
            SetThemeBrush("SearchBorderBrush", _appDarkModeEnabled ? Color.FromRgb(51, 65, 85) : Color.FromRgb(224, 224, 224));
            SetThemeBrush("PrimaryTextBrush", _appDarkModeEnabled ? Color.FromRgb(241, 245, 249) : Color.FromRgb(51, 51, 51));
            SetThemeBrush("SecondaryTextBrush", _appDarkModeEnabled ? Color.FromRgb(148, 163, 184) : Color.FromRgb(102, 102, 102));
            SetThemeBrush("TertiaryTextBrush", _appDarkModeEnabled ? Color.FromRgb(100, 116, 139) : Color.FromRgb(148, 163, 184));
            SetThemeBrush("CardBackgroundBrush", _appDarkModeEnabled ? Color.FromRgb(17, 24, 39) : Colors.White);
            SetThemeBrush("MutedCardBackgroundBrush", _appDarkModeEnabled ? Color.FromRgb(22, 31, 49) : Color.FromRgb(248, 250, 252));
            SetThemeBrush("CardBorderBrush", _appDarkModeEnabled ? Color.FromRgb(51, 65, 85) : Color.FromRgb(226, 232, 240));
            SetThemeBrush("AccentBrush", _appDarkModeEnabled ? Color.FromRgb(56, 189, 248) : Color.FromRgb(0, 120, 212));
            SetThemeBrush("AccentMutedBrush", _appDarkModeEnabled ? Color.FromRgb(8, 47, 73) : Color.FromRgb(234, 244, 255));
            SetThemeBrush("ToggleTrackOffBrush", _appDarkModeEnabled ? Color.FromRgb(51, 65, 85) : Color.FromRgb(215, 222, 231));
            SetThemeBrush("ToggleTrackOnBrush", _appDarkModeEnabled ? Color.FromRgb(14, 165, 233) : Color.FromRgb(14, 165, 233));
            SetThemeBrush("ToggleThumbBrush", Colors.White);

            Background = GetThemeBrush("WindowBackgroundBrush");
            GlowBrush = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(56, 189, 248))
                : new SolidColorBrush(Color.FromRgb(0, 120, 212));

            UpdateChatsBadge();
        }

        private void SetThemeBrush(string key, Color color)
        {
            Resources[key] = new SolidColorBrush(color);
        }

        private SolidColorBrush GetThemeBrush(string key)
        {
            return (SolidColorBrush)Resources[key];
        }

        private void ShowConversationInMainWindow(UserInfo contactUser)
        {
            try
            {
                DebugHelper.WriteLine($"[ShowConversationInMainWindow] Adicionando conversa com {contactUser.Name}");
                
                // Verificar se já existe conversa com este contato
                var existing = _conversations.FirstOrDefault(c => c.ContactId == contactUser.UserId);
                if (existing == null)
                {
                    // Criar nova conversa
                    var conversation = new Conversation
                    {
                        ConversationId = Guid.NewGuid().ToString(),
                        ContactId = contactUser.UserId,
                        ContactName = contactUser.Name,
                        LastMessage = "Conversa iniciada",
                        LastMessageTime = DateTime.Now,
                        Messages = new List<ChatMessage>()
                    };
                    _conversations.Add(conversation);
                    _selectedConversation = conversation;
                }
                else
                {
                    _selectedConversation = existing;
                }
                
                // Mostrar aba Chats
                ResetNavigation();
                ChatsContent.Visibility = Visibility.Visible;
                UpdateChatsBadge();
                
                // Renderizar UI (será carregado de forma async)
                RefreshChatsUI();
                
                // Carregar mensagens do Firebase em background
                _ = LoadConversationMessagesAsync(_selectedConversation);
                
                DebugHelper.WriteLine($"[ShowConversationInMainWindow] Sucesso");
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[ShowConversationInMainWindow ERROR] {ex.Message}");
                MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadConversationMessagesAsync(Conversation? conversation)
        {
            if (conversation == null)
                return;

            try
            {
                DebugHelper.WriteLine($"[LoadConversationMessages] Carregando mensagens para {conversation.ContactName}");
                
                if (string.IsNullOrEmpty(_idToken))
                {
                    DebugHelper.WriteLine($"[LoadConversationMessages] AVISO: Token não disponível, usando dados locais");
                    return;
                }

                var chatService = new ChatService(_idToken, _currentProfile?.UserId ?? "");
                var messages = await chatService.LoadMessagesAsync(conversation.ContactId);

                if (messages.Count > 0)
                {
                    DebugHelper.WriteLine($"[LoadConversationMessages] Carregadas {messages.Count} mensagens do Firebase");
                    conversation.Messages = messages;
                    conversation.HasUnread = false;
                    conversation.LastReadAt = DateTime.Now;
                    UpdateChatsBadge();

                    var markReadResult = await chatService.MarkConversationAsReadAsync(conversation.ContactId);
                    if (!markReadResult.Success)
                    {
                        DebugHelper.WriteLine($"[LoadConversationMessages] Falha ao marcar como lida: {markReadResult.ErrorMessage}");
                    }
                    
                    // Se esta é a conversa que está sendo exibida, atualizar a UI
                    if (_selectedConversation?.ContactId == conversation.ContactId)
                    {
                        RefreshChatsUI();
                    }
                }
                else
                {
                    DebugHelper.WriteLine($"[LoadConversationMessages] Nenhuma mensagem encontrada no Firebase");
                }
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[LoadConversationMessages ERROR] {ex.Message}");
                DebugHelper.WriteLine($"[LoadConversationMessages ERROR Stack] {ex.StackTrace}");
            }
        }

        private void RefreshChatsUI()
        {
            try
            {
                ChatsContent.Children.Clear();

                if (_selectedConversation == null)
                {
                    // Mostrar lista de conversas
                    RenderConversationsList();
                }
                else
                {
                    // Mostrar tela de chat
                    RenderChatScreen(_selectedConversation);
                }
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[RefreshChatsUI ERROR] {ex.Message}");
            }
        }

        private void RenderConversationsList()
        {
            var primaryText = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(241, 245, 249))
                : new SolidColorBrush(Color.FromRgb(51, 51, 51));
            var secondaryText = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(148, 163, 184))
                : new SolidColorBrush(Color.FromRgb(102, 102, 102));
            var emptyStateBackground = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(22, 31, 49))
                : new SolidColorBrush(Color.FromRgb(248, 250, 252));
            var emptyStateBorder = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(51, 65, 85))
                : new SolidColorBrush(Color.FromRgb(226, 232, 240));

            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Header
            var header = new TextBlock
            {
                Text = "💬 Conversas Ativas",
                FontSize = 20,
                FontWeight = FontWeights.ExtraBold,
                Foreground = primaryText,
                Margin = new Thickness(0, 0, 0, 6)
            };
            mainGrid.Children.Add(header);
            Grid.SetRow(header, 0);

            var subtitle = new TextBlock
            {
                Text = _conversations.Count == 0
                    ? "Nenhuma conversa recente no momento."
                    : $"{_conversations.Count} conversa(s) sincronizada(s) com seu histórico.",
                FontSize = 12,
                Foreground = secondaryText,
                Margin = new Thickness(0, 0, 0, 18)
            };
            mainGrid.Children.Add(subtitle);
            Grid.SetRow(subtitle, 1);

            // Lista de conversas
            var listStack = new StackPanel { Orientation = Orientation.Vertical };
            if (_conversations.Count == 0)
            {
                var emptyState = new Border
                {
                    Background = emptyStateBackground,
                    BorderBrush = emptyStateBorder,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(16),
                    Padding = new Thickness(18),
                    Child = new TextBlock
                    {
                        Text = "As conversas recentes aparecerão aqui assim que novas mensagens forem sincronizadas.",
                        FontSize = 12,
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = secondaryText
                    }
                };
                listStack.Children.Add(emptyState);
            }

            foreach (var conv in _conversations)
            {
                var convButton = CreateConversationButton(conv);
                listStack.Children.Add(convButton);
            }

            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = listStack
            };
            mainGrid.Children.Add(scrollViewer);
            Grid.SetRow(scrollViewer, 2);

            ChatsContent.Children.Add(mainGrid);
        }

        private Border CreateConversationButton(Conversation conv)
        {
            var background = _appDarkModeEnabled
                ? (conv.HasUnread
                    ? new SolidColorBrush(Color.FromRgb(15, 43, 64))
                    : new SolidColorBrush(Color.FromRgb(17, 24, 39)))
                : (conv.HasUnread
                    ? new SolidColorBrush(Color.FromRgb(245, 250, 255))
                    : new SolidColorBrush(Colors.White));
            var borderBrush = _appDarkModeEnabled
                ? (conv.HasUnread
                    ? new SolidColorBrush(Color.FromRgb(56, 189, 248))
                    : new SolidColorBrush(Color.FromRgb(51, 65, 85)))
                : (conv.HasUnread
                    ? new SolidColorBrush(Color.FromRgb(191, 219, 254))
                    : new SolidColorBrush(Color.FromRgb(232, 232, 232)));
            var primaryText = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(241, 245, 249))
                : new SolidColorBrush(Color.FromRgb(51, 51, 51));
            var secondaryText = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(148, 163, 184))
                : new SolidColorBrush(Color.FromRgb(119, 119, 119));
            var timeText = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(100, 116, 139))
                : new SolidColorBrush(Color.FromRgb(153, 153, 153));

            var border = new Border
            {
                Background = background,
                BorderBrush = borderBrush,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(14, 14, 14, 14),
                Margin = new Thickness(0, 0, 0, 10),
                CornerRadius = new CornerRadius(14),
                Cursor = Cursors.Hand
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var avatarGrid = new Grid
            {
                Width = 48,
                Height = 48,
                Margin = new Thickness(0, 0, 12, 0)
            };

            avatarGrid.Children.Add(new Ellipse
            {
                Fill = conv.HasUnread
                    ? new SolidColorBrush(Color.FromRgb(14, 116, 144))
                    : new SolidColorBrush(Color.FromRgb(0, 120, 212))
            });

            avatarGrid.Children.Add(new TextBlock
            {
                Text = string.IsNullOrWhiteSpace(conv.ContactName) ? "?" : conv.ContactName[..1].ToUpperInvariant(),
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });

            Grid.SetColumn(avatarGrid, 0);
            grid.Children.Add(avatarGrid);

            // Conteúdo (nome + mensagem)
            var contentStack = new StackPanel { Orientation = Orientation.Vertical, VerticalAlignment = VerticalAlignment.Center };
            contentStack.Children.Add(new TextBlock
            {
                Text = conv.ContactName,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = primaryText
            });
            contentStack.Children.Add(new TextBlock
            {
                Text = conv.HasUnread ? $"• {conv.LastMessage}" : conv.LastMessage,
                FontSize = 12,
                Foreground = secondaryText,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Margin = new Thickness(0, 4, 0, 0),
                FontWeight = conv.HasUnread ? FontWeights.SemiBold : FontWeights.Normal
            });

            Grid.SetColumn(contentStack, 1);
            grid.Children.Add(contentStack);

            // Horário
            var timeBlock = new TextBlock
            {
                Text = conv.FormattedTime,
                FontSize = 11,
                Foreground = timeText,
                Margin = new Thickness(12, 0, 0, 0)
            };

            var rightStack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            rightStack.Children.Add(timeBlock);

            if (conv.HasUnread)
            {
                rightStack.Children.Add(new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(0, 120, 212)),
                    CornerRadius = new CornerRadius(8),
                    Width = 8,
                    Height = 8,
                    Margin = new Thickness(0, 8, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Right
                });
            }

            Grid.SetColumn(rightStack, 2);
            grid.Children.Add(rightStack);

            border.Child = grid;

            // Click para abrir conversa
            border.MouseUp += (s, e) =>
            {
                _selectedConversation = conv;
                conv.HasUnread = false;
                UpdateChatsBadge();
                RefreshChatsUI();
                
                // Carregar mensagens do Firebase em background
                _ = LoadConversationMessagesAsync(conv);
            };

            return border;
        }

        private void RenderChatScreen(Conversation conv)
        {
            var headerBackground = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(17, 27, 33))
                : new SolidColorBrush(Color.FromRgb(255, 255, 255));
            var headerBorderBrush = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(32, 44, 51))
                : new SolidColorBrush(Color.FromRgb(226, 232, 240));
            var headerPrimaryText = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(233, 237, 239))
                : new SolidColorBrush(Color.FromRgb(15, 23, 42));
            var headerSecondaryText = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(134, 150, 160))
                : new SolidColorBrush(Color.FromRgb(100, 116, 139));
            var actionForeground = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(134, 150, 160))
                : new SolidColorBrush(Color.FromRgb(71, 85, 105));
            var chatCanvasBackground = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(12, 19, 24))
                : new SolidColorBrush(Color.FromRgb(248, 250, 252));
            var composerBackground = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(17, 27, 33))
                : new SolidColorBrush(Color.FromRgb(255, 255, 255));
            var composerBorderBrush = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(32, 44, 51))
                : new SolidColorBrush(Color.FromRgb(226, 232, 240));
            var inputBackground = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(47, 61, 69))
                : new SolidColorBrush(Color.FromRgb(241, 245, 249));
            var inputForeground = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(233, 237, 239))
                : new SolidColorBrush(Color.FromRgb(15, 23, 42));

            var mainGrid = new Grid();
            mainGrid.Background = chatCanvasBackground;
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Header
            var headerBorder = new Border
            {
                Background = headerBackground,
                BorderBrush = headerBorderBrush,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(18, 14, 18, 14)
            };

            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var backButton = new Button
            {
                Content = "← Voltar",
                Background = new SolidColorBrush(Colors.Transparent),
                Foreground = actionForeground,
                FontSize = 12,
                Padding = new Thickness(10, 6, 10, 6),
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                Margin = new Thickness(0, 0, 12, 0)
            };
            backButton.Click += (s, e) =>
            {
                _selectedConversation = null;
                RefreshChatsUI();
            };

            Grid.SetColumn(backButton, 0);
            headerGrid.Children.Add(backButton);

            var avatarGrid = new Grid
            {
                Width = 42,
                Height = 42,
                Margin = new Thickness(0, 0, 14, 0)
            };
            avatarGrid.Children.Add(new Ellipse
            {
                Fill = new SolidColorBrush(Color.FromRgb(0, 168, 132))
            });

            avatarGrid.Children.Add(new TextBlock
            {
                Text = string.IsNullOrWhiteSpace(conv.ContactName) ? "?" : conv.ContactName[..1].ToUpperInvariant(),
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 17,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });

            Grid.SetColumn(avatarGrid, 1);
            headerGrid.Children.Add(avatarGrid);

            var titleStack = new StackPanel { Orientation = Orientation.Vertical, VerticalAlignment = VerticalAlignment.Center };
            titleStack.Children.Add(new TextBlock
            {
                Text = conv.ContactName,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = headerPrimaryText
            });
            titleStack.Children.Add(new TextBlock
            {
                Text = "Online agora • laboratório de colaboração ativo",
                FontSize = 11,
                Foreground = headerSecondaryText
            });
            Grid.SetColumn(titleStack, 2);
            headerGrid.Children.Add(titleStack);

            var actionPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };

            actionPanel.Children.Add(CreateHeaderIconButton("🎧", "Iniciar áudio"));
            actionPanel.Children.Add(CreateHeaderIconButton("📹", "Iniciar vídeo"));
            actionPanel.Children.Add(CreateHeaderIconButton("🔎", "Buscar na conversa"));

            var menuButton = CreateHeaderIconButton("⋯", "Mais ações");
            var actionsPopup = CreateChatActionsPopup(menuButton, conv);
            menuButton.Click += (s, e) =>
            {
                actionsPopup.IsOpen = !actionsPopup.IsOpen;
            };
            actionPanel.Children.Add(menuButton);

            Grid.SetColumn(actionPanel, 3);
            headerGrid.Children.Add(actionPanel);

            headerBorder.Child = headerGrid;
            mainGrid.Children.Add(headerBorder);
            Grid.SetRow(headerBorder, 0);

            // Mensagens
            var messagesList = new StackPanel { Orientation = Orientation.Vertical };
            foreach (var msg in conv.Messages)
            {
                messagesList.Children.Add(CreateMessageBubble(msg));
            }

            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = messagesList,
                Padding = new Thickness(18, 20, 18, 20),
                Background = chatCanvasBackground
            };
            mainGrid.Children.Add(scrollViewer);
            Grid.SetRow(scrollViewer, 1);

            // Input
            var inputBorder = new Border
            {
                Background = composerBackground,
                BorderBrush = composerBorderBrush,
                BorderThickness = new Thickness(0, 1, 0, 0),
                Padding = new Thickness(16, 14, 16, 16)
            };

            var composerStack = new StackPanel { Orientation = Orientation.Vertical };

            var quickActionPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 12)
            };
            quickActionPanel.Children.Add(CreateComposerChipButton("😊 Emoji"));
            quickActionPanel.Children.Add(CreateComposerChipButton("GIF"));
            quickActionPanel.Children.Add(CreateComposerChipButton("📷 Imagem"));
            quickActionPanel.Children.Add(CreateComposerChipButton("📎 Arquivo"));
            quickActionPanel.Children.Add(CreateComposerChipButton("🎤 Áudio"));
            composerStack.Children.Add(quickActionPanel);

            var inputGrid = new Grid();
            inputGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            inputGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            inputGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            inputGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            inputGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var emojiButton = CreateHeaderIconButton("😊", "Emoji");
            Grid.SetColumn(emojiButton, 0);
            inputGrid.Children.Add(emojiButton);

            var attachmentButton = CreateHeaderIconButton("＋", "Anexar mídia");
            Grid.SetColumn(attachmentButton, 1);
            inputGrid.Children.Add(attachmentButton);

            var inputBox = new TextBox
            {
                Background = inputBackground,
                Foreground = inputForeground,
                BorderBrush = inputBackground,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(14, 12, 14, 12),
                FontSize = 13,
                MinHeight = 48,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 10, 0),
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                MaxHeight = 100
            };
            Grid.SetColumn(inputBox, 2);
            inputGrid.Children.Add(inputBox);

            var micButton = CreateHeaderIconButton("🎙", "Gravar áudio");
            Grid.SetColumn(micButton, 3);
            inputGrid.Children.Add(micButton);

            var sendButton = new Button
            {
                Content = "➤",
                Background = new SolidColorBrush(Color.FromRgb(0, 168, 132)),
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 15,
                Width = 42,
                Height = 42,
                Padding = new Thickness(0, 0, 0, 0),
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                VerticalAlignment = VerticalAlignment.Center
            };

            sendButton.Click += async (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(inputBox.Text))
                {
                    var newMsg = new ChatMessage
                    {
                        SenderId = _currentProfile?.UserId ?? "self",
                        SenderName = _currentProfile?.Name ?? "Você",
                        Content = inputBox.Text,
                        Timestamp = DateTime.Now,
                        IsOwn = true
                    };
                    
                    // Salvar no Firebase
                    if (!string.IsNullOrEmpty(_idToken))
                    {
                        DebugHelper.WriteLine($"[SendButton] Enviando mensagem para Firebase");
                        var chatService = new ChatService(_idToken, _currentProfile?.UserId ?? "");
                        var sendResult = await chatService.SendMessageAsync(conv.ContactId, conv.ContactName, newMsg.SenderName, newMsg.Content);
                        
                        if (!sendResult.Success)
                        {
                            MessageBox.Show(
                                $"Erro ao enviar mensagem.\n\n{sendResult.ErrorMessage}\n\nLog salvo em:\n{DebugHelper.GetLogFilePath()}",
                                "Erro",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning
                            );
                            DebugHelper.WriteLine($"[SendButton] Falha ao salvar no Firebase: {sendResult.ErrorMessage}");
                            return;
                        }
                        
                        DebugHelper.WriteLine($"[SendButton] Mensagem salva no Firebase");
                    }
                    
                    conv.Messages.Add(newMsg);
                    conv.LastMessage = inputBox.Text;
                    conv.LastMessageTime = DateTime.Now;
                    conv.LastSenderId = _currentProfile?.UserId ?? "";
                    conv.LastReadAt = DateTime.Now;
                    conv.HasUnread = false;
                    UpdateChatsBadge();
                    inputBox.Clear();
                    RefreshChatsUI();
                }
            };

            Grid.SetColumn(sendButton, 4);
            inputGrid.Children.Add(sendButton);

            composerStack.Children.Add(inputGrid);
            inputBorder.Child = composerStack;
            mainGrid.Children.Add(inputBorder);
            Grid.SetRow(inputBorder, 2);

            ChatsContent.Children.Add(mainGrid);
        }

        private Border CreateMessageBubble(ChatMessage msg)
        {
            var ownBubbleBackground = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(0, 92, 75))
                : new SolidColorBrush(Color.FromRgb(219, 234, 254));
            var otherBubbleBackground = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(32, 44, 51))
                : new SolidColorBrush(Color.FromRgb(255, 255, 255));
            var ownTextBrush = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(233, 237, 239))
                : new SolidColorBrush(Color.FromRgb(15, 23, 42));
            var otherTextBrush = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(233, 237, 239))
                : new SolidColorBrush(Color.FromRgb(30, 41, 59));
            var ownTimeBrush = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(167, 220, 197))
                : new SolidColorBrush(Color.FromRgb(37, 99, 235));
            var otherTimeBrush = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(134, 150, 160))
                : new SolidColorBrush(Color.FromRgb(100, 116, 139));

            var container = new Border
            {
                Margin = new Thickness(0, 6, 0, 6),
                HorizontalAlignment = msg.IsOwn ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                MaxWidth = 520
            };

            var bubble = new Border
            {
                Background = msg.IsOwn 
                    ? ownBubbleBackground
                    : otherBubbleBackground,
                CornerRadius = msg.IsOwn
                    ? new CornerRadius(16, 16, 4, 16)
                    : new CornerRadius(16, 16, 16, 4),
                Padding = new Thickness(14, 10, 14, 10)
            };

            var stack = new StackPanel { Orientation = Orientation.Vertical };
            if (!msg.IsOwn)
            {
                stack.Children.Add(new TextBlock
                {
                    Text = msg.SenderName,
                    FontSize = 11,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = _appDarkModeEnabled
                        ? new SolidColorBrush(Color.FromRgb(83, 189, 235))
                        : new SolidColorBrush(Color.FromRgb(14, 116, 144)),
                    Margin = new Thickness(0, 0, 0, 4)
                });
            }

            stack.Children.Add(new TextBlock
            {
                Text = msg.Content,
                FontSize = 13,
                Foreground = msg.IsOwn 
                    ? ownTextBrush
                    : otherTextBrush,
                TextWrapping = TextWrapping.Wrap
            });
            stack.Children.Add(new TextBlock
            {
                Text = msg.Timestamp.ToString("HH:mm"),
                FontSize = 10,
                Foreground = msg.IsOwn 
                    ? ownTimeBrush
                    : otherTimeBrush,
                Margin = new Thickness(0, 6, 0, 0),
                TextAlignment = TextAlignment.Right
            });

            bubble.Child = stack;
            container.Child = bubble;
            return container;
        }

        private async Task LoadActiveConversationsAsync()
        {
            try
            {
                DebugHelper.WriteLine("[LoadActiveConversations] Carregando conversas ativas do Firebase");

                var chatService = new ChatService(_idToken, _currentProfile?.UserId ?? "");
                var conversations = await chatService.LoadConversationsAsync();

                _conversations = conversations;

                if (_selectedConversation != null)
                {
                    _selectedConversation = _conversations.FirstOrDefault(c => c.ContactId == _selectedConversation.ContactId);
                }

                UpdateChatsBadge();
                RefreshChatsUI();
                DebugHelper.WriteLine($"[LoadActiveConversations] {conversations.Count} conversas carregadas");
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[LoadActiveConversations ERROR] {ex.Message}");
            }
        }

        private void RenderChatsLoadingState()
        {
            var primaryText = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(241, 245, 249))
                : new SolidColorBrush(Color.FromRgb(51, 51, 51));
            var secondaryText = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(148, 163, 184))
                : new SolidColorBrush(Color.FromRgb(100, 116, 139));

            ChatsContent.Children.Clear();
            ChatsContent.Children.Add(new TextBlock
            {
                Text = "💬 Conversas Ativas",
                FontSize = 20,
                FontWeight = FontWeights.ExtraBold,
                Margin = new Thickness(0, 0, 0, 8),
                Foreground = primaryText
            });
            ChatsContent.Children.Add(new TextBlock
            {
                Text = "Atualizando suas conversas recentes...",
                FontSize = 12,
                Foreground = secondaryText
            });
        }

        private void UpdateChatsBadge()
        {
            var unreadCount = _conversations.Count(conversation => conversation.HasUnread);

            if (unreadCount > 0)
            {
                ChatsUnreadBadge.Visibility = Visibility.Visible;
                ChatsUnreadCountText.Text = unreadCount > 99 ? "99+" : unreadCount.ToString();
                ChatsNavButton.Background = _appDarkModeEnabled
                    ? new SolidColorBrush(Color.FromRgb(8, 47, 73))
                    : new SolidColorBrush(Color.FromRgb(239, 246, 255));
            }
            else
            {
                ChatsUnreadBadge.Visibility = Visibility.Collapsed;
                ChatsUnreadCountText.Text = "0";
                ChatsNavButton.Background = new SolidColorBrush(Colors.Transparent);
            }
        }

        private Button CreateHeaderIconButton(string content, string tooltip)
        {
            return new Button
            {
                Content = content,
                ToolTip = tooltip,
                Background = new SolidColorBrush(Colors.Transparent),
                Foreground = _appDarkModeEnabled
                    ? new SolidColorBrush(Color.FromRgb(134, 150, 160))
                    : new SolidColorBrush(Color.FromRgb(71, 85, 105)),
                BorderThickness = new Thickness(0),
                Width = 34,
                Height = 34,
                Margin = new Thickness(4, 0, 0, 0),
                Cursor = Cursors.Hand,
                FontSize = 15,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center
            };
        }

        private Button CreateComposerChipButton(string content)
        {
            return new Button
            {
                Content = content,
                Background = _appDarkModeEnabled
                    ? new SolidColorBrush(Color.FromRgb(32, 44, 51))
                    : new SolidColorBrush(Color.FromRgb(241, 245, 249)),
                Foreground = _appDarkModeEnabled
                    ? new SolidColorBrush(Color.FromRgb(233, 237, 239))
                    : new SolidColorBrush(Color.FromRgb(30, 41, 59)),
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                Padding = new Thickness(12, 6, 12, 6),
                Margin = new Thickness(0, 0, 8, 0),
                FontSize = 11,
                FontWeight = FontWeights.SemiBold
            };
        }

        private Popup CreateChatActionsPopup(Button anchorButton, Conversation conv)
        {
            var popupBorderBrush = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(51, 65, 85))
                : new SolidColorBrush(Color.FromRgb(226, 232, 240));
            var popupBackground = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(15, 23, 42))
                : new SolidColorBrush(Colors.White);
            var titleBrush = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(241, 245, 249))
                : new SolidColorBrush(Color.FromRgb(15, 23, 42));
            var subtitleBrush = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(148, 163, 184))
                : new SolidColorBrush(Color.FromRgb(100, 116, 139));
            var separatorBrush = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(30, 41, 59))
                : new SolidColorBrush(Color.FromRgb(241, 245, 249));

            var popupContent = new StackPanel();
            popupContent.Children.Add(new TextBlock
            {
                Text = "Central de ações",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = titleBrush,
                Margin = new Thickness(0, 0, 0, 4)
            });
            popupContent.Children.Add(new TextBlock
            {
                Text = $"Gerencie a conversa com {conv.ContactName} sem sair do chat.",
                FontSize = 11,
                TextWrapping = TextWrapping.Wrap,
                Foreground = subtitleBrush,
                Margin = new Thickness(0, 0, 0, 14)
            });

            var popup = new Popup
            {
                PlacementTarget = anchorButton,
                Placement = PlacementMode.Bottom,
                HorizontalOffset = -240,
                VerticalOffset = 10,
                AllowsTransparency = true,
                StaysOpen = false
            };

            Button CreateActionButton(string icon, string title, string subtitle, Color accentColor, Action action)
            {
                var button = new Button
                {
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(0),
                    Margin = new Thickness(0, 0, 0, 8),
                    Cursor = Cursors.Hand,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch
                };

                var idleBackground = _appDarkModeEnabled
                    ? new SolidColorBrush(Color.FromRgb(18, 30, 49))
                    : new SolidColorBrush(Color.FromRgb(248, 250, 252));
                var hoverBackground = _appDarkModeEnabled
                    ? new SolidColorBrush(Color.FromRgb(30, 41, 59))
                    : new SolidColorBrush(Color.FromRgb(239, 246, 255));

                var cardBorder = new Border
                {
                    Background = idleBackground,
                    BorderBrush = popupBorderBrush,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(16),
                    Padding = new Thickness(14)
                };

                var cardGrid = new Grid();
                cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var iconShell = new Border
                {
                    Width = 42,
                    Height = 42,
                    CornerRadius = new CornerRadius(14),
                    Background = new SolidColorBrush(accentColor),
                    Margin = new Thickness(0, 0, 12, 0),
                    Child = new TextBlock
                    {
                        Text = icon,
                        FontSize = 16,
                        Foreground = Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        TextAlignment = TextAlignment.Center
                    }
                };
                Grid.SetColumn(iconShell, 0);
                cardGrid.Children.Add(iconShell);

                var textStack = new StackPanel { Orientation = Orientation.Vertical, VerticalAlignment = VerticalAlignment.Center };
                textStack.Children.Add(new TextBlock
                {
                    Text = title,
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = titleBrush
                });
                textStack.Children.Add(new TextBlock
                {
                    Text = subtitle,
                    FontSize = 10.5,
                    Margin = new Thickness(0, 4, 0, 0),
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = subtitleBrush
                });
                Grid.SetColumn(textStack, 1);
                cardGrid.Children.Add(textStack);

                var arrow = new TextBlock
                {
                    Text = "›",
                    FontSize = 18,
                    FontWeight = FontWeights.Bold,
                    Foreground = subtitleBrush,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(12, 0, 0, 0)
                };
                Grid.SetColumn(arrow, 2);
                cardGrid.Children.Add(arrow);

                cardBorder.Child = cardGrid;
                button.Content = cardBorder;
                button.MouseEnter += (s, e) => cardBorder.Background = hoverBackground;
                button.MouseLeave += (s, e) => cardBorder.Background = idleBackground;
                button.Click += (s, e) =>
                {
                    popup.IsOpen = false;
                    action();
                };

                return button;
            }

            popupContent.Children.Add(CreateActionButton(
                "🔕",
                "Silenciar conversa",
                "Pausa alertas desta conversa por 8 horas para reduzir distrações.",
                Color.FromRgb(14, 165, 233),
                () => MessageBox.Show(
                    $"As notificações da conversa com {conv.ContactName} foram silenciadas por 8 horas.",
                    "Conversa silenciada",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information)));

            popupContent.Children.Add(CreateActionButton(
                "📌",
                "Fixar no topo",
                "Mantém este contato destacado no início da lista de conversas.",
                Color.FromRgb(59, 130, 246),
                () => MessageBox.Show(
                    $"A conversa com {conv.ContactName} foi marcada como prioritária.",
                    "Conversa fixada",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information)));

            popupContent.Children.Add(new Border
            {
                Height = 1,
                Background = separatorBrush,
                Margin = new Thickness(4, 4, 4, 12)
            });

            popupContent.Children.Add(CreateActionButton(
                "🛡",
                "Denunciar ao professor",
                "Encaminha um alerta acadêmico com o contexto desta conversa.",
                Color.FromRgb(245, 158, 11),
                () => MessageBox.Show(
                    $"Um aviso foi encaminhado ao professor responsável sobre a conversa com {conv.ContactName}.",
                    "Encaminhamento acadêmico",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information)));

            popupContent.Children.Add(CreateActionButton(
                "🗂",
                "Exportar histórico",
                "Prepara esta conversa para compartilhamento e auditoria do projeto.",
                Color.FromRgb(16, 185, 129),
                () => MessageBox.Show(
                    $"A exportação do histórico com {conv.ContactName} foi preparada para a próxima etapa de implementação.",
                    "Exportação iniciada",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information)));

            popup.Child = new Border
            {
                Width = 320,
                Background = popupBackground,
                BorderBrush = popupBorderBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(22),
                Padding = new Thickness(16),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    BlurRadius = 28,
                    ShadowDepth = 8,
                    Opacity = 0.22,
                    Color = Colors.Black
                },
                Child = popupContent
            };

            return popup;
        }
    }
}