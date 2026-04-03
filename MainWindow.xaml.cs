using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using Microsoft.Win32;

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
        private const string AvatarDefaultBody = "PelePardo";
        private static readonly string[] LegacyAvatarBodyOptions = { "1", "2", "3" };
        private static readonly string[] LegacyAvatarHairOptions = { "1", "2", "3", "4", "5", "6" };
        private static readonly string[] AvatarBodyOptions = { "PeleBranca", "PelePardo", "PelePreto" };
        private static readonly string[] AvatarHairOptions =
        {
            "FEMALE_CastanhoEscuro_Curto_0",
            "FEMALE_CastanhoEscuro_Curto_1",
            "FEMALE_Castanho_Curto_0",
            "FEMALE_Castanho_Curto_1",
            "FEMALE_Loiro_Curto_0",
            "FEMALE_Loiro_Curto_1",
            "FEMALE_Preto_Curto_0",
            "FEMALE_Preto_Curto_1",
            "FEMALE_Ruivo_Curto_0",
            "FEMALE_Ruivo_Curto_1",
            "Male_CastanhoClaro_Curto_0",
            "Male_CastanhoEscuro_Curto_0",
            "Male_CastanhoEscuro_Curto_1",
            "Male_Loiro_Curto_0",
            "Male_Loiro_Curto_1",
            "Male_Preto_Curto_0",
            "Male_Preto_Curto_1",
            "Male_Ruivo_Curto_0",
            "Male_Ruivo_Curto_1",
            "FEMALE_CastanhoEscuro_Medio_0",
            "FEMALE_CastanhoEscuro_Medio_1",
            "FEMALE_Castanho_Medio_0",
            "FEMALE_Castanho_Medio_1",
            "FEMALE_Loiro_medio_0",
            "FEMALE_Loiro_medio_1",
            "FEMALE_Preto_Medio_0",
            "FEMALE_Preto_Medio_1",
            "FEMALE_Ruivo_Medio_0",
            "FEMALE_Ruivo_Medio_1",
            "Male_CastanhoClaro_Medio_0",
            "Male_CastanhoClaro_Medio_1",
            "Male_CastanhoEscuro_Medio_0",
            "Male_CastanhoEscuro_Medio_1",
            "Male_Loiro_Medio_0",
            "Male_Loiro_Medio_1",
            "Male_Preto_Medio_0",
            "Male_Preto_Medio_1",
            "Male_Ruivo_Medio_0",
            "Male_Ruivo_Medio_1",
            "FEMALE_CastanhoEscuro_Longo_0",
            "FEMALE_CastanhoEscuro_Longo_1",
            "FEMALE_Castanho_Longo_0",
            "FEMALE_Castanho_Longo_1",
            "FEMALE_Loiro_Longo_0",
            "FEMALE_Loiro_Longo_1",
            "FEMALE_Preto_Longo_0",
            "FEMALE_Preto_Longo_1",
            "FEMALE_Ruivo_Longo_0",
            "FEMALE_Ruivo_Longo_1"
        };
        private static readonly string[] AvatarHatOptions =
        {
            "Hat_0", "Hat_1", "Hat_2", "Hat_3", "Hat_4", "Hat_5", "Hat_6", "Hat_7",
            "Hat_8", "Hat_9", "Hat_10", "Hat_11", "Hat_12", "Hat_16"
        };
        private static readonly string[] AvatarAccessoryOptions = { "1", "2", "3" };
        private static readonly string[] AvatarClothingOptions =
        {
            "Roupa_Azul",
            "Roupa_AzulEscuro",
            "Roupa_Branca",
            "Roupa_Laranja",
            "Roupa_Preta",
            "Roupa_Rosa",
            "Roupa_Roxa",
            "Roupa_Vermelha"
        };
        private static readonly string[] ChatStickerAssets =
        {
            "Chao_0.png",
            "Chao_1.png",
            "chao_2.png",
            "chao_3.png",
            "chao_4.png",
            "chao_5.png",
            "chao_6.png"
        };
        private static readonly string[] KnownTeamCourses =
        {
            "Analise e Desenvolvimento de Sistemas",
            "Ciencia da Computacao",
            "Engenharia de Software",
            "Sistemas de Informacao",
            "Redes de Computadores",
            "Gestao da Tecnologia da Informacao"
        };
        private static readonly string[] KnownTeamClasses =
        {
            "Turma A - Manha",
            "Turma B - Tarde",
            "Turma C - Noite",
            "Turma ADS 2026.1",
            "Turma PI Lab",
            "Turma Integradores"
        };
        private static readonly string[] KnownTeamUcs =
        {
            "Projeto Integrador",
            "Modelagem de Software",
            "Banco de Dados",
            "Experiencia do Usuario",
            "Programacao Web",
            "Gestao de Projetos",
            "Arquitetura de Software",
            "Qualidade de Software"
        };
        private static readonly string[] KnownTeamMembers =
        {
            "Ana Carolina",
            "Bruno Silva",
            "Camila Rocha",
            "Diego Costa",
            "Fernanda Lima",
            "Gabriel Souza",
            "Julia Martins",
            "Lucas Pereira",
            "Mariana Alves",
            "Pedro Henrique",
            "Rafaela Gomes"
        };
        private UserProfile? _currentProfile;
        private string _idToken = "";
        private List<Conversation> _conversations = new List<Conversation>();
        private readonly List<string> _selectedProgrammingLanguages = new List<string>();
        private readonly List<UserInfo> _draftTeamMembers = new List<UserInfo>();
        private readonly List<string> _draftTeamUcs = new List<string>();
        private readonly List<UserInfo> _teamMemberSearchResults = new List<UserInfo>();
        private readonly List<TeamWorkspaceInfo> _teamWorkspaces = new List<TeamWorkspaceInfo>();
        private int _teamMemberSearchVersion = 0;
        private bool _suppressTeamMemberSearch = false;
        private TeamBoardView _activeTeamBoardView = TeamBoardView.Trello;
        private TeamTaskCardInfo? _draggedTeamTaskCard = null;
        private Conversation? _selectedConversation = null;
        private TeamWorkspaceInfo? _activeTeamWorkspace = null;
        private TeamEntryMode _teamEntryMode = TeamEntryMode.None;
        private string _chatListFilter = string.Empty;
        private bool _appDarkModeEnabled = false;
        private TeamService? _teamService = null;
        private ConnectionService? _connectionService = null;
        private readonly HashSet<string> _connectedUserIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, Task<UserInfo?>> _userInfoCache = new ConcurrentDictionary<string, Task<UserInfo?>>(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, Task<UserProfile?>> _userProfileCache = new ConcurrentDictionary<string, Task<UserProfile?>>(StringComparer.OrdinalIgnoreCase);
        private List<UserConnectionInfo> _connectionEntries = new List<UserConnectionInfo>();
        private List<UserInfo> _searchSlideResults = new List<UserInfo>();
        private string _searchSlideQuery = string.Empty;
        private int _searchSlideTypingVersion = 0;
        private int _searchSlideRequestVersion = 0;
        private int _teamSyncSequence = 0;

        public MainWindow()
        {
            InitializeComponent();
            ProgrammingLanguageInput.ItemsSource = KnownProgrammingLanguages.OrderBy(language => language).ToList();
            InitializeTeamsUi();
            ApplyAppTheme();
            this.KeyDown += MainWindow_KeyDown;
            this.Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DebugHelper.WriteLine("MainWindow carregada");

            if (!string.IsNullOrWhiteSpace(_idToken) && _currentProfile != null)
            {
                // Inicializar TeamService
                _teamService = new TeamService(_idToken, _currentProfile.UserId ?? "");
                _connectionService = new ConnectionService(_idToken, _currentProfile);

                RenderChatsLoadingState();
                await Task.WhenAll(
                    LoadActiveConversationsAsync(),
                    LoadTeamsFromDatabaseAsync(),
                    RefreshConnectionsCacheAsync());
            }
            else
            {
                RefreshChatsUI();
                UpdateConnectionsBadge();
            }
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && SearchSlidePanel.Visibility == Visibility.Visible)
            {
                HideSearchSlidePanel();
                e.Handled = true;
                return;
            }

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

        private void SearchFriendsBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _searchSlideTypingVersion++;
                SearchFriends_Click(sender, new RoutedEventArgs());
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Escape && SearchSlidePanel.Visibility == Visibility.Visible)
            {
                HideSearchSlidePanel();
                e.Handled = true;
            }
        }

        private async void SearchFriendsBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var query = SearchFriendsBox.Text?.Trim() ?? string.Empty;
            var typingVersion = ++_searchSlideTypingVersion;

            if (string.IsNullOrWhiteSpace(query))
            {
                HideSearchSlidePanel();
                return;
            }

            await Task.Delay(260);
            if (typingVersion != _searchSlideTypingVersion)
            {
                return;
            }

            await ExecuteSearchFriendsAsync(query, true);
        }

        public MainWindow(UserProfile profile) : this()
        {
            _currentProfile = profile;
            ApplyUserProfile(profile);
        }

        private void ApplyUserProfile(UserProfile profile)
        {
            _currentProfile = profile;
            if (!string.IsNullOrWhiteSpace(profile.UserId))
            {
                _userProfileCache[profile.UserId] = Task.FromResult<UserProfile?>(profile);
                _userInfoCache[profile.UserId] = Task.FromResult<UserInfo?>(new UserInfo
                {
                    UserId = profile.UserId,
                    Name = profile.Name,
                    Email = profile.Email,
                    Registration = profile.Registration,
                    Course = profile.Course,
                    Phone = profile.Phone,
                    Nickname = profile.Nickname,
                    ProfessionalTitle = profile.ProfessionalTitle,
                    Bio = profile.Bio,
                    Skills = profile.Skills,
                    ProgrammingLanguages = profile.ProgrammingLanguages,
                    PortfolioLink = profile.PortfolioLink,
                    LinkedInLink = profile.LinkedInLink,
                    AvatarBody = profile.AvatarBody,
                    AvatarHair = profile.AvatarHair,
                    AvatarHat = profile.AvatarHat,
                    AvatarAccessory = profile.AvatarAccessory,
                    AvatarClothing = profile.AvatarClothing
                });
            }

            WelcomeText.Text = $"Bem-vindo, {profile.Name}";
            SidebarUserName.Text = profile.Name;
            SidebarUserEmail.Text = profile.Email;
            SidebarUserRole.Text = $"{profile.Course} | Matrícula: {profile.Registration}";
            PopulateProfessionalProfileFields(profile);
            RefreshAvatarUi(profile);
            SyncTeamDefaultsWithProfile(profile);
        }

        public void UpdateUserProfile(UserProfile profile)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => UpdateUserProfile(profile));
                return;
            }

            ApplyUserProfile(profile);
        }

        private enum TeamEntryMode
        {
            None,
            Join,
            Create
        }

        private enum TeamBoardView
        {
            Trello,
            Kanban,
            Csd
        }

        private void InitializeTeamsUi()
        {
            TeamCourseComboBox.ItemsSource = KnownTeamCourses.OrderBy(item => item).ToList();
            TeamClassComboBox.ItemsSource = KnownTeamClasses.OrderBy(item => item).ToList();
            TeamUcInput.ItemsSource = KnownTeamUcs.OrderBy(item => item).ToList();
            TeamMemberInput.ItemsSource = _teamMemberSearchResults;
            TeamMemberInput.AddHandler(TextBox.TextChangedEvent, new TextChangedEventHandler(TeamMemberInput_TextChanged));

            TeamClassComboBox.Text = KnownTeamClasses.First();
            TeamClassIdTextBox.Text = "TURMA-PI-001";
            TeamCreationStatusText.Text = string.Empty;
            TeamJoinStatusText.Text = string.Empty;
            TeamEntryOptionsPopup.IsOpen = false;

            RenderTeamMembersDraft();
            RenderTeamUcsDraft();
            RenderTeamsList();
            UpdateTeamsViewState();
        }

        private void SyncTeamDefaultsWithProfile(UserProfile profile)
        {
            TeamCourseComboBox.Text = string.IsNullOrWhiteSpace(profile.Course)
                ? TeamCourseComboBox.Text
                : profile.Course;

            if (string.IsNullOrWhiteSpace(TeamNameTextBox.Text))
            {
                TeamNameTextBox.Text = $"Equipe {profile.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "Projeto"}";
            }

            EnsureCurrentUserInTeamDraft();
        }

        private void EnsureCurrentUserInTeamDraft()
        {
            var currentUser = CreateCurrentUserInfo();
            if (currentUser == null)
            {
                return;
            }

            if (_draftTeamMembers.Any(member => string.Equals(member.UserId, currentUser.UserId, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            _draftTeamMembers.Insert(0, currentUser);
            RenderTeamMembersDraft();
        }

        private string NormalizeTeamValue(string value)
        {
            return value?.Trim() ?? string.Empty;
        }

        private UserInfo? CreateCurrentUserInfo()
        {
            if (_currentProfile == null || string.IsNullOrWhiteSpace(_currentProfile.Name))
            {
                return null;
            }

            return new UserInfo
            {
                UserId = string.IsNullOrWhiteSpace(_currentProfile.UserId) ? "current-user" : _currentProfile.UserId,
                Name = _currentProfile.Name,
                Email = _currentProfile.Email,
                Phone = _currentProfile.Phone,
                Registration = _currentProfile.Registration,
                Course = _currentProfile.Course,
                Nickname = _currentProfile.Nickname,
                ProfessionalTitle = _currentProfile.ProfessionalTitle,
                Bio = _currentProfile.Bio,
                Skills = _currentProfile.Skills,
                ProgrammingLanguages = _currentProfile.ProgrammingLanguages,
                PortfolioLink = _currentProfile.PortfolioLink,
                LinkedInLink = _currentProfile.LinkedInLink,
                AvatarBody = _currentProfile.AvatarBody,
                AvatarHair = _currentProfile.AvatarHair,
                AvatarHat = _currentProfile.AvatarHat,
                AvatarAccessory = _currentProfile.AvatarAccessory,
                AvatarClothing = _currentProfile.AvatarClothing
            };
        }

        private UserInfo CreateUserFromConnection(UserConnectionInfo item)
        {
            return new UserInfo
            {
                UserId = item.ConnectedUserId,
                Name = item.ConnectedUserName,
                Email = item.ConnectedUserEmail
            };
        }

        private string GetTeamMemberChipLabel(UserInfo member)
        {
            var registration = string.IsNullOrWhiteSpace(member.Registration) ? "Sem matricula" : member.Registration;
            return $"{member.Name} ({registration})";
        }

        private List<TeamTaskColumnInfo> CreateDefaultTeamColumns()
        {
            return new List<TeamTaskColumnInfo>
            {
                new TeamTaskColumnInfo
                {
                    Title = "Backlog",
                    AccentColor = Color.FromRgb(59, 130, 246),
                    Cards = new List<TeamTaskCardInfo>()
                },
                new TeamTaskColumnInfo
                {
                    Title = "Em andamento",
                    AccentColor = Color.FromRgb(245, 158, 11),
                    Cards = new List<TeamTaskCardInfo>()
                },
                new TeamTaskColumnInfo
                {
                    Title = "Revisao",
                    AccentColor = Color.FromRgb(168, 85, 247),
                    Cards = new List<TeamTaskCardInfo>()
                },
                new TeamTaskColumnInfo
                {
                    Title = "Concluido",
                    AccentColor = Color.FromRgb(16, 185, 129),
                    Cards = new List<TeamTaskCardInfo>()
                }
            };
        }

        private TeamCsdBoardInfo CreateDefaultCsdBoard()
        {
            return new TeamCsdBoardInfo
            {
                Certainties = new List<string>(),
                Assumptions = new List<string>(),
                Doubts = new List<string>()
            };
        }

        private List<TeamMilestoneInfo> CreateDefaultMilestones()
        {
            return new List<TeamMilestoneInfo>();
        }

        private TeamWorkspaceInfo EnsureTeamWorkspaceDefaults(TeamWorkspaceInfo team)
        {
            team.TeamId = string.IsNullOrWhiteSpace(team.TeamId)
                ? TeamService.GenerateTeamId(team.ClassId, team.TeamName)
                : TeamService.NormalizeTeamCode(team.TeamId);
            team.CreatedBy = string.IsNullOrWhiteSpace(team.CreatedBy)
                ? _currentProfile?.UserId ?? string.Empty
                : team.CreatedBy;
            if (team.CreatedAt == default)
            {
                team.CreatedAt = DateTime.Now;
            }
            if (team.UpdatedAt == default)
            {
                team.UpdatedAt = DateTime.Now;
            }

            team.Members = team.Members
                .Where(member => member != null && !string.IsNullOrWhiteSpace(member.UserId))
                .GroupBy(member => member.UserId, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .OrderBy(member => member.Name)
                .ToList();

            team.Ucs = team.Ucs
                .Where(uc => !string.IsNullOrWhiteSpace(uc))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(uc => uc)
                .ToList();

            team.Assets ??= new List<TeamAssetInfo>();
            team.Milestones ??= new List<TeamMilestoneInfo>();
            team.Notifications ??= new List<TeamNotificationInfo>();
            team.ChatMessages ??= new List<TeamChatMessageInfo>();
            team.ProjectProgress = Math.Max(0, Math.Min(100, team.ProjectProgress));
            team.ProjectStatus = string.IsNullOrWhiteSpace(team.ProjectStatus) ? "Planejamento" : team.ProjectStatus;

            if (team.TaskColumns == null || team.TaskColumns.Count == 0)
            {
                team.TaskColumns = CreateDefaultTeamColumns();
            }

            if (team.CsdBoard == null)
            {
                team.CsdBoard = CreateDefaultCsdBoard();
            }
            else
            {
                team.CsdBoard.Certainties ??= new List<string>();
                team.CsdBoard.Assumptions ??= new List<string>();
                team.CsdBoard.Doubts ??= new List<string>();
            }

            if (team.Milestones.Count == 0)
            {
                team.Milestones = CreateDefaultMilestones();
            }

            return team;
        }

        private string GetCurrentUserId()
        {
            return _currentProfile?.UserId ?? string.Empty;
        }

        private bool IsCurrentUserTeamCreator(TeamWorkspaceInfo team)
        {
            return !string.IsNullOrWhiteSpace(team.CreatedBy)
                && string.Equals(team.CreatedBy, GetCurrentUserId(), StringComparison.OrdinalIgnoreCase);
        }

        private int CalculateTeamProgressPercentage(TeamWorkspaceInfo team)
        {
            return Math.Max(0, Math.Min(100, team.ProjectProgress));
        }

        private string GetNextTeamDeadlineLabel(TeamWorkspaceInfo team)
        {
            if (team.ProjectDeadline.HasValue)
            {
                return FormatRelativeDate(team.ProjectDeadline.Value);
            }

            var nextDate = team.TaskColumns
                .SelectMany(column => column.Cards)
                .Where(card => card.DueDate.HasValue)
                .Select(card => card.DueDate!.Value)
                .OrderBy(date => date)
                .FirstOrDefault();

            return nextDate == default ? "Sem prazo definido" : FormatRelativeDate(nextDate);
        }

        private string GetProjectDeadlineText(TeamWorkspaceInfo team)
        {
            return team.ProjectDeadline.HasValue
                ? $"{team.ProjectDeadline.Value:dd/MM/yyyy} ({FormatRelativeDate(team.ProjectDeadline.Value)})"
                : "Nao definido";
        }

        private void AddTeamNotification(TeamWorkspaceInfo team, string message)
        {
            team.Notifications.Insert(0, new TeamNotificationInfo
            {
                Message = message,
                CreatedAt = DateTime.Now
            });

            if (team.Notifications.Count > 10)
            {
                team.Notifications = team.Notifications.Take(10).ToList();
            }
        }

        private string FormatRelativeDate(DateTime date)
        {
            var difference = date.Date - DateTime.Today;
            if (difference.TotalDays == 0)
            {
                return "Hoje";
            }

            if (difference.TotalDays == 1)
            {
                return "Amanha";
            }

            if (difference.TotalDays == -1)
            {
                return "Ontem";
            }

            return date.ToString("dd/MM");
        }

        private Border CreateDraftChip(string text, Brush background, Brush foreground, Action? onRemove = null)
        {
            var content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };

            content.Children.Add(new TextBlock
            {
                Text = text,
                Foreground = foreground,
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center
            });

            if (onRemove != null)
            {
                var removeButton = new Button
                {
                    Content = "x",
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
                removeButton.Click += (sender, args) => onRemove();
                content.Children.Add(removeButton);
            }

            return new Border
            {
                Background = background,
                CornerRadius = new CornerRadius(16),
                Padding = new Thickness(10, 6, 10, 6),
                Margin = new Thickness(0, 0, 8, 8),
                Child = content
            };
        }

        private Border CreateMemberChip(UserInfo member, Brush background, Brush foreground, Action? onRemove = null)
        {
            var content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };

            content.Children.Add(new Border
            {
                Width = 28,
                Height = 28,
                CornerRadius = new CornerRadius(14),
                Margin = new Thickness(0, 0, 8, 0),
                ClipToBounds = true,
                Child = CreateUserAvatarVisual(member, 28)
            });

            content.Children.Add(new TextBlock
            {
                Text = string.IsNullOrWhiteSpace(member.Name) ? member.Email : member.Name,
                Foreground = foreground,
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center
            });

            if (onRemove != null)
            {
                var removeButton = new Button
                {
                    Content = "x",
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
                removeButton.Click += (sender, args) => onRemove();
                content.Children.Add(removeButton);
            }

            return new Border
            {
                Background = background,
                CornerRadius = new CornerRadius(18),
                Padding = new Thickness(8, 6, 10, 6),
                Margin = new Thickness(0, 0, 8, 8),
                Child = content
            };
        }

        private void RenderTeamMembersDraft()
        {
            TeamMembersPanel.Children.Clear();

            if (_draftTeamMembers.Count == 0)
            {
                TeamMembersPanel.Children.Add(new TextBlock
                {
                    Text = "Nenhum aluno adicionado ainda.",
                    Foreground = GetThemeBrush("SecondaryTextBrush"),
                    FontSize = 12
                });
                return;
            }

            foreach (var member in _draftTeamMembers.OrderBy(item => item.Name))
            {
                TeamMembersPanel.Children.Add(CreateMemberChip(
                    member,
                    GetThemeBrush("AccentMutedBrush"),
                    GetThemeBrush("AccentBrush"),
                    () =>
                    {
                        _draftTeamMembers.RemoveAll(item => string.Equals(item.UserId, member.UserId, StringComparison.OrdinalIgnoreCase));
                        EnsureCurrentUserInTeamDraft();
                        RenderTeamMembersDraft();
                    }));
            }
        }

        private void RenderTeamUcsDraft()
        {
            TeamUcsPanel.Children.Clear();

            if (_draftTeamUcs.Count == 0)
            {
                TeamUcsPanel.Children.Add(new TextBlock
                {
                    Text = "Nenhuma UC adicionada ainda.",
                    Foreground = GetThemeBrush("SecondaryTextBrush"),
                    FontSize = 12
                });
                return;
            }

            foreach (var uc in _draftTeamUcs.OrderBy(item => item))
            {
                TeamUcsPanel.Children.Add(CreateDraftChip(
                    uc,
                    GetThemeBrush("MutedCardBackgroundBrush"),
                    GetThemeBrush("PrimaryTextBrush"),
                    () =>
                    {
                        _draftTeamUcs.RemoveAll(item => string.Equals(item, uc, StringComparison.OrdinalIgnoreCase));
                        RenderTeamUcsDraft();
                    }));
            }
        }

        private void RenderTeamWorkspace()
        {
            if (_activeTeamWorkspace == null)
            {
                TeamWorkspaceHost.Content = null;
                UpdateTeamsViewState();
                return;
            }

            EnsureTeamWorkspaceDefaults(_activeTeamWorkspace);
            TeamWorkspaceHost.Content = CreateTeamWorkspaceContent(_activeTeamWorkspace);

            UpdateTeamsViewState();
        }

        private UIElement CreateTeamWorkspaceContent(TeamWorkspaceInfo team)
        {
            var root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var header = CreateTeamWorkspaceHeader(team);
            root.Children.Add(header);
            Grid.SetRow(header, 0);

            var metrics = CreateTeamWorkspaceMetrics(team);
            root.Children.Add(metrics);
            Grid.SetRow(metrics, 1);

            var highlights = CreateTeamWorkspaceHighlights(team);
            root.Children.Add(highlights);
            Grid.SetRow(highlights, 2);

            var contentGrid = new Grid
            {
                Margin = new Thickness(22, 0, 22, 22)
            };
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2.2, GridUnitType.Star) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(18) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var boardPanel = CreateTeamBoardPanel(team);
            contentGrid.Children.Add(boardPanel);
            Grid.SetColumn(boardPanel, 0);

            var sidePanel = CreateTeamWorkspaceSidebar(team);
            contentGrid.Children.Add(sidePanel);
            Grid.SetColumn(sidePanel, 2);

            root.Children.Add(contentGrid);
            Grid.SetRow(contentGrid, 3);

            return root;
        }

        private UIElement CreateTeamWorkspaceHeader(TeamWorkspaceInfo team)
        {
            var border = new Border
            {
                Padding = new Thickness(22, 22, 22, 18),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(0, 0, 0, 1)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var titleStack = new StackPanel();
            titleStack.Children.Add(new TextBlock
            {
                Text = team.TeamName,
                FontSize = 22,
                FontWeight = FontWeights.ExtraBold,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            });
            titleStack.Children.Add(new TextBlock
            {
                Text = $"{team.Course} • {team.ClassName} • ID {team.ClassId}",
                FontSize = 12,
                Margin = new Thickness(0, 6, 0, 0),
                Foreground = GetThemeBrush("SecondaryTextBrush")
            });
            titleStack.Children.Add(new TextBlock
            {
                Text = $"Codigo de ingresso: {team.TeamId}",
                FontSize = 12,
                Margin = new Thickness(0, 6, 0, 0),
                Foreground = GetThemeBrush("AccentBrush"),
                FontWeight = FontWeights.SemiBold
            });
            grid.Children.Add(titleStack);

            var actions = new WrapPanel
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top
            };

            actions.Children.Add(CreateTeamWorkspaceActionButton("Copiar codigo", Color.FromRgb(14, 165, 233), CopyTeamCode_Click));
            actions.Children.Add(CreateTeamWorkspaceActionButton("Adicionar membro", Color.FromRgb(37, 99, 235), (s, e) => OpenAddTeamMemberDialog(team)));
            actions.Children.Add(CreateTeamWorkspaceActionButton("Remover membro", Color.FromRgb(245, 158, 11), (s, e) => OpenRemoveTeamMemberDialog(team)));
            if (IsCurrentUserTeamCreator(team))
            {
                actions.Children.Add(CreateTeamWorkspaceActionButton("Apagar equipe", Color.FromRgb(220, 38, 38), DeleteActiveTeamWorkspace));
            }
            actions.Children.Add(CreateTeamWorkspaceActionButton("Fechar", Color.FromRgb(100, 116, 139), CloseTeamWorkspace_Click));

            Grid.SetColumn(actions, 1);
            grid.Children.Add(actions);

            border.Child = grid;
            return border;
        }

        private UIElement CreateTeamWorkspaceMetrics(TeamWorkspaceInfo team)
        {
            var overdueCount = team.TaskColumns.SelectMany(column => column.Cards).Count(card => card.DueDate.HasValue && card.DueDate.Value.Date < DateTime.Today);
            var completedMilestones = team.Milestones.Count(milestone => string.Equals(milestone.Status, "Concluida", StringComparison.OrdinalIgnoreCase));

            var wrap = new WrapPanel
            {
                Margin = new Thickness(22, 18, 22, 18)
            };

            wrap.Children.Add(CreateTeamMetricCard("Membros", $"{team.Members.Count}", "Equipe ativa", Color.FromRgb(37, 99, 235)));
            wrap.Children.Add(CreateTeamMetricCard("Tarefas", $"{team.TaskColumns.Sum(column => column.Cards.Count)}", "Cards no board", Color.FromRgb(16, 185, 129)));
            wrap.Children.Add(CreateTeamMetricCard("Progresso", $"{CalculateTeamProgressPercentage(team)}%", "Fluxo concluido", Color.FromRgb(14, 165, 233)));
            wrap.Children.Add(CreateTeamMetricCard("Atrasos", $"{overdueCount}", "Itens fora do prazo", Color.FromRgb(220, 38, 38)));
            wrap.Children.Add(CreateTeamMetricCard("Entregas", $"{completedMilestones}/{team.Milestones.Count}", "Marcos academicos", Color.FromRgb(168, 85, 247)));

            return wrap;
        }

        private UIElement CreateTeamWorkspaceHighlights(TeamWorkspaceInfo team)
        {
            var grid = new Grid
            {
                Margin = new Thickness(22, 0, 22, 18)
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.3, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(14) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var ucCard = CreateSidebarSection("UCs ativas", "Base curricular e disciplinas que sustentam essa equipe.");
            var ucContent = (StackPanel)ucCard.Child;
            var ucWrap = new WrapPanel { Margin = new Thickness(0, 12, 0, 0) };
            foreach (var uc in team.Ucs)
            {
                ucWrap.Children.Add(CreateStaticTeamChip(
                    uc,
                    GetThemeBrush("AccentMutedBrush"),
                    GetThemeBrush("AccentBrush")));
            }

            if (team.Ucs.Count == 0)
            {
                ucContent.Children.Add(new TextBlock
                {
                    Text = "Nenhuma UC foi vinculada ainda.",
                    Margin = new Thickness(0, 12, 0, 0),
                    FontSize = 11,
                    Foreground = GetThemeBrush("SecondaryTextBrush")
                });
            }
            else
            {
                ucContent.Children.Add(ucWrap);
            }

            grid.Children.Add(ucCard);

            var deadlineCard = CreateSidebarSection("Radar de prazos", "Entregas mais proximas e o que exige atencao imediata.");
            var deadlineContent = (StackPanel)deadlineCard.Child;
            var upcomingCards = team.TaskColumns
                .SelectMany(column => column.Cards.Select(card => new { Column = column, Card = card }))
                .Where(item => item.Card.DueDate.HasValue)
                .OrderBy(item => item.Card.DueDate)
                .Take(4)
                .ToList();

            if (upcomingCards.Count == 0)
            {
                deadlineContent.Children.Add(new TextBlock
                {
                    Text = "Ainda nao existem prazos registrados no board.",
                    Margin = new Thickness(0, 12, 0, 0),
                    FontSize = 11,
                    Foreground = GetThemeBrush("SecondaryTextBrush")
                });
            }
            else
            {
                foreach (var item in upcomingCards)
                {
                    var isLate = item.Card.DueDate!.Value.Date < DateTime.Today;
                    deadlineContent.Children.Add(new Border
                    {
                        Background = isLate ? new SolidColorBrush(Color.FromRgb(254, 242, 242)) : GetThemeBrush("MutedCardBackgroundBrush"),
                        BorderBrush = isLate ? new SolidColorBrush(Color.FromRgb(220, 38, 38)) : GetThemeBrush("CardBorderBrush"),
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(14),
                        Padding = new Thickness(12),
                        Margin = new Thickness(0, 12, 0, 0),
                        Child = new StackPanel
                        {
                            Children =
                            {
                                new TextBlock
                                {
                                    Text = item.Card.Title,
                                    FontSize = 12,
                                    FontWeight = FontWeights.SemiBold,
                                    Foreground = GetThemeBrush("PrimaryTextBrush"),
                                    TextWrapping = TextWrapping.Wrap
                                },
                                new TextBlock
                                {
                                    Text = $"{item.Column.Title} • {FormatRelativeDate(item.Card.DueDate.Value)}",
                                    Margin = new Thickness(0, 6, 0, 0),
                                    FontSize = 11,
                                    Foreground = isLate ? new SolidColorBrush(Color.FromRgb(220, 38, 38)) : GetThemeBrush("SecondaryTextBrush")
                                }
                            }
                        }
                    });
                }
            }

            Grid.SetColumn(deadlineCard, 2);
            grid.Children.Add(deadlineCard);
            return grid;
        }

        private Border CreateTeamMetricCard(string title, string value, string subtitle, Color accent)
        {
            var border = new Border
            {
                Width = 190,
                Margin = new Thickness(0, 0, 12, 12),
                Padding = new Thickness(16),
                Background = GetThemeBrush("MutedCardBackgroundBrush"),
                BorderBrush = new SolidColorBrush(accent),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(16)
            };

            var stack = new StackPanel();
            stack.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 11,
                Foreground = GetThemeBrush("SecondaryTextBrush")
            });
            stack.Children.Add(new TextBlock
            {
                Text = value,
                FontSize = 24,
                FontWeight = FontWeights.ExtraBold,
                Margin = new Thickness(0, 6, 0, 4),
                Foreground = new SolidColorBrush(accent)
            });
            stack.Children.Add(new TextBlock
            {
                Text = subtitle,
                FontSize = 11,
                Foreground = GetThemeBrush("TertiaryTextBrush")
            });
            border.Child = stack;
            return border;
        }

        private Button CreateTeamWorkspaceActionButton(string text, Color backgroundColor, RoutedEventHandler onClick)
        {
            var button = new Button
            {
                Content = text,
                Background = new SolidColorBrush(backgroundColor),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(16, 10, 16, 10),
                Margin = new Thickness(10, 0, 0, 10),
                FontWeight = FontWeights.SemiBold,
                Cursor = Cursors.Hand
            };
            button.Click += onClick;
            return button;
        }

        private UIElement CreateTeamBoardPanel(TeamWorkspaceInfo team)
        {
            var panel = new Border
            {
                Background = GetThemeBrush("MutedCardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(20),
                Padding = new Thickness(18)
            };

            var stack = new StackPanel();

            var header = new Grid { Margin = new Thickness(0, 0, 0, 14) };
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var headerText = new StackPanel();
            headerText.Children.Add(new TextBlock
            {
                Text = _activeTeamBoardView == TeamBoardView.Csd ? "Modelo CSD" : "Workspace de entregas",
                FontSize = 18,
                FontWeight = FontWeights.SemiBold,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            });
            headerText.Children.Add(new TextBlock
            {
                Text = _activeTeamBoardView == TeamBoardView.Trello
                    ? "Quadro estilo Trello com cards arrastaveis, atribuicoes e prazos."
                    : _activeTeamBoardView == TeamBoardView.Kanban
                        ? "Visao operacional focada em gargalos, fluxo e atrasos."
                        : "Mapa de certezas, suposicoes e duvidas para apoiar decisoes do projeto.",
                FontSize = 12,
                Margin = new Thickness(0, 6, 0, 0),
                Foreground = GetThemeBrush("SecondaryTextBrush")
            });
            header.Children.Add(headerText);

            var actionWrap = new WrapPanel
            {
                HorizontalAlignment = HorizontalAlignment.Right
            };
            actionWrap.Children.Add(CreateTeamBoardModeButton("Trello", TeamBoardView.Trello));
            actionWrap.Children.Add(CreateTeamBoardModeButton("KANBAN", TeamBoardView.Kanban));
            actionWrap.Children.Add(CreateTeamBoardModeButton("CSD", TeamBoardView.Csd));

            var addButton = new Button
            {
                Content = _activeTeamBoardView == TeamBoardView.Csd ? "Nova nota CSD" : "Nova tarefa",
                Background = GetThemeBrush("AccentBrush"),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(16, 10, 16, 10),
                Margin = new Thickness(10, 0, 0, 10),
                FontWeight = FontWeights.SemiBold,
                Cursor = Cursors.Hand
            };
            addButton.Click += _activeTeamBoardView == TeamBoardView.Csd
                ? (s, e) => OpenAddCsdNoteDialog(team)
                : (s, e) => OpenCreateTaskDialog(team);
            actionWrap.Children.Add(addButton);

            Grid.SetColumn(actionWrap, 1);
            header.Children.Add(actionWrap);
            stack.Children.Add(header);

            stack.Children.Add(_activeTeamBoardView == TeamBoardView.Csd
                ? CreateCsdBoardView(team)
                : CreateTaskBoardView(team));

            panel.Child = stack;
            return panel;
        }

        private Button CreateTeamBoardModeButton(string label, TeamBoardView view)
        {
            var isActive = _activeTeamBoardView == view;
            var button = new Button
            {
                Content = label,
                Background = isActive ? GetThemeBrush("AccentBrush") : GetThemeBrush("CardBackgroundBrush"),
                Foreground = isActive ? Brushes.White : GetThemeBrush("PrimaryTextBrush"),
                BorderBrush = isActive ? GetThemeBrush("AccentBrush") : GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(14, 8, 14, 8),
                Margin = new Thickness(0, 0, 10, 10),
                FontWeight = FontWeights.SemiBold,
                Cursor = Cursors.Hand,
                Tag = view
            };
            button.Click += ChangeTeamBoardView_Click;
            return button;
        }

        private UIElement CreateTaskBoardView(TeamWorkspaceInfo team)
        {
            var totalCards = team.TaskColumns.Sum(item => item.Cards.Count);
            var overdueCards = team.TaskColumns
                .SelectMany(item => item.Cards)
                .Count(task => task.DueDate.HasValue && task.DueDate.Value.Date < DateTime.Today);
            var completedCards = team.TaskColumns
                .Where(item => item.Title.Contains("Conclu", StringComparison.OrdinalIgnoreCase))
                .Sum(item => item.Cards.Count);
            var assignedCards = team.TaskColumns
                .SelectMany(item => item.Cards)
                .Count(task => task.AssignedUserIds.Count > 0);
            var isKanban = _activeTeamBoardView == TeamBoardView.Kanban;

            var stack = new StackPanel();

            var overview = new Border
            {
                Background = GetThemeBrush("CardBackgroundBrush"),
                BorderBrush = isKanban
                    ? new SolidColorBrush(Color.FromRgb(56, 189, 248))
                    : GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(18),
                Padding = new Thickness(18),
                Margin = new Thickness(0, 0, 0, 16)
            };

            var overviewWrap = new WrapPanel();
            overviewWrap.Children.Add(CreateBoardOverviewMetric(
                isKanban ? "Fluxo" : "Visao geral",
                totalCards.ToString(),
                isKanban ? "Itens circulando entre etapas" : "Total de tarefas",
                isKanban ? Color.FromRgb(14, 165, 233) : Color.FromRgb(37, 99, 235)));
            overviewWrap.Children.Add(CreateBoardOverviewMetric("Em risco", overdueCards.ToString(), overdueCards == 1 ? "Prazo vencido" : "Prazos vencidos", Color.FromRgb(220, 38, 38)));
            overviewWrap.Children.Add(CreateBoardOverviewMetric(
                isKanban ? "Saidas" : "Concluidas",
                completedCards.ToString(),
                isKanban ? "Itens finalizados no fluxo" : "Ja entregues",
                Color.FromRgb(16, 185, 129)));
            overviewWrap.Children.Add(CreateBoardOverviewMetric(
                isKanban ? "Responsaveis" : "Com responsavel",
                assignedCards.ToString(),
                isKanban ? "Cards com dono definido" : "Cards com dono",
                Color.FromRgb(168, 85, 247)));
            overview.Child = overviewWrap;
            stack.Children.Add(overview);

            stack.Children.Add(new TextBlock
            {
                Text = isKanban
                    ? "O modo Kanban agora prioriza leitura operacional: etapas empilhadas, resumo por faixa e cards mais compactos para enxergar gargalos rapidamente."
                    : "O modo Trello ficou mais editorial: colunas amplas, cards com mais respiro e leitura mais clara de prioridade, prazo e responsaveis.",
                FontSize = 12,
                Margin = new Thickness(2, 0, 0, 14),
                TextWrapping = TextWrapping.Wrap,
                Foreground = GetThemeBrush("SecondaryTextBrush")
            });

            stack.Children.Add(isKanban
                ? CreateKanbanTaskBoardView(team)
                : CreateTrelloTaskBoardView(team));

            return stack;
        }

        private UIElement CreateTrelloTaskBoardView(TeamWorkspaceInfo team)
        {
            var columnsWrap = new WrapPanel
            {
                Orientation = Orientation.Horizontal,
                ItemWidth = 360,
                Margin = new Thickness(0, 0, -16, 0)
            };

            foreach (var column in team.TaskColumns)
            {
                columnsWrap.Children.Add(CreateTaskBoardColumn(team, column, false));
            }

            return columnsWrap;
        }

        private UIElement CreateKanbanTaskBoardView(TeamWorkspaceInfo team)
        {
            var lanes = new StackPanel();

            foreach (var column in team.TaskColumns)
            {
                lanes.Children.Add(CreateKanbanTaskLane(team, column));
            }

            return lanes;
        }

        private Border CreateTaskBoardColumn(TeamWorkspaceInfo team, TeamTaskColumnInfo column, bool compactCards)
        {
            var nextDueDate = column.Cards
                .Where(task => task.DueDate.HasValue)
                .OrderBy(task => task.DueDate)
                .Select(task => task.DueDate)
                .FirstOrDefault();

            var columnBorder = new Border
            {
                Width = 344,
                Margin = new Thickness(0, 0, 16, 16),
                Background = GetThemeBrush("CardBackgroundBrush"),
                BorderBrush = new SolidColorBrush(column.AccentColor),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(20),
                AllowDrop = true,
                Tag = column
            };
            columnBorder.Drop += TeamBoardColumn_Drop;
            columnBorder.DragOver += TeamBoardColumn_DragOver;

            var stack = new StackPanel();
            stack.Children.Add(new Border
            {
                Height = 6,
                Background = new SolidColorBrush(column.AccentColor),
                CornerRadius = new CornerRadius(20, 20, 0, 0)
            });

            var headerSection = new Border
            {
                Background = GetThemeBrush("MutedCardBackgroundBrush"),
                Padding = new Thickness(16, 14, 16, 14),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(0, 0, 0, 1)
            };

            var headerStack = new StackPanel();
            var header = new Grid { Margin = new Thickness(0, 0, 0, 8) };
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            header.Children.Add(new TextBlock
            {
                Text = column.Title,
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            });

            var countBadge = new Border
            {
                Background = new SolidColorBrush(column.AccentColor),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(8, 3, 8, 3),
                Child = new TextBlock
                {
                    Text = column.Cards.Count.ToString(),
                    Foreground = Brushes.White,
                    FontSize = 10,
                    FontWeight = FontWeights.Bold
                }
            };
            Grid.SetColumn(countBadge, 1);
            header.Children.Add(countBadge);

            headerStack.Children.Add(header);
            headerStack.Children.Add(new TextBlock
            {
                Text = nextDueDate.HasValue
                    ? $"Proximo prazo {FormatRelativeDate(nextDueDate.Value)}"
                    : "Sem prazo definido nesta etapa.",
                FontSize = 11,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            });
            headerSection.Child = headerStack;
            stack.Children.Add(headerSection);

            var cardsHost = new StackPanel
            {
                Margin = new Thickness(14, 14, 14, 14)
            };

            foreach (var card in column.Cards.OrderBy(task => task.DueDate ?? DateTime.MaxValue))
            {
                cardsHost.Children.Add(CreateTaskCard(team, column, card, compactCards));
            }

            if (column.Cards.Count == 0)
            {
                var emptyState = new Border
                {
                    Background = GetThemeBrush("MutedCardBackgroundBrush"),
                    BorderBrush = GetThemeBrush("CardBorderBrush"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(14),
                    Padding = new Thickness(16),
                    Margin = new Thickness(0, 2, 0, 0),
                    Child = new StackPanel
                    {
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "Nenhuma tarefa nesta coluna ainda.",
                                FontSize = 11,
                                TextWrapping = TextWrapping.Wrap,
                                Foreground = GetThemeBrush("SecondaryTextBrush")
                            },
                            new Button
                            {
                                Content = "Criar primeira tarefa",
                                Background = GetThemeBrush("AccentBrush"),
                                Foreground = Brushes.White,
                                BorderThickness = new Thickness(0),
                                Padding = new Thickness(12, 8, 12, 8),
                                Margin = new Thickness(0, 10, 0, 0),
                                FontWeight = FontWeights.SemiBold,
                                Cursor = Cursors.Hand,
                                Tag = team
                            }
                        }
                    }
                };

                if (emptyState.Child is StackPanel emptyStack && emptyStack.Children.OfType<Button>().FirstOrDefault() is Button addTaskButton)
                {
                    addTaskButton.Click += (s, e) => OpenCreateTaskDialog(team);
                }

                cardsHost.Children.Add(emptyState);
            }

            stack.Children.Add(cardsHost);
            columnBorder.Child = stack;
            return columnBorder;
        }

        private Border CreateKanbanTaskLane(TeamWorkspaceInfo team, TeamTaskColumnInfo column)
        {
            var overdueCount = column.Cards.Count(task => task.DueDate.HasValue && task.DueDate.Value.Date < DateTime.Today);
            var lane = new Border
            {
                Background = GetThemeBrush("CardBackgroundBrush"),
                BorderBrush = new SolidColorBrush(column.AccentColor),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(20),
                Margin = new Thickness(0, 0, 0, 16),
                Padding = new Thickness(18)
            };

            var layout = new Grid();
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(18) });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var leftRail = new Border
            {
                Background = GetThemeBrush("MutedCardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(18),
                Padding = new Thickness(16)
            };

            var leftStack = new StackPanel();
            leftStack.Children.Add(new Border
            {
                Width = 42,
                Height = 8,
                CornerRadius = new CornerRadius(999),
                Background = new SolidColorBrush(column.AccentColor),
                Margin = new Thickness(0, 0, 0, 12)
            });
            leftStack.Children.Add(new TextBlock
            {
                Text = column.Title,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            });
            leftStack.Children.Add(new TextBlock
            {
                Text = overdueCount > 0
                    ? $"{overdueCount} item(ns) exigindo atencao imediata."
                    : "Fluxo estavel nesta etapa.",
                FontSize = 12,
                Margin = new Thickness(0, 8, 0, 14),
                TextWrapping = TextWrapping.Wrap,
                Foreground = GetThemeBrush("SecondaryTextBrush")
            });
            leftStack.Children.Add(CreateBoardOverviewMetric("Cards", column.Cards.Count.ToString(), "Volume atual", column.AccentColor));
            leftStack.Children.Add(CreateBoardOverviewMetric("Atrasos", overdueCount.ToString(), "Itens vencidos", Color.FromRgb(220, 38, 38)));
            leftRail.Child = leftStack;
            layout.Children.Add(leftRail);

            var cardsWrap = new WrapPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, -12, -12)
            };

            foreach (var card in column.Cards.OrderBy(task => task.DueDate ?? DateTime.MaxValue))
            {
                cardsWrap.Children.Add(CreateTaskCard(team, column, card, true));
            }

            if (column.Cards.Count == 0)
            {
                cardsWrap.Children.Add(new Border
                {
                    Width = 260,
                    Background = GetThemeBrush("MutedCardBackgroundBrush"),
                    BorderBrush = GetThemeBrush("CardBorderBrush"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(16),
                    Padding = new Thickness(16),
                    Child = new StackPanel
                    {
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "Etapa vazia. Use esse espaco para puxar novas tarefas ou abrir um item agora.",
                                FontSize = 11,
                                TextWrapping = TextWrapping.Wrap,
                                Foreground = GetThemeBrush("SecondaryTextBrush")
                            },
                            new Button
                            {
                                Content = "Criar tarefa nesta etapa",
                                Background = new SolidColorBrush(column.AccentColor),
                                Foreground = Brushes.White,
                                BorderThickness = new Thickness(0),
                                Padding = new Thickness(12, 8, 12, 8),
                                Margin = new Thickness(0, 12, 0, 0),
                                FontWeight = FontWeights.SemiBold,
                                Cursor = Cursors.Hand,
                                Tag = team
                            }
                        }
                    }
                });

                if (cardsWrap.Children.OfType<Border>().LastOrDefault()?.Child is StackPanel emptyStack
                    && emptyStack.Children.OfType<Button>().FirstOrDefault() is Button createButton)
                {
                    createButton.Click += (s, e) => OpenCreateTaskDialog(team);
                }
            }

            Grid.SetColumn(cardsWrap, 2);
            layout.Children.Add(cardsWrap);

            lane.Child = layout;
            return lane;
        }

        private Border CreateTaskCard(TeamWorkspaceInfo team, TeamTaskColumnInfo column, TeamTaskCardInfo card, bool compactMode)
        {
            var isOverdue = card.DueDate.HasValue && card.DueDate.Value.Date < DateTime.Today;
            var priorityColor = card.Priority switch
            {
                "Alta" => Color.FromRgb(220, 38, 38),
                "Baixa" => Color.FromRgb(16, 185, 129),
                _ => Color.FromRgb(245, 158, 11)
            };

            var cardBorder = new Border
            {
                Background = isOverdue
                    ? new SolidColorBrush(Color.FromRgb(254, 242, 242))
                    : Brushes.White,
                BorderBrush = new SolidColorBrush(isOverdue ? Color.FromRgb(220, 38, 38) : Color.FromRgb(226, 232, 240)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(18),
                Padding = compactMode ? new Thickness(14) : new Thickness(16),
                Margin = new Thickness(0, 0, 0, 12),
                Width = compactMode ? 256 : double.NaN,
                Cursor = Cursors.Hand,
                Tag = card
            };
            cardBorder.MouseMove += TeamTaskCard_MouseMove;

            var stack = new StackPanel();
            stack.Children.Add(new TextBlock
            {
                Text = column.Title.ToUpperInvariant(),
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(column.AccentColor),
                Margin = new Thickness(0, 0, 0, 6)
            });
            stack.Children.Add(new TextBlock
            {
                Text = card.Title,
                FontSize = compactMode ? 13 : 14,
                FontWeight = FontWeights.SemiBold,
                TextWrapping = TextWrapping.Wrap,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            });
            stack.Children.Add(new TextBlock
            {
                Text = card.Description,
                FontSize = 11,
                Margin = new Thickness(0, 8, 0, 0),
                TextWrapping = TextWrapping.Wrap,
                MaxHeight = compactMode ? 54 : double.PositiveInfinity,
                Foreground = GetThemeBrush("SecondaryTextBrush")
            });

            var chips = new WrapPanel
            {
                Margin = new Thickness(0, 10, 0, 0)
            };
            chips.Children.Add(CreateStaticTeamChip(card.Priority, new SolidColorBrush(priorityColor), Brushes.White));

            if (card.DueDate.HasValue)
            {
                chips.Children.Add(CreateStaticTeamChip(
                    isOverdue ? $"Atrasado {FormatRelativeDate(card.DueDate.Value)}" : $"Prazo {FormatRelativeDate(card.DueDate.Value)}",
                    isOverdue ? new SolidColorBrush(Color.FromRgb(220, 38, 38)) : GetThemeBrush("AccentMutedBrush"),
                    isOverdue ? Brushes.White : GetThemeBrush("AccentBrush")));
            }

            stack.Children.Add(chips);

            var assignedMembers = team.Members
                .Where(member => card.AssignedUserIds.Contains(member.UserId))
                .ToList();

            if (assignedMembers.Count > 0)
            {
                stack.Children.Add(new TextBlock
                {
                    Text = assignedMembers.Count == 1 ? "Responsavel" : "Responsaveis",
                    FontSize = 10,
                    Margin = new Thickness(0, 10, 0, 6),
                    Foreground = GetThemeBrush("TertiaryTextBrush")
                });
                stack.Children.Add(CreateTaskAssigneesPanel(assignedMembers, compactMode));
            }

            var footer = new Grid { Margin = new Thickness(0, 10, 0, 0) };
            footer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            footer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            footer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            footer.Children.Add(new TextBlock
            {
                Text = $"Criado em {card.CreatedAt:dd/MM}",
                FontSize = 10,
                Foreground = GetThemeBrush("TertiaryTextBrush")
            });

            var editButton = new Button
            {
                Content = "Editar",
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = GetThemeBrush("AccentBrush"),
                FontSize = 10,
                FontWeight = FontWeights.SemiBold,
                Cursor = Cursors.Hand,
                Padding = new Thickness(6, 0, 0, 0),
                Tag = Tuple.Create(team, column, card)
            };
            editButton.Click += EditTeamTask_Click;
            Grid.SetColumn(editButton, 1);
            footer.Children.Add(editButton);

            var deleteButton = new Button
            {
                Content = "Excluir",
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = new SolidColorBrush(Color.FromRgb(220, 38, 38)),
                FontSize = 10,
                FontWeight = FontWeights.SemiBold,
                Cursor = Cursors.Hand,
                Padding = new Thickness(8, 0, 0, 0),
                Tag = Tuple.Create(team, column, card)
            };
            deleteButton.Click += DeleteTeamTask_Click;
            Grid.SetColumn(deleteButton, 2);
            footer.Children.Add(deleteButton);

            stack.Children.Add(footer);
            cardBorder.Child = stack;
            return cardBorder;
        }

        private WrapPanel CreateTaskAssigneesPanel(List<UserInfo> assignedMembers, bool compactMode)
        {
            var wrap = new WrapPanel
            {
                Margin = new Thickness(0, 0, 0, 0)
            };

            foreach (var member in assignedMembers)
            {
                wrap.Children.Add(CreateAssigneeAvatarPill(member, compactMode));
            }

            return wrap;
        }

        private Border CreateAssigneeAvatarPill(UserInfo member, bool compactMode)
        {
            var displayName = string.IsNullOrWhiteSpace(member.Name)
                ? member.Email
                : member.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? member.Name;

            var content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };

            content.Children.Add(new Border
            {
                Width = compactMode ? 24 : 28,
                Height = compactMode ? 24 : 28,
                CornerRadius = new CornerRadius(compactMode ? 12 : 14),
                ClipToBounds = true,
                Child = CreateUserAvatarVisual(member, compactMode ? 24 : 28, true)
            });

            content.Children.Add(new TextBlock
            {
                Text = displayName,
                Margin = new Thickness(8, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = compactMode ? 10 : 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            });

            return new Border
            {
                Background = GetThemeBrush("MutedCardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(999),
                Padding = new Thickness(6, 4, 10, 4),
                Margin = new Thickness(0, 0, 8, 8),
                Child = content
            };
        }

        private Border CreateBoardOverviewMetric(string title, string value, string subtitle, Color accentColor)
        {
            return new Border
            {
                Width = 160,
                Background = GetThemeBrush("MutedCardBackgroundBrush"),
                BorderBrush = new SolidColorBrush(accentColor),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(16),
                Padding = new Thickness(14),
                Margin = new Thickness(0, 0, 12, 12),
                Child = new StackPanel
                {
                    Children =
                    {
                        new TextBlock
                        {
                            Text = title,
                            FontSize = 11,
                            Foreground = GetThemeBrush("SecondaryTextBrush")
                        },
                        new TextBlock
                        {
                            Text = value,
                            FontSize = 24,
                            FontWeight = FontWeights.Bold,
                            Margin = new Thickness(0, 6, 0, 4),
                            Foreground = GetThemeBrush("PrimaryTextBrush")
                        },
                        new TextBlock
                        {
                            Text = subtitle,
                            FontSize = 11,
                            TextWrapping = TextWrapping.Wrap,
                            Foreground = GetThemeBrush("SecondaryTextBrush")
                        }
                    }
                }
            };
        }

        private Border CreateStaticTeamChip(string text, Brush background, Brush foreground)
        {
            return new Border
            {
                Background = background,
                CornerRadius = new CornerRadius(14),
                Padding = new Thickness(10, 5, 10, 5),
                Margin = new Thickness(0, 0, 8, 8),
                Child = new TextBlock
                {
                    Text = text,
                    Foreground = foreground,
                    FontSize = 10,
                    FontWeight = FontWeights.SemiBold
                }
            };
        }

        private UIElement CreateCsdBoardView(TeamWorkspaceInfo team)
        {
            var wrap = new WrapPanel();
            wrap.Children.Add(CreateCsdColumn(team, "Certezas", team.CsdBoard.Certainties, Color.FromRgb(37, 99, 235)));
            wrap.Children.Add(CreateCsdColumn(team, "Suposicoes", team.CsdBoard.Assumptions, Color.FromRgb(245, 158, 11)));
            wrap.Children.Add(CreateCsdColumn(team, "Duvidas", team.CsdBoard.Doubts, Color.FromRgb(168, 85, 247)));
            return wrap;
        }

        private Border CreateCsdColumn(TeamWorkspaceInfo team, string title, List<string> notes, Color accent)
        {
            var border = new Border
            {
                Width = 300,
                Margin = new Thickness(0, 0, 14, 0),
                Background = GetThemeBrush("CardBackgroundBrush"),
                BorderBrush = new SolidColorBrush(accent),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(18),
                Padding = new Thickness(14)
            };

            var stack = new StackPanel();
            stack.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(accent)
            });

            foreach (var note in notes)
            {
                var noteCard = new Border
                {
                    Background = GetThemeBrush("MutedCardBackgroundBrush"),
                    BorderBrush = GetThemeBrush("CardBorderBrush"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(14),
                    Padding = new Thickness(12),
                    Margin = new Thickness(0, 10, 0, 0),
                };

                var noteGrid = new Grid();
                noteGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                noteGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                noteGrid.Children.Add(new TextBlock
                {
                    Text = note,
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 11,
                    Margin = new Thickness(0, 0, 10, 0),
                    Foreground = GetThemeBrush("PrimaryTextBrush")
                });

                var deleteButton = new Button
                {
                    Content = "Remover",
                    Background = Brushes.Transparent,
                    Foreground = new SolidColorBrush(Color.FromRgb(220, 38, 38)),
                    BorderThickness = new Thickness(0),
                    Cursor = Cursors.Hand,
                    Padding = new Thickness(6, 0, 0, 0),
                    FontSize = 10,
                    FontWeight = FontWeights.SemiBold,
                    VerticalAlignment = VerticalAlignment.Top,
                    Tag = Tuple.Create(team, title, note)
                };
                deleteButton.Click += DeleteCsdNote_Click;
                Grid.SetColumn(deleteButton, 1);
                noteGrid.Children.Add(deleteButton);

                noteCard.Child = noteGrid;
                stack.Children.Add(noteCard);
            }

            if (notes.Count == 0)
            {
                stack.Children.Add(new Border
                {
                    Background = GetThemeBrush("MutedCardBackgroundBrush"),
                    BorderBrush = GetThemeBrush("CardBorderBrush"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(14),
                    Padding = new Thickness(12),
                    Margin = new Thickness(0, 10, 0, 0),
                    Child = new TextBlock
                    {
                        Text = "Nenhuma nota criada ainda. Use o botao 'Nova nota CSD' acima para adicionar manualmente.",
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = 11,
                        Foreground = GetThemeBrush("SecondaryTextBrush")
                    }
                });
            }

            border.Child = stack;
            return border;
        }

        private void DeleteCsdNote_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not Tuple<TeamWorkspaceInfo, string, string> payload)
            {
                return;
            }

            var team = payload.Item1;
            var bucket = payload.Item2;
            var note = payload.Item3;

            List<string> notes = bucket switch
            {
                "Suposicoes" => team.CsdBoard.Assumptions,
                "Duvidas" => team.CsdBoard.Doubts,
                _ => team.CsdBoard.Certainties
            };

            if (MessageBox.Show("Remover esta nota da matriz CSD?", "Matriz CSD", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            notes.Remove(note);
            AddTeamNotification(team, $"Nota removida da matriz CSD em {bucket.ToLowerInvariant()}.");
            SaveTeamWorkspace(team);
            RenderTeamWorkspace();
        }

        private UIElement CreateTeamWorkspaceSidebar(TeamWorkspaceInfo team)
        {
            var stack = new StackPanel();
            stack.Children.Add(CreateProjectManagementSection(team));
            stack.Children.Add(CreateTeamMilestonesSection(team));
            stack.Children.Add(CreateProjectChatSection(team));
            stack.Children.Add(CreateTeamMembersSection(team));
            stack.Children.Add(CreateTeamAssetsSection(team));
            stack.Children.Add(CreateTeamNotificationsSection(team));
            return stack;
        }

        private Border CreateProjectManagementSection(TeamWorkspaceInfo team)
        {
            var border = CreateSidebarSection("Gestao do projeto", "Defina o andamento geral e o prazo principal da equipe.");
            var content = (StackPanel)border.Child;

            content.Children.Add(CreateStaticTeamChip($"Status: {team.ProjectStatus}", GetThemeBrush("AccentMutedBrush"), GetThemeBrush("AccentBrush")));

            content.Children.Add(new TextBlock
            {
                Text = $"Progresso atual: {CalculateTeamProgressPercentage(team)}%",
                FontSize = 11,
                Margin = new Thickness(0, 12, 0, 6),
                Foreground = GetThemeBrush("PrimaryTextBrush")
            });

            content.Children.Add(new ProgressBar
            {
                Minimum = 0,
                Maximum = 100,
                Value = CalculateTeamProgressPercentage(team),
                Height = 12,
                Margin = new Thickness(0, 0, 0, 10),
                Foreground = GetThemeBrush("AccentBrush"),
                Background = GetThemeBrush("MutedCardBackgroundBrush")
            });

            content.Children.Add(new TextBlock
            {
                Text = $"Prazo principal: {GetProjectDeadlineText(team)}",
                FontSize = 11,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            });

            content.Children.Add(CreateSidebarButton("Atualizar progresso e prazo", Color.FromRgb(14, 165, 233), (s, e) => OpenProjectManagementDialog(team)));
            return border;
        }

        private Border CreateProjectChatSection(TeamWorkspaceInfo team)
        {
            var border = CreateSidebarSection("Chat do projeto", "Converse com todos os integrantes no mesmo fluxo do projeto.");
            var content = (StackPanel)border.Child;

            if (team.ChatMessages.Count == 0)
            {
                content.Children.Add(new TextBlock
                {
                    Text = "Nenhuma mensagem enviada ainda. Abra o chat do projeto para iniciar a conversa em grupo.",
                    FontSize = 11,
                    Margin = new Thickness(0, 12, 0, 0),
                    Foreground = GetThemeBrush("SecondaryTextBrush"),
                    TextWrapping = TextWrapping.Wrap
                });
            }
            else
            {
                foreach (var message in team.ChatMessages.OrderByDescending(item => item.SentAt).Take(3))
                {
                    content.Children.Add(new Border
                    {
                        Background = GetThemeBrush("MutedCardBackgroundBrush"),
                        BorderBrush = GetThemeBrush("CardBorderBrush"),
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(14),
                        Padding = new Thickness(12),
                        Margin = new Thickness(0, 10, 0, 0),
                        Child = new StackPanel
                        {
                            Children =
                            {
                                new TextBlock
                                {
                                    Text = message.SenderName,
                                    FontSize = 11,
                                    FontWeight = FontWeights.SemiBold,
                                    Foreground = GetThemeBrush("PrimaryTextBrush")
                                },
                                new TextBlock
                                {
                                    Text = message.Content,
                                    FontSize = 11,
                                    Margin = new Thickness(0, 6, 0, 0),
                                    Foreground = GetThemeBrush("SecondaryTextBrush"),
                                    TextWrapping = TextWrapping.Wrap
                                }
                            }
                        }
                    });
                }
            }

            content.Children.Add(CreateSidebarButton("Abrir chat do projeto", Color.FromRgb(37, 99, 235), (s, e) => OpenProjectChatDialog(team)));
            return border;
        }

        private Border CreateTeamMilestonesSection(TeamWorkspaceInfo team)
        {
            var border = CreateSidebarSection("Plano de entregas", "Marcos, checkpoints e pequenas entregas para a equipe acompanhar.");
            var content = (StackPanel)border.Child;

            var completedCount = team.Milestones.Count(item => string.Equals(item.Status, "Concluida", StringComparison.OrdinalIgnoreCase));
            var nextMilestone = team.Milestones
                .Where(item => item.DueDate.HasValue)
                .OrderBy(item => item.DueDate)
                .FirstOrDefault();

            var overview = new WrapPanel { Margin = new Thickness(0, 12, 0, 4) };
            overview.Children.Add(CreateSidebarMiniMetric("Ativas", (team.Milestones.Count - completedCount).ToString(), Color.FromRgb(124, 58, 237)));
            overview.Children.Add(CreateSidebarMiniMetric("Concluidas", completedCount.ToString(), Color.FromRgb(16, 185, 129)));
            overview.Children.Add(CreateSidebarMiniMetric("Proxima", nextMilestone?.DueDate.HasValue == true ? FormatRelativeDate(nextMilestone.DueDate.Value) : "Sem prazo", Color.FromRgb(245, 158, 11)));
            content.Children.Add(overview);
            content.Children.Add(CreateSidebarButton("Adicionar entrega", Color.FromRgb(124, 58, 237), (s, e) => OpenAddMilestoneDialog(team)));

            if (team.Milestones.Count == 0)
            {
                content.Children.Add(new TextBlock
                {
                    Text = "Nenhuma entrega criada ainda. Use o botao acima para adicionar manualmente.",
                    FontSize = 11,
                    Margin = new Thickness(0, 12, 0, 0),
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = GetThemeBrush("SecondaryTextBrush")
                });
                return border;
            }

            foreach (var milestone in team.Milestones.OrderBy(item => item.DueDate ?? DateTime.MaxValue).Take(5))
            {
                var isDone = string.Equals(milestone.Status, "Concluida", StringComparison.OrdinalIgnoreCase);
                var statusColor = isDone ? Color.FromRgb(16, 185, 129) : Color.FromRgb(245, 158, 11);
                var isLate = milestone.DueDate.HasValue && milestone.DueDate.Value.Date < DateTime.Today && !isDone;

                var row = new Border
                {
                    Background = isLate ? new SolidColorBrush(Color.FromRgb(255, 247, 237)) : GetThemeBrush("MutedCardBackgroundBrush"),
                    BorderBrush = isLate ? new SolidColorBrush(Color.FromRgb(245, 158, 11)) : GetThemeBrush("CardBorderBrush"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(16),
                    Padding = new Thickness(14),
                    Margin = new Thickness(0, 10, 0, 0)
                };

                var rowStack = new Grid();
                rowStack.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(4) });
                rowStack.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
                rowStack.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                rowStack.Children.Add(new Border
                {
                    Background = new SolidColorBrush(statusColor),
                    CornerRadius = new CornerRadius(999)
                });

                var info = new StackPanel();
                info.Children.Add(new TextBlock
                {
                    Text = milestone.Title,
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = GetThemeBrush("PrimaryTextBrush")
                });

                if (!string.IsNullOrWhiteSpace(milestone.Notes))
                {
                    info.Children.Add(new TextBlock
                    {
                        Text = milestone.Notes,
                        FontSize = 11,
                        Margin = new Thickness(0, 6, 0, 0),
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = GetThemeBrush("SecondaryTextBrush")
                    });
                }

                var badges = new WrapPanel
                {
                    Margin = new Thickness(0, 10, 0, 0)
                };
                badges.Children.Add(CreateStaticTeamChip(milestone.Status, new SolidColorBrush(statusColor), Brushes.White));
                if (milestone.DueDate.HasValue)
                {
                    badges.Children.Add(CreateStaticTeamChip($"Prazo {FormatRelativeDate(milestone.DueDate.Value)}", GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
                }
                if (isLate)
                {
                    badges.Children.Add(CreateStaticTeamChip("Atrasada", new SolidColorBrush(Color.FromRgb(245, 158, 11)), Brushes.White));
                }
                info.Children.Add(badges);

                var actions = new WrapPanel
                {
                    Margin = new Thickness(0, 8, 0, 0)
                };

                var toggleButton = new Button
                {
                    Content = isDone ? "Reabrir" : "Concluir",
                    Background = Brushes.Transparent,
                    Foreground = GetThemeBrush("AccentBrush"),
                    BorderThickness = new Thickness(0),
                    Cursor = Cursors.Hand,
                    Padding = new Thickness(0),
                    FontWeight = FontWeights.SemiBold,
                    Tag = Tuple.Create(team, milestone)
                };
                toggleButton.Click += ToggleMilestoneStatus_Click;
                actions.Children.Add(toggleButton);

                var removeButton = new Button
                {
                    Content = "Remover",
                    Background = Brushes.Transparent,
                    Foreground = new SolidColorBrush(Color.FromRgb(220, 38, 38)),
                    BorderThickness = new Thickness(0),
                    Cursor = Cursors.Hand,
                    Padding = new Thickness(12, 0, 0, 0),
                    FontWeight = FontWeights.SemiBold,
                    Tag = Tuple.Create(team, milestone)
                };
                removeButton.Click += DeleteMilestone_Click;
                actions.Children.Add(removeButton);

                info.Children.Add(actions);

                Grid.SetColumn(info, 2);
                rowStack.Children.Add(info);
                row.Child = rowStack;
                content.Children.Add(row);
            }

            return border;
        }

        private Border CreateTeamMembersSection(TeamWorkspaceInfo team)
        {
            var border = CreateSidebarSection("Membros da equipe", "Adicione e gerencie o time sem sair do board.");
            var content = (StackPanel)border.Child;

            var membersWrap = new WrapPanel();
            foreach (var member in team.Members.OrderBy(item => item.Name))
            {
                membersWrap.Children.Add(CreateMemberChip(
                    member,
                    GetThemeBrush("AccentMutedBrush"),
                    GetThemeBrush("AccentBrush"),
                    team.Members.Count > 1 ? () => RemoveMemberFromActiveTeam(member) : null));
            }

            content.Children.Add(membersWrap);
            var actions = new WrapPanel();
            actions.Children.Add(CreateSidebarButton("Adicionar novo membro", Color.FromRgb(37, 99, 235), (s, e) => OpenAddTeamMemberDialog(team)));
            if (team.Members.Count > 1)
            {
                actions.Children.Add(CreateSidebarButton("Remover membro", Color.FromRgb(245, 158, 11), (s, e) => OpenRemoveTeamMemberDialog(team)));
            }

            content.Children.Add(actions);
            return border;
        }

        private Border CreateTeamAssetsSection(TeamWorkspaceInfo team)
        {
            var border = CreateSidebarSection("Materiais e planos", "Arquivos e artefatos da equipe ficam centralizados aqui.");
            var content = (StackPanel)border.Child;

            var imagesCount = team.Assets.Count(item => string.Equals(item.Category, "imagens", StringComparison.OrdinalIgnoreCase));
            var documentsCount = team.Assets.Count(item => string.Equals(item.Category, "documentos", StringComparison.OrdinalIgnoreCase));
            var plansCount = team.Assets.Count(item => string.Equals(item.Category, "planos", StringComparison.OrdinalIgnoreCase));

            var stats = new WrapPanel { Margin = new Thickness(0, 12, 0, 4) };
            stats.Children.Add(CreateSidebarMiniMetric("Imagens", imagesCount.ToString(), Color.FromRgb(14, 165, 233)));
            stats.Children.Add(CreateSidebarMiniMetric("Docs", documentsCount.ToString(), Color.FromRgb(37, 99, 235)));
            stats.Children.Add(CreateSidebarMiniMetric("Planos", plansCount.ToString(), Color.FromRgb(16, 185, 129)));
            content.Children.Add(stats);

            var actions = new WrapPanel();
            actions.Children.Add(CreateSidebarButton("Imagens", Color.FromRgb(14, 165, 233), (s, e) => AddTeamAsset("imagens")));
            actions.Children.Add(CreateSidebarButton("Documentos", Color.FromRgb(37, 99, 235), (s, e) => AddTeamAsset("documentos")));
            actions.Children.Add(CreateSidebarButton("Planos", Color.FromRgb(16, 185, 129), (s, e) => AddTeamAsset("planos")));
            content.Children.Add(actions);

            if (team.Assets.Count == 0)
            {
                content.Children.Add(new TextBlock
                {
                    Text = "Nenhum material foi anexado ainda.",
                    FontSize = 11,
                    Margin = new Thickness(0, 12, 0, 0),
                    Foreground = GetThemeBrush("SecondaryTextBrush")
                });
            }
            else
            {
                foreach (var asset in team.Assets.OrderByDescending(item => item.AddedAt).Take(6))
                {
                    content.Children.Add(CreateAssetSidebarCard(asset));
                }
            }

            return border;
        }

        private Border CreateAssetSidebarCard(TeamAssetInfo asset)
        {
            var accent = asset.Category.ToLowerInvariant() switch
            {
                "imagens" => Color.FromRgb(14, 165, 233),
                "planos" => Color.FromRgb(16, 185, 129),
                _ => Color.FromRgb(37, 99, 235)
            };

            var card = new Border
            {
                Background = GetThemeBrush("MutedCardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(16),
                Padding = new Thickness(14),
                Margin = new Thickness(0, 10, 0, 0)
            };

            var stack = new StackPanel();
            stack.Children.Add(CreateStaticTeamChip(asset.Category.ToUpperInvariant(), new SolidColorBrush(accent), Brushes.White));
            stack.Children.Add(new TextBlock
            {
                Text = asset.FileName,
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 8, 0, 0),
                TextWrapping = TextWrapping.Wrap,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            });
            stack.Children.Add(new TextBlock
            {
                Text = $"Adicionado em {asset.AddedAt:dd/MM 'às' HH:mm}",
                FontSize = 11,
                Margin = new Thickness(0, 6, 0, 0),
                Foreground = GetThemeBrush("SecondaryTextBrush")
            });
            card.Child = stack;
            return card;
        }

        private Border CreateSidebarMiniMetric(string title, string value, Color accent)
        {
            return new Border
            {
                Background = GetThemeBrush("MutedCardBackgroundBrush"),
                BorderBrush = new SolidColorBrush(accent),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(14),
                Padding = new Thickness(12, 10, 12, 10),
                Margin = new Thickness(0, 0, 8, 8),
                Child = new StackPanel
                {
                    Children =
                    {
                        new TextBlock
                        {
                            Text = title,
                            FontSize = 10,
                            Foreground = GetThemeBrush("SecondaryTextBrush")
                        },
                        new TextBlock
                        {
                            Text = value,
                            FontSize = 16,
                            FontWeight = FontWeights.Bold,
                            Margin = new Thickness(0, 4, 0, 0),
                            Foreground = GetThemeBrush("PrimaryTextBrush")
                        }
                    }
                }
            };
        }

        private Border CreateTeamNotificationsSection(TeamWorkspaceInfo team)
        {
            var border = CreateSidebarSection("Notificacoes", "Alertas de atribuicao, prazos e movimentacao do board.");
            var content = (StackPanel)border.Child;

            if (team.Notifications.Count == 0)
            {
                content.Children.Add(new TextBlock
                {
                    Text = "Nenhuma notificacao ainda.",
                    FontSize = 11,
                    Margin = new Thickness(0, 10, 0, 0),
                    Foreground = GetThemeBrush("SecondaryTextBrush")
                });
                return border;
            }

            foreach (var notification in team.Notifications.Take(6))
            {
                content.Children.Add(new Border
                {
                    Background = GetThemeBrush("MutedCardBackgroundBrush"),
                    BorderBrush = GetThemeBrush("CardBorderBrush"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(14),
                    Padding = new Thickness(12),
                    Margin = new Thickness(0, 10, 0, 0),
                    Child = new StackPanel
                    {
                        Children =
                        {
                            new TextBlock
                            {
                                Text = notification.Message,
                                TextWrapping = TextWrapping.Wrap,
                                FontSize = 11,
                                Foreground = GetThemeBrush("PrimaryTextBrush")
                            },
                            new TextBlock
                            {
                                Text = notification.CreatedAt.ToString("dd/MM HH:mm"),
                                FontSize = 10,
                                Margin = new Thickness(0, 6, 0, 0),
                                Foreground = GetThemeBrush("TertiaryTextBrush")
                            }
                        }
                    }
                });
            }

            return border;
        }

        private Border CreateSidebarSection(string title, string subtitle)
        {
            var border = new Border
            {
                Background = GetThemeBrush("CardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(18),
                Padding = new Thickness(16),
                Margin = new Thickness(0, 0, 0, 14),
                Child = new StackPanel()
            };

            var stack = (StackPanel)border.Child;
            stack.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            });
            stack.Children.Add(new TextBlock
            {
                Text = subtitle,
                FontSize = 11,
                Margin = new Thickness(0, 6, 0, 0),
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            });

            return border;
        }

        private Button CreateSidebarButton(string text, Color color, RoutedEventHandler onClick)
        {
            var button = new Button
            {
                Content = text,
                Background = new SolidColorBrush(color),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(0, 12, 10, 0),
                FontWeight = FontWeights.SemiBold,
                Cursor = Cursors.Hand
            };
            button.Click += onClick;
            return button;
        }

        private void RenderTeamsList()
        {
            TeamListPanel.Children.Clear();

            foreach (var team in _teamWorkspaces.OrderBy(item => item.TeamName))
            {
                TeamListPanel.Children.Add(CreateTeamListItem(team));
            }
        }

        private Border CreateTeamListItem(TeamWorkspaceInfo team)
        {
            var progress = CalculateTeamProgressPercentage(team);
            var card = new Border
            {
                Background = GetThemeBrush("CardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(20),
                Padding = new Thickness(18),
                Margin = new Thickness(0, 0, 0, 12)
            };

            var layout = new Grid();
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var textStack = new StackPanel();
            textStack.Children.Add(new TextBlock
            {
                Text = team.TeamName,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            });
            textStack.Children.Add(new TextBlock
            {
                Text = $"{team.Course} • {team.ClassName} • ID {team.ClassId}",
                FontSize = 11,
                Margin = new Thickness(0, 4, 0, 0),
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            });
            textStack.Children.Add(new TextBlock
            {
                Text = $"{team.Members.Count} membro(s) • {team.Ucs.Count} UC(s)",
                FontSize = 11,
                Margin = new Thickness(0, 6, 0, 0),
                Foreground = GetThemeBrush("TertiaryTextBrush")
            });
            var metaWrap = new WrapPanel
            {
                Margin = new Thickness(0, 10, 0, 0)
            };
            metaWrap.Children.Add(CreateStaticTeamChip($"Codigo {team.TeamId}", GetThemeBrush("AccentMutedBrush"), GetThemeBrush("AccentBrush")));
            metaWrap.Children.Add(CreateStaticTeamChip($"Progresso {progress}%", GetThemeBrush("MutedCardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            metaWrap.Children.Add(CreateStaticTeamChip($"Proximo prazo {GetNextTeamDeadlineLabel(team)}", GetThemeBrush("MutedCardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            textStack.Children.Add(metaWrap);
            layout.Children.Add(textStack);

            var actionWrap = new WrapPanel
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };

            var openButton = new Button
            {
                Content = "Abrir equipe",
                Background = GetThemeBrush("AccentBrush"),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(16, 10, 16, 10),
                Margin = new Thickness(16, 0, 0, 10),
                FontWeight = FontWeights.SemiBold,
                Cursor = Cursors.Hand,
                VerticalAlignment = VerticalAlignment.Center,
                Tag = team
            };
            openButton.Click += OpenTeamWorkspace_Click;

            var copyButton = new Button
            {
                Content = "Copiar codigo",
                Background = GetThemeBrush("MutedCardBackgroundBrush"),
                Foreground = GetThemeBrush("PrimaryTextBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(14, 10, 14, 10),
                Margin = new Thickness(16, 0, 0, 10),
                FontWeight = FontWeights.SemiBold,
                Cursor = Cursors.Hand,
                VerticalAlignment = VerticalAlignment.Center,
                Tag = team
            };
            copyButton.Click += CopyTeamCode_Click;

            actionWrap.Children.Add(openButton);
            actionWrap.Children.Add(copyButton);

            Grid.SetColumn(actionWrap, 1);
            layout.Children.Add(actionWrap);

            card.Child = layout;
            return card;
        }

        private void SaveTeamWorkspace(TeamWorkspaceInfo team, bool persistInBackground = true)
        {
            EnsureTeamWorkspaceDefaults(team);

            var existingIndex = _teamWorkspaces.FindIndex(item =>
                string.Equals(item.TeamId, team.TeamId, StringComparison.OrdinalIgnoreCase));

            if (existingIndex >= 0)
            {
                _teamWorkspaces[existingIndex] = team;
            }
            else
            {
                _teamWorkspaces.Add(team);
            }

            RenderTeamsList();

            if (persistInBackground)
            {
                var syncSequence = ++_teamSyncSequence;
                SetTeamSyncStatus("Sincronizando alterações da equipe no Firebase...", GetThemeBrush("SecondaryTextBrush"));
                _ = PersistTeamWorkspaceInBackgroundAsync(team, syncSequence);
            }
        }

        private async Task PersistTeamWorkspaceInBackgroundAsync(TeamWorkspaceInfo team, int syncSequence)
        {
            var result = await SaveTeamToFirestoreAsync(team);

            await Dispatcher.InvokeAsync(() =>
            {
                if (syncSequence != _teamSyncSequence)
                {
                    return;
                }

                if (result.Success)
                {
                    SetTeamSyncStatus("Equipe sincronizada com sucesso no Firebase.", new SolidColorBrush(Color.FromRgb(21, 128, 61)));
                    return;
                }

                SetTeamSyncStatus(
                    $"Falha na sincronização automática: {result.ErrorMessage ?? "erro não identificado"}",
                    new SolidColorBrush(Color.FromRgb(220, 38, 38)));
            });
        }

        private void SetTeamSyncStatus(string message, Brush foreground)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => SetTeamSyncStatus(message, foreground));
                return;
            }

            TeamSyncStatusText.Text = message;
            TeamSyncStatusText.Foreground = foreground;
        }

        private async Task RefreshConnectionsCacheAsync()
        {
            _connectedUserIds.Clear();
            _connectionEntries.Clear();

            if (_connectionService == null)
            {
                UpdateConnectionsBadge();
                return;
            }

            _connectionEntries = await _connectionService.LoadConnectionsAsync();

            foreach (var userId in _connectionEntries
                .Where(item => string.Equals(item.Status, "connected", StringComparison.OrdinalIgnoreCase))
                .Select(item => item.ConnectedUserId)
                .Where(userId => !string.IsNullOrWhiteSpace(userId)))
            {
                _connectedUserIds.Add(userId);
            }

            UpdateConnectionsBadge();
            RenderConnectionsView();
        }

        private void RenderConnectionsView()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(RenderConnectionsView);
                return;
            }

            ConnectionsSectionsHost.Children.Clear();

            var incomingRequests = _connectionEntries
                .Where(item => string.Equals(item.Status, "pendingIncoming", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(item => item.UpdatedAt)
                .ToList();
            var outgoingRequests = _connectionEntries
                .Where(item => string.Equals(item.Status, "pendingOutgoing", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(item => item.UpdatedAt)
                .ToList();
            var acceptedNotifications = _connectionEntries
                .Where(item => string.Equals(item.NotificationType, "accepted", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(item => item.UpdatedAt)
                .ToList();
            var connectedUsers = _connectionEntries
                .Where(item => string.Equals(item.Status, "connected", StringComparison.OrdinalIgnoreCase))
                .OrderBy(item => item.ConnectedUserName)
                .ToList();

            ConnectionsSectionsHost.Children.Add(CreateConnectionsSection(
                "Solicitações recebidas",
                "Quem quer entrar na sua rede agora.",
                incomingRequests,
                ConnectionSectionMode.IncomingRequests));

            ConnectionsSectionsHost.Children.Add(CreateConnectionsSection(
                "Atualizações",
                "Aceites recentes e avisos da sua rede.",
                acceptedNotifications,
                ConnectionSectionMode.Notifications));

            ConnectionsSectionsHost.Children.Add(CreateConnectionsSection(
                "Convites enviados",
                "Solicitações aguardando resposta.",
                outgoingRequests,
                ConnectionSectionMode.OutgoingRequests));

            ConnectionsSectionsHost.Children.Add(CreateConnectionsSection(
                "Sua rede",
                "Perfis que já aceitaram a conexão.",
                connectedUsers,
                ConnectionSectionMode.Connected));

            var hasAnyItem = incomingRequests.Count > 0 || outgoingRequests.Count > 0 || acceptedNotifications.Count > 0 || connectedUsers.Count > 0;
            ConnectionsStatusText.Text = hasAnyItem
                ? $"{connectedUsers.Count} conexão(ões) ativa(s), {incomingRequests.Count} solicitação(ões) pendente(s) e {acceptedNotifications.Count} atualização(ões)."
                : "Sua rede ainda está vazia. Use a busca no topo para enviar convites de conexão.";
            ConnectionsStatusText.Foreground = GetThemeBrush("SecondaryTextBrush");
        }

        private enum ConnectionSectionMode
        {
            IncomingRequests,
            Notifications,
            OutgoingRequests,
            Connected
        }

        private Border CreateConnectionsSection(string title, string subtitle, List<UserConnectionInfo> items, ConnectionSectionMode mode)
        {
            var card = new Border
            {
                Background = GetThemeBrush("CardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(18),
                Padding = new Thickness(22),
                Margin = new Thickness(0, 0, 0, 14)
            };

            var stack = new StackPanel();
            stack.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            });
            stack.Children.Add(new TextBlock
            {
                Text = subtitle,
                Margin = new Thickness(0, 6, 0, 16),
                FontSize = 12,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            });

            if (items.Count == 0)
            {
                stack.Children.Add(new Border
                {
                    Background = GetThemeBrush("MutedCardBackgroundBrush"),
                    BorderBrush = GetThemeBrush("CardBorderBrush"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(14),
                    Padding = new Thickness(16),
                    Child = new TextBlock
                    {
                        Text = mode switch
                        {
                            ConnectionSectionMode.IncomingRequests => "Nenhuma solicitação aguardando sua resposta.",
                            ConnectionSectionMode.Notifications => "Nenhuma atualização de conexão no momento.",
                            ConnectionSectionMode.OutgoingRequests => "Você não possui convites enviados pendentes.",
                            _ => "Nenhuma conexão ativa ainda."
                        },
                        Foreground = GetThemeBrush("SecondaryTextBrush"),
                        FontSize = 12,
                        TextWrapping = TextWrapping.Wrap
                    }
                });
                card.Child = stack;
                return card;
            }

            foreach (var item in items)
            {
                stack.Children.Add(CreateConnectionListItem(item, mode));
            }

            card.Child = stack;
            return card;
        }

        private Border CreateConnectionListItem(UserConnectionInfo item, ConnectionSectionMode mode)
        {
            var container = new Border
            {
                Background = GetThemeBrush("MutedCardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(14),
                Padding = new Thickness(16),
                Margin = new Thickness(0, 0, 0, 10)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var textStack = new StackPanel();
            textStack.Children.Add(new TextBlock
            {
                Text = item.ConnectedUserName,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            });
            textStack.Children.Add(new TextBlock
            {
                Text = string.IsNullOrWhiteSpace(item.NotificationMessage)
                    ? mode switch
                    {
                        ConnectionSectionMode.Connected => $"Conectado desde {item.UpdatedAt:dd/MM 'às' HH:mm}.",
                        ConnectionSectionMode.OutgoingRequests => $"Convite enviado em {item.AddedAt:dd/MM 'às' HH:mm}.",
                        _ => $"Solicitação recebida em {item.AddedAt:dd/MM 'às' HH:mm}."
                    }
                    : item.NotificationMessage,
                Margin = new Thickness(0, 4, 0, 0),
                FontSize = 12,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 520
            });
            if (!string.IsNullOrWhiteSpace(item.ConnectedUserEmail))
            {
                textStack.Children.Add(new TextBlock
                {
                    Text = item.ConnectedUserEmail,
                    Margin = new Thickness(0, 8, 0, 0),
                    FontSize = 11,
                    Foreground = GetThemeBrush("TertiaryTextBrush")
                });
            }
            grid.Children.Add(textStack);

            var actions = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(16, 0, 0, 0)
            };

            if (mode == ConnectionSectionMode.IncomingRequests)
            {
                actions.Children.Add(CreateConnectionSecondaryButton("Detalhes", item, OpenConnectionProfile_Click));
                actions.Children.Add(CreateConnectionSecondaryButton("Conversar", item, OpenConnectionChat_Click));
                actions.Children.Add(CreateConnectionActionButton("Recusar", item, DeclineConnectionRequest_Click, false));
                actions.Children.Add(CreateConnectionActionButton("Aceitar", item, AcceptConnectionRequest_Click, true));
            }
            else if (mode == ConnectionSectionMode.Notifications)
            {
                actions.Children.Add(CreateConnectionSecondaryButton("Detalhes", item, OpenConnectionProfile_Click));
                actions.Children.Add(CreateConnectionSecondaryButton("Conversar", item, OpenConnectionChat_Click));
                actions.Children.Add(CreateConnectionActionButton("Marcar como lida", item, DismissConnectionNotification_Click, false));
            }
            else if (mode == ConnectionSectionMode.OutgoingRequests)
            {
                actions.Children.Add(CreateConnectionSecondaryButton("Detalhes", item, OpenConnectionProfile_Click));
                actions.Children.Add(CreateConnectionSecondaryButton("Conversar", item, OpenConnectionChat_Click));
                actions.Children.Add(CreateConnectionStatusPill("Aguardando resposta"));
            }
            else
            {
                actions.Children.Add(CreateConnectionSecondaryButton("Detalhes", item, OpenConnectionProfile_Click));
                actions.Children.Add(CreateConnectionActionButton("Conversar", item, OpenConnectionChat_Click, true));
                actions.Children.Add(CreateConnectionStatusPill("Conectado"));
            }

            Grid.SetColumn(actions, 1);
            grid.Children.Add(actions);
            container.Child = grid;
            return container;
        }

        private Button CreateConnectionActionButton(string label, UserConnectionInfo item, RoutedEventHandler clickHandler, bool isPrimary)
        {
            var button = new Button
            {
                Content = label,
                Tag = item,
                Height = 38,
                MinWidth = 110,
                Margin = new Thickness(10, 0, 0, 0),
                Padding = new Thickness(14, 8, 14, 8),
                Background = isPrimary ? GetThemeBrush("AccentBrush") : GetThemeBrush("MutedCardBackgroundBrush"),
                Foreground = isPrimary ? Brushes.White : GetThemeBrush("PrimaryTextBrush"),
                BorderBrush = isPrimary ? Brushes.Transparent : GetThemeBrush("CardBorderBrush"),
                BorderThickness = isPrimary ? new Thickness(0) : new Thickness(1),
                Cursor = Cursors.Hand,
                FontWeight = FontWeights.SemiBold
            };
            button.Click += clickHandler;
            return button;
        }

        private Button CreateConnectionSecondaryButton(string label, UserConnectionInfo item, RoutedEventHandler clickHandler)
        {
            return CreateConnectionActionButton(label, item, clickHandler, false);
        }

        private Border CreateConnectionStatusPill(string text)
        {
            return new Border
            {
                Background = GetThemeBrush("AccentMutedBrush"),
                CornerRadius = new CornerRadius(999),
                Padding = new Thickness(10, 5, 10, 5),
                Margin = new Thickness(10, 0, 0, 0),
                Child = new TextBlock
                {
                    Text = text,
                    Foreground = GetThemeBrush("AccentBrush"),
                    FontSize = 11,
                    FontWeight = FontWeights.SemiBold
                }
            };
        }

        private async void RefreshConnections_Click(object sender, RoutedEventArgs e)
        {
            ConnectionsStatusText.Text = "Atualizando conexões...";
            await RefreshConnectionsCacheAsync();
        }

        private async void AcceptConnectionRequest_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: UserConnectionInfo item } || _connectionService == null)
            {
                return;
            }

            var result = await _connectionService.AcceptConnectionAsync(item);
            if (!result.Success)
            {
                ConnectionsStatusText.Text = $"Falha ao aceitar conexão: {result.ErrorMessage}";
                ConnectionsStatusText.Foreground = new SolidColorBrush(Color.FromRgb(220, 38, 38));
                return;
            }

            await RefreshConnectionsCacheAsync();
            ConnectionsStatusText.Text = $"Conexão com {item.ConnectedUserName} aceita com sucesso.";
            ConnectionsStatusText.Foreground = new SolidColorBrush(Color.FromRgb(21, 128, 61));
        }

        private async void DeclineConnectionRequest_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: UserConnectionInfo item } || _connectionService == null)
            {
                return;
            }

            var result = await _connectionService.DeclineConnectionAsync(item);
            if (!result.Success)
            {
                ConnectionsStatusText.Text = $"Falha ao recusar conexão: {result.ErrorMessage}";
                ConnectionsStatusText.Foreground = new SolidColorBrush(Color.FromRgb(220, 38, 38));
                return;
            }

            await RefreshConnectionsCacheAsync();
            ConnectionsStatusText.Text = "Solicitação recusada sem notificar o outro usuário.";
            ConnectionsStatusText.Foreground = GetThemeBrush("SecondaryTextBrush");
        }

        private async void DismissConnectionNotification_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: UserConnectionInfo item } || _connectionService == null)
            {
                return;
            }

            var result = await _connectionService.MarkNotificationAsReadAsync(item);
            if (!result.Success)
            {
                ConnectionsStatusText.Text = $"Falha ao atualizar notificação: {result.ErrorMessage}";
                ConnectionsStatusText.Foreground = new SolidColorBrush(Color.FromRgb(220, 38, 38));
                return;
            }

            await RefreshConnectionsCacheAsync();
            ConnectionsStatusText.Text = "Atualização arquivada.";
            ConnectionsStatusText.Foreground = GetThemeBrush("SecondaryTextBrush");
        }

        private void OpenConnectionChat_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: UserConnectionInfo item })
            {
                return;
            }

            ShowConversationInMainWindow(CreateUserFromConnection(item));
        }

        private async void OpenConnectionProfile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: UserConnectionInfo item })
            {
                return;
            }

            await ShowUserProfileDialogAsync(CreateUserFromConnection(item));
        }

        private void UpdateConnectionsBadge()
        {
            var unreadCount = _connectionEntries.Count(item =>
                string.Equals(item.Status, "pendingIncoming", StringComparison.OrdinalIgnoreCase)
                || (!item.IsRead && !string.IsNullOrWhiteSpace(item.NotificationType)));

            if (unreadCount > 0)
            {
                ConnectionsUnreadBadge.Visibility = Visibility.Visible;
                ConnectionsUnreadCountText.Text = unreadCount > 99 ? "99+" : unreadCount.ToString();
                ConnectionsNavButton.Background = _appDarkModeEnabled
                    ? new SolidColorBrush(Color.FromRgb(6, 78, 59))
                    : new SolidColorBrush(Color.FromRgb(236, 253, 245));
            }
            else
            {
                ConnectionsUnreadBadge.Visibility = Visibility.Collapsed;
                ConnectionsUnreadCountText.Text = "0";
                ConnectionsNavButton.Background = new SolidColorBrush(Colors.Transparent);
            }
        }

        /// <summary>
        /// Salva a equipe no Firebase Firestore
        /// </summary>
        private async Task<TeamOperationResult> SaveTeamToFirestoreAsync(TeamWorkspaceInfo team)
        {
            try
            {
                if (_teamService == null)
                {
                    DebugHelper.WriteLine("[SaveTeamToFirebase] TeamService não inicializado");
                    return TeamOperationResult.Fail("TeamService nao inicializado.");
                }

                DebugHelper.WriteLine($"[SaveTeamToFirebase] Salvando equipe '{team.TeamName}' no Firebase...");

                var result = await _teamService.SaveTeamAsync(team);
                if (!result.Success)
                {
                    DebugHelper.WriteLine($"[SaveTeamToFirebase] Erro ao salvar: {result.ErrorMessage}");
                }
                else
                {
                    DebugHelper.WriteLine($"[SaveTeamToFirebase] Equipe salva com sucesso!");
                }

                return result;
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[SaveTeamToFirebase ERROR] {ex.Message}");
                DebugHelper.WriteLine($"[SaveTeamToFirebase ERROR] Stack: {ex.StackTrace}");
                return TeamOperationResult.Fail(ex.Message);
            }
        }

        private void UpdateTeamsViewState()
        {
            TeamEntryOptionsPopup.IsOpen = false;
            TeamsEmptyStateCard.Visibility = _teamWorkspaces.Count == 0 && _teamEntryMode == TeamEntryMode.None
                ? Visibility.Visible
                : Visibility.Collapsed;
            TeamJoinCard.Visibility = _teamEntryMode == TeamEntryMode.Join
                ? Visibility.Visible
                : Visibility.Collapsed;
            TeamCreationCard.Visibility = _teamEntryMode == TeamEntryMode.Create
                ? Visibility.Visible
                : Visibility.Collapsed;
            TeamListCard.Visibility = _teamWorkspaces.Count > 0
                ? Visibility.Visible
                : Visibility.Collapsed;
            TeamWorkspaceCard.Visibility = _activeTeamWorkspace == null
                ? Visibility.Collapsed
                : Visibility.Visible;
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
            RefreshAvatarUi(profile);
            ProfessionalProfileStatusText.Text = string.Empty;
        }

        private void RefreshAvatarUi(UserProfile profile)
        {
            RenderProfileAvatarPreview(profile);
            RenderSidebarAvatarPreview(profile);

            var hasAvatar = HasCustomAvatar(profile);
            AvatarActionButton.Content = hasAvatar ? "Editar avatar" : "Criar seu ícone";
            AvatarEditorHintText.Text = hasAvatar
                ? "Seu avatar já está configurado. Ajuste personagem, cabelo, hat, acessório e roupa quando quiser."
                : "Monte seu personagem com pele, cabelo, hat e roupa. O acessório continua opcional.";
        }

        private void RenderProfileAvatarPreview(UserProfile profile)
        {
            ProfileAvatarPreviewHost.Children.Clear();
            ProfileAvatarPreviewHost.Children.Add(CreateProfileAvatarVisual(profile));
        }

        private void RenderSidebarAvatarPreview(UserProfile profile)
        {
            SidebarAvatarHost.Children.Clear();
            SidebarAvatarHost.Children.Add(CreateSidebarAvatarVisual(profile));
        }

        private bool HasCustomAvatar(UserInfo? user)
        {
            return user != null && IsValidAvatarSelection(user.AvatarBody, user.AvatarHair, user.AvatarHat, user.AvatarAccessory, user.AvatarClothing);
        }

        private bool HasCustomAvatar(Conversation? conversation)
        {
            return conversation != null && IsValidAvatarSelection(conversation.ContactAvatarBody, conversation.ContactAvatarHair, conversation.ContactAvatarHat, conversation.ContactAvatarAccessory, conversation.ContactAvatarClothing);
        }

        private bool HasCustomAvatar(UserProfile? profile)
        {
            return profile != null && IsValidAvatarSelection(profile.AvatarBody, profile.AvatarHair, profile.AvatarHat, profile.AvatarAccessory, profile.AvatarClothing);
        }

        private bool IsValidAvatarSelection(string? body, string? hair, string? hat, string? accessory, string? clothing)
        {
            var normalizedBody = body?.Trim() ?? string.Empty;
            var normalizedHair = hair?.Trim() ?? string.Empty;
            var normalizedHat = hat?.Trim() ?? string.Empty;
            var normalizedAccessory = accessory?.Trim() ?? string.Empty;
            var normalizedClothing = clothing?.Trim() ?? string.Empty;

            return IsValidNewAvatarSelection(normalizedBody, normalizedHair, normalizedHat, normalizedAccessory, normalizedClothing)
                || IsValidLegacyAvatarSelection(normalizedBody, normalizedHair, normalizedAccessory);
        }

        private bool IsValidNewAvatarSelection(string body, string hair, string hat, string accessory, string clothing)
        {
            return AvatarBodyOptions.Contains(body, StringComparer.Ordinal)
                && AvatarHairOptions.Contains(hair, StringComparer.Ordinal)
                && AvatarHatOptions.Contains(hat, StringComparer.Ordinal)
                && AvatarClothingOptions.Contains(clothing, StringComparer.Ordinal)
                && (string.IsNullOrWhiteSpace(accessory)
                    || AvatarAccessoryOptions.Contains(accessory, StringComparer.Ordinal));
        }

        private bool IsValidLegacyAvatarSelection(string body, string hair, string accessory)
        {
            return LegacyAvatarBodyOptions.Contains(body, StringComparer.Ordinal)
                && LegacyAvatarHairOptions.Contains(hair, StringComparer.Ordinal)
                && (string.IsNullOrWhiteSpace(accessory)
                    || AvatarAccessoryOptions.Contains(accessory, StringComparer.Ordinal));
        }

        private string NormalizeAvatarOption(string? selectedValue, IEnumerable<string> validOptions, string fallback)
        {
            var normalized = selectedValue?.Trim() ?? string.Empty;
            return validOptions.Contains(normalized, StringComparer.Ordinal) ? normalized : fallback;
        }

        private string NormalizeOptionalAvatarOption(string? selectedValue, IEnumerable<string> validOptions)
        {
            var normalized = selectedValue?.Trim() ?? string.Empty;
            return validOptions.Contains(normalized, StringComparer.Ordinal) ? normalized : string.Empty;
        }

        private string NormalizeAvatarBodySelection(string? selectedValue)
        {
            var normalized = selectedValue?.Trim() ?? string.Empty;
            return normalized switch
            {
                "1" => "PeleBranca",
                "2" => "PelePardo",
                "3" => "PelePreto",
                _ => NormalizeAvatarOption(normalized, AvatarBodyOptions, AvatarDefaultBody)
            };
        }

        private string NormalizeAvatarHairSelection(string? selectedValue)
        {
            var normalized = selectedValue?.Trim() ?? string.Empty;
            if (AvatarHairOptions.Contains(normalized, StringComparer.Ordinal))
            {
                return normalized;
            }

            return normalized switch
            {
                "1" => "Male_CastanhoEscuro_Curto_0",
                "2" => "Male_Preto_Curto_0",
                "3" => "Male_Loiro_Curto_0",
                "4" => "FEMALE_CastanhoEscuro_Medio_0",
                "5" => "FEMALE_Preto_Longo_0",
                "6" => "FEMALE_Ruivo_Longo_0",
                _ => AvatarHairOptions.First()
            };
        }

        private void NormalizeProfileAvatarSelection(UserProfile profile)
        {
            profile.AvatarBody = NormalizeAvatarBodySelection(profile.AvatarBody);
            profile.AvatarHair = NormalizeAvatarHairSelection(profile.AvatarHair);
            profile.AvatarHat = NormalizeAvatarOption(profile.AvatarHat, AvatarHatOptions, AvatarHatOptions.First());
            profile.AvatarAccessory = NormalizeOptionalAvatarOption(profile.AvatarAccessory, AvatarAccessoryOptions);
            profile.AvatarClothing = NormalizeAvatarOption(profile.AvatarClothing, AvatarClothingOptions, AvatarClothingOptions.First());
        }

        private UIElement CreateProfileAvatarVisual(UserProfile? profile)
        {
            if (!HasCustomAvatar(profile))
            {
                var placeholder = new Border
                {
                    Width = 132,
                    Height = 132,
                    CornerRadius = new CornerRadius(22),
                    Background = GetThemeBrush("AccentMutedBrush"),
                    BorderBrush = GetThemeBrush("CardBorderBrush"),
                    BorderThickness = new Thickness(1)
                };

                var content = new StackPanel
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                content.Children.Add(new TextBlock
                {
                    Text = "+",
                    FontSize = 36,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = GetThemeBrush("AccentBrush")
                });
                content.Children.Add(new TextBlock
                {
                    Text = "Crie seu ícone",
                    Margin = new Thickness(0, 4, 0, 0),
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = GetThemeBrush("PrimaryTextBrush")
                });

                placeholder.Child = content;
                return placeholder;
            }

            return CreateAvatarCompositeVisual(profile!, 132, new CornerRadius(22), showBorder: true);
        }

        private UIElement CreateSidebarAvatarVisual(UserProfile? profile)
        {
            if (HasCustomAvatar(profile))
            {
                return CreateAvatarCompositeVisual(profile!, 40, new CornerRadius(20), showBorder: false);
            }

            var initial = (profile?.Name ?? "U")
                .Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();
            var letter = string.IsNullOrWhiteSpace(initial) ? "U" : initial.Substring(0, 1).ToUpperInvariant();

            var grid = new Grid
            {
                Width = 40,
                Height = 40
            };
            grid.Children.Add(new Ellipse
            {
                Fill = GetThemeBrush("AccentBrush"),
                Width = 40,
                Height = 40
            });
            grid.Children.Add(new TextBlock
            {
                Text = letter,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.White
            });

            return grid;
        }

        private Border CreateAvatarCompositeVisual(UserProfile profile, double size, CornerRadius cornerRadius, bool showBorder)
        {
            return CreateAvatarCompositeVisual(profile.AvatarBody, profile.AvatarHair, profile.AvatarHat, profile.AvatarAccessory, profile.AvatarClothing, size, cornerRadius, showBorder);
        }

        private Border CreateAvatarCompositeVisual(string body, string hair, string hat, string accessory, string clothing, double size, CornerRadius cornerRadius, bool showBorder)
        {
            var container = new Border
            {
                Width = size,
                Height = size,
                CornerRadius = cornerRadius,
                Background = new SolidColorBrush(Color.FromRgb(248, 250, 252)),
                BorderBrush = showBorder ? GetThemeBrush("CardBorderBrush") : Brushes.Transparent,
                BorderThickness = showBorder ? new Thickness(1) : new Thickness(0),
                ClipToBounds = true
            };

            var viewbox = new Viewbox
            {
                Stretch = Stretch.Uniform
            };

            var layers = new Grid
            {
                Width = 256,
                Height = 256,
                ClipToBounds = true
            };

            if (IsValidNewAvatarSelection(body, hair, hat, accessory, clothing))
            {
                AddAvatarLayer(layers, "Personagens", body);
                AddAvatarLayer(layers, GetHairFolder(hair), hair);
                AddAvatarLayer(layers, "Hats", hat);
                AddAvatarLayer(layers, "Acessory", accessory);
                AddAvatarLayer(layers, "Roupas", clothing);
            }
            else
            {
                AddAvatarLayer(layers, "Chars", body);
                AddAvatarLayer(layers, "Hairs", hair);
                AddAvatarLayer(layers, "Acessory", accessory);
            }

            viewbox.Child = layers;
            container.Child = viewbox;
            return container;
        }

        private UIElement CreateUserAvatarVisual(UserInfo? user, double size, bool showBorder = false)
        {
            if (HasCustomAvatar(user))
            {
                return CreateAvatarCompositeVisual(user!.AvatarBody, user.AvatarHair, user.AvatarHat, user.AvatarAccessory, user.AvatarClothing, size, new CornerRadius(size / 2), showBorder);
            }

            return CreateFallbackAvatarVisual(user?.Name, size, showBorder, GetThemeBrush("AccentBrush"));
        }

        private UIElement CreateConversationAvatarVisual(Conversation? conversation, double size, bool showBorder = false)
        {
            if (HasCustomAvatar(conversation))
            {
                return CreateAvatarCompositeVisual(conversation!.ContactAvatarBody, conversation.ContactAvatarHair, conversation.ContactAvatarHat, conversation.ContactAvatarAccessory, conversation.ContactAvatarClothing, size, new CornerRadius(size / 2), showBorder);
            }

            return CreateFallbackAvatarVisual(conversation?.ContactName, size, showBorder, new SolidColorBrush(Color.FromRgb(0, 168, 132)));
        }

        private Grid CreateFallbackAvatarVisual(string? displayName, double size, bool showBorder, Brush fill)
        {
            var grid = new Grid
            {
                Width = size,
                Height = size
            };
            grid.Children.Add(new Ellipse
            {
                Fill = fill,
                Width = size,
                Height = size,
                Stroke = showBorder ? GetThemeBrush("CardBorderBrush") : Brushes.Transparent,
                StrokeThickness = showBorder ? 1 : 0
            });
            grid.Children.Add(new TextBlock
            {
                Text = string.IsNullOrWhiteSpace(displayName) ? "?" : displayName[..1].ToUpperInvariant(),
                Foreground = Brushes.White,
                FontSize = Math.Max(12, size * 0.38),
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });
            return grid;
        }

        private async Task<UserInfo?> LoadUserInfoByIdAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(_idToken))
            {
                return null;
            }

            if (string.Equals(userId, _currentProfile?.UserId, StringComparison.OrdinalIgnoreCase) && _currentProfile != null)
            {
                return CreateCurrentUserInfo();
            }

            try
            {
                var endpoint = $"https://firestore.googleapis.com/v1/projects/{FirebaseProjectId}/databases/(default)/documents/users/{userId}";
                var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);

                var response = await httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    DebugHelper.WriteLine($"[LoadUserInfoByIdAsync] Falha ao carregar {userId}: {response.StatusCode} | {content}");
                    return null;
                }

                using var doc = JsonDocument.Parse(content);
                if (!doc.RootElement.TryGetProperty("fields", out var fields))
                {
                    return null;
                }

                return new UserInfo
                {
                    UserId = userId,
                    Name = GetFirestoreStringValue(fields, "name"),
                    Email = GetFirestoreStringValue(fields, "email"),
                    Phone = GetFirestoreStringValue(fields, "phone"),
                    Registration = GetFirestoreStringValue(fields, "registration"),
                    Course = GetFirestoreStringValue(fields, "course"),
                    Role = GetFirestoreStringValue(fields, "role"),
                    Nickname = GetFirestoreStringValue(fields, "nickname"),
                    ProfessionalTitle = GetFirestoreStringValue(fields, "professionalTitle"),
                    Bio = GetFirestoreStringValue(fields, "bio"),
                    Skills = GetFirestoreStringValue(fields, "skills"),
                    ProgrammingLanguages = GetFirestoreStringValue(fields, "programmingLanguages"),
                    PortfolioLink = GetFirestoreStringValue(fields, "portfolioLink"),
                    LinkedInLink = GetFirestoreStringValue(fields, "linkedInLink"),
                    AvatarBody = GetFirestoreStringValue(fields, "avatarBody"),
                    AvatarHair = GetFirestoreStringValue(fields, "avatarHair"),
                    AvatarHat = GetFirestoreStringValue(fields, "avatarHat"),
                    AvatarAccessory = GetFirestoreStringValue(fields, "avatarAccessory"),
                    AvatarClothing = GetFirestoreStringValue(fields, "avatarClothing")
                };
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[LoadUserInfoByIdAsync] Exceção ao carregar {userId}: {ex.Message}");
                return null;
            }
        }

        private Task<UserInfo?> LoadUserInfoCachedAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Task.FromResult<UserInfo?>(null);
            }

            return _userInfoCache.GetOrAdd(userId, LoadUserInfoByIdAsync);
        }

        private async Task<UserProfile?> LoadUserProfileByIdAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(_idToken))
            {
                return null;
            }

            if (string.Equals(userId, _currentProfile?.UserId, StringComparison.OrdinalIgnoreCase) && _currentProfile != null)
            {
                return _currentProfile;
            }

            try
            {
                var endpoint = $"https://firestore.googleapis.com/v1/projects/{FirebaseProjectId}/databases/(default)/documents/users/{userId}";
                var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _idToken);

                var response = await httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    DebugHelper.WriteLine($"[LoadUserProfileByIdAsync] Falha ao carregar {userId}: {response.StatusCode} | {content}");
                    return null;
                }

                using var doc = JsonDocument.Parse(content);
                if (!doc.RootElement.TryGetProperty("fields", out var fields))
                {
                    return null;
                }

                return new UserProfile
                {
                    UserId = userId,
                    Name = GetFirestoreStringValue(fields, "name"),
                    Email = GetFirestoreStringValue(fields, "email"),
                    Phone = GetFirestoreStringValue(fields, "phone"),
                    Course = GetFirestoreStringValue(fields, "course"),
                    Registration = GetFirestoreStringValue(fields, "registration"),
                    Nickname = GetFirestoreStringValue(fields, "nickname"),
                    ProfessionalTitle = GetFirestoreStringValue(fields, "professionalTitle"),
                    Bio = GetFirestoreStringValue(fields, "bio"),
                    Skills = GetFirestoreStringValue(fields, "skills"),
                    ProgrammingLanguages = GetFirestoreStringValue(fields, "programmingLanguages"),
                    PortfolioLink = GetFirestoreStringValue(fields, "portfolioLink"),
                    LinkedInLink = GetFirestoreStringValue(fields, "linkedInLink"),
                    AvatarBody = GetFirestoreStringValue(fields, "avatarBody"),
                    AvatarHair = GetFirestoreStringValue(fields, "avatarHair"),
                    AvatarHat = GetFirestoreStringValue(fields, "avatarHat"),
                    AvatarAccessory = GetFirestoreStringValue(fields, "avatarAccessory"),
                    AvatarClothing = GetFirestoreStringValue(fields, "avatarClothing")
                };
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[LoadUserProfileByIdAsync] Exceção ao carregar {userId}: {ex.Message}");
                return null;
            }
        }

        private Task<UserProfile?> LoadUserProfileCachedAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Task.FromResult<UserProfile?>(null);
            }

            return _userProfileCache.GetOrAdd(userId, LoadUserProfileByIdAsync);
        }

        private async Task ShowUserProfileDialogAsync(UserInfo summaryUser)
        {
            if (summaryUser == null || string.IsNullOrWhiteSpace(summaryUser.UserId))
            {
                return;
            }

            var profile = await LoadUserProfileCachedAsync(summaryUser.UserId) ?? new UserProfile();
            profile.UserId = string.IsNullOrWhiteSpace(profile.UserId) ? summaryUser.UserId : profile.UserId;
            profile.Name = string.IsNullOrWhiteSpace(profile.Name) ? summaryUser.Name : profile.Name;
            profile.Email = string.IsNullOrWhiteSpace(profile.Email) ? summaryUser.Email : profile.Email;
            profile.Phone = string.IsNullOrWhiteSpace(profile.Phone) ? summaryUser.Phone : profile.Phone;
            profile.Course = string.IsNullOrWhiteSpace(profile.Course) ? summaryUser.Course : profile.Course;
            profile.Registration = string.IsNullOrWhiteSpace(profile.Registration) ? summaryUser.Registration : profile.Registration;
            profile.Nickname = string.IsNullOrWhiteSpace(profile.Nickname) ? summaryUser.Nickname : profile.Nickname;
            profile.ProfessionalTitle = string.IsNullOrWhiteSpace(profile.ProfessionalTitle) ? summaryUser.ProfessionalTitle : profile.ProfessionalTitle;
            profile.Bio = string.IsNullOrWhiteSpace(profile.Bio) ? summaryUser.Bio : profile.Bio;
            profile.Skills = string.IsNullOrWhiteSpace(profile.Skills) ? summaryUser.Skills : profile.Skills;
            profile.ProgrammingLanguages = string.IsNullOrWhiteSpace(profile.ProgrammingLanguages) ? summaryUser.ProgrammingLanguages : profile.ProgrammingLanguages;
            profile.PortfolioLink = string.IsNullOrWhiteSpace(profile.PortfolioLink) ? summaryUser.PortfolioLink : profile.PortfolioLink;
            profile.LinkedInLink = string.IsNullOrWhiteSpace(profile.LinkedInLink) ? summaryUser.LinkedInLink : profile.LinkedInLink;
            profile.AvatarBody = string.IsNullOrWhiteSpace(profile.AvatarBody) ? summaryUser.AvatarBody : profile.AvatarBody;
            profile.AvatarHair = string.IsNullOrWhiteSpace(profile.AvatarHair) ? summaryUser.AvatarHair : profile.AvatarHair;
            profile.AvatarHat = string.IsNullOrWhiteSpace(profile.AvatarHat) ? summaryUser.AvatarHat : profile.AvatarHat;
            profile.AvatarAccessory = string.IsNullOrWhiteSpace(profile.AvatarAccessory) ? summaryUser.AvatarAccessory : profile.AvatarAccessory;
            profile.AvatarClothing = string.IsNullOrWhiteSpace(profile.AvatarClothing) ? summaryUser.AvatarClothing : profile.AvatarClothing;

            var avatarUser = new UserInfo
            {
                UserId = profile.UserId,
                Name = profile.Name,
                Email = profile.Email,
                Registration = profile.Registration,
                Course = profile.Course,
                AvatarBody = profile.AvatarBody,
                AvatarHair = profile.AvatarHair,
                AvatarHat = profile.AvatarHat,
                AvatarAccessory = profile.AvatarAccessory,
                AvatarClothing = profile.AvatarClothing
            };

            var dialog = new UserProfileViewWindow(profile, CreateUserAvatarVisual(avatarUser, 108, true))
            {
                Owner = this
            };

            dialog.ShowDialog();
        }

        private string GetFirestoreStringValue(JsonElement fields, string fieldName)
        {
            return fields.TryGetProperty(fieldName, out var field) && field.TryGetProperty("stringValue", out var value)
                ? value.GetString() ?? string.Empty
                : string.Empty;
        }

        private async Task EnrichConversationAvatarsAsync(List<Conversation> conversations)
        {
            var pendingConversations = conversations
                .Where(conversation => !HasCustomAvatar(conversation) && !string.IsNullOrWhiteSpace(conversation.ContactId))
                .ToList();

            if (pendingConversations.Count == 0)
            {
                return;
            }

            var usersById = await Task.WhenAll(pendingConversations
                .Select(async conversation => new
                {
                    conversation.ContactId,
                    User = await LoadUserInfoCachedAsync(conversation.ContactId)
                }));

            var userLookup = usersById
                .Where(item => item.User != null)
                .GroupBy(item => item.ContactId, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First().User!, StringComparer.OrdinalIgnoreCase);

            foreach (var conversation in pendingConversations)
            {
                if (!userLookup.TryGetValue(conversation.ContactId, out var user))
                {
                    continue;
                }

                conversation.ContactAvatarBody = user.AvatarBody;
                conversation.ContactAvatarHair = user.AvatarHair;
                conversation.ContactAvatarHat = user.AvatarHat;
                conversation.ContactAvatarAccessory = user.AvatarAccessory;
                conversation.ContactAvatarClothing = user.AvatarClothing;
            }
        }

        private async Task EnrichTeamMembersAvatarsAsync(List<TeamWorkspaceInfo> teams)
        {
            var pendingMembers = teams
                .SelectMany(team => team.Members)
                .Where(member => !HasCustomAvatar(member) && !string.IsNullOrWhiteSpace(member.UserId))
                .ToList();

            if (pendingMembers.Count == 0)
            {
                return;
            }

            var usersById = await Task.WhenAll(pendingMembers
                .Select(async member => new
                {
                    member.UserId,
                    User = await LoadUserInfoCachedAsync(member.UserId)
                }));

            var userLookup = usersById
                .Where(item => item.User != null)
                .GroupBy(item => item.UserId, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First().User!, StringComparer.OrdinalIgnoreCase);

            foreach (var member in pendingMembers)
            {
                if (!userLookup.TryGetValue(member.UserId, out var user))
                {
                    continue;
                }

                member.AvatarBody = user.AvatarBody;
                member.AvatarHair = user.AvatarHair;
                member.AvatarHat = user.AvatarHat;
                member.AvatarAccessory = user.AvatarAccessory;
                member.AvatarClothing = user.AvatarClothing;
            }
        }

        private string GetHairFolder(string option)
        {
            if (option.Contains("_Curto_", StringComparison.OrdinalIgnoreCase))
            {
                return "Cabelos_Curtos";
            }

            if (option.Contains("_Longo_", StringComparison.OrdinalIgnoreCase))
            {
                return "Cabelos_Longos";
            }

            return "Cabelos_Medios";
        }

        private static string GetAvatarBodyLabel(string option)
        {
            return option switch
            {
                "PeleBranca" => "Pele branca",
                "PelePardo" => "Pele parda",
                "PelePreto" => "Pele preta",
                _ => option
            };
        }

        private static string GetAvatarHairLabel(string option)
        {
            return option
                .Replace("FEMALE", "Feminino", StringComparison.Ordinal)
                .Replace("Male", "Masculino", StringComparison.Ordinal)
                .Replace("CastanhoEscuro", "Castanho escuro", StringComparison.Ordinal)
                .Replace("CastanhoClaro", "Castanho claro", StringComparison.Ordinal)
                .Replace("Castanho", "Castanho", StringComparison.Ordinal)
                .Replace("Loiro", "Loiro", StringComparison.Ordinal)
                .Replace("Preto", "Preto", StringComparison.Ordinal)
                .Replace("Ruivo", "Ruivo", StringComparison.Ordinal)
                .Replace("Curto", "Curto", StringComparison.Ordinal)
                .Replace("Medio", "Medio", StringComparison.Ordinal)
                .Replace("medio", "medio", StringComparison.Ordinal)
                .Replace("Longo", "Longo", StringComparison.Ordinal)
                .Replace("_0", " · estilo 1", StringComparison.Ordinal)
                .Replace("_1", " · estilo 2", StringComparison.Ordinal)
                .Replace("_", " ", StringComparison.Ordinal)
                .Trim();
        }

        private static string GetAvatarHairPresentation(string option)
        {
            return option.StartsWith("FEMALE", StringComparison.OrdinalIgnoreCase)
                ? "Feminino"
                : "Masculino";
        }

        private static string GetAvatarHairLength(string option)
        {
            if (option.Contains("_Curto_", StringComparison.OrdinalIgnoreCase))
            {
                return "Curto";
            }

            if (option.Contains("_Longo_", StringComparison.OrdinalIgnoreCase))
            {
                return "Longo";
            }

            return "Médio";
        }

        private static string ExtractAvatarHairColorKey(string option)
        {
            var colors = new[]
            {
                "CastanhoEscuro",
                "CastanhoClaro",
                "Castanho",
                "Loiro",
                "Preto",
                "Ruivo"
            };

            return colors.FirstOrDefault(color => option.Contains(color, StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
        }

        private static int GetAvatarHairVariantIndex(string option)
        {
            return option.EndsWith("_1", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
        }

        private static int GetAvatarHairSimilarityScore(string referenceHair, string candidateHair)
        {
            if (string.Equals(referenceHair, candidateHair, StringComparison.Ordinal))
            {
                return 1000;
            }

            var score = 0;
            if (string.Equals(GetAvatarHairPresentation(referenceHair), GetAvatarHairPresentation(candidateHair), StringComparison.Ordinal))
            {
                score += 220;
            }

            if (string.Equals(GetAvatarHairLength(referenceHair), GetAvatarHairLength(candidateHair), StringComparison.Ordinal))
            {
                score += 180;
            }

            if (string.Equals(ExtractAvatarHairColorKey(referenceHair), ExtractAvatarHairColorKey(candidateHair), StringComparison.Ordinal))
            {
                score += 120;
            }

            if (GetAvatarHairVariantIndex(referenceHair) == GetAvatarHairVariantIndex(candidateHair))
            {
                score += 40;
            }

            return score;
        }

        private static string GetAvatarHatLabel(string option)
        {
            return option.Replace("_", " ", StringComparison.Ordinal);
        }

        private static string GetAvatarClothingLabel(string option)
        {
            return option
                .Replace("Roupa_", string.Empty, StringComparison.Ordinal)
                .Replace("AzulEscuro", "Azul escuro", StringComparison.Ordinal)
                .Replace("Azul", "Azul", StringComparison.Ordinal)
                .Replace("Branca", "Branca", StringComparison.Ordinal)
                .Replace("Laranja", "Laranja", StringComparison.Ordinal)
                .Replace("Preta", "Preta", StringComparison.Ordinal)
                .Replace("Rosa", "Rosa", StringComparison.Ordinal)
                .Replace("Roxa", "Roxa", StringComparison.Ordinal)
                .Replace("Vermelha", "Vermelha", StringComparison.Ordinal);
        }

        private static int GetAvatarHatNumber(string option)
        {
            return int.TryParse(option.Replace("Hat_", string.Empty, StringComparison.OrdinalIgnoreCase), out var number)
                ? number
                : int.MaxValue;
        }

        private static int GetAvatarHatSimilarityScore(string referenceHat, string candidateHat)
        {
            if (string.Equals(referenceHat, candidateHat, StringComparison.Ordinal))
            {
                return 1000;
            }

            return Math.Max(0, 100 - Math.Abs(GetAvatarHatNumber(referenceHat) - GetAvatarHatNumber(candidateHat)) * 8);
        }

        private static int GetAvatarClothingSimilarityScore(string referenceClothing, string candidateClothing)
        {
            if (string.Equals(referenceClothing, candidateClothing, StringComparison.Ordinal))
            {
                return 1000;
            }

            var referenceLabel = GetAvatarClothingLabel(referenceClothing);
            var candidateLabel = GetAvatarClothingLabel(candidateClothing);

            if (string.Equals(referenceLabel, candidateLabel, StringComparison.Ordinal))
            {
                return 600;
            }

            var isReferenceDark = referenceLabel.Contains("escuro", StringComparison.OrdinalIgnoreCase)
                || referenceLabel.Contains("preta", StringComparison.OrdinalIgnoreCase);
            var isCandidateDark = candidateLabel.Contains("escuro", StringComparison.OrdinalIgnoreCase)
                || candidateLabel.Contains("preta", StringComparison.OrdinalIgnoreCase);

            return isReferenceDark == isCandidateDark ? 120 : 40;
        }

        private sealed class AvatarQuickSuggestion
        {
            public string Title { get; init; } = string.Empty;
            public string Subtitle { get; init; } = string.Empty;
            public string Badge { get; init; } = string.Empty;
            public string Body { get; init; } = string.Empty;
            public string Hair { get; init; } = string.Empty;
            public string Hat { get; init; } = string.Empty;
            public string Accessory { get; init; } = string.Empty;
            public string Clothing { get; init; } = string.Empty;
            public int Score { get; init; }
        }

        private void AddAvatarLayer(Panel host, string folder, string option)
        {
            if (string.IsNullOrWhiteSpace(option))
            {
                return;
            }

            var source = TryCreateAvatarImageSource(folder, option);
            if (source == null)
            {
                return;
            }

            host.Children.Add(new Image
            {
                Source = source,
                Stretch = Stretch.Fill,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                SnapsToDevicePixels = true
            });
        }

        private ImageSource? TryCreateAvatarImageSource(string folder, string option)
        {
            try
            {
                return new BitmapImage(new Uri($"pack://application:,,,/img/avatar/{folder}/{option}.png", UriKind.Absolute));
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[Avatar] Falha ao carregar recurso {folder}/{option}.png: {ex.Message}");
                return null;
            }
        }

        private UIElement CreateAvatarOptionPreview(string body, string hair, string hat, string accessory, string clothing, string? overlayLabel = null)
        {
            var frame = new Border
            {
                Width = 96,
                Height = 96,
                CornerRadius = new CornerRadius(18),
                Background = new LinearGradientBrush(
                    Color.FromRgb(255, 255, 255),
                    Color.FromRgb(241, 245, 249),
                    90),
                BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                BorderThickness = new Thickness(1),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var previewGrid = new Grid();
            previewGrid.Children.Add(CreateAvatarCompositeVisual(body, hair, hat, accessory, clothing, 96, new CornerRadius(18), showBorder: false));

            if (!string.IsNullOrWhiteSpace(overlayLabel))
            {
                previewGrid.Children.Add(new Border
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(6),
                    Padding = new Thickness(6, 2, 6, 2),
                    CornerRadius = new CornerRadius(999),
                    Background = new SolidColorBrush(Color.FromArgb(220, 15, 23, 42)),
                    Child = new TextBlock
                    {
                        Text = overlayLabel,
                        Foreground = Brushes.White,
                        FontSize = 9,
                        FontWeight = FontWeights.Bold
                    }
                });
            }

            frame.Child = previewGrid;
            return frame;
        }

        private Button CreateAvatarOptionButton(string title, string? badgeText, UIElement previewContent, bool isSelected, RoutedEventHandler onClick)
        {
            var button = new Button
            {
                Width = 138,
                Height = 178,
                Margin = new Thickness(0, 0, 14, 14),
                Padding = new Thickness(12),
                Background = isSelected ? new SolidColorBrush(Color.FromRgb(239, 246, 255)) : GetThemeBrush("MutedCardBackgroundBrush"),
                BorderBrush = isSelected ? GetThemeBrush("AccentBrush") : GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(isSelected ? 2 : 1),
                Cursor = Cursors.Hand,
                Tag = title
            };

            var stack = new StackPanel();
            var topRow = new Grid { Margin = new Thickness(0, 0, 0, 10) };
            topRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            topRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            if (!string.IsNullOrWhiteSpace(badgeText))
            {
                topRow.Children.Add(new Border
                {
                    Background = isSelected ? GetThemeBrush("AccentBrush") : GetThemeBrush("CardBackgroundBrush"),
                    BorderBrush = isSelected ? GetThemeBrush("AccentBrush") : GetThemeBrush("CardBorderBrush"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(999),
                    Padding = new Thickness(8, 3, 8, 3),
                    Child = new TextBlock
                    {
                        Text = badgeText,
                        FontSize = 9,
                        FontWeight = FontWeights.Bold,
                        Foreground = isSelected ? Brushes.White : GetThemeBrush("SecondaryTextBrush")
                    }
                });
            }

            var statusBadge = new Border
            {
                Background = isSelected ? new SolidColorBrush(Color.FromRgb(16, 185, 129)) : new SolidColorBrush(Color.FromRgb(203, 213, 225)),
                Width = 10,
                Height = 10,
                CornerRadius = new CornerRadius(999),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(statusBadge, 1);
            topRow.Children.Add(statusBadge);

            stack.Children.Add(topRow);
            stack.Children.Add(previewContent);
            stack.Children.Add(new TextBlock
            {
                Text = title,
                Margin = new Thickness(0, 12, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                TextAlignment = TextAlignment.Left,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            });
            stack.Children.Add(new TextBlock
            {
                Text = isSelected ? "Selecionado" : "Clique para aplicar",
                Margin = new Thickness(0, 6, 0, 0),
                FontSize = 10,
                FontWeight = FontWeights.Medium,
                Foreground = isSelected ? GetThemeBrush("AccentBrush") : GetThemeBrush("TertiaryTextBrush")
            });

            button.Content = stack;
            button.Click += onClick;
            return button;
        }

        private Button CreateAvatarFilterChipButton(string label, bool isSelected, RoutedEventHandler onClick)
        {
            var button = new Button
            {
                Content = label,
                MinWidth = 88,
                Height = 34,
                Margin = new Thickness(0, 0, 10, 10),
                Padding = new Thickness(14, 0, 14, 0),
                Background = isSelected ? GetThemeBrush("AccentBrush") : GetThemeBrush("CardBackgroundBrush"),
                Foreground = isSelected ? Brushes.White : GetThemeBrush("PrimaryTextBrush"),
                BorderBrush = isSelected ? GetThemeBrush("AccentBrush") : GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                Cursor = Cursors.Hand,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold
            };
            button.Click += onClick;
            return button;
        }

        private Border CreateAvatarQuickSuggestionButton(AvatarQuickSuggestion suggestion, bool isSelected, RoutedEventHandler onClick)
        {
            var button = new Button
            {
                Width = 210,
                Height = 230,
                Padding = new Thickness(12),
                Background = isSelected ? new SolidColorBrush(Color.FromRgb(239, 246, 255)) : GetThemeBrush("CardBackgroundBrush"),
                BorderBrush = isSelected ? GetThemeBrush("AccentBrush") : GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(isSelected ? 2 : 1),
                Cursor = Cursors.Hand
            };

            var stack = new StackPanel();
            stack.Children.Add(CreateAvatarOptionPreview(suggestion.Body, suggestion.Hair, suggestion.Hat, suggestion.Accessory, suggestion.Clothing, suggestion.Badge));
            stack.Children.Add(new TextBlock
            {
                Text = suggestion.Title,
                Margin = new Thickness(0, 12, 0, 0),
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = GetThemeBrush("PrimaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            });
            stack.Children.Add(new TextBlock
            {
                Text = suggestion.Subtitle,
                Margin = new Thickness(0, 6, 0, 0),
                FontSize = 11,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            });
            stack.Children.Add(new TextBlock
            {
                Text = isSelected ? "Combinação aplicada no editor" : "Aplicar combinação completa",
                Margin = new Thickness(0, 10, 0, 0),
                FontSize = 10,
                FontWeight = FontWeights.SemiBold,
                Foreground = isSelected ? GetThemeBrush("AccentBrush") : GetThemeBrush("TertiaryTextBrush")
            });

            button.Content = stack;
            button.Click += onClick;

            return new Border
            {
                Margin = new Thickness(0, 0, 14, 14),
                Child = button
            };
        }

        private IEnumerable<string> GetFilteredHairOptions(string referenceHair, string presentationFilter, string lengthFilter)
        {
            return AvatarHairOptions
                .Where(option => presentationFilter == "Todos" || string.Equals(GetAvatarHairPresentation(option), presentationFilter, StringComparison.Ordinal))
                .Where(option => lengthFilter == "Todos" || string.Equals(GetAvatarHairLength(option), lengthFilter, StringComparison.Ordinal))
                .OrderByDescending(option => GetAvatarHairSimilarityScore(referenceHair, option))
                .ThenBy(option => GetAvatarHairLabel(option), StringComparer.OrdinalIgnoreCase);
        }

        private List<AvatarQuickSuggestion> BuildAvatarQuickSuggestions(string body, string hair, string hat, string accessory, string clothing)
        {
            var hairCandidates = AvatarHairOptions
                .OrderByDescending(option => GetAvatarHairSimilarityScore(hair, option))
                .Take(4)
                .ToList();

            var hatCandidates = AvatarHatOptions
                .OrderByDescending(option => GetAvatarHatSimilarityScore(hat, option))
                .Take(3)
                .ToList();

            var clothingCandidates = AvatarClothingOptions
                .OrderByDescending(option => GetAvatarClothingSimilarityScore(clothing, option))
                .Take(3)
                .ToList();

            var accessoryCandidates = new List<string> { accessory };
            if (!string.IsNullOrWhiteSpace(accessory))
            {
                accessoryCandidates.Add(string.Empty);
            }
            else
            {
                accessoryCandidates.AddRange(AvatarAccessoryOptions.Take(1));
            }

            var combinations = new List<AvatarQuickSuggestion>();
            var seenKeys = new HashSet<string>(StringComparer.Ordinal);

            foreach (var hairOption in hairCandidates)
            {
                foreach (var hatOption in hatCandidates)
                {
                    foreach (var clothingOption in clothingCandidates)
                    {
                        foreach (var accessoryOption in accessoryCandidates)
                        {
                            var key = string.Join("|", body, hairOption, hatOption, accessoryOption, clothingOption);
                            if (!seenKeys.Add(key))
                            {
                                continue;
                            }

                            var score = 0;
                            score += string.Equals(body, body, StringComparison.Ordinal) ? 120 : 0;
                            score += GetAvatarHairSimilarityScore(hair, hairOption);
                            score += GetAvatarHatSimilarityScore(hat, hatOption);
                            score += GetAvatarClothingSimilarityScore(clothing, clothingOption);
                            score += string.Equals(accessory, accessoryOption, StringComparison.Ordinal) ? 180 : 40;

                            var title = "Combinação próxima";
                            var subtitle = $"{GetAvatarHairLabel(hairOption)} com {GetAvatarHatLabel(hatOption)} e roupa {GetAvatarClothingLabel(clothingOption)}.";
                            var badge = "Próxima";

                            if (string.Equals(hair, hairOption, StringComparison.Ordinal)
                                && string.Equals(hat, hatOption, StringComparison.Ordinal)
                                && string.Equals(clothing, clothingOption, StringComparison.Ordinal)
                                && string.Equals(accessory, accessoryOption, StringComparison.Ordinal))
                            {
                                title = "Seu visual atual";
                                subtitle = "Atalho para voltar rapidamente ao conjunto que já está montado.";
                                badge = "Atual";
                            }
                            else if (!string.Equals(hair, hairOption, StringComparison.Ordinal)
                                && string.Equals(hat, hatOption, StringComparison.Ordinal)
                                && string.Equals(clothing, clothingOption, StringComparison.Ordinal))
                            {
                                title = "Mesmo estilo, novo cabelo";
                                badge = "Cabelo";
                            }
                            else if (string.Equals(hair, hairOption, StringComparison.Ordinal)
                                && !string.Equals(hat, hatOption, StringComparison.Ordinal)
                                && string.Equals(clothing, clothingOption, StringComparison.Ordinal))
                            {
                                title = "Mesmo estilo, novo hat";
                                badge = "Hat";
                            }
                            else if (string.Equals(hair, hairOption, StringComparison.Ordinal)
                                && string.Equals(hat, hatOption, StringComparison.Ordinal)
                                && !string.Equals(clothing, clothingOption, StringComparison.Ordinal))
                            {
                                title = "Mesmo estilo, nova roupa";
                                badge = "Roupa";
                            }

                            combinations.Add(new AvatarQuickSuggestion
                            {
                                Title = title,
                                Subtitle = subtitle,
                                Badge = badge,
                                Body = body,
                                Hair = hairOption,
                                Hat = hatOption,
                                Accessory = accessoryOption,
                                Clothing = clothingOption,
                                Score = score
                            });
                        }
                    }
                }
            }

            return combinations
                .OrderByDescending(item => item.Score)
                .ThenBy(item => item.Title, StringComparer.OrdinalIgnoreCase)
                .Take(4)
                .ToList();
        }

        private void OpenAvatarEditor_Click(object sender, RoutedEventArgs e)
        {
            if (_currentProfile == null)
            {
                return;
            }

            var selectedBody = NormalizeAvatarBodySelection(_currentProfile.AvatarBody);
            var selectedHair = NormalizeAvatarHairSelection(_currentProfile.AvatarHair);
            var selectedHat = NormalizeAvatarOption(_currentProfile.AvatarHat, AvatarHatOptions, AvatarHatOptions.First());
            var selectedAccessory = NormalizeOptionalAvatarOption(_currentProfile.AvatarAccessory, AvatarAccessoryOptions);
            var selectedClothing = NormalizeAvatarOption(_currentProfile.AvatarClothing, AvatarClothingOptions, AvatarClothingOptions.First());
            var selectedHairPresentationFilter = "Todos";
            var selectedHairLengthFilter = "Todos";

            var dialog = new MetroWindow
            {
                Title = "Criador de avatar",
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Width = 940,
                Height = 720,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false,
                Background = GetThemeBrush("WindowBackgroundBrush"),
                GlowBrush = (Brush)GlowBrush,
                BorderBrush = GetThemeBrush("CardBorderBrush")
            };

            var root = new Grid
            {
                Margin = new Thickness(24)
            };
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var header = new StackPanel
            {
                Margin = new Thickness(0, 0, 0, 18)
            };
            header.Children.Add(new TextBlock
            {
                Text = "Monte seu avatar acadêmico",
                FontSize = 24,
                FontWeight = FontWeights.ExtraBold,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            });
            header.Children.Add(new TextBlock
            {
                Text = "A nova composição usa personagem, cabelo, hat, acessório e roupa. Apenas o acessório continua opcional.",
                Margin = new Thickness(0, 8, 0, 0),
                FontSize = 13,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            });
            Grid.SetRow(header, 0);
            root.Children.Add(header);

            var contentGrid = new Grid();
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(300) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var previewCard = new Border
            {
                Background = GetThemeBrush("CardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(20),
                Padding = new Thickness(22),
                Margin = new Thickness(0, 0, 20, 0)
            };
            var previewStack = new StackPanel();
            previewStack.Children.Add(new TextBlock
            {
                Text = "Prévia",
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            });
            previewStack.Children.Add(new TextBlock
            {
                Text = "Seu personagem aparecerá no perfil, barra lateral, conversas e equipes.",
                Margin = new Thickness(0, 6, 0, 18),
                FontSize = 12,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            });

            var previewHost = new Grid
            {
                Width = 220,
                Height = 220,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            var selectionSummary = new TextBlock
            {
                Margin = new Thickness(0, 16, 0, 0),
                FontSize = 12,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            };

            previewStack.Children.Add(previewHost);
            previewStack.Children.Add(selectionSummary);
            previewCard.Child = previewStack;
            Grid.SetColumn(previewCard, 0);
            contentGrid.Children.Add(previewCard);

            var selectorsScroll = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            var selectorsStack = new StackPanel();

            var quickSuggestionsPanel = new WrapPanel { Margin = new Thickness(0, 10, 0, 18) };
            var bodyPanel = new WrapPanel { Margin = new Thickness(0, 10, 0, 14) };
            var hairPresentationFiltersPanel = new WrapPanel { Margin = new Thickness(0, 8, 0, 0) };
            var hairLengthFiltersPanel = new WrapPanel { Margin = new Thickness(0, 6, 0, 0) };
            var hairPanel = new WrapPanel { Margin = new Thickness(0, 12, 0, 14) };
            var hatPanel = new WrapPanel { Margin = new Thickness(0, 10, 0, 14) };
            var accessoryPanel = new WrapPanel { Margin = new Thickness(0, 10, 0, 14) };
            var clothingPanel = new WrapPanel { Margin = new Thickness(0, 10, 0, 14) };

            selectorsStack.Children.Add(CreateAvatarSectionHeader("Combinações rápidas", "Sugestões automáticas baseadas no visual atual para acelerar a escolha."));
            selectorsStack.Children.Add(quickSuggestionsPanel);
            selectorsStack.Children.Add(CreateAvatarSectionHeader("Personagem", "Layer 0 · obrigatório · começa com pele parda como padrão."));
            selectorsStack.Children.Add(bodyPanel);
            selectorsStack.Children.Add(CreateAvatarSectionHeader("Cabelos", "Layer 1 · filtre por apresentação e comprimento para chegar mais rápido ao estilo certo."));
            selectorsStack.Children.Add(hairPresentationFiltersPanel);
            selectorsStack.Children.Add(hairLengthFiltersPanel);
            selectorsStack.Children.Add(hairPanel);
            selectorsStack.Children.Add(CreateAvatarSectionHeader("Hats", "Layer 2 · obrigatório · escolha um chapéu para o personagem."));
            selectorsStack.Children.Add(hatPanel);
            selectorsStack.Children.Add(CreateAvatarSectionHeader("Acessory", "Layer 3 · opcional · mantém os acessórios atuais."));
            selectorsStack.Children.Add(accessoryPanel);
            selectorsStack.Children.Add(CreateAvatarSectionHeader("Roupas", "Layer 4 · obrigatório · finalize com uma roupa."));
            selectorsStack.Children.Add(clothingPanel);

            selectorsScroll.Content = selectorsStack;
            Grid.SetColumn(selectorsScroll, 1);
            contentGrid.Children.Add(selectorsScroll);

            void RenderEditorPreview()
            {
                previewHost.Children.Clear();
                previewHost.Children.Add(CreateAvatarCompositeVisual(new UserProfile
                {
                    AvatarBody = selectedBody,
                    AvatarHair = selectedHair,
                    AvatarHat = selectedHat,
                    AvatarAccessory = selectedAccessory,
                    AvatarClothing = selectedClothing
                }, 220, new CornerRadius(28), showBorder: true));

                selectionSummary.Text = string.IsNullOrWhiteSpace(selectedAccessory)
                    ? $"{GetAvatarBodyLabel(selectedBody)}, {GetAvatarHairLabel(selectedHair)}, {GetAvatarHatLabel(selectedHat)} e roupa {GetAvatarClothingLabel(selectedClothing)}."
                    : $"{GetAvatarBodyLabel(selectedBody)}, {GetAvatarHairLabel(selectedHair)}, {GetAvatarHatLabel(selectedHat)}, acessório {selectedAccessory} e roupa {GetAvatarClothingLabel(selectedClothing)}.";
            }

            void RenderEditorOptions()
            {
                quickSuggestionsPanel.Children.Clear();
                foreach (var suggestion in BuildAvatarQuickSuggestions(selectedBody, selectedHair, selectedHat, selectedAccessory, selectedClothing))
                {
                    var currentSuggestion = suggestion;
                    var isSuggestionSelected = string.Equals(selectedBody, currentSuggestion.Body, StringComparison.Ordinal)
                        && string.Equals(selectedHair, currentSuggestion.Hair, StringComparison.Ordinal)
                        && string.Equals(selectedHat, currentSuggestion.Hat, StringComparison.Ordinal)
                        && string.Equals(selectedAccessory, currentSuggestion.Accessory, StringComparison.Ordinal)
                        && string.Equals(selectedClothing, currentSuggestion.Clothing, StringComparison.Ordinal);

                    quickSuggestionsPanel.Children.Add(CreateAvatarQuickSuggestionButton(currentSuggestion, isSuggestionSelected, (buttonSender, buttonArgs) =>
                    {
                        selectedBody = currentSuggestion.Body;
                        selectedHair = currentSuggestion.Hair;
                        selectedHat = currentSuggestion.Hat;
                        selectedAccessory = currentSuggestion.Accessory;
                        selectedClothing = currentSuggestion.Clothing;
                        selectedHairPresentationFilter = GetAvatarHairPresentation(selectedHair);
                        selectedHairLengthFilter = GetAvatarHairLength(selectedHair);
                        RenderEditorOptions();
                        RenderEditorPreview();
                    }));
                }

                bodyPanel.Children.Clear();
                foreach (var option in AvatarBodyOptions)
                {
                    var currentOption = option;
                    bodyPanel.Children.Add(CreateAvatarOptionButton(
                        GetAvatarBodyLabel(currentOption),
                        "Base",
                        CreateAvatarOptionPreview(currentOption, selectedHair, selectedHat, selectedAccessory, selectedClothing, "Layer 0"),
                        selectedBody == currentOption,
                        (buttonSender, buttonArgs) =>
                    {
                        selectedBody = currentOption;
                        RenderEditorOptions();
                        RenderEditorPreview();
                    }));
                }

                hairPresentationFiltersPanel.Children.Clear();
                foreach (var option in new[] { "Todos", "Feminino", "Masculino" })
                {
                    var currentOption = option;
                    hairPresentationFiltersPanel.Children.Add(CreateAvatarFilterChipButton(currentOption, selectedHairPresentationFilter == currentOption, (buttonSender, buttonArgs) =>
                    {
                        selectedHairPresentationFilter = currentOption;
                        RenderEditorOptions();
                    }));
                }

                hairLengthFiltersPanel.Children.Clear();
                foreach (var option in new[] { "Todos", "Curto", "Médio", "Longo" })
                {
                    var currentOption = option;
                    hairLengthFiltersPanel.Children.Add(CreateAvatarFilterChipButton(currentOption, selectedHairLengthFilter == currentOption, (buttonSender, buttonArgs) =>
                    {
                        selectedHairLengthFilter = currentOption;
                        RenderEditorOptions();
                    }));
                }

                hairPanel.Children.Clear();
                foreach (var option in GetFilteredHairOptions(selectedHair, selectedHairPresentationFilter, selectedHairLengthFilter))
                {
                    var currentOption = option;
                    hairPanel.Children.Add(CreateAvatarOptionButton(
                        GetAvatarHairLabel(currentOption),
                        $"{GetAvatarHairPresentation(currentOption)} · {GetAvatarHairLength(currentOption)}",
                        CreateAvatarOptionPreview(selectedBody, currentOption, selectedHat, selectedAccessory, selectedClothing, "Layer 1"),
                        selectedHair == currentOption,
                        (buttonSender, buttonArgs) =>
                    {
                        selectedHair = currentOption;
                        selectedHairPresentationFilter = GetAvatarHairPresentation(currentOption);
                        selectedHairLengthFilter = GetAvatarHairLength(currentOption);
                        RenderEditorOptions();
                        RenderEditorPreview();
                    }));
                }

                hatPanel.Children.Clear();
                foreach (var option in AvatarHatOptions)
                {
                    var currentOption = option;
                    hatPanel.Children.Add(CreateAvatarOptionButton(
                        GetAvatarHatLabel(currentOption),
                        "Hat",
                        CreateAvatarOptionPreview(selectedBody, selectedHair, currentOption, selectedAccessory, selectedClothing, "Layer 2"),
                        selectedHat == currentOption,
                        (buttonSender, buttonArgs) =>
                    {
                        selectedHat = currentOption;
                        RenderEditorOptions();
                        RenderEditorPreview();
                    }));
                }

                accessoryPanel.Children.Clear();
                accessoryPanel.Children.Add(CreateAvatarOptionButton(
                    "Sem acessório",
                    "Opcional",
                    CreateAvatarOptionPreview(selectedBody, selectedHair, selectedHat, string.Empty, selectedClothing, "Layer 3"),
                    string.IsNullOrWhiteSpace(selectedAccessory),
                    (buttonSender, buttonArgs) =>
                {
                    selectedAccessory = string.Empty;
                    RenderEditorOptions();
                    RenderEditorPreview();
                }));

                foreach (var option in AvatarAccessoryOptions)
                {
                    var currentOption = option;
                    accessoryPanel.Children.Add(CreateAvatarOptionButton(
                        $"Acessório {currentOption}",
                        "Acessory",
                        CreateAvatarOptionPreview(selectedBody, selectedHair, selectedHat, currentOption, selectedClothing, "Layer 3"),
                        selectedAccessory == currentOption,
                        (buttonSender, buttonArgs) =>
                    {
                        selectedAccessory = currentOption;
                        RenderEditorOptions();
                        RenderEditorPreview();
                    }));
                }

                clothingPanel.Children.Clear();
                foreach (var option in AvatarClothingOptions)
                {
                    var currentOption = option;
                    clothingPanel.Children.Add(CreateAvatarOptionButton(
                        GetAvatarClothingLabel(currentOption),
                        "Roupa",
                        CreateAvatarOptionPreview(selectedBody, selectedHair, selectedHat, selectedAccessory, currentOption, "Layer 4"),
                        selectedClothing == currentOption,
                        (buttonSender, buttonArgs) =>
                    {
                        selectedClothing = currentOption;
                        RenderEditorOptions();
                        RenderEditorPreview();
                    }));
                }
            }

            RenderEditorOptions();
            RenderEditorPreview();

            Grid.SetRow(contentGrid, 1);
            root.Children.Add(contentGrid);

            var footer = new Grid
            {
                Margin = new Thickness(0, 20, 0, 0)
            };
            footer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            footer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            footer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var footerHint = new TextBlock
            {
                Text = "Ao aplicar, o personagem é atualizado na tela e salvo no Firebase imediatamente.",
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 12,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 16, 0)
            };
            Grid.SetColumn(footerHint, 0);
            footer.Children.Add(footerHint);

            var cancelButton = new Button
            {
                Content = "Cancelar",
                Width = 120,
                Height = 42,
                Margin = new Thickness(0, 0, 12, 0),
                Background = GetThemeBrush("MutedCardBackgroundBrush"),
                Foreground = GetThemeBrush("PrimaryTextBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                Cursor = Cursors.Hand
            };
            cancelButton.Click += (buttonSender, buttonArgs) => dialog.Close();
            Grid.SetColumn(cancelButton, 1);
            footer.Children.Add(cancelButton);

            var applyButton = new Button
            {
                Content = "Aplicar personagem",
                Width = 150,
                Height = 42,
                Background = GetThemeBrush("AccentBrush"),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontWeight = FontWeights.SemiBold,
                Cursor = Cursors.Hand
            };
            applyButton.Click += async (buttonSender, buttonArgs) =>
            {
                if (_currentProfile == null)
                {
                    dialog.Close();
                    return;
                }

                _currentProfile.AvatarBody = selectedBody;
                _currentProfile.AvatarHair = selectedHair;
                _currentProfile.AvatarHat = selectedHat;
                _currentProfile.AvatarAccessory = selectedAccessory;
                _currentProfile.AvatarClothing = selectedClothing;
                NormalizeProfileAvatarSelection(_currentProfile);
                RefreshAvatarUi(_currentProfile);

                applyButton.IsEnabled = false;
                cancelButton.IsEnabled = false;
                footerHint.Text = "Salvando personagem no Firebase...";

                try
                {
                    if (string.IsNullOrWhiteSpace(_idToken) || string.IsNullOrWhiteSpace(_currentProfile.UserId))
                    {
                        ProfessionalProfileStatusText.Text = "Personagem atualizado localmente. Faça login novamente para sincronizar com o Firebase.";
                        dialog.Close();
                        return;
                    }

                    var saveResult = await SaveProfessionalProfileAsync(_currentProfile, _idToken);
                    if (saveResult.Success)
                    {
                        ProfessionalProfileStatusText.Text = "Personagem salvo com sucesso no Firebase.";
                        dialog.Close();
                        return;
                    }

                    ProfessionalProfileStatusText.Text = "Personagem atualizado localmente, mas o Firebase recusou a gravação.";
                    footerHint.Text = "Falha ao salvar personagem no Firebase. Você pode tentar novamente.";
                    MessageBox.Show(
                        $"Não foi possível salvar o personagem no Firebase.\n\n{saveResult.ErrorMessage}",
                        "Erro ao salvar personagem",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
                catch (Exception ex)
                {
                    ProfessionalProfileStatusText.Text = "Personagem atualizado localmente, mas ocorreu um erro ao salvar no Firebase.";
                    footerHint.Text = "Erro ao salvar personagem. Revise sua conexão e tente novamente.";
                    MessageBox.Show(
                        $"Erro inesperado ao salvar personagem: {ex.Message}",
                        "Erro",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
                finally
                {
                    applyButton.IsEnabled = true;
                    cancelButton.IsEnabled = true;
                }
            };
            Grid.SetColumn(applyButton, 2);
            footer.Children.Add(applyButton);

            Grid.SetRow(footer, 2);
            root.Children.Add(footer);

            dialog.Content = root;
            dialog.ShowDialog();
        }

        private StackPanel CreateAvatarSectionHeader(string title, string description)
        {
            var section = new StackPanel
            {
                Margin = new Thickness(0, 0, 0, 4)
            };

            section.Children.Add(new Border
            {
                Width = 42,
                Height = 5,
                CornerRadius = new CornerRadius(999),
                Background = GetThemeBrush("AccentBrush"),
                Margin = new Thickness(0, 0, 0, 8)
            });

            section.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            });

            section.Children.Add(new TextBlock
            {
                Text = description,
                Margin = new Thickness(0, 4, 0, 0),
                FontSize = 12,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            });
            return section;
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

        private void AddTeamMember_Click(object sender, RoutedEventArgs e)
        {
            if (TeamMemberInput.SelectedItem is not UserInfo selectedUser)
            {
                TeamCreationStatusText.Text = "Selecione um aluno nas previas antes de adicionar.";
                return;
            }

            AddResolvedTeamMember(selectedUser);
        }

        private void OpenTeamEntryOptions_Click(object sender, RoutedEventArgs e)
        {
            TeamEntryOptionsPopup.IsOpen = !TeamEntryOptionsPopup.IsOpen;
        }

        private void ShowJoinTeam_Click(object sender, RoutedEventArgs e)
        {
            _teamEntryMode = TeamEntryMode.Join;
            TeamJoinStatusText.Text = string.Empty;
            UpdateTeamsViewState();
        }

        private void ShowCreateTeam_Click(object sender, RoutedEventArgs e)
        {
            _teamEntryMode = TeamEntryMode.Create;
            TeamCreationStatusText.Text = string.Empty;
            UpdateTeamsViewState();
        }

        private void CancelTeamEntry_Click(object sender, RoutedEventArgs e)
        {
            _teamEntryMode = TeamEntryMode.None;
            TeamCreationStatusText.Text = string.Empty;
            TeamJoinStatusText.Text = string.Empty;
            UpdateTeamsViewState();
        }

        private void TeamMemberInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
            }
        }

        private void TeamMemberInput_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TeamMemberInput.SelectedItem is not UserInfo selectedUser)
            {
                return;
            }

            TeamCreationStatusText.Text = $"Aluno selecionado: {selectedUser.Name}. Clique em Adicionar aluno para confirmar.";
        }

        private async void TeamMemberInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_suppressTeamMemberSearch || _teamEntryMode != TeamEntryMode.Create)
            {
                return;
            }

            if (TeamMemberInput.SelectedItem is UserInfo selectedUser &&
                string.Equals(TeamMemberInput.Text, selectedUser.DisplayLabel, StringComparison.Ordinal))
            {
                return;
            }

            var query = NormalizeTeamValue(TeamMemberInput.Text);
            if (string.IsNullOrWhiteSpace(query))
            {
                PopulateTeamMemberResults(Array.Empty<UserInfo>());
                TeamMemberInput.SelectedItem = null;
                TeamMemberInput.IsDropDownOpen = false;
                TeamCreationStatusText.Text = string.Empty;
                return;
            }

            var currentSearchVersion = ++_teamMemberSearchVersion;
            TeamCreationStatusText.Text = "Buscando alunos no Firebase...";

            TeamMemberInput.SelectedItem = null;

            try
            {
                await Task.Delay(300);
                if (currentSearchVersion != _teamMemberSearchVersion || _suppressTeamMemberSearch)
                {
                    return;
                }

                var results = await SearchTeamMembersAsync(query);
                if (currentSearchVersion != _teamMemberSearchVersion || _suppressTeamMemberSearch)
                {
                    return;
                }

                PopulateTeamMemberResults(results);

                if (results.Count > 1)
                {
                    TeamCreationStatusText.Text = $"{results.Count} alunos encontrados. Selecione um nas previas.";
                    TeamMemberInput.IsDropDownOpen = true;
                    return;
                }

                if (results.Count == 1)
                {
                    TeamCreationStatusText.Text = "1 aluno encontrado. Selecione a previa e clique em Adicionar aluno.";
                    TeamMemberInput.IsDropDownOpen = true;
                    return;
                }

                TeamCreationStatusText.Text = "Nenhum aluno encontrado com esse criterio.";
                TeamMemberInput.IsDropDownOpen = false;
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeamMemberInput_TextChanged ERROR] {ex.Message}");
                TeamCreationStatusText.Text = "Nao foi possivel buscar alunos agora.";
            }
        }

        private async Task<List<UserInfo>> SearchTeamMembersAsync(string query)
        {
            List<UserInfo>? results = null;

            if (!string.IsNullOrWhiteSpace(_idToken))
            {
                try
                {
                    var searchService = new UserSearchService(_idToken);
                    results = await searchService.SearchUsersAsync(query);
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine($"[SearchTeamMembersAsync ERROR] {ex.Message}");
                }
            }

            results ??= new List<UserInfo>();

            if (results.Count == 0)
            {
                results = MockData.SearchMockUsers(query);
            }

            var currentUserId = _currentProfile?.UserId ?? string.Empty;

            return results
                .Where(user => !string.IsNullOrWhiteSpace(user.UserId))
                .Where(user => !string.Equals(user.UserId, currentUserId, StringComparison.OrdinalIgnoreCase))
                .GroupBy(user => user.UserId, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .OrderBy(user => user.Name)
                .ToList();
        }

        private void PopulateTeamMemberResults(IEnumerable<UserInfo> results)
        {
            _teamMemberSearchResults.Clear();
            foreach (var result in results)
            {
                _teamMemberSearchResults.Add(result);
            }

            TeamMemberInput.ItemsSource = null;
            TeamMemberInput.ItemsSource = _teamMemberSearchResults;
        }

        private void AddResolvedTeamMember(UserInfo member)
        {
            if (_draftTeamMembers.Any(item => string.Equals(item.UserId, member.UserId, StringComparison.OrdinalIgnoreCase)))
            {
                TeamCreationStatusText.Text = "Esse aluno ja faz parte da equipe.";
                ClearTeamMemberSearchUi();
                return;
            }

            _draftTeamMembers.Add(member);
            RenderTeamMembersDraft();
            ClearTeamMemberSearchUi();
            TeamCreationStatusText.Text = $"Aluno {member.Name} adicionado com sucesso.";
        }

        private void ClearTeamMemberSearchUi()
        {
            _suppressTeamMemberSearch = true;
            try
            {
                _teamMemberSearchVersion++;
                TeamMemberInput.Text = string.Empty;
                TeamMemberInput.SelectedItem = null;
                TeamMemberInput.IsDropDownOpen = false;
                PopulateTeamMemberResults(Array.Empty<UserInfo>());
            }
            finally
            {
                _suppressTeamMemberSearch = false;
            }
        }

        private void AddTeamUc_Click(object sender, RoutedEventArgs e)
        {
            AddTeamUcFromInput();
        }

        private void TeamUcInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddTeamUcFromInput();
                e.Handled = true;
            }
        }

        private void AddTeamUcFromInput()
        {
            var uc = NormalizeTeamValue(TeamUcInput.Text);
            if (string.IsNullOrWhiteSpace(uc))
            {
                return;
            }

            if (_draftTeamUcs.Any(item => string.Equals(item, uc, StringComparison.OrdinalIgnoreCase)))
            {
                TeamUcInput.Text = string.Empty;
                return;
            }

            _draftTeamUcs.Add(uc);
            RenderTeamUcsDraft();
            TeamUcInput.Text = string.Empty;
            TeamCreationStatusText.Text = string.Empty;
        }

        private async void CreateTeam_Click(object sender, RoutedEventArgs e)
        {
            EnsureCurrentUserInTeamDraft();

            var teamName = NormalizeTeamValue(TeamNameTextBox.Text);
            var course = NormalizeTeamValue(TeamCourseComboBox.Text);
            var className = NormalizeTeamValue(TeamClassComboBox.Text);
            var classId = NormalizeTeamValue(TeamClassIdTextBox.Text);

            if (string.IsNullOrWhiteSpace(teamName) ||
                string.IsNullOrWhiteSpace(course) ||
                string.IsNullOrWhiteSpace(className) ||
                string.IsNullOrWhiteSpace(classId))
            {
                TeamCreationStatusText.Text = "Preencha nome da equipe, curso, turma e ID da turma.";
                return;
            }

            if (_draftTeamUcs.Count == 0)
            {
                TeamCreationStatusText.Text = "Adicione ao menos uma UC para criar a equipe.";
                return;
            }

            var teamWorkspace = new TeamWorkspaceInfo
            {
                TeamId = TeamService.GenerateTeamId(classId, teamName),
                TeamName = teamName,
                Course = course,
                ClassName = className,
                ClassId = classId,
                CreatedBy = GetCurrentUserId(),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                ProjectProgress = 0,
                ProjectDeadline = null,
                ProjectStatus = "Planejamento",
                Members = _draftTeamMembers
                    .GroupBy(item => item.UserId, StringComparer.OrdinalIgnoreCase)
                    .Select(group => group.First())
                    .OrderBy(item => item.Name)
                    .ToList(),
                Ucs = _draftTeamUcs
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(item => item)
                    .ToList(),
                Milestones = CreateDefaultMilestones(),
                Assets = new List<TeamAssetInfo>(),
                TaskColumns = CreateDefaultTeamColumns(),
                Notifications = new List<TeamNotificationInfo>(),
                ChatMessages = new List<TeamChatMessageInfo>(),
                CsdBoard = CreateDefaultCsdBoard()
            };

            CreateTeamButton.IsEnabled = false;
            TeamCreationStatusText.Text = "Salvando equipe no Firebase...";

            var saveResult = await SaveTeamToFirestoreAsync(teamWorkspace);
            if (!saveResult.Success)
            {
                TeamCreationStatusText.Text = $"Erro ao salvar equipe: {saveResult.ErrorMessage}";
                CreateTeamButton.IsEnabled = true;
                return;
            }

            SaveTeamWorkspace(teamWorkspace, persistInBackground: false);
            _activeTeamWorkspace = null;
            _teamEntryMode = TeamEntryMode.None;
            TeamCreationStatusText.Text = $"Equipe salva com sucesso. Codigo da equipe: {teamWorkspace.TeamId}";
            TeamJoinStatusText.Text = string.Empty;
            UpdateTeamsViewState();
            CreateTeamButton.IsEnabled = true;
        }

        private async void JoinTeamByCode_Click(object sender, RoutedEventArgs e)
        {
            var joinCode = NormalizeTeamValue(TeamJoinCodeTextBox.Text);
            if (string.IsNullOrWhiteSpace(joinCode))
            {
                TeamJoinStatusText.Text = "Informe um codigo valido para ingressar.";
                return;
            }

            if (_teamService == null)
            {
                TeamJoinStatusText.Text = "Servico de equipes nao inicializado.";
                return;
            }

            TeamJoinStatusText.Text = "Buscando equipe no Firebase...";

            var joinResult = await _teamService.JoinTeamByCodeAsync(joinCode);
            if (!joinResult.Success || joinResult.Team == null)
            {
                TeamJoinStatusText.Text = $"Erro ao ingressar: {joinResult.ErrorMessage}";
                return;
            }

            SaveTeamWorkspace(joinResult.Team, persistInBackground: false);
            _activeTeamWorkspace = null;
            TeamJoinCodeTextBox.Text = string.Empty;
            TeamJoinStatusText.Text = "Equipe vinculada com sucesso. Abra a equipe na lista para ver os detalhes.";
            TeamCreationStatusText.Text = string.Empty;
            _teamEntryMode = TeamEntryMode.None;
            UpdateTeamsViewState();
        }

        private void OpenTeamWorkspace_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not TeamWorkspaceInfo team)
            {
                return;
            }

            _activeTeamWorkspace = team;
            _activeTeamBoardView = TeamBoardView.Trello;
            RenderTeamWorkspace();
        }

        private void CloseTeamWorkspace_Click(object sender, RoutedEventArgs e)
        {
            _activeTeamWorkspace = null;
            TeamWorkspaceHost.Content = null;
            UpdateTeamsViewState();
        }

        private void ChangeTeamBoardView_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not TeamBoardView view)
            {
                return;
            }

            _activeTeamBoardView = view;
            RenderTeamWorkspace();
        }

        private void TeamTaskCard_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || sender is not Border border || border.Tag is not TeamTaskCardInfo card)
            {
                return;
            }

            _draggedTeamTaskCard = card;
            DragDrop.DoDragDrop(border, new DataObject(typeof(TeamTaskCardInfo), card), DragDropEffects.Move);
            _draggedTeamTaskCard = null;
        }

        private void TeamBoardColumn_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(typeof(TeamTaskCardInfo)) ? DragDropEffects.Move : DragDropEffects.None;
            e.Handled = true;
        }

        private void TeamBoardColumn_Drop(object sender, DragEventArgs e)
        {
            if (_activeTeamWorkspace == null ||
                sender is not Border border ||
                border.Tag is not TeamTaskColumnInfo targetColumn ||
                !e.Data.GetDataPresent(typeof(TeamTaskCardInfo)))
            {
                return;
            }

            var draggedCard = e.Data.GetData(typeof(TeamTaskCardInfo)) as TeamTaskCardInfo ?? _draggedTeamTaskCard;
            if (draggedCard == null)
            {
                return;
            }

            var sourceColumn = _activeTeamWorkspace.TaskColumns.FirstOrDefault(column => column.Cards.Any(card => card.Id == draggedCard.Id));
            if (sourceColumn == null || sourceColumn.Id == targetColumn.Id)
            {
                return;
            }

            sourceColumn.Cards.RemoveAll(card => card.Id == draggedCard.Id);
            targetColumn.Cards.Add(draggedCard);
            AddTeamNotification(_activeTeamWorkspace, $"Tarefa \"{draggedCard.Title}\" movida para {targetColumn.Title}.");
            SaveTeamWorkspace(_activeTeamWorkspace);
            RenderTeamWorkspace();
        }

        private void OpenCreateTaskDialog(TeamWorkspaceInfo team)
        {
            var dialog = CreateTaskDialog(team, null, null);
            if (dialog is not { } taskDialog)
            {
                return;
            }

            var targetColumn = team.TaskColumns.FirstOrDefault(column => column.Id == taskDialog.TargetColumnId) ?? team.TaskColumns.First();
            targetColumn.Cards.Add(taskDialog.Card);

            NotifyTaskAssignments(team, taskDialog.Card);
            AddTeamNotification(team, $"Nova tarefa criada: {taskDialog.Card.Title}.");
            SaveTeamWorkspace(team);
            RenderTeamWorkspace();
        }

        private void EditTeamTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not Tuple<TeamWorkspaceInfo, TeamTaskColumnInfo, TeamTaskCardInfo> payload)
            {
                return;
            }

            var team = payload.Item1;
            var column = payload.Item2;
            var card = payload.Item3;

            var dialog = CreateTaskDialog(team, column, card);
            if (dialog is not { } taskDialog)
            {
                return;
            }

            if (column.Id != taskDialog.TargetColumnId)
            {
                column.Cards.RemoveAll(item => item.Id == card.Id);
                var newColumn = team.TaskColumns.FirstOrDefault(item => item.Id == taskDialog.TargetColumnId) ?? column;
                newColumn.Cards.Add(taskDialog.Card);
            }

            NotifyTaskAssignments(team, taskDialog.Card);
            AddTeamNotification(team, $"Tarefa atualizada: {taskDialog.Card.Title}.");
            SaveTeamWorkspace(team);
            RenderTeamWorkspace();
        }

        private void DeleteTeamTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not Tuple<TeamWorkspaceInfo, TeamTaskColumnInfo, TeamTaskCardInfo> payload)
            {
                return;
            }

            var team = payload.Item1;
            var column = payload.Item2;
            var card = payload.Item3;

            if (MessageBox.Show($"Excluir a tarefa \"{card.Title}\"?", "Excluir tarefa", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            column.Cards.RemoveAll(item => item.Id == card.Id);
            AddTeamNotification(team, $"Tarefa removida: {card.Title}.");
            SaveTeamWorkspace(team);
            RenderTeamWorkspace();
        }

        private (TeamTaskCardInfo Card, string TargetColumnId)? CreateTaskDialog(TeamWorkspaceInfo team, TeamTaskColumnInfo? currentColumn, TeamTaskCardInfo? existingCard)
        {
            var dialog = new Window
            {
                Title = existingCard == null ? "Nova tarefa" : "Editar tarefa",
                Width = 560,
                Height = 720,
                MinHeight = 680,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
                Background = GetThemeBrush("SurfaceBrush")
            };

            var root = new Grid { Margin = new Thickness(20) };
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var header = new StackPanel { Margin = new Thickness(0, 0, 0, 16) };
            header.Children.Add(new TextBlock
            {
                Text = existingCard == null ? "Criar nova tarefa" : "Editar tarefa da equipe",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            });
            header.Children.Add(new TextBlock
            {
                Text = "Preencha os dados abaixo e confirme para adicionar a tarefa ao board.",
                FontSize = 12,
                Margin = new Thickness(0, 6, 0, 0),
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            });
            root.Children.Add(header);

            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            };
            Grid.SetRow(scrollViewer, 1);
            root.Children.Add(scrollViewer);

            var form = new StackPanel();
            scrollViewer.Content = form;

            var titleBox = new TextBox
            {
                Text = existingCard?.Title ?? string.Empty,
                Height = 44,
                Padding = new Thickness(12, 10, 12, 10),
                Background = GetThemeBrush("SearchBackgroundBrush"),
                BorderBrush = GetThemeBrush("SearchBorderBrush"),
                Foreground = GetThemeBrush("PrimaryTextBrush"),
                Margin = new Thickness(0, 6, 0, 14)
            };
            var descriptionBox = new TextBox
            {
                Text = existingCard?.Description ?? string.Empty,
                Height = 110,
                Padding = new Thickness(12),
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Background = GetThemeBrush("SearchBackgroundBrush"),
                BorderBrush = GetThemeBrush("SearchBorderBrush"),
                Foreground = GetThemeBrush("PrimaryTextBrush"),
                Margin = new Thickness(0, 6, 0, 14)
            };
            var priorityBox = new ComboBox
            {
                ItemsSource = new[] { "Alta", "Media", "Baixa" },
                SelectedItem = existingCard?.Priority ?? "Media",
                Height = 44,
                Padding = new Thickness(12, 8, 12, 8),
                Background = GetThemeBrush("SearchBackgroundBrush"),
                BorderBrush = GetThemeBrush("SearchBorderBrush"),
                Foreground = GetThemeBrush("PrimaryTextBrush"),
                Margin = new Thickness(0, 6, 0, 14)
            };
            var dueDatePicker = new DatePicker
            {
                SelectedDate = existingCard?.DueDate,
                Height = 44,
                Padding = new Thickness(12, 8, 12, 8),
                Background = GetThemeBrush("SearchBackgroundBrush"),
                BorderBrush = GetThemeBrush("SearchBorderBrush"),
                Foreground = GetThemeBrush("PrimaryTextBrush"),
                Margin = new Thickness(0, 6, 0, 14)
            };
            var columnBox = new ComboBox
            {
                ItemsSource = team.TaskColumns,
                DisplayMemberPath = "Title",
                SelectedItem = currentColumn ?? team.TaskColumns.FirstOrDefault(),
                Height = 44,
                Padding = new Thickness(12, 8, 12, 8),
                Background = GetThemeBrush("SearchBackgroundBrush"),
                BorderBrush = GetThemeBrush("SearchBorderBrush"),
                Foreground = GetThemeBrush("PrimaryTextBrush"),
                Margin = new Thickness(0, 6, 0, 14)
            };
            var membersBox = new ListBox
            {
                SelectionMode = SelectionMode.Multiple,
                Height = 180,
                Background = GetThemeBrush("SearchBackgroundBrush"),
                BorderBrush = GetThemeBrush("SearchBorderBrush"),
                Foreground = GetThemeBrush("PrimaryTextBrush"),
                Margin = new Thickness(0, 6, 0, 14)
            };
            foreach (var member in team.Members.OrderBy(item => item.Name))
            {
                var item = new ListBoxItem
                {
                    Content = member.DisplayLabel,
                    Tag = member
                };
                if (existingCard?.AssignedUserIds.Contains(member.UserId) == true)
                {
                    item.IsSelected = true;
                }

                membersBox.Items.Add(item);
            }

            form.Children.Add(CreateDialogFieldLabel("Titulo da tarefa"));
            form.Children.Add(titleBox);
            form.Children.Add(CreateDialogFieldLabel("Descricao"));
            form.Children.Add(descriptionBox);
            form.Children.Add(CreateDialogFieldLabel("Prioridade"));
            form.Children.Add(priorityBox);
            form.Children.Add(CreateDialogFieldLabel("Prazo"));
            form.Children.Add(dueDatePicker);
            form.Children.Add(CreateDialogFieldLabel("Coluna"));
            form.Children.Add(columnBox);
            form.Children.Add(CreateDialogFieldLabel("Atribuir para"));
            form.Children.Add(membersBox);

            var footer = new Border
            {
                Padding = new Thickness(0, 14, 0, 0),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(0, 1, 0, 0)
            };
            Grid.SetRow(footer, 2);
            root.Children.Add(footer);

            var footerButtons = new WrapPanel { HorizontalAlignment = HorizontalAlignment.Right };
            var cancelButton = new Button
            {
                Content = "Cancelar",
                Width = 110,
                Height = 40,
                Margin = new Thickness(0, 0, 10, 0),
                Background = GetThemeBrush("MutedCardBackgroundBrush"),
                Foreground = GetThemeBrush("PrimaryTextBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush")
            };
            var saveButton = new Button
            {
                Content = existingCard == null ? "Confirmar adicao" : "Confirmar alteracao",
                Width = 160,
                Height = 40,
                Background = GetThemeBrush("AccentBrush"),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0)
            };
            footerButtons.Children.Add(cancelButton);
            footerButtons.Children.Add(saveButton);
            footer.Child = footerButtons;
            dialog.Content = root;

            TeamTaskCardInfo? resultCard = null;
            string? resultColumnId = null;

            cancelButton.Click += (s, e) => dialog.Close();
            saveButton.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(titleBox.Text) || columnBox.SelectedItem is not TeamTaskColumnInfo selectedColumn)
                {
                    MessageBox.Show("Preencha ao menos o titulo e a coluna da tarefa.", "Tarefa", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                resultCard = existingCard ?? new TeamTaskCardInfo();
                resultCard.Title = titleBox.Text.Trim();
                resultCard.Description = descriptionBox.Text.Trim();
                resultCard.Priority = priorityBox.SelectedItem?.ToString() ?? "Media";
                resultCard.DueDate = dueDatePicker.SelectedDate;
                resultCard.AssignedUserIds = membersBox.SelectedItems
                    .OfType<ListBoxItem>()
                    .Select(item => item.Tag)
                    .OfType<UserInfo>()
                    .Select(member => member.UserId)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                resultColumnId = selectedColumn.Id;
                dialog.DialogResult = true;
                dialog.Close();
            };

            return dialog.ShowDialog() == true && resultCard != null && resultColumnId != null
                ? (resultCard, resultColumnId)
                : null;
        }

        private void NotifyTaskAssignments(TeamWorkspaceInfo team, TeamTaskCardInfo card)
        {
            foreach (var member in team.Members.Where(user => card.AssignedUserIds.Contains(user.UserId)))
            {
                AddTeamNotification(team, $"Tarefa \"{card.Title}\" atribuida para {member.Name}.");
            }
        }

        private void OpenAddCsdNoteDialog(TeamWorkspaceInfo team)
        {
            var dialog = new Window
            {
                Title = "Nova nota CSD",
                Width = 500,
                Height = 420,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
                Background = GetThemeBrush("SurfaceBrush")
            };

            var root = new Grid { Margin = new Thickness(20) };
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var header = new StackPanel { Margin = new Thickness(0, 0, 0, 16) };
            header.Children.Add(new TextBlock
            {
                Text = "Adicionar nota ao quadro CSD",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            });
            header.Children.Add(new TextBlock
            {
                Text = "Escolha a categoria e confirme a nova nota do projeto.",
                FontSize = 12,
                Margin = new Thickness(0, 6, 0, 0),
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            });
            root.Children.Add(header);

            var form = new StackPanel();
            Grid.SetRow(form, 1);
            root.Children.Add(form);

            var bucketBox = new ComboBox
            {
                ItemsSource = new[] { "Certezas", "Suposicoes", "Duvidas" },
                SelectedIndex = 0,
                Height = 44,
                Padding = new Thickness(12, 8, 12, 8),
                Background = GetThemeBrush("SearchBackgroundBrush"),
                BorderBrush = GetThemeBrush("SearchBorderBrush"),
                Foreground = GetThemeBrush("PrimaryTextBrush"),
                Margin = new Thickness(0, 6, 0, 14)
            };
            var noteBox = new TextBox
            {
                Height = 180,
                Padding = new Thickness(12),
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Background = GetThemeBrush("SearchBackgroundBrush"),
                BorderBrush = GetThemeBrush("SearchBorderBrush"),
                Foreground = GetThemeBrush("PrimaryTextBrush"),
                Margin = new Thickness(0, 6, 0, 14)
            };

            form.Children.Add(CreateDialogFieldLabel("Categoria"));
            form.Children.Add(bucketBox);
            form.Children.Add(CreateDialogFieldLabel("Conteudo da nota"));
            form.Children.Add(noteBox);

            var footer = new Border
            {
                Padding = new Thickness(0, 14, 0, 0),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(0, 1, 0, 0)
            };
            Grid.SetRow(footer, 2);
            var footerButtons = new WrapPanel { HorizontalAlignment = HorizontalAlignment.Right };
            var cancelButton = new Button
            {
                Content = "Cancelar",
                Width = 110,
                Height = 40,
                Margin = new Thickness(0, 0, 10, 0),
                Background = GetThemeBrush("MutedCardBackgroundBrush"),
                Foreground = GetThemeBrush("PrimaryTextBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush")
            };
            var addButton = new Button
            {
                Content = "Confirmar adicao",
                Width = 160,
                Height = 40,
                Background = GetThemeBrush("AccentBrush"),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0)
            };
            footerButtons.Children.Add(cancelButton);
            footerButtons.Children.Add(addButton);
            footer.Child = footerButtons;
            root.Children.Add(footer);
            dialog.Content = root;

            cancelButton.Click += (s, e) => dialog.Close();

            addButton.Click += (s, e) =>
            {
                var note = noteBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(note))
                {
                    MessageBox.Show("Digite a nota antes de confirmar.", "CSD", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                switch (bucketBox.SelectedItem?.ToString())
                {
                    case "Suposicoes":
                        team.CsdBoard.Assumptions.Add(note);
                        break;
                    case "Duvidas":
                        team.CsdBoard.Doubts.Add(note);
                        break;
                    default:
                        team.CsdBoard.Certainties.Add(note);
                        break;
                }

                AddTeamNotification(team, "Nova nota adicionada ao quadro CSD.");
                SaveTeamWorkspace(team);
                dialog.DialogResult = true;
                dialog.Close();
                RenderTeamWorkspace();
            };

            dialog.ShowDialog();
        }

        private TextBlock CreateDialogFieldLabel(string text)
        {
            return new TextBlock
            {
                Text = text,
                FontWeight = FontWeights.SemiBold,
                FontSize = 12,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            };
        }

        private FrameworkElementFactory CreateTeamMemberSearchItemTemplate()
        {
            var root = new FrameworkElementFactory(typeof(Border));
            root.SetValue(Border.PaddingProperty, new Thickness(12));
            root.SetValue(Border.MarginProperty, new Thickness(0, 0, 0, 8));
            root.SetValue(Border.BorderThicknessProperty, new Thickness(1));
            root.SetValue(Border.CornerRadiusProperty, new CornerRadius(14));
            root.SetValue(Border.BackgroundProperty, GetThemeBrush("MutedCardBackgroundBrush"));
            root.SetValue(Border.BorderBrushProperty, GetThemeBrush("CardBorderBrush"));

            var stack = new FrameworkElementFactory(typeof(StackPanel));

            var name = new FrameworkElementFactory(typeof(TextBlock));
            name.SetValue(TextBlock.FontSizeProperty, 13.0);
            name.SetValue(TextBlock.FontWeightProperty, FontWeights.SemiBold);
            name.SetValue(TextBlock.ForegroundProperty, GetThemeBrush("PrimaryTextBrush"));
            name.SetBinding(TextBlock.TextProperty, new Binding(nameof(UserInfo.Name)));
            stack.AppendChild(name);

            var secondary = new FrameworkElementFactory(typeof(TextBlock));
            secondary.SetValue(TextBlock.MarginProperty, new Thickness(0, 4, 0, 0));
            secondary.SetValue(TextBlock.FontSizeProperty, 11.0);
            secondary.SetValue(TextBlock.ForegroundProperty, GetThemeBrush("SecondaryTextBrush"));
            secondary.SetBinding(TextBlock.TextProperty, new Binding(nameof(UserInfo.DisplayLabel)));
            stack.AppendChild(secondary);

            var course = new FrameworkElementFactory(typeof(TextBlock));
            course.SetValue(TextBlock.MarginProperty, new Thickness(0, 4, 0, 0));
            course.SetValue(TextBlock.FontSizeProperty, 10.0);
            course.SetValue(TextBlock.ForegroundProperty, GetThemeBrush("TertiaryTextBrush"));
            course.SetBinding(TextBlock.TextProperty, new Binding(nameof(UserInfo.Course)));
            stack.AppendChild(course);

            root.AppendChild(stack);
            return root;
        }

        private void OpenAddTeamMemberDialog(TeamWorkspaceInfo team)
        {
            var dialog = new Window
            {
                Title = "Adicionar membro",
                Width = 620,
                Height = 580,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
                Background = GetThemeBrush("CardBackgroundBrush")
            };

            var root = new Grid { Margin = new Thickness(20) };
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var titleBlock = new TextBlock
            {
                Text = "Adicionar novo integrante",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            };
            root.Children.Add(titleBlock);

            var subtitleBlock = new TextBlock
            {
                Text = "Busque em tempo real por nome, matricula ou email e confirme manualmente quem entra na equipe.",
                Margin = new Thickness(0, 8, 0, 0),
                TextWrapping = TextWrapping.Wrap,
                FontSize = 12,
                Foreground = GetThemeBrush("SecondaryTextBrush")
            };
            root.Children.Add(subtitleBlock);
            Grid.SetRow(subtitleBlock, 1);

            var queryBox = new TextBox
            {
                Height = 42,
                Margin = new Thickness(0, 16, 0, 12)
            };
            root.Children.Add(queryBox);
            Grid.SetRow(queryBox, 2);

            var resultsList = new ListBox
            {
                Height = 300
            };
            resultsList.ItemTemplate = new DataTemplate(typeof(UserInfo))
            {
                VisualTree = CreateTeamMemberSearchItemTemplate()
            };
            root.Children.Add(resultsList);
            Grid.SetRow(resultsList, 3);

            var statusText = new TextBlock
            {
                Margin = new Thickness(0, 12, 0, 10),
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                Text = "Digite para buscar no Firebase."
            };
            root.Children.Add(statusText);
            Grid.SetRow(statusText, 4);

            var footer = new DockPanel { LastChildFill = false, Margin = new Thickness(0, 8, 0, 0) };
            var cancelButton = new Button
            {
                Content = "Cancelar",
                Width = 110,
                Height = 40,
                Margin = new Thickness(0, 0, 10, 0)
            };
            var addButton = new Button
            {
                Content = "Adicionar",
                Width = 120,
                Height = 40,
                Background = GetThemeBrush("AccentBrush"),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0)
            };
            DockPanel.SetDock(cancelButton, Dock.Right);
            DockPanel.SetDock(addButton, Dock.Right);
            footer.Children.Add(addButton);
            footer.Children.Add(cancelButton);
            dialog.Content = root;

            var localSearchVersion = 0;
            async Task RefreshSearchResultsAsync()
            {
                var query = NormalizeTeamValue(queryBox.Text);
                if (string.IsNullOrWhiteSpace(query))
                {
                    resultsList.ItemsSource = Array.Empty<UserInfo>();
                    statusText.Text = "Digite para buscar por nome, matricula ou email.";
                    return;
                }

                var currentSearch = ++localSearchVersion;
                statusText.Text = "Buscando alunos...";
                await Task.Delay(220);
                if (currentSearch != localSearchVersion)
                {
                    return;
                }

                var results = await SearchTeamMembersAsync(query);
                if (currentSearch != localSearchVersion)
                {
                    return;
                }

                var filteredResults = results
                    .Where(user => !team.Members.Any(member => string.Equals(member.UserId, user.UserId, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                resultsList.ItemsSource = filteredResults;
                statusText.Text = filteredResults.Count == 0
                    ? "Nenhum aluno disponivel para adicionar com esse criterio."
                    : $"{filteredResults.Count} aluno(s) disponivel(is). Selecione um para confirmar.";
            }

            queryBox.TextChanged += async (s, e) => await RefreshSearchResultsAsync();
            cancelButton.Click += (s, e) => dialog.Close();

            addButton.Click += (s, e) =>
            {
                if (resultsList.SelectedItem is not UserInfo selected)
                {
                    statusText.Text = "Selecione um aluno da lista.";
                    return;
                }

                if (team.Members.Any(member => string.Equals(member.UserId, selected.UserId, StringComparison.OrdinalIgnoreCase)))
                {
                    statusText.Text = "Esse aluno ja faz parte da equipe.";
                    return;
                }

                team.Members.Add(selected);
                AddTeamNotification(team, $"{selected.Name} foi adicionado(a) a equipe.");
                SaveTeamWorkspace(team);
                dialog.DialogResult = true;
                dialog.Close();
            };

            root.Children.Add(footer);
            Grid.SetRow(footer, 5);

            dialog.Loaded += async (s, e) =>
            {
                queryBox.Focus();
                await RefreshSearchResultsAsync();
            };

            if (dialog.ShowDialog() == true)
            {
                RenderTeamWorkspace();
            }
        }

        private void OpenRemoveTeamMemberDialog(TeamWorkspaceInfo team)
        {
            var removableMembers = team.Members.Where(member => !string.Equals(member.UserId, _currentProfile?.UserId, StringComparison.OrdinalIgnoreCase)).ToList();
            if (removableMembers.Count == 0)
            {
                MessageBox.Show("Nao ha membros disponiveis para remocao.", "Equipe", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new Window
            {
                Title = "Remover membro",
                Width = 560,
                Height = 500,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
                Background = GetThemeBrush("CardBackgroundBrush")
            };

            var root = new Grid { Margin = new Thickness(20) };
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var helperText = new TextBlock
            {
                Text = "Selecione quem deve sair da equipe. As atribuicoes dessa pessoa tambem serao removidas das tarefas.",
                Margin = new Thickness(0, 0, 0, 12),
                TextWrapping = TextWrapping.Wrap,
                FontSize = 12,
                Foreground = GetThemeBrush("SecondaryTextBrush")
            };
            root.Children.Add(helperText);

            var filterBox = new TextBox
            {
                Height = 40,
                Margin = new Thickness(0, 0, 0, 12)
            };
            root.Children.Add(filterBox);
            Grid.SetRow(filterBox, 1);

            var list = new ListBox
            {
                Height = 280,
                ItemsSource = removableMembers,
                DisplayMemberPath = "DisplayLabel"
            };
            root.Children.Add(list);
            Grid.SetRow(list, 2);

            var footer = new DockPanel { LastChildFill = false, Margin = new Thickness(0, 14, 0, 0) };
            var cancelButton = new Button
            {
                Content = "Cancelar",
                Width = 110,
                Height = 40,
                Margin = new Thickness(0, 0, 10, 0)
            };
            var removeButton = new Button
            {
                Content = "Remover selecionado",
                Width = 170,
                Height = 40,
                Background = new SolidColorBrush(Color.FromRgb(220, 38, 38)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0)
            };
            DockPanel.SetDock(cancelButton, Dock.Right);
            DockPanel.SetDock(removeButton, Dock.Right);
            footer.Children.Add(removeButton);
            footer.Children.Add(cancelButton);
            dialog.Content = root;

            filterBox.TextChanged += (s, e) =>
            {
                var query = NormalizeTeamValue(filterBox.Text);
                var filtered = removableMembers
                    .Where(member =>
                        string.IsNullOrWhiteSpace(query) ||
                        member.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        (!string.IsNullOrWhiteSpace(member.Email) && member.Email.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrWhiteSpace(member.Registration) && member.Registration.Contains(query, StringComparison.OrdinalIgnoreCase)))
                    .OrderBy(member => member.Name)
                    .ToList();

                list.ItemsSource = filtered;
            };

            cancelButton.Click += (s, e) => dialog.Close();

            removeButton.Click += (s, e) =>
            {
                if (list.SelectedItem is not UserInfo selected)
                {
                    return;
                }

                dialog.DialogResult = true;
                dialog.Tag = selected;
                dialog.Close();
            };

            root.Children.Add(footer);
            Grid.SetRow(footer, 3);

            if (dialog.ShowDialog() == true && dialog.Tag is UserInfo member)
            {
                RemoveMemberFromActiveTeam(member);
            }
        }

        private void RemoveMemberFromActiveTeam(UserInfo member)
        {
            if (_activeTeamWorkspace == null)
            {
                return;
            }

            if (MessageBox.Show($"Remover {member.Name} da equipe?", "Remover membro", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            _activeTeamWorkspace.Members.RemoveAll(item => string.Equals(item.UserId, member.UserId, StringComparison.OrdinalIgnoreCase));
            foreach (var column in _activeTeamWorkspace.TaskColumns)
            {
                foreach (var card in column.Cards)
                {
                    card.AssignedUserIds.RemoveAll(id => string.Equals(id, member.UserId, StringComparison.OrdinalIgnoreCase));
                }
            }

            AddTeamNotification(_activeTeamWorkspace, $"{member.Name} foi removido(a) da equipe.");
            SaveTeamWorkspace(_activeTeamWorkspace);
            RenderTeamWorkspace();
        }

        private async void DeleteActiveTeamWorkspace(object? sender, RoutedEventArgs e)
        {
            if (_activeTeamWorkspace == null)
            {
                return;
            }

            if (!IsCurrentUserTeamCreator(_activeTeamWorkspace))
            {
                MessageBox.Show("Apenas quem criou a equipe pode apagá-la.", "Equipe", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show($"Deseja apagar a equipe {_activeTeamWorkspace.TeamName}?", "Apagar equipe", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            if (_teamService != null)
            {
                var deleteResult = await _teamService.DeleteTeamAsync(_activeTeamWorkspace);
                if (!deleteResult.Success)
                {
                    MessageBox.Show($"Nao foi possivel apagar a equipe.\n\n{deleteResult.ErrorMessage}", "Equipe", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            _teamWorkspaces.RemoveAll(team => string.Equals(team.TeamId, _activeTeamWorkspace.TeamId, StringComparison.OrdinalIgnoreCase));
            _activeTeamWorkspace = null;
            TeamWorkspaceHost.Content = null;
            RenderTeamsList();
            UpdateTeamsViewState();
        }

        private void AddTeamAsset(string category)
        {
            if (_activeTeamWorkspace == null)
            {
                return;
            }

            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                Title = $"Selecionar {category}"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            foreach (var file in dialog.FileNames)
            {
                _activeTeamWorkspace.Assets.Add(new TeamAssetInfo
                {
                    Category = category,
                    FileName = System.IO.Path.GetFileName(file),
                    AddedAt = DateTime.Now
                });
            }

            AddTeamNotification(_activeTeamWorkspace, $"{dialog.FileNames.Length} arquivo(s) adicionado(s) em {category}.");
            SaveTeamWorkspace(_activeTeamWorkspace);
            RenderTeamWorkspace();
        }

        private void CopyTeamCode_Click(object? sender, RoutedEventArgs e)
        {
            TeamWorkspaceInfo? team = null;
            if (sender is Button button && button.Tag is TeamWorkspaceInfo taggedTeam)
            {
                team = taggedTeam;
            }

            team ??= _activeTeamWorkspace;
            if (team == null || string.IsNullOrWhiteSpace(team.TeamId))
            {
                return;
            }

            Clipboard.SetText(team.TeamId);
            MessageBox.Show($"Codigo da equipe copiado: {team.TeamId}", "Equipe", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenAddMilestoneDialog(TeamWorkspaceInfo team)
        {
            var dialog = new Window
            {
                Title = "Nova entrega",
                Width = 500,
                Height = 420,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
                Background = GetThemeBrush("CardBackgroundBrush")
            };

            var root = new StackPanel { Margin = new Thickness(20) };
            var titleBox = new TextBox { Height = 40, Margin = new Thickness(0, 6, 0, 14) };
            var notesBox = new TextBox
            {
                Height = 110,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                Margin = new Thickness(0, 6, 0, 14)
            };
            var statusBox = new ComboBox
            {
                ItemsSource = new[] { "Planejada", "Em andamento", "Concluida" },
                SelectedIndex = 0,
                Height = 40,
                Margin = new Thickness(0, 6, 0, 14)
            };
            var dueDatePicker = new DatePicker { Height = 40, Margin = new Thickness(0, 6, 0, 14) };

            root.Children.Add(new TextBlock { Text = "Titulo da entrega", FontWeight = FontWeights.SemiBold });
            root.Children.Add(titleBox);
            root.Children.Add(new TextBlock { Text = "Notas", FontWeight = FontWeights.SemiBold });
            root.Children.Add(notesBox);
            root.Children.Add(new TextBlock { Text = "Status", FontWeight = FontWeights.SemiBold });
            root.Children.Add(statusBox);
            root.Children.Add(new TextBlock { Text = "Prazo", FontWeight = FontWeights.SemiBold });
            root.Children.Add(dueDatePicker);

            var footer = new WrapPanel { HorizontalAlignment = HorizontalAlignment.Right };
            var cancelButton = new Button { Content = "Cancelar", Width = 110, Height = 40, Margin = new Thickness(0, 0, 10, 0) };
            var saveButton = new Button
            {
                Content = "Salvar entrega",
                Width = 130,
                Height = 40,
                Background = GetThemeBrush("AccentBrush"),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0)
            };
            footer.Children.Add(cancelButton);
            footer.Children.Add(saveButton);
            root.Children.Add(footer);
            dialog.Content = root;

            cancelButton.Click += (s, e) => dialog.Close();
            saveButton.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(titleBox.Text))
                {
                    MessageBox.Show("Informe um titulo para a entrega.", "Equipe", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                team.Milestones.Add(new TeamMilestoneInfo
                {
                    Title = titleBox.Text.Trim(),
                    Notes = notesBox.Text.Trim(),
                    Status = statusBox.SelectedItem?.ToString() ?? "Planejada",
                    DueDate = dueDatePicker.SelectedDate,
                    CreatedAt = DateTime.Now
                });

                AddTeamNotification(team, $"Nova entrega planejada: {titleBox.Text.Trim()}.");
                SaveTeamWorkspace(team);
                dialog.DialogResult = true;
                dialog.Close();
            };

            if (dialog.ShowDialog() == true)
            {
                RenderTeamWorkspace();
            }
        }

        private void ToggleMilestoneStatus_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not Tuple<TeamWorkspaceInfo, TeamMilestoneInfo> payload)
            {
                return;
            }

            var team = payload.Item1;
            var milestone = payload.Item2;
            milestone.Status = string.Equals(milestone.Status, "Concluida", StringComparison.OrdinalIgnoreCase)
                ? "Em andamento"
                : "Concluida";
            AddTeamNotification(team, $"Entrega atualizada: {milestone.Title} agora está {milestone.Status.ToLowerInvariant()}.");
            SaveTeamWorkspace(team);
            RenderTeamWorkspace();
        }

        private void DeleteMilestone_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not Tuple<TeamWorkspaceInfo, TeamMilestoneInfo> payload)
            {
                return;
            }

            var team = payload.Item1;
            var milestone = payload.Item2;
            if (MessageBox.Show($"Remover a entrega '{milestone.Title}'?", "Equipe", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            team.Milestones.RemoveAll(item => string.Equals(item.Id, milestone.Id, StringComparison.OrdinalIgnoreCase));
            AddTeamNotification(team, $"Entrega removida: {milestone.Title}.");
            SaveTeamWorkspace(team);
            RenderTeamWorkspace();
        }

        private void OpenProjectManagementDialog(TeamWorkspaceInfo team)
        {
            var dialog = new Window
            {
                Title = "Gestao do projeto",
                Width = 520,
                Height = 430,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
                Background = GetThemeBrush("SurfaceBrush")
            };

            var root = new Grid { Margin = new Thickness(20) };
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var header = new StackPanel { Margin = new Thickness(0, 0, 0, 16) };
            header.Children.Add(new TextBlock
            {
                Text = "Atualizar progresso e prazo",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            });
            header.Children.Add(new TextBlock
            {
                Text = "Defina um andamento geral para o projeto e a data principal de entrega.",
                FontSize = 12,
                Margin = new Thickness(0, 6, 0, 0),
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            });
            root.Children.Add(header);

            var form = new StackPanel();
            Grid.SetRow(form, 1);
            root.Children.Add(form);

            var progressLabel = new TextBlock
            {
                Text = $"Progresso: {team.ProjectProgress}%",
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            };
            var progressSlider = new Slider
            {
                Minimum = 0,
                Maximum = 100,
                TickFrequency = 5,
                IsSnapToTickEnabled = true,
                Value = team.ProjectProgress,
                Margin = new Thickness(0, 10, 0, 18)
            };
            progressSlider.ValueChanged += (s, e) => progressLabel.Text = $"Progresso: {(int)progressSlider.Value}%";

            var statusBox = new ComboBox
            {
                ItemsSource = new[] { "Planejamento", "Em andamento", "Em revisao", "Concluido" },
                SelectedItem = team.ProjectStatus,
                Height = 44,
                Padding = new Thickness(12, 8, 12, 8),
                Background = GetThemeBrush("SearchBackgroundBrush"),
                BorderBrush = GetThemeBrush("SearchBorderBrush"),
                Foreground = GetThemeBrush("PrimaryTextBrush"),
                Margin = new Thickness(0, 6, 0, 14)
            };

            var deadlinePicker = new DatePicker
            {
                SelectedDate = team.ProjectDeadline,
                Height = 44,
                Padding = new Thickness(12, 8, 12, 8),
                Background = GetThemeBrush("SearchBackgroundBrush"),
                BorderBrush = GetThemeBrush("SearchBorderBrush"),
                Foreground = GetThemeBrush("PrimaryTextBrush"),
                Margin = new Thickness(0, 6, 0, 14)
            };

            form.Children.Add(progressLabel);
            form.Children.Add(progressSlider);
            form.Children.Add(CreateDialogFieldLabel("Status do projeto"));
            form.Children.Add(statusBox);
            form.Children.Add(CreateDialogFieldLabel("Prazo principal"));
            form.Children.Add(deadlinePicker);

            var footer = new Border
            {
                Padding = new Thickness(0, 14, 0, 0),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(0, 1, 0, 0)
            };
            Grid.SetRow(footer, 2);
            var footerButtons = new WrapPanel { HorizontalAlignment = HorizontalAlignment.Right };
            var cancelButton = new Button
            {
                Content = "Cancelar",
                Width = 110,
                Height = 40,
                Margin = new Thickness(0, 0, 10, 0),
                Background = GetThemeBrush("MutedCardBackgroundBrush"),
                Foreground = GetThemeBrush("PrimaryTextBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush")
            };
            var saveButton = new Button
            {
                Content = "Salvar gestao",
                Width = 150,
                Height = 40,
                Background = GetThemeBrush("AccentBrush"),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0)
            };
            footerButtons.Children.Add(cancelButton);
            footerButtons.Children.Add(saveButton);
            footer.Child = footerButtons;
            root.Children.Add(footer);
            dialog.Content = root;

            cancelButton.Click += (s, e) => dialog.Close();
            saveButton.Click += (s, e) =>
            {
                team.ProjectProgress = (int)progressSlider.Value;
                team.ProjectStatus = statusBox.SelectedItem?.ToString() ?? "Planejamento";
                team.ProjectDeadline = deadlinePicker.SelectedDate;
                AddTeamNotification(team, $"Gestao do projeto atualizada: {team.ProjectProgress}% e status {team.ProjectStatus.ToLowerInvariant()}.");
                SaveTeamWorkspace(team);
                dialog.DialogResult = true;
                dialog.Close();
            };

            if (dialog.ShowDialog() == true)
            {
                RenderTeamWorkspace();
            }
        }

        private void OpenProjectChatDialog(TeamWorkspaceInfo team)
        {
            var dialog = new Window
            {
                Title = $"Chat do projeto - {team.TeamName}",
                Width = 860,
                Height = 640,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                Background = GetThemeBrush("SurfaceBrush")
            };

            var root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var topBar = new Border
            {
                Background = GetThemeBrush("CardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(20, 16, 20, 16)
            };
            var topStack = new StackPanel();
            topStack.Children.Add(new TextBlock
            {
                Text = "Chat do projeto",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            });
            topStack.Children.Add(new TextBlock
            {
                Text = string.Join(" • ", team.Members.Select(member => member.Name).Take(6)),
                FontSize = 12,
                Margin = new Thickness(0, 6, 0, 0),
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            });
            topBar.Child = topStack;
            root.Children.Add(topBar);

            var messagesScroll = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Background = GetThemeBrush("MainContentBackgroundBrush")
            };
            Grid.SetRow(messagesScroll, 1);
            var messagesPanel = new StackPanel { Margin = new Thickness(20, 16, 20, 16) };
            messagesScroll.Content = messagesPanel;
            root.Children.Add(messagesScroll);

            void RenderProjectChatMessages()
            {
                messagesPanel.Children.Clear();
                if (team.ChatMessages.Count == 0)
                {
                    messagesPanel.Children.Add(new Border
                    {
                        Background = GetThemeBrush("MutedCardBackgroundBrush"),
                        BorderBrush = GetThemeBrush("CardBorderBrush"),
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(18),
                        Padding = new Thickness(18),
                        Child = new TextBlock
                        {
                            Text = "Nenhuma mensagem ainda. Use o campo abaixo para iniciar o chat em grupo do projeto.",
                            TextWrapping = TextWrapping.Wrap,
                            Foreground = GetThemeBrush("SecondaryTextBrush")
                        }
                    });
                    return;
                }

                foreach (var message in team.ChatMessages.OrderBy(item => item.SentAt))
                {
                    var isOwn = string.Equals(message.SenderId, GetCurrentUserId(), StringComparison.OrdinalIgnoreCase);
                    var wrapper = new StackPanel
                    {
                        Margin = new Thickness(0, 0, 0, 12),
                        HorizontalAlignment = isOwn ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                        MaxWidth = 520
                    };

                    wrapper.Children.Add(new TextBlock
                    {
                        Text = $"{message.SenderName} • {message.SentAt:dd/MM HH:mm}",
                        FontSize = 10,
                        Margin = new Thickness(6, 0, 6, 4),
                        Foreground = GetThemeBrush("TertiaryTextBrush"),
                        HorizontalAlignment = isOwn ? HorizontalAlignment.Right : HorizontalAlignment.Left
                    });

                    wrapper.Children.Add(new Border
                    {
                        Background = isOwn ? GetThemeBrush("AccentBrush") : GetThemeBrush("CardBackgroundBrush"),
                        BorderBrush = isOwn ? GetThemeBrush("AccentBrush") : GetThemeBrush("CardBorderBrush"),
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(18),
                        Padding = new Thickness(14, 10, 14, 10),
                        Child = new TextBlock
                        {
                            Text = message.Content,
                            TextWrapping = TextWrapping.Wrap,
                            Foreground = isOwn ? Brushes.White : GetThemeBrush("PrimaryTextBrush"),
                            FontSize = 12
                        }
                    });

                    messagesPanel.Children.Add(wrapper);
                }

                messagesScroll.ScrollToEnd();
            }

            var composer = new Border
            {
                Background = GetThemeBrush("CardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(0, 1, 0, 0),
                Padding = new Thickness(20, 14, 20, 14)
            };
            Grid.SetRow(composer, 2);
            var composerGrid = new Grid();
            composerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            composerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            var messageBox = new TextBox
            {
                Height = 46,
                Padding = new Thickness(14, 12, 14, 12),
                Background = GetThemeBrush("SearchBackgroundBrush"),
                BorderBrush = GetThemeBrush("SearchBorderBrush"),
                Foreground = GetThemeBrush("PrimaryTextBrush")
            };
            composerGrid.Children.Add(messageBox);
            var sendButton = new Button
            {
                Content = "Enviar",
                Width = 120,
                Height = 46,
                Margin = new Thickness(12, 0, 0, 0),
                Background = GetThemeBrush("AccentBrush"),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontWeight = FontWeights.SemiBold
            };
            Grid.SetColumn(sendButton, 1);
            composerGrid.Children.Add(sendButton);
            composer.Child = composerGrid;
            root.Children.Add(composer);
            dialog.Content = root;

            void SubmitProjectMessage()
            {
                var content = messageBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(content))
                {
                    return;
                }

                team.ChatMessages.Add(new TeamChatMessageInfo
                {
                    SenderId = GetCurrentUserId(),
                    SenderName = _currentProfile?.Name ?? "Usuario",
                    Content = content,
                    SentAt = DateTime.Now
                });
                AddTeamNotification(team, "Nova mensagem enviada no chat do projeto.");
                SaveTeamWorkspace(team);
                messageBox.Text = string.Empty;
                RenderProjectChatMessages();
                RenderTeamWorkspace();
            }

            sendButton.Click += (s, e) => SubmitProjectMessage();
            messageBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter && Keyboard.Modifiers != ModifierKeys.Shift)
                {
                    SubmitProjectMessage();
                    e.Handled = true;
                }
            };

            RenderProjectChatMessages();
            dialog.ShowDialog();
        }

        private void TeamActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button)
            {
                return;
            }

            var teamName = _activeTeamWorkspace?.TeamName ?? "sua equipe";
            var tag = button.Tag?.ToString() ?? string.Empty;
            var message = tag switch
            {
                "imagens" => $"A area de imagens da equipe {teamName} sera aberta aqui.",
                "documentos" => $"A biblioteca de documentos da equipe {teamName} sera aberta aqui.",
                "planos" => $"Os planos de entrega da equipe {teamName} serao centralizados aqui.",
                "trello" => $"A integracao com Trello da equipe {teamName} sera conectada nesta acao.",
                "kanban" => $"O quadro KANBAN da equipe {teamName} sera exibido nesta acao.",
                _ => $"A acao selecionada para {teamName} sera disponibilizada aqui."
            };

            MessageBox.Show(
                message,
                "Central da equipe",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Funcionou!");
        }

        private async void SearchFriends_Click(object sender, RoutedEventArgs e)
        {
            _searchSlideTypingVersion++;
            await ExecuteSearchFriendsAsync(SearchFriendsBox.Text?.Trim() ?? string.Empty, false);
        }

        private async Task ExecuteSearchFriendsAsync(string query, bool triggeredByTyping)
        {
            DebugHelper.WriteLine($"=== BUSCA INICIADA ===");
            DebugHelper.WriteLine($"Query do usuário: '{query}'");
            DebugHelper.WriteLine($"ID Token disponível: {!string.IsNullOrEmpty(_idToken)}");
            DebugHelper.WriteLine($"Perfil do usuário: {_currentProfile?.Name}");

            if (string.IsNullOrWhiteSpace(query))
            {
                HideSearchSlidePanel();
                DebugHelper.WriteLine("Busca cancelada: query vazia");
                return;
            }

            if (string.IsNullOrWhiteSpace(_idToken))
            {
                if (!triggeredByTyping)
                {
                    MessageBox.Show("Token de autenticação não disponível. Faça login novamente.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                DebugHelper.WriteLine("Busca cancelada: token vazio");
                return;
            }

            var requestVersion = ++_searchSlideRequestVersion;

            await RefreshConnectionsCacheAsync();
            SetSearchSlideLoadingState(query, triggeredByTyping);
            DebugHelper.WriteLine("Drawer de busca aberto, iniciando busca...");

            try
            {
                var searchService = new UserSearchService(_idToken);
                DebugHelper.WriteLine("UserSearchService criado");

                List<UserInfo> results = await searchService.SearchUsersAsync(query) ?? new List<UserInfo>();
                if (requestVersion != _searchSlideRequestVersion)
                {
                    return;
                }

                results = results
                    .Where(user => !string.Equals(user.UserId, _currentProfile?.UserId, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var user in results)
                {
                    user.IsCurrentUser = string.Equals(user.UserId, _currentProfile?.UserId, StringComparison.OrdinalIgnoreCase);
                    user.ConnectionState = _connectionService?.GetRelationshipState(user.UserId, _connectionEntries) ?? "none";
                }

                DebugHelper.WriteLine($"Busca concluída. Resultados: {results.Count}");

                if (results.Count > 0)
                {
                    foreach (var result in results)
                    {
                        DebugHelper.WriteLine($"  - {result.Name} ({result.Registration}): {result.Email}");
                    }
                }

                RenderSearchSlideResults(results);
            }
            catch (Exception ex)
            {
                if (requestVersion != _searchSlideRequestVersion)
                {
                    return;
                }

                DebugHelper.WriteLine($"EXCEÇÃO na busca: {ex.GetType().Name}");
                DebugHelper.WriteLine($"Mensagem: {ex.Message}");
                DebugHelper.WriteLine($"Stack: {ex.StackTrace}");

                SearchSlideStatusText.Text = $"Nao foi possivel consultar conexoes agora. {ex.Message}";
                SearchSlideStatusText.Foreground = new SolidColorBrush(Color.FromRgb(220, 38, 38));
                RenderSearchSlideResults(new List<UserInfo>());
                SearchSlideEmptyStateText.Text = "Revise sua conexão com o Firebase e tente novamente.";
                SearchSlideEmptyStateText.Visibility = Visibility.Visible;
            }

            DebugHelper.WriteLine("=== BUSCA FINALIZADA ===\n");
        }

        private void SetSearchSlideLoadingState(string query, bool triggeredByTyping)
        {
            _searchSlideQuery = query;
            _searchSlideResults.Clear();
            SearchSlideTitleText.Text = $"Resultados para \"{query}\"";
            SearchSlideStatusText.Text = triggeredByTyping
                ? "Atualizando resultados enquanto você digita..."
                : "Buscando por nome, matricula e email dentro da sua rede academica...";
            SearchSlideStatusText.Foreground = GetThemeBrush("SecondaryTextBrush");
            SearchSlideResultsHost.Children.Clear();
            SearchSlideEmptyStateText.Visibility = Visibility.Collapsed;
            SearchSlideResultsHost.Children.Add(CreateSearchSlideInfoCard("Buscando agora", "Assim que os perfis forem encontrados, eles aparecem aqui sem abrir uma nova janela."));
            ShowSearchSlidePanel();
        }

        private void RenderSearchSlideResults(List<UserInfo> results)
        {
            _searchSlideResults = results;
            SearchSlideResultsHost.Children.Clear();

            if (results.Count == 0)
            {
                SearchSlideStatusText.Text = string.IsNullOrWhiteSpace(_searchSlideQuery)
                    ? "Nenhuma pesquisa ativa."
                    : $"Nenhum perfil encontrado para \"{_searchSlideQuery}\".";
                SearchSlideStatusText.Foreground = GetThemeBrush("SecondaryTextBrush");
                SearchSlideEmptyStateText.Text = "Tente outro nome, matrícula ou email para localizar a pessoa certa.";
                SearchSlideEmptyStateText.Visibility = Visibility.Visible;
                return;
            }

            SearchSlideEmptyStateText.Visibility = Visibility.Collapsed;
            SearchSlideStatusText.Text = results.Count == 1
                ? "1 perfil encontrado. Você já pode conversar, conectar ou abrir os detalhes daqui."
                : $"{results.Count} perfis encontrados. Converse, conecte ou abra os detalhes do aluno.";
            SearchSlideStatusText.Foreground = GetThemeBrush("SecondaryTextBrush");

            foreach (var user in results)
            {
                SearchSlideResultsHost.Children.Add(CreateSearchSlideResultCard(user));
            }
        }

        private Border CreateSearchSlideInfoCard(string title, string description)
        {
            return new Border
            {
                Background = GetThemeBrush("MutedCardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(18),
                Padding = new Thickness(16),
                Margin = new Thickness(0, 0, 0, 10),
                Child = new StackPanel
                {
                    Children =
                    {
                        new TextBlock
                        {
                            Text = title,
                            FontSize = 13,
                            FontWeight = FontWeights.SemiBold,
                            Foreground = GetThemeBrush("PrimaryTextBrush")
                        },
                        new TextBlock
                        {
                            Text = description,
                            Margin = new Thickness(0, 6, 0, 0),
                            FontSize = 11,
                            TextWrapping = TextWrapping.Wrap,
                            Foreground = GetThemeBrush("SecondaryTextBrush")
                        }
                    }
                }
            };
        }

        private Border CreateSearchSlideResultCard(UserInfo user)
        {
            var card = new Border
            {
                Background = GetThemeBrush("MutedCardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(18),
                Padding = new Thickness(16),
                Margin = new Thickness(0, 0, 0, 12)
            };

            var layout = new Grid();
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            layout.Children.Add(new Border
            {
                Width = 52,
                Height = 52,
                CornerRadius = new CornerRadius(26),
                ClipToBounds = true,
                Margin = new Thickness(0, 0, 14, 0),
                Child = CreateUserAvatarVisual(user, 52, true)
            });

            var info = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center
            };
            info.Children.Add(CreateHighlightedSearchTextBlock(
                string.IsNullOrWhiteSpace(user.Name) ? "Sem nome" : user.Name,
                GetThemeBrush("PrimaryTextBrush"),
                14,
                FontWeights.SemiBold));
            var emailBlock = CreateHighlightedSearchTextBlock(
                string.IsNullOrWhiteSpace(user.Email) ? "Email indisponivel" : user.Email,
                GetThemeBrush("SecondaryTextBrush"),
                11,
                FontWeights.Normal);
            emailBlock.Margin = new Thickness(0, 4, 0, 0);
            info.Children.Add(emailBlock);

            var registrationBlock = new TextBlock
            {
                Margin = new Thickness(0, 2, 0, 0),
                FontSize = 11,
                Foreground = GetThemeBrush("TertiaryTextBrush")
            };
            registrationBlock.Inlines.Add(new Run("Matricula: "));
            AppendHighlightedInlines(registrationBlock.Inlines, !string.IsNullOrWhiteSpace(user.Registration) ? user.Registration : "nao informada", GetThemeBrush("TertiaryTextBrush"));
            info.Children.Add(registrationBlock);
            if (!string.IsNullOrWhiteSpace(user.Course))
            {
                var courseBlock = CreateHighlightedSearchTextBlock(user.Course, GetThemeBrush("TertiaryTextBrush"), 11, FontWeights.Normal);
                courseBlock.Margin = new Thickness(0, 2, 0, 0);
                info.Children.Add(courseBlock);
            }
            Grid.SetColumn(info, 1);
            layout.Children.Add(info);

            var actions = new StackPanel
            {
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(14, 0, 0, 0)
            };

            var conversationButton = new Button
            {
                Content = "Conversar",
                Background = GetThemeBrush("AccentBrush"),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(14, 8, 14, 8),
                Margin = new Thickness(0, 0, 0, 8),
                Cursor = Cursors.Hand,
                FontWeight = FontWeights.SemiBold,
                Tag = user
            };
            conversationButton.Click += SearchSlideStartConversation_Click;
            actions.Children.Add(conversationButton);

            var detailsButton = new Button
            {
                Content = "Detalhes",
                Background = GetThemeBrush("CardBackgroundBrush"),
                Foreground = GetThemeBrush("PrimaryTextBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(14, 8, 14, 8),
                Margin = new Thickness(0, 0, 0, 8),
                Cursor = Cursors.Hand,
                FontWeight = FontWeights.SemiBold,
                Tag = user
            };
            detailsButton.Click += SearchSlideShowProfile_Click;
            actions.Children.Add(detailsButton);

            var connectionButton = new Button
            {
                Content = user.ConnectionButtonLabel,
                Background = GetThemeBrush("CardBackgroundBrush"),
                Foreground = GetThemeBrush("PrimaryTextBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(14, 8, 14, 8),
                Cursor = Cursors.Hand,
                FontWeight = FontWeights.SemiBold,
                IsEnabled = user.CanCreateConnection,
                Tag = user
            };
            connectionButton.Click += SearchSlideCreateConnection_Click;
            actions.Children.Add(connectionButton);

            Grid.SetColumn(actions, 2);
            layout.Children.Add(actions);

            card.Child = layout;
            return card;
        }

        private TextBlock CreateHighlightedSearchTextBlock(string text, Brush baseForeground, double fontSize, FontWeight fontWeight)
        {
            var block = new TextBlock
            {
                FontSize = fontSize,
                FontWeight = fontWeight,
                Foreground = baseForeground,
                TextWrapping = TextWrapping.Wrap
            };
            AppendHighlightedInlines(block.Inlines, text, baseForeground);
            return block;
        }

        private void AppendHighlightedInlines(InlineCollection inlines, string text, Brush baseForeground)
        {
            var value = text ?? string.Empty;
            var query = _searchSlideQuery?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(query) || string.IsNullOrWhiteSpace(value))
            {
                inlines.Add(new Run(value) { Foreground = baseForeground });
                return;
            }

            var comparison = StringComparison.OrdinalIgnoreCase;
            var startIndex = 0;
            while (startIndex < value.Length)
            {
                var matchIndex = value.IndexOf(query, startIndex, comparison);
                if (matchIndex < 0)
                {
                    inlines.Add(new Run(value[startIndex..]) { Foreground = baseForeground });
                    break;
                }

                if (matchIndex > startIndex)
                {
                    inlines.Add(new Run(value[startIndex..matchIndex]) { Foreground = baseForeground });
                }

                inlines.Add(new Run(value.Substring(matchIndex, query.Length))
                {
                    Foreground = GetSearchHighlightForeground(),
                    Background = GetSearchHighlightBackground(),
                    FontWeight = FontWeights.Bold
                });
                startIndex = matchIndex + query.Length;
            }
        }

        private Brush GetSearchHighlightBackground()
        {
            return _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(30, 64, 175))
                : new SolidColorBrush(Color.FromRgb(219, 234, 254));
        }

        private Brush GetSearchHighlightForeground()
        {
            return _appDarkModeEnabled
                ? Brushes.White
                : new SolidColorBrush(Color.FromRgb(30, 64, 175));
        }

        private async void SearchSlideCreateConnection_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: UserInfo user } || _connectionService == null)
            {
                return;
            }

            user.IsConnecting = true;
            RenderSearchSlideResults(_searchSlideResults);

            try
            {
                var result = await _connectionService.CreateConnectionRequestAsync(user);
                if (!result.Success)
                {
                    SearchSlideStatusText.Text = $"Falha ao enviar conexao para {user.Name}: {result.ErrorMessage}";
                    SearchSlideStatusText.Foreground = new SolidColorBrush(Color.FromRgb(220, 38, 38));
                    return;
                }

                user.IsConnecting = false;
                user.ConnectionState = "pendingOutgoing";
                await RefreshConnectionsCacheAsync();
                SearchSlideStatusText.Text = $"Solicitacao enviada para {user.Name}.";
                SearchSlideStatusText.Foreground = new SolidColorBrush(Color.FromRgb(21, 128, 61));
                RenderSearchSlideResults(_searchSlideResults);
            }
            catch (Exception ex)
            {
                user.IsConnecting = false;
                SearchSlideStatusText.Text = $"Erro ao criar conexao: {ex.Message}";
                SearchSlideStatusText.Foreground = new SolidColorBrush(Color.FromRgb(220, 38, 38));
                RenderSearchSlideResults(_searchSlideResults);
            }
        }

        private void SearchSlideStartConversation_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: UserInfo user })
            {
                return;
            }

            DebugHelper.WriteLine($"[MainWindow] Conversa iniciada pelo painel inline com: {user.Name}");
            ShowConversationInMainWindow(user);
            SearchFriendsBox.Clear();
            HideSearchSlidePanel();
        }

        private async void SearchSlideShowProfile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: UserInfo user })
            {
                return;
            }

            await ShowUserProfileDialogAsync(user);
        }

        private void CloseSearchSlide_Click(object sender, RoutedEventArgs e)
        {
            HideSearchSlidePanel();
        }

        private void ShowSearchSlidePanel()
        {
            SearchSlideOverlay.Visibility = Visibility.Visible;
            SearchSlidePanel.Visibility = Visibility.Visible;

            var opacityAnimation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(180));
            SearchSlidePanel.BeginAnimation(OpacityProperty, opacityAnimation);

            if (SearchSlidePanel.RenderTransform is TranslateTransform transform)
            {
                transform.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(56, 0, TimeSpan.FromMilliseconds(220)));
            }
        }

        private void HideSearchSlidePanel()
        {
            if (SearchSlideOverlay.Visibility != Visibility.Visible)
            {
                return;
            }

            var opacityAnimation = new DoubleAnimation(0, TimeSpan.FromMilliseconds(160));
            opacityAnimation.Completed += (s, e) =>
            {
                SearchSlideOverlay.Visibility = Visibility.Collapsed;
                SearchSlidePanel.Visibility = Visibility.Collapsed;
                SearchSlideResultsHost.Children.Clear();
                SearchSlideEmptyStateText.Visibility = Visibility.Collapsed;
            };
            SearchSlidePanel.BeginAnimation(OpacityProperty, opacityAnimation);

            if (SearchSlidePanel.RenderTransform is TranslateTransform transform)
            {
                transform.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(56, TimeSpan.FromMilliseconds(160)));
            }
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
                    case "Conexoes":
                        ConnectionsContent.Visibility = Visibility.Visible;
                        ConnectionsStatusText.Text = "Atualizando conexões...";
                        await RefreshConnectionsCacheAsync();
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
            ConnectionsContent.Visibility = Visibility.Collapsed;
            TeamsContent.Visibility = Visibility.Collapsed;
            CalendarContent.Visibility = Visibility.Collapsed;
            FilesContent.Visibility = Visibility.Collapsed;
            SettingsContent.Visibility = Visibility.Collapsed;
            HideSearchSlidePanel();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ShowLogoutConfirmationDialog())
            {
                return;
            }

            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }

        private bool ShowLogoutConfirmationDialog()
        {
            var dialog = new Window
            {
                Title = "Sair da conta",
                Width = 460,
                Height = 280,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = Brushes.Transparent,
                ShowInTaskbar = false
            };

            var outerBorder = new Border
            {
                Background = GetThemeBrush("SurfaceBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(24),
                Padding = new Thickness(24),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    BlurRadius = 24,
                    ShadowDepth = 8,
                    Opacity = 0.24,
                    Color = Colors.Black
                }
            };

            var root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var header = new Grid { Margin = new Thickness(0, 0, 0, 18) };
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var iconHost = new Border
            {
                Width = 56,
                Height = 56,
                CornerRadius = new CornerRadius(18),
                Background = _appDarkModeEnabled
                    ? new SolidColorBrush(Color.FromRgb(59, 24, 24))
                    : new SolidColorBrush(Color.FromRgb(254, 242, 242)),
                Child = new TextBlock
                {
                    Text = "↩",
                    FontSize = 24,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = _appDarkModeEnabled
                        ? new SolidColorBrush(Color.FromRgb(252, 165, 165))
                        : new SolidColorBrush(Color.FromRgb(220, 38, 38))
                }
            };
            header.Children.Add(iconHost);

            var titleStack = new StackPanel
            {
                Margin = new Thickness(16, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            titleStack.Children.Add(new TextBlock
            {
                Text = "Encerrar sessão",
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            });
            titleStack.Children.Add(new TextBlock
            {
                Text = "Você voltará para a tela de login imediatamente.",
                Margin = new Thickness(0, 6, 0, 0),
                FontSize = 12,
                Foreground = GetThemeBrush("SecondaryTextBrush")
            });
            Grid.SetColumn(titleStack, 1);
            header.Children.Add(titleStack);
            root.Children.Add(header);

            var body = new StackPanel();
            body.Children.Add(new TextBlock
            {
                Text = "Deseja realmente sair da conta atual?",
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                Foreground = GetThemeBrush("PrimaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            });
            body.Children.Add(new TextBlock
            {
                Text = "Suas informações permanecem salvas, mas será necessário fazer login novamente para acessar conversas, conexões e equipes.",
                Margin = new Thickness(0, 10, 0, 0),
                FontSize = 12,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            });
            body.Children.Add(new Border
            {
                Margin = new Thickness(0, 18, 0, 0),
                Padding = new Thickness(14, 12, 14, 12),
                CornerRadius = new CornerRadius(16),
                Background = GetThemeBrush("MutedCardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                Child = new TextBlock
                {
                    Text = "Nenhuma alteração do seu perfil será perdida.",
                    FontSize = 11,
                    Foreground = GetThemeBrush("SecondaryTextBrush")
                }
            });
            Grid.SetRow(body, 1);
            root.Children.Add(body);

            var footer = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 22, 0, 0)
            };

            var cancelButton = new Button
            {
                Content = "Cancelar",
                Width = 118,
                Height = 42,
                Margin = new Thickness(0, 0, 10, 0),
                Background = GetThemeBrush("MutedCardBackgroundBrush"),
                Foreground = GetThemeBrush("PrimaryTextBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                Cursor = Cursors.Hand,
                FontWeight = FontWeights.SemiBold
            };
            cancelButton.Click += (_, __) =>
            {
                dialog.DialogResult = false;
                dialog.Close();
            };

            var confirmButton = new Button
            {
                Content = "Sair agora",
                Width = 118,
                Height = 42,
                Background = new SolidColorBrush(Color.FromRgb(220, 38, 38)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                FontWeight = FontWeights.Bold
            };
            confirmButton.Click += (_, __) =>
            {
                dialog.DialogResult = true;
                dialog.Close();
            };

            footer.Children.Add(cancelButton);
            footer.Children.Add(confirmButton);
            Grid.SetRow(footer, 2);
            root.Children.Add(footer);

            dialog.KeyDown += (_, args) =>
            {
                if (args.Key == Key.Escape)
                {
                    dialog.DialogResult = false;
                    dialog.Close();
                }
            };

            outerBorder.Child = root;
            dialog.Content = outerBorder;

            return dialog.ShowDialog() == true;
        }

        private void ChatDarkModeToggle_Changed(object sender, RoutedEventArgs e)
        {
            _appDarkModeEnabled = ChatDarkModeToggle.IsChecked == true;
            ApplyAppTheme();

            if (ChatsContent.Visibility == Visibility.Visible)
            {
                RefreshChatsUI();
            }

            if (TeamsContent.Visibility == Visibility.Visible)
            {
                RenderTeamMembersDraft();
                RenderTeamUcsDraft();
                RenderTeamsList();
                RenderTeamWorkspace();
            }

            if (ConnectionsContent.Visibility == Visibility.Visible)
            {
                RenderConnectionsView();
            }

            if (SearchSlidePanel.Visibility == Visibility.Visible)
            {
                RenderSearchSlideResults(_searchSlideResults);
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
            NormalizeProfileAvatarSelection(_currentProfile);

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
                    avatarBody = new { stringValue = profile.AvatarBody },
                    avatarHair = new { stringValue = profile.AvatarHair },
                    avatarHat = new { stringValue = profile.AvatarHat },
                    avatarAccessory = new { stringValue = profile.AvatarAccessory },
                    avatarClothing = new { stringValue = profile.AvatarClothing },
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
            UpdateConnectionsBadge();
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
                        ContactAvatarBody = contactUser.AvatarBody,
                        ContactAvatarHair = contactUser.AvatarHair,
                        ContactAvatarHat = contactUser.AvatarHat,
                        ContactAvatarAccessory = contactUser.AvatarAccessory,
                        ContactAvatarClothing = contactUser.AvatarClothing,
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

                var shellGrid = new Grid();
                shellGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(360) });
                shellGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) });
                shellGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var sidebar = CreateConversationsSidebar();
                Grid.SetColumn(sidebar, 0);
                shellGrid.Children.Add(sidebar);

                var workspace = _selectedConversation == null
                    ? CreateChatsWelcomePanel()
                    : CreateChatWorkspace(_selectedConversation);
                Grid.SetColumn(workspace, 2);
                shellGrid.Children.Add(workspace);

                ChatsContent.Children.Add(shellGrid);
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[RefreshChatsUI ERROR] {ex.Message}");
            }
        }

        private IEnumerable<Conversation> GetVisibleConversations()
        {
            var conversations = _conversations
                .OrderByDescending(conversation => conversation.LastMessageTime == default ? DateTime.MinValue : conversation.LastMessageTime)
                .AsEnumerable();

            if (string.IsNullOrWhiteSpace(_chatListFilter))
            {
                return conversations;
            }

            return conversations.Where(conversation =>
                conversation.ContactName.Contains(_chatListFilter, StringComparison.OrdinalIgnoreCase) ||
                conversation.LastMessage.Contains(_chatListFilter, StringComparison.OrdinalIgnoreCase));
        }

        private Border CreateConversationsSidebar(bool isLoading = false)
        {
            var cardBackground = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(17, 24, 39))
                : new SolidColorBrush(Colors.White);
            var borderBrush = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(51, 65, 85))
                : new SolidColorBrush(Color.FromRgb(226, 232, 240));
            var titleBrush = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(241, 245, 249))
                : new SolidColorBrush(Color.FromRgb(15, 23, 42));
            var subtitleBrush = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(148, 163, 184))
                : new SolidColorBrush(Color.FromRgb(100, 116, 139));
            var searchBackground = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(30, 41, 59))
                : new SolidColorBrush(Color.FromRgb(248, 250, 252));
            var searchForeground = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(226, 232, 240))
                : new SolidColorBrush(Color.FromRgb(30, 41, 59));
            var unreadCount = _conversations.Count(conversation => conversation.HasUnread);

            var sidebar = new Border
            {
                Background = cardBackground,
                BorderBrush = borderBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(24)
            };

            var layout = new Grid();
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            layout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var header = new Border
            {
                Padding = new Thickness(22, 22, 22, 18),
                BorderBrush = borderBrush,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Child = new StackPanel
                {
                    Children =
                    {
                        new TextBlock
                        {
                            Text = "Conversas",
                            FontSize = 22,
                            FontWeight = FontWeights.ExtraBold,
                            Foreground = titleBrush
                        },
                        new TextBlock
                        {
                            Text = isLoading
                                ? "Sincronizando historico recente..."
                                : unreadCount > 0
                                    ? $"{_conversations.Count} conversa(s) | {unreadCount} com novidades"
                                    : $"{_conversations.Count} conversa(s) sincronizada(s)",
                            FontSize = 12,
                            Margin = new Thickness(0, 6, 0, 0),
                            Foreground = subtitleBrush
                        }
                    }
                }
            };
            layout.Children.Add(header);
            Grid.SetRow(header, 0);

            var searchShell = new Border
            {
                Margin = new Thickness(18, 16, 18, 12),
                Background = searchBackground,
                BorderBrush = borderBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(18),
                Padding = new Thickness(14, 10, 14, 10)
            };

            var searchGrid = new Grid();
            searchGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            searchGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            searchGrid.Children.Add(new TextBlock
            {
                Text = "Buscar",
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = subtitleBrush,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            });

            var searchBox = new TextBox
            {
                Text = _chatListFilter,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = searchForeground,
                FontSize = 12,
                VerticalAlignment = VerticalAlignment.Center
            };
            searchBox.TextChanged += (sender, args) =>
            {
                if (isLoading)
                {
                    return;
                }

                var text = searchBox.Text?.Trim() ?? string.Empty;
                if (string.Equals(text, _chatListFilter, StringComparison.Ordinal))
                {
                    return;
                }

                _chatListFilter = text;
                RefreshChatsUI();
            };

            Grid.SetColumn(searchBox, 1);
            searchGrid.Children.Add(searchBox);
            searchShell.Child = searchGrid;
            layout.Children.Add(searchShell);
            Grid.SetRow(searchShell, 1);

            var summaryStrip = new Border
            {
                Margin = new Thickness(18, 0, 18, 12),
                Padding = new Thickness(14, 10, 14, 10),
                Background = _appDarkModeEnabled
                    ? new SolidColorBrush(Color.FromRgb(15, 23, 42))
                    : new SolidColorBrush(Color.FromRgb(248, 250, 252)),
                BorderBrush = borderBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(16)
            };

            var summaryGrid = new Grid();
            summaryGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            summaryGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            summaryGrid.Children.Add(new TextBlock
            {
                Text = isLoading ? "Atualizando lista..." : "Conversas ativas",
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = titleBrush,
                VerticalAlignment = VerticalAlignment.Center
            });

            var summaryBadge = new Border
            {
                Background = unreadCount > 0
                    ? new SolidColorBrush(Color.FromRgb(0, 168, 132))
                    : new SolidColorBrush(Color.FromRgb(148, 163, 184)),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(10, 4, 10, 4),
                Child = new TextBlock
                {
                    Text = isLoading ? "..." : unreadCount > 0 ? $"{unreadCount} novas" : "em dia",
                    FontSize = 10,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White
                }
            };
            Grid.SetColumn(summaryBadge, 1);
            summaryGrid.Children.Add(summaryBadge);

            summaryStrip.Child = summaryGrid;
            layout.Children.Add(summaryStrip);
            Grid.SetRow(summaryStrip, 2);

            var listStack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(12, 0, 12, 12)
            };

            if (isLoading)
            {
                listStack.Children.Add(CreateConversationPlaceholderCard("Carregando conversas recentes"));
                listStack.Children.Add(CreateConversationPlaceholderCard("Atualizando ultimas mensagens"));
                listStack.Children.Add(CreateConversationPlaceholderCard("Sincronizando contatos"));
            }
            else
            {
                var visibleConversations = GetVisibleConversations().ToList();

                if (visibleConversations.Count == 0)
                {
                    listStack.Children.Add(new Border
                    {
                        Background = _appDarkModeEnabled
                            ? new SolidColorBrush(Color.FromRgb(15, 23, 42))
                            : new SolidColorBrush(Color.FromRgb(248, 250, 252)),
                        BorderBrush = borderBrush,
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(18),
                        Padding = new Thickness(18),
                        Child = new TextBlock
                        {
                            Text = string.IsNullOrWhiteSpace(_chatListFilter)
                                ? "As conversas recentes vao aparecer aqui assim que novas mensagens forem sincronizadas."
                                : "Nenhuma conversa combina com esse filtro.",
                            FontSize = 12,
                            TextWrapping = TextWrapping.Wrap,
                            Foreground = subtitleBrush
                        }
                    });
                }

                foreach (var conversation in visibleConversations)
                {
                    listStack.Children.Add(CreateConversationButton(conversation));
                }
            }

            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = listStack
            };
            layout.Children.Add(scrollViewer);
            Grid.SetRow(scrollViewer, 3);

            sidebar.Child = layout;
            return sidebar;
        }

        private Border CreateConversationButton(Conversation conv)
        {
            var isSelected = _selectedConversation?.ContactId == conv.ContactId;
            var background = _appDarkModeEnabled
                ? isSelected
                    ? new SolidColorBrush(Color.FromRgb(15, 43, 64))
                    : conv.HasUnread
                        ? new SolidColorBrush(Color.FromRgb(22, 31, 49))
                        : new SolidColorBrush(Color.FromRgb(17, 24, 39))
                : isSelected
                    ? new SolidColorBrush(Color.FromRgb(232, 245, 233))
                    : conv.HasUnread
                        ? new SolidColorBrush(Color.FromRgb(245, 250, 255))
                        : new SolidColorBrush(Colors.White);
            var borderBrush = _appDarkModeEnabled
                ? isSelected
                    ? new SolidColorBrush(Color.FromRgb(45, 212, 191))
                    : conv.HasUnread
                        ? new SolidColorBrush(Color.FromRgb(56, 189, 248))
                        : new SolidColorBrush(Color.FromRgb(51, 65, 85))
                : isSelected
                    ? new SolidColorBrush(Color.FromRgb(134, 239, 172))
                    : conv.HasUnread
                        ? new SolidColorBrush(Color.FromRgb(191, 219, 254))
                        : new SolidColorBrush(Color.FromRgb(232, 232, 232));
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
            avatarGrid.Children.Add(CreateConversationAvatarVisual(conv, 48));

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
                Text = string.IsNullOrWhiteSpace(conv.LastMessage)
                    ? "Nenhuma mensagem ainda"
                    : conv.HasUnread ? $"• {conv.LastMessage}" : conv.LastMessage,
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
                    Background = new SolidColorBrush(Color.FromRgb(0, 168, 132)),
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(8, 3, 8, 3),
                    Margin = new Thickness(0, 8, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Child = new TextBlock
                    {
                        Text = "Nova",
                        Foreground = Brushes.White,
                        FontSize = 9,
                        FontWeight = FontWeights.Bold
                    }
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

        private Border CreateConversationPlaceholderCard(string title)
        {
            var cardBackground = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(15, 23, 42))
                : new SolidColorBrush(Color.FromRgb(248, 250, 252));
            var borderBrush = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(51, 65, 85))
                : new SolidColorBrush(Color.FromRgb(226, 232, 240));
            var textBrush = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(148, 163, 184))
                : new SolidColorBrush(Color.FromRgb(100, 116, 139));

            return new Border
            {
                Background = cardBackground,
                BorderBrush = borderBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(18),
                Padding = new Thickness(18),
                Margin = new Thickness(0, 0, 0, 10),
                Child = new TextBlock
                {
                    Text = title,
                    FontSize = 12,
                    Foreground = textBrush
                }
            };
        }

        private Border CreateChatsWelcomePanel()
        {
            var panelBackground = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(17, 27, 33))
                : new SolidColorBrush(Colors.White);
            var headerBorderBrush = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(32, 44, 51))
                : new SolidColorBrush(Color.FromRgb(226, 232, 240));
            var primaryText = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(233, 237, 239))
                : new SolidColorBrush(Color.FromRgb(15, 23, 42));
            var secondaryText = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(134, 150, 160))
                : new SolidColorBrush(Color.FromRgb(100, 116, 139));
            var mutedBackground = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(15, 23, 42))
                : new SolidColorBrush(Color.FromRgb(248, 250, 252));

            var panel = new Border
            {
                Background = panelBackground,
                BorderBrush = headerBorderBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(24),
                Child = new Grid
                {
                    Children =
                    {
                        new StackPanel
                        {
                            Width = 420,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            Children =
                            {
                                new Border
                                {
                                    Width = 86,
                                    Height = 86,
                                    CornerRadius = new CornerRadius(43),
                                    Background = new SolidColorBrush(Color.FromRgb(0, 168, 132)),
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    Child = new TextBlock
                                    {
                                        Text = "Chat",
                                        Foreground = Brushes.White,
                                        FontSize = 20,
                                        FontWeight = FontWeights.Bold,
                                        HorizontalAlignment = HorizontalAlignment.Center,
                                        VerticalAlignment = VerticalAlignment.Center
                                    }
                                },
                                new TextBlock
                                {
                                    Text = "Selecione uma conversa para abrir o painel principal",
                                    FontSize = 22,
                                    FontWeight = FontWeights.ExtraBold,
                                    Foreground = primaryText,
                                    Margin = new Thickness(0, 22, 0, 10),
                                    TextAlignment = TextAlignment.Center,
                                    TextWrapping = TextWrapping.Wrap
                                },
                                new TextBlock
                                {
                                    Text = "A lista de conversas fica sempre visivel na esquerda, e o painel branco da direita abre a conversa ativa sem tirar voce do contexto.",
                                    FontSize = 13,
                                    Foreground = secondaryText,
                                    TextAlignment = TextAlignment.Center,
                                    TextWrapping = TextWrapping.Wrap,
                                    Margin = new Thickness(0, 0, 0, 24)
                                },
                                new Border
                                {
                                    Background = mutedBackground,
                                    BorderBrush = headerBorderBrush,
                                    BorderThickness = new Thickness(1),
                                    CornerRadius = new CornerRadius(18),
                                    Padding = new Thickness(18),
                                    Child = new TextBlock
                                    {
                                        Text = "Use a busca do topo para iniciar novas conversas e acompanhe mensagens, anexos e acoes sem voltar para outra tela.",
                                        FontSize = 12,
                                        Foreground = secondaryText,
                                        TextWrapping = TextWrapping.Wrap,
                                        TextAlignment = TextAlignment.Center
                                    }
                                }
                            }
                        }
                    }
                }
            };

            return panel;
        }

        private Border CreateChatWorkspace(Conversation conv)
        {
            var panelBackground = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(17, 27, 33))
                : new SolidColorBrush(Colors.White);
            var borderBrush = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(32, 44, 51))
                : new SolidColorBrush(Color.FromRgb(226, 232, 240));
            var primaryText = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(233, 237, 239))
                : new SolidColorBrush(Color.FromRgb(15, 23, 42));
            var secondaryText = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(134, 150, 160))
                : new SolidColorBrush(Color.FromRgb(100, 116, 139));
            var canvasBackground = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(12, 19, 24))
                : new SolidColorBrush(Color.FromRgb(248, 250, 252));
            var composerBackground = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(17, 27, 33))
                : new SolidColorBrush(Color.FromRgb(255, 255, 255));
            var inputBackground = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(47, 61, 69))
                : new SolidColorBrush(Color.FromRgb(241, 245, 249));
            var inputForeground = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(233, 237, 239))
                : new SolidColorBrush(Color.FromRgb(15, 23, 42));

            var panel = new Border
            {
                Background = panelBackground,
                BorderBrush = borderBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(24)
            };

            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var headerBorder = new Border
            {
                BorderBrush = borderBrush,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(18, 16, 18, 16)
            };

            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var avatarGrid = new Grid
            {
                Width = 46,
                Height = 46,
                Margin = new Thickness(0, 0, 14, 0)
            };
            avatarGrid.Children.Add(CreateConversationAvatarVisual(conv, 46));
            Grid.SetColumn(avatarGrid, 0);
            headerGrid.Children.Add(avatarGrid);

            var titleStack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Center
            };
            titleStack.Children.Add(new TextBlock
            {
                Text = conv.ContactName,
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                Foreground = primaryText
            });
            titleStack.Children.Add(new TextBlock
            {
                Text = conv.Messages.Count > 0
                    ? $"Ultima atividade {conv.FormattedTime}"
                    : "Conversa pronta para receber novas mensagens",
                FontSize = 11,
                Margin = new Thickness(0, 3, 0, 0),
                Foreground = secondaryText
            });
            Grid.SetColumn(titleStack, 1);
            headerGrid.Children.Add(titleStack);

            var actionPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };
            actionPanel.Children.Add(CreateHeaderIconButton("📞", "Iniciar audio"));
            actionPanel.Children.Add(CreateHeaderIconButton("🎥", "Iniciar video"));
            actionPanel.Children.Add(CreateHeaderIconButton("🔎", "Buscar na conversa"));

            var menuButton = CreateHeaderIconButton("⋯", "Mais acoes");
            var actionsPopup = CreateChatActionsPopup(menuButton, conv);
            menuButton.Click += (s, e) => actionsPopup.IsOpen = !actionsPopup.IsOpen;
            actionPanel.Children.Add(menuButton);

            Grid.SetColumn(actionPanel, 2);
            headerGrid.Children.Add(actionPanel);

            headerBorder.Child = headerGrid;
            mainGrid.Children.Add(headerBorder);
            Grid.SetRow(headerBorder, 0);

            var quickToolsBorder = new Border
            {
                Background = _appDarkModeEnabled
                    ? new SolidColorBrush(Color.FromRgb(15, 23, 42))
                    : new SolidColorBrush(Color.FromRgb(248, 250, 252)),
                BorderBrush = borderBrush,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(18, 12, 18, 12)
            };

            var quickTools = new WrapPanel();
            var filesButton = CreateComposerChipButton("Arquivos");
            filesButton.Click += (s, e) => MessageBox.Show(
                $"A central de arquivos da conversa com {conv.ContactName} sera conectada aqui.",
                "Arquivos da conversa",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            quickTools.Children.Add(filesButton);

            var mediaButton = CreateComposerChipButton("Midia");
            mediaButton.Click += (s, e) => MessageBox.Show(
                $"O historico de midia compartilhada com {conv.ContactName} aparecera aqui.",
                "Midia compartilhada",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            quickTools.Children.Add(mediaButton);

            var mentionButton = CreateComposerChipButton("@Professor");
            mentionButton.Click += (s, e) => MessageBox.Show(
                $"Voce podera chamar o professor orientador direto desta conversa com {conv.ContactName}.",
                "Contato academico",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            quickTools.Children.Add(mentionButton);

            var exportButton = CreateComposerChipButton("Exportar");
            exportButton.Click += (s, e) => MessageBox.Show(
                $"A exportacao do historico com {conv.ContactName} sera iniciada nesta area.",
                "Exportar conversa",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            quickTools.Children.Add(exportButton);

            quickToolsBorder.Child = quickTools;
            mainGrid.Children.Add(quickToolsBorder);
            Grid.SetRow(quickToolsBorder, 1);

            var messagesHost = new Border
            {
                Background = canvasBackground
            };

            if (conv.Messages.Count == 0)
            {
                messagesHost.Child = new StackPanel
                {
                    Width = 360,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = "Nenhuma mensagem por aqui ainda",
                            FontSize = 20,
                            FontWeight = FontWeights.Bold,
                            Foreground = primaryText,
                            TextAlignment = TextAlignment.Center
                        },
                        new TextBlock
                        {
                            Text = "Envie a primeira mensagem e mantenha a conversa aberta enquanto acessa as acoes principais acima.",
                            FontSize = 12,
                            Margin = new Thickness(0, 10, 0, 0),
                            Foreground = secondaryText,
                            TextWrapping = TextWrapping.Wrap,
                            TextAlignment = TextAlignment.Center
                        }
                    }
                };
            }
            else
            {
                var messagesList = new StackPanel { Orientation = Orientation.Vertical };
                foreach (var msg in conv.Messages.OrderBy(message => message.Timestamp))
                {
                    messagesList.Children.Add(CreateMessageBubble(msg));
                }

                var scrollViewer = new ScrollViewer
                {
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    Content = messagesList,
                    Padding = new Thickness(18, 20, 18, 20),
                    Background = canvasBackground
                };
                scrollViewer.Loaded += (s, e) => scrollViewer.ScrollToEnd();
                messagesHost.Child = scrollViewer;
            }

            mainGrid.Children.Add(messagesHost);
            Grid.SetRow(messagesHost, 2);

            var inputBorder = new Border
            {
                Background = composerBackground,
                BorderBrush = borderBrush,
                BorderThickness = new Thickness(0, 1, 0, 0),
                Padding = new Thickness(16, 14, 16, 16)
            };

            var composerStack = new StackPanel();
            var quickActionPanel = new WrapPanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            quickActionPanel.Children.Add(CreateComposerChipButton("✨ Ações rápidas"));
            quickActionPanel.Children.Add(CreateComposerChipButton("🖼️ Midia"));
            quickActionPanel.Children.Add(CreateComposerChipButton("📎 Arquivo"));
            quickActionPanel.Children.Add(CreateComposerChipButton("🎓 Professor"));
            quickActionPanel.Children.Add(CreateComposerChipButton("📤 Exportar"));
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

            var attachmentButton = CreateHeaderIconButton("＋", "Enviar figurinha");
            attachmentButton.Background = new SolidColorBrush(Color.FromRgb(0, 168, 132));
            attachmentButton.Foreground = Brushes.White;
            attachmentButton.MinWidth = 40;
            attachmentButton.Height = 40;
            attachmentButton.FontSize = 16;
            var stickerPopup = CreateStickerPickerPopup(attachmentButton, conv);
            attachmentButton.Click += (s, e) => stickerPopup.IsOpen = !stickerPopup.IsOpen;
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
            inputBox.KeyDown += async (s, e) =>
            {
                if (e.Key == Key.Enter && Keyboard.Modifiers != ModifierKeys.Shift)
                {
                    e.Handled = true;
                    await SendConversationMessageAsync(conv, inputBox);
                }
            };
            Grid.SetColumn(inputBox, 2);
            inputGrid.Children.Add(inputBox);

            var micButton = CreateHeaderIconButton("🎙", "Gravar audio");
            Grid.SetColumn(micButton, 3);
            inputGrid.Children.Add(micButton);

            var sendButton = new Button
            {
                Content = "➤",
                Background = new SolidColorBrush(Color.FromRgb(0, 168, 132)),
                Foreground = Brushes.White,
                FontSize = 16,
                Width = 52,
                Height = 42,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                ToolTip = "Enviar mensagem"
            };
            sendButton.Click += async (s, e) => await SendConversationMessageAsync(conv, inputBox);
            Grid.SetColumn(sendButton, 4);
            inputGrid.Children.Add(sendButton);

            composerStack.Children.Add(inputGrid);
            inputBorder.Child = composerStack;
            mainGrid.Children.Add(inputBorder);
            Grid.SetRow(inputBorder, 3);

            panel.Child = mainGrid;
            return panel;
        }

        private async Task SendConversationStickerAsync(Conversation conv, string stickerAsset)
        {
            if (string.IsNullOrWhiteSpace(stickerAsset))
            {
                return;
            }

            var previewText = GetStickerPreviewText(stickerAsset);
            var stickerMessage = new ChatMessage
            {
                SenderId = _currentProfile?.UserId ?? "self",
                SenderName = _currentProfile?.Name ?? "Voce",
                Content = previewText,
                MessageType = "sticker",
                StickerAsset = stickerAsset,
                Timestamp = DateTime.Now,
                IsOwn = true
            };

            if (!string.IsNullOrEmpty(_idToken))
            {
                var chatService = new ChatService(_idToken, _currentProfile?.UserId ?? "");
                var sendResult = await chatService.SendMessageAsync(conv.ContactId, conv.ContactName, stickerMessage.SenderName, stickerMessage.Content, stickerMessage.MessageType, stickerMessage.StickerAsset);
                if (!sendResult.Success)
                {
                    MessageBox.Show(
                        $"Erro ao enviar figurinha.\n\n{sendResult.ErrorMessage}\n\nLog salvo em:\n{DebugHelper.GetLogFilePath()}",
                        "Erro",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }
            }

            conv.Messages.Add(stickerMessage);
            conv.LastMessage = stickerMessage.ConversationPreview;
            conv.LastMessageTime = stickerMessage.Timestamp;
            conv.LastSenderId = stickerMessage.SenderId;
            conv.LastReadAt = stickerMessage.Timestamp;
            conv.HasUnread = false;
            UpdateChatsBadge();
            RefreshChatsUI();
        }

        private async Task SendConversationMessageAsync(Conversation conv, TextBox inputBox)
        {
            var messageText = inputBox.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(messageText))
            {
                return;
            }

            var newMsg = new ChatMessage
            {
                SenderId = _currentProfile?.UserId ?? "self",
                SenderName = _currentProfile?.Name ?? "Voce",
                Content = messageText,
                MessageType = "text",
                Timestamp = DateTime.Now,
                IsOwn = true
            };

            if (!string.IsNullOrEmpty(_idToken))
            {
                DebugHelper.WriteLine("[SendConversationMessageAsync] Enviando mensagem para Firebase");
                var chatService = new ChatService(_idToken, _currentProfile?.UserId ?? "");
                var sendResult = await chatService.SendMessageAsync(conv.ContactId, conv.ContactName, newMsg.SenderName, newMsg.Content, newMsg.MessageType, newMsg.StickerAsset);

                if (!sendResult.Success)
                {
                    MessageBox.Show(
                        $"Erro ao enviar mensagem.\n\n{sendResult.ErrorMessage}\n\nLog salvo em:\n{DebugHelper.GetLogFilePath()}",
                        "Erro",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    DebugHelper.WriteLine($"[SendConversationMessageAsync] Falha ao salvar no Firebase: {sendResult.ErrorMessage}");
                    return;
                }

                DebugHelper.WriteLine("[SendConversationMessageAsync] Mensagem salva no Firebase");
            }

            conv.Messages.Add(newMsg);
            conv.LastMessage = newMsg.ConversationPreview;
            conv.LastMessageTime = newMsg.Timestamp;
            conv.LastSenderId = newMsg.SenderId;
            conv.LastReadAt = newMsg.Timestamp;
            conv.HasUnread = false;

            UpdateChatsBadge();
            inputBox.Clear();
            RefreshChatsUI();
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

            var row = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = msg.IsOwn ? HorizontalAlignment.Right : HorizontalAlignment.Left
            };

            if (!msg.IsOwn)
            {
                row.Children.Add(new Border
                {
                    Width = 36,
                    Height = 36,
                    CornerRadius = new CornerRadius(18),
                    Margin = new Thickness(0, 0, 8, 0),
                    VerticalAlignment = VerticalAlignment.Top,
                    Child = _selectedConversation != null
                        ? CreateConversationAvatarVisual(_selectedConversation, 36)
                        : CreateFallbackAvatarVisual(msg.SenderName, 36, false, GetThemeBrush("AccentBrush"))
                });
            }

            var bubble = new Border
            {
                Background = msg.IsOwn 
                    ? ownBubbleBackground
                    : otherBubbleBackground,
                CornerRadius = msg.IsOwn
                    ? new CornerRadius(16, 16, 4, 16)
                    : new CornerRadius(16, 16, 16, 4),
                Padding = msg.IsSticker ? new Thickness(8) : new Thickness(14, 10, 14, 10)
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

            if (msg.IsSticker)
            {
                stack.Children.Add(CreateStickerBubbleContent(msg));
            }
            else
            {
                stack.Children.Add(new TextBlock
                {
                    Text = msg.Content,
                    FontSize = 13,
                    Foreground = msg.IsOwn 
                        ? ownTextBrush
                        : otherTextBrush,
                    TextWrapping = TextWrapping.Wrap
                });
            }
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
            row.Children.Add(bubble);
            container.Child = row;
            return container;
        }

        private UIElement CreateStickerBubbleContent(ChatMessage msg)
        {
            var stickerSource = TryCreateStickerImageSource(msg.StickerAsset);
            if (stickerSource == null)
            {
                return new TextBlock
                {
                    Text = msg.ConversationPreview,
                    FontSize = 13,
                    Foreground = msg.IsOwn ? Brushes.White : GetThemeBrush("PrimaryTextBrush"),
                    TextWrapping = TextWrapping.Wrap
                };
            }

            return new StackPanel
            {
                Children =
                {
                    new Image
                    {
                        Source = stickerSource,
                        Width = 148,
                        Height = 148,
                        Stretch = Stretch.Uniform,
                        HorizontalAlignment = HorizontalAlignment.Center
                    },
                    new TextBlock
                    {
                        Text = GetStickerDisplayName(msg.StickerAsset),
                        FontSize = 10,
                        Margin = new Thickness(0, 6, 0, 0),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Foreground = msg.IsOwn
                            ? new SolidColorBrush(Color.FromRgb(209, 250, 229))
                            : GetThemeBrush("SecondaryTextBrush")
                    }
                }
            };
        }

        private ImageSource? TryCreateStickerImageSource(string? assetFileName)
        {
            if (string.IsNullOrWhiteSpace(assetFileName))
            {
                return null;
            }

            static string? ResolveStickerPath(string fileName)
            {
                var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var candidateDirectories = new[]
                {
                    System.IO.Path.Combine(baseDirectory, "img", "emojiobsseract"),
                    System.IO.Path.Combine(baseDirectory, "..", "..", "..", "img", "emojiobsseract")
                };

                foreach (var directory in candidateDirectories)
                {
                    if (!Directory.Exists(directory))
                    {
                        continue;
                    }

                    var directPath = System.IO.Path.Combine(directory, fileName);
                    if (File.Exists(directPath))
                    {
                        return System.IO.Path.GetFullPath(directPath);
                    }

                    foreach (var candidatePath in Directory.GetFiles(directory, "*.png"))
                    {
                        if (string.Equals(System.IO.Path.GetFileName(candidatePath), fileName, StringComparison.OrdinalIgnoreCase))
                        {
                            return System.IO.Path.GetFullPath(candidatePath);
                        }
                    }
                }

                return null;
            }

            try
            {
                return new BitmapImage(new Uri($"pack://application:,,,/img/emojiobsseract/{assetFileName}", UriKind.Absolute));
            }
            catch
            {
                try
                {
                    var stickerPath = ResolveStickerPath(assetFileName);
                    if (string.IsNullOrWhiteSpace(stickerPath))
                    {
                        DebugHelper.WriteLine($"[Sticker] Arquivo não encontrado: {assetFileName}");
                        return null;
                    }

                    return new BitmapImage(new Uri(stickerPath, UriKind.Absolute));
                }
                catch (Exception fallbackEx)
                {
                    DebugHelper.WriteLine($"[Sticker] Falha no fallback de arquivo {assetFileName}: {fallbackEx.Message}");
                    return null;
                }
            }
        }

        private string GetStickerPreviewText(string? assetFileName)
        {
            return $"Figurinha • {GetStickerDisplayName(assetFileName)}";
        }

        private string GetStickerDisplayName(string? assetFileName)
        {
            return (assetFileName ?? string.Empty).ToLowerInvariant() switch
            {
                "chao_0.png" => "Feliz",
                "chao_1.png" => "Triste",
                "chao_2.png" => "Irritado",
                "chao_3.png" => "Annya Smile",
                "chao_4.png" => "Pikachu Shock",
                "chao_5.png" => "Love Chao",
                "chao_6.png" => "Close-up Face",
                _ => "Figurinha"
            };
        }

        private async Task LoadActiveConversationsAsync()
        {
            try
            {
                DebugHelper.WriteLine("[LoadActiveConversations] Carregando conversas ativas do Firebase");

                var chatService = new ChatService(_idToken, _currentProfile?.UserId ?? "");
                var conversations = await chatService.LoadConversationsAsync();
                await EnrichConversationAvatarsAsync(conversations);

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

        /// <summary>
        /// Carrega as equipes do banco de dados Firebase
        /// </summary>
        private async Task LoadTeamsFromDatabaseAsync()
        {
            try
            {
                DebugHelper.WriteLine("[LoadTeamsFromDatabase] Carregando equipes do Firebase");

                if (_teamService == null)
                {
                    DebugHelper.WriteLine("[LoadTeamsFromDatabase] TeamService não inicializado");
                    return;
                }

                var teams = await _teamService.LoadTeamsAsync();
                await EnrichTeamMembersAvatarsAsync(teams);

                _teamWorkspaces.Clear();
                foreach (var team in teams)
                {
                    _teamWorkspaces.Add(team);
                }

                UpdateTeamsViewState();
                RenderTeamsList();

                DebugHelper.WriteLine($"[LoadTeamsFromDatabase] {teams.Count} equipes carregadas do banco de dados");
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[LoadTeamsFromDatabase ERROR] {ex.Message}");
                DebugHelper.WriteLine($"[LoadTeamsFromDatabase ERROR] Stack: {ex.StackTrace}");
            }
        }

        private void RenderChatsLoadingState()
        {
            ChatsContent.Children.Clear();

            var shellGrid = new Grid();
            shellGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(360) });
            shellGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) });
            shellGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var sidebar = CreateConversationsSidebar(isLoading: true);
            Grid.SetColumn(sidebar, 0);
            shellGrid.Children.Add(sidebar);

            var loadingPanel = new Border
            {
                Background = _appDarkModeEnabled
                    ? new SolidColorBrush(Color.FromRgb(17, 27, 33))
                    : new SolidColorBrush(Colors.White),
                BorderBrush = _appDarkModeEnabled
                    ? new SolidColorBrush(Color.FromRgb(32, 44, 51))
                    : new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(24),
                Child = new Grid
                {
                    Children =
                    {
                        new StackPanel
                        {
                            Width = 340,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            Children =
                            {
                                new TextBlock
                                {
                                    Text = "Atualizando conversas",
                                    FontSize = 22,
                                    FontWeight = FontWeights.ExtraBold,
                                    Foreground = _appDarkModeEnabled
                                        ? new SolidColorBrush(Color.FromRgb(233, 237, 239))
                                        : new SolidColorBrush(Color.FromRgb(15, 23, 42)),
                                    TextAlignment = TextAlignment.Center
                                },
                                new TextBlock
                                {
                                    Text = "Estamos sincronizando mensagens e ultimas atividades para montar o novo painel de conversa.",
                                    FontSize = 12,
                                    Margin = new Thickness(0, 10, 0, 0),
                                    Foreground = _appDarkModeEnabled
                                        ? new SolidColorBrush(Color.FromRgb(134, 150, 160))
                                        : new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                                    TextWrapping = TextWrapping.Wrap,
                                    TextAlignment = TextAlignment.Center
                                }
                            }
                        }
                    }
                }
            };
            Grid.SetColumn(loadingPanel, 2);
            shellGrid.Children.Add(loadingPanel);

            ChatsContent.Children.Add(shellGrid);
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
                MinWidth = 34,
                Height = 34,
                Padding = new Thickness(10, 0, 10, 0),
                Margin = new Thickness(4, 0, 0, 0),
                Cursor = Cursors.Hand,
                FontSize = 12,
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

        private Popup CreateStickerPickerPopup(Button anchorButton, Conversation conv)
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

            var popup = new Popup
            {
                PlacementTarget = anchorButton,
                Placement = PlacementMode.Top,
                HorizontalOffset = -24,
                VerticalOffset = -12,
                AllowsTransparency = true,
                StaysOpen = false
            };

            var host = new StackPanel();
            host.Children.Add(new TextBlock
            {
                Text = "Figurinhas do app",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = titleBrush
            });
            host.Children.Add(new TextBlock
            {
                Text = "Envie uma despedida rápida com as figurinhas nativas do Obsseract.",
                FontSize = 11,
                Margin = new Thickness(0, 4, 0, 14),
                TextWrapping = TextWrapping.Wrap,
                Foreground = subtitleBrush
            });

            host.Children.Add(new Border
            {
                Background = _appDarkModeEnabled
                    ? new SolidColorBrush(Color.FromRgb(18, 30, 49))
                    : new SolidColorBrush(Color.FromRgb(248, 250, 252)),
                BorderBrush = popupBorderBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(16),
                Padding = new Thickness(12, 10, 12, 10),
                Margin = new Thickness(0, 0, 0, 14),
                Child = new TextBlock
                {
                    Text = "7 opções disponíveis. Clique em uma figurinha para enviar instantaneamente.",
                    FontSize = 11,
                    Foreground = subtitleBrush,
                    TextWrapping = TextWrapping.Wrap
                }
            });

            var stickersWrap = new WrapPanel
            {
                ItemWidth = 146,
                Margin = new Thickness(0, 0, -10, -10)
            };
            foreach (var stickerAsset in ChatStickerAssets)
            {
                var stickerCardBackground = _appDarkModeEnabled
                    ? new SolidColorBrush(Color.FromRgb(18, 30, 49))
                    : new SolidColorBrush(Color.FromRgb(248, 250, 252));

                var stickerButton = new Button
                {
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(0),
                    Margin = new Thickness(0, 0, 10, 10),
                    Cursor = Cursors.Hand,
                    Tag = stickerAsset,
                    Content = new Border
                    {
                        Background = stickerCardBackground,
                        BorderBrush = popupBorderBrush,
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(20),
                        Padding = new Thickness(12),
                        Width = 136,
                        Child = new StackPanel
                        {
                            Children =
                            {
                                new Image
                                {
                                    Source = TryCreateStickerImageSource(stickerAsset),
                                    Width = 96,
                                    Height = 96,
                                    Stretch = Stretch.Uniform,
                                    HorizontalAlignment = HorizontalAlignment.Center
                                },
                                new TextBlock
                                {
                                    Text = GetStickerDisplayName(stickerAsset),
                                    FontSize = 11,
                                    FontWeight = FontWeights.SemiBold,
                                    Margin = new Thickness(0, 8, 0, 0),
                                    TextAlignment = TextAlignment.Center,
                                    Foreground = titleBrush,
                                    TextWrapping = TextWrapping.Wrap
                                },
                                new TextBlock
                                {
                                    Text = "Clique para enviar",
                                    FontSize = 10,
                                    Margin = new Thickness(0, 6, 0, 0),
                                    TextAlignment = TextAlignment.Center,
                                    Foreground = subtitleBrush,
                                    TextWrapping = TextWrapping.Wrap
                                }
                            }
                        }
                    }
                };

                stickerButton.Click += async (_, __) =>
                {
                    popup.IsOpen = false;
                    await SendConversationStickerAsync(conv, stickerAsset);
                };

                stickersWrap.Children.Add(stickerButton);
            }

            host.Children.Add(new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                MaxHeight = 360,
                Content = stickersWrap
            });
            popup.Child = new Border
            {
                Width = 520,
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
                Child = host
            };

            return popup;
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
