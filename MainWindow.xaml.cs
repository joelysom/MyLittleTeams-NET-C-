using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
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
using System.Windows.Threading;
using DocumentFormat.OpenXml.Packaging;
using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Win32;
using WpfAnimatedGif;
using A = DocumentFormat.OpenXml.Drawing;
using IOPath = System.IO.Path;
using S = DocumentFormat.OpenXml.Spreadsheet;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace MeuApp
{
    public partial class MainWindow : MetroWindow
    {
        private const int MaxProfileGalleryImages = 6;
        private const int ProfileGalleryImageMaxSide = 720;
        private const int TeamLogoOutputSize = 420;
        private const int TeamLogoJpegQuality = 86;
        private const int MaxRemoteTeamAssetBytes = 524288;
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
        private static readonly string[] ChatComposerEmojis =
        {
            "😀", "😂", "😍", "🥳", "🤔", "😎", "😭", "🔥",
            "👏", "🙏", "✅", "❌", "📚", "💻", "🚀", "🎯",
            "⭐", "📌", "⚠️", "💡", "🫶", "👍", "👀", "🎉"
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
        private TeamAssetInfo? _draftTeamLogoAsset = null;
        private readonly List<UserInfo> _teamMemberSearchResults = new List<UserInfo>();
        private readonly List<TeamWorkspaceInfo> _teamWorkspaces = new List<TeamWorkspaceInfo>();
        private List<TeamWorkspaceInfo> _teamListSearchResults = new List<TeamWorkspaceInfo>();
        private int _teamMemberSearchVersion = 0;
        private int _teamListSearchVersion = 0;
        private bool _suppressTeamMemberSearch = false;
        private TeamBoardView _activeTeamBoardView = TeamBoardView.Trello;
        private TeamTaskCardInfo? _draggedTeamTaskCard = null;
        private CsdNoteDragInfo? _draggedCsdNote = null;
        private Popup? _boardDragPreviewPopup = null;
        private FrameworkElement? _boardDragPreviewVisual = null;
        private ScaleTransform? _boardDragPreviewScaleTransform = null;
        private FrameworkElement? _boardDragSourceElement = null;
        private double _boardDragSourceOriginalOpacity = 1;
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
        private readonly ConcurrentDictionary<string, Task<List<TeamWorkspaceInfo>>> _userAcademicPortfolioCache = new ConcurrentDictionary<string, Task<List<TeamWorkspaceInfo>>>(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, ImageSource?> _avatarImageCache = new ConcurrentDictionary<string, ImageSource?>(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, ImageSource?> _stickerImageCache = new ConcurrentDictionary<string, ImageSource?>(StringComparer.OrdinalIgnoreCase);
        private List<UserConnectionInfo> _connectionEntries = new List<UserConnectionInfo>();
        private List<UserInfo> _searchSlideResults = new List<UserInfo>();
        private List<TeamWorkspaceInfo> _searchSlideTeamResults = new List<TeamWorkspaceInfo>();
        private readonly DispatcherTimer _realtimeSyncTimer = new DispatcherTimer();
        private List<CalendarAgendaItem> _lastCalendarAgendaItems = new List<CalendarAgendaItem>();
        private List<TeamWorkspaceInfo> _lastCalendarAgendaTeams = new List<TeamWorkspaceInfo>();
        private string _searchSlideQuery = string.Empty;
        private int _searchSlideTypingVersion = 0;
        private int _searchSlideRequestVersion = 0;
        private int _teamSyncSequence = 0;
        private int _chatRenderSequence = 0;
        private int _teamWorkspaceRenderSequence = 0;
        private bool _realtimeSyncInFlight = false;
        private string _calendarFilterTeamId = string.Empty;
        private string _calendarFilterKind = "Todos";
        private string _calendarFilterStatus = "Todos";
        private int _calendarFilterWindowDays = 14;
        private FilesHubState _filesHubState = new FilesHubState();
        private bool _filesHubStateLoaded = false;
        private bool _showChoasIntroBubble = false;
        private string _filesHubStatusMessage = string.Empty;
        private BitmapImage? _choasGifSource = null;
        private string? _choasGifSourcePath = null;

        [StructLayout(LayoutKind.Sequential)]
        private struct NativePoint
        {
            public int X;
            public int Y;
        }

        private sealed class TeamTaskDragInfo
        {
            public TeamWorkspaceInfo Team { get; init; } = new TeamWorkspaceInfo();
            public TeamTaskColumnInfo Column { get; init; } = new TeamTaskColumnInfo();
            public TeamTaskCardInfo Card { get; init; } = new TeamTaskCardInfo();
            public bool CompactMode { get; init; }
        }

        private sealed class CsdNoteDragInfo
        {
            public TeamWorkspaceInfo Team { get; init; } = new TeamWorkspaceInfo();
            public string SourceBucket { get; init; } = string.Empty;
            public string Note { get; init; } = string.Empty;
            public int SourceIndex { get; init; }
        }

        private sealed class FilesHubState
        {
            public bool IntroSeen { get; set; }
            public bool ChoasMinimized { get; set; }
            public List<FilesHubItem> Items { get; set; } = new List<FilesHubItem>();
        }

        private sealed class FilesHubItem
        {
            public string ItemId { get; set; } = Guid.NewGuid().ToString("N");
            public string FileName { get; set; } = string.Empty;
            public string StoredFilePath { get; set; } = string.Empty;
            public string AssociationType { get; set; } = "Projeto";
            public string AssociationLabel { get; set; } = string.Empty;
            public DateTime AddedAt { get; set; } = DateTime.Now;
            public long FileSizeBytes { get; set; }
            public string FileExtension { get; set; } = string.Empty;
            public string RemoteTeamId { get; set; } = string.Empty;
            public string RemoteTeamName { get; set; } = string.Empty;
            public string RemoteAssetId { get; set; } = string.Empty;
            public string RemotePermissionScope { get; set; } = "team";
            public string RemoteStorageReference { get; set; } = string.Empty;
            public int RemoteVersion { get; set; }
            public DateTime? RemoteLastSyncedAt { get; set; }
        }

        private sealed class FilesHubAssociationSelection
        {
            public string AssociationType { get; init; } = "Projeto";
            public string AssociationLabel { get; init; } = string.Empty;
        }

        private sealed class FilesHubSyncSelection
        {
            public TeamWorkspaceInfo Team { get; init; } = new TeamWorkspaceInfo();
            public string PermissionScope { get; init; } = "team";
            public string ChangeSummary { get; init; } = string.Empty;
        }

        private sealed class PendingAttachmentFile
        {
            public string FilePath { get; init; } = string.Empty;
            public string FileName { get; init; } = string.Empty;
            public string PreviewImageDataUri { get; init; } = string.Empty;
        }

        private sealed class CalendarAgendaItem
        {
            public TeamWorkspaceInfo Team { get; init; } = new TeamWorkspaceInfo();
            public string KindLabel { get; init; } = string.Empty;
            public string Title { get; init; } = string.Empty;
            public string Subtitle { get; init; } = string.Empty;
            public string Notes { get; init; } = string.Empty;
            public string StatusLabel { get; init; } = string.Empty;
            public DateTime DueDate { get; init; }
            public Color AccentColor { get; init; }
            public PackIconMaterialKind IconKind { get; init; }
            public bool IsOverdue { get; init; }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out NativePoint point);

        public MainWindow()
        {
            InitializeComponent();
            ProgrammingLanguageInput.ItemsSource = KnownProgrammingLanguages.OrderBy(language => language).ToList();
            InitializeTeamsUi();
            InitializeRealtimeSync();
            ApplyAppTheme();
            this.KeyDown += MainWindow_KeyDown;
            this.Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DebugHelper.WriteLine("MainWindow carregada");

            if (!string.IsNullOrWhiteSpace(_idToken) && _currentProfile != null)
            {
                _teamService = new TeamService(_idToken, _currentProfile.UserId ?? "");
                _connectionService = new ConnectionService(_idToken, _currentProfile);

                RenderChatsLoadingState();
                await Dispatcher.Yield(DispatcherPriority.Background);

                await LoadActiveConversationsAsync();
                _ = LoadTeamsFromDatabaseAsync();
                _ = RefreshConnectionsCacheAsync();
                _realtimeSyncTimer.Start();
            }
            else
            {
                RefreshChatsUI();
                UpdateConnectionsBadge();
                _realtimeSyncTimer.Stop();
            }
        }

        private void InitializeRealtimeSync()
        {
            _realtimeSyncTimer.Interval = TimeSpan.FromSeconds(45);
            _realtimeSyncTimer.Tick += RealtimeSyncTimer_Tick;
        }

        private async void RealtimeSyncTimer_Tick(object? sender, EventArgs e)
        {
            if (_realtimeSyncInFlight || string.IsNullOrWhiteSpace(_idToken) || _currentProfile == null)
            {
                return;
            }

            _realtimeSyncInFlight = true;
            try
            {
                await LoadActiveConversationsAsync();
                await LoadTeamsFromDatabaseAsync();
                await RefreshConnectionsCacheAsync();
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[RealtimeSync] Falha durante sincronizacao: {ex.Message}");
            }
            finally
            {
                _realtimeSyncInFlight = false;
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
            NormalizeProfileCollections(profile);
            ResetFilesHubState();
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
                    Role = profile.Role,
                    AcademicDepartment = profile.AcademicDepartment,
                    AcademicFocus = profile.AcademicFocus,
                    OfficeHours = profile.OfficeHours,
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
            UpdateRoleAwareShellState(profile);
            PopulateProfessionalProfileFields(profile);
            RefreshAvatarUi(profile);
            SyncCurrentUserAvatarAcrossTeams(profile);
            SyncTeamDefaultsWithProfile(profile);
            RenderProfessorDashboard();

            if (FilesContent.Visibility == Visibility.Visible)
            {
                RenderFilesHub();
            }
        }

        private void UpdateRoleAwareShellState(UserProfile? profile)
        {
            var normalizedRole = TeamPermissionService.NormalizeRole(profile?.Role);
            var roleLabel = TeamPermissionService.GetRoleLabel(normalizedRole);
            var subtitleParts = new List<string> { roleLabel };

            if (TeamPermissionService.IsProfessorLike(normalizedRole))
            {
                if (!string.IsNullOrWhiteSpace(profile?.AcademicDepartment))
                {
                    subtitleParts.Add(profile.AcademicDepartment.Trim());
                }
                if (!string.IsNullOrWhiteSpace(profile?.AcademicFocus))
                {
                    subtitleParts.Add(profile.AcademicFocus.Trim());
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(profile?.Course))
                {
                    subtitleParts.Add(profile.Course.Trim());
                }
                if (!string.IsNullOrWhiteSpace(profile?.Registration))
                {
                    subtitleParts.Add($"Matrícula {profile.Registration.Trim()}");
                }
            }

            SidebarUserRole.Text = string.Join(" • ", subtitleParts.Where(part => !string.IsNullOrWhiteSpace(part)));

            if (AccountRoleText != null)
            {
                AccountRoleText.Text = roleLabel;
            }

            if (ProfessorAccessText != null)
            {
                ProfessorAccessText.Text = string.IsNullOrWhiteSpace(profile?.ProfessorAccessLevel)
                    ? (TeamPermissionService.IsProfessorLike(normalizedRole) ? "professor-advisory" : "student-workspace")
                    : profile!.ProfessorAccessLevel;
            }

            if (ProfessorNavButton != null)
            {
                ProfessorNavButton.Visibility = TeamPermissionService.CanUseProfessorDashboard(profile)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
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

        private void ResetFilesHubState()
        {
            _filesHubState = new FilesHubState();
            _filesHubStateLoaded = false;
            _filesHubStatusMessage = string.Empty;
            _showChoasIntroBubble = false;
            StopChoasAnimation();
        }

        private void EnsureFilesHubStateLoaded()
        {
            if (_filesHubStateLoaded)
            {
                return;
            }

            _filesHubState = LoadFilesHubState();
            _filesHubState.Items = (_filesHubState.Items ?? new List<FilesHubItem>())
                .Where(item => item != null && !string.IsNullOrWhiteSpace(item.FileName))
                .OrderByDescending(item => item.AddedAt)
                .ToList();
            _filesHubStateLoaded = true;
        }

        private FilesHubState LoadFilesHubState()
        {
            var statePath = GetFilesHubStateFilePath();
            try
            {
                if (File.Exists(statePath))
                {
                    var json = File.ReadAllText(statePath);
                    var state = JsonSerializer.Deserialize<FilesHubState>(json);
                    if (state != null)
                    {
                        state.Items ??= new List<FilesHubItem>();
                        return state;
                    }
                }
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"Falha ao carregar o hub de arquivos: {ex.Message}");
                _filesHubStatusMessage = "Nao foi possivel recuperar a mochila local; um espaco limpo foi iniciado.";
            }

            return new FilesHubState();
        }

        private bool TrySaveFilesHubState(bool showErrorDialog = false)
        {
            try
            {
                var statePath = GetFilesHubStateFilePath();
                var directory = IOPath.GetDirectoryName(statePath);
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonSerializer.Serialize(_filesHubState, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(statePath, json);
                return true;
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"Falha ao salvar o hub de arquivos: {ex.Message}");
                _filesHubStatusMessage = "A mochila foi atualizada na tela, mas nao conseguiu salvar localmente.";

                if (showErrorDialog)
                {
                    ShowStyledAlertDialog(
                        "ARQUIVOS",
                        "Falha ao salvar",
                        "O estado local da mochila nao pode ser persistido agora. Verifique permissoes da pasta local do aplicativo e tente novamente.",
                        "Fechar",
                        new SolidColorBrush(Color.FromRgb(234, 88, 12)));
                }

                return false;
            }
        }

        private string GetFilesHubRootDirectory()
        {
            return IOPath.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Obsseract",
                "FilesHub",
                GetFilesHubUserKey());
        }

        private string GetFilesHubArchiveDirectory()
        {
            return IOPath.Combine(GetFilesHubRootDirectory(), "archive");
        }

        private string GetFilesHubStateFilePath()
        {
            return IOPath.Combine(GetFilesHubRootDirectory(), "state.json");
        }

        private string GetFilesHubUserKey()
        {
            var rawValue = string.IsNullOrWhiteSpace(_currentProfile?.UserId)
                ? "guest"
                : _currentProfile!.UserId;

            return SanitizeFileNameSegment(rawValue);
        }

        private string SanitizeFileNameSegment(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "perfil";
            }

            var invalidChars = IOPath.GetInvalidFileNameChars();
            var builder = new StringBuilder(value.Length);
            foreach (var character in value.Trim())
            {
                builder.Append(Array.IndexOf(invalidChars, character) >= 0 ? '_' : character);
            }

            var sanitized = builder.ToString().Trim();
            return string.IsNullOrWhiteSpace(sanitized) ? "perfil" : sanitized;
        }

        private void AddFilesHub_Click(object sender, RoutedEventArgs e)
        {
            EnsureFilesHubStateLoaded();

            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                Title = "Selecionar arquivos para o hub",
                Filter = "Todos os arquivos|*.*"
            };

            if (dialog.ShowDialog() != true || dialog.FileNames.Length == 0)
            {
                return;
            }

            var associationSelection = ShowFilesHubAssociationDialog(dialog.FileNames);
            if (associationSelection == null)
            {
                return;
            }

            var importedCount = 0;
            var failedFiles = new List<string>();

            foreach (var filePath in dialog.FileNames)
            {
                try
                {
                    _filesHubState.Items.Insert(0, ArchiveFilesHubItem(filePath, associationSelection));
                    importedCount++;
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine($"Falha ao arquivar {filePath}: {ex.Message}");
                    failedFiles.Add(IOPath.GetFileName(filePath));
                }
            }

            if (importedCount > 0)
            {
                _filesHubState.Items = _filesHubState.Items
                    .OrderByDescending(item => item.AddedAt)
                    .ToList();

                _filesHubStatusMessage = importedCount == 1
                    ? $"{IOPath.GetFileName(dialog.FileNames[0])} entrou na mochila com vinculo de {associationSelection.AssociationType.ToLowerInvariant()}."
                    : $"{importedCount} arquivo(s) foram organizados como {associationSelection.AssociationType.ToLowerInvariant()}.";
                TrySaveFilesHubState(true);
                RenderFilesHub();
            }

            if (failedFiles.Count > 0)
            {
                var failurePreview = string.Join(", ", failedFiles.Take(3));
                if (failedFiles.Count > 3)
                {
                    failurePreview += ", ...";
                }

                ShowStyledAlertDialog(
                    "ARQUIVOS",
                    "Nem tudo entrou",
                    $"Alguns itens nao puderam ser guardados agora: {failurePreview}",
                    "Fechar",
                    new SolidColorBrush(Color.FromRgb(234, 88, 12)));
            }
        }

        private FilesHubAssociationSelection? ShowFilesHubAssociationDialog(IReadOnlyList<string> filePaths)
        {
            var accentBrush = new SolidColorBrush(Color.FromRgb(14, 165, 233));
            var dialog = CreateStyledDialogWindow("Vincular arquivos", 660, 620, 620);

            var selectedType = "Projeto";
            FilesHubAssociationSelection? result = null;

            var contextBox = new TextBox
            {
                Height = 44,
                Margin = new Thickness(0, 8, 0, 0),
                Text = string.Empty
            };
            ApplyDialogInputStyle(contextBox);

            var choicePanel = new WrapPanel();
            void RenderChoiceCards()
            {
                choicePanel.Children.Clear();

                var options = new[]
                {
                    (Label: "Projeto", Description: "Material estrutural para a jornada maior do grupo."),
                    (Label: "Sessao", Description: "Arquivo amarrado a uma rodada, aula ou encontro especifico."),
                    (Label: "Trabalho", Description: "Entrega formal com dono e contexto mais fechado."),
                    (Label: "Atividade", Description: "Peca tatica do dia a dia, checklist ou apoio rapido.")
                };

                foreach (var option in options)
                {
                    var localOption = option;
                    choicePanel.Children.Add(CreateDialogChoiceCard(
                        localOption.Label,
                        localOption.Description,
                        accentBrush,
                        string.Equals(selectedType, localOption.Label, StringComparison.OrdinalIgnoreCase),
                        (_, __) =>
                        {
                            selectedType = localOption.Label;
                            RenderChoiceCards();
                        },
                        270));
                }
            }

            RenderChoiceCards();

            var selectedFilesText = string.Join(
                Environment.NewLine,
                filePaths.Take(4).Select(path => $"- {IOPath.GetFileName(path)}"));
            if (filePaths.Count > 4)
            {
                selectedFilesText += $"{Environment.NewLine}• +{filePaths.Count - 4} arquivo(s)";
            }

            var summaryContent = new StackPanel();
            var summaryChips = new WrapPanel();
            summaryChips.Children.Add(CreateStaticTeamChip($"{filePaths.Count} arquivo(s)", GetThemeBrush("AccentMutedBrush"), GetThemeBrush("AccentBrush")));
            summaryChips.Children.Add(CreateStaticTeamChip("Fluxo assistido pelo CHOAS", GetThemeBrush("MutedCardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            summaryContent.Children.Add(summaryChips);
            summaryContent.Children.Add(new TextBlock
            {
                Text = selectedFilesText,
                Margin = new Thickness(0, 8, 0, 0),
                FontSize = 12,
                Foreground = GetThemeBrush("PrimaryTextBrush"),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20
            });

            var contextContent = new StackPanel();
            contextContent.Children.Add(new TextBlock
            {
                Text = "Se quiser, nomeie o alvo para o CHOAS lembrar algo mais especifico, como Sprint 02, Aula 05 ou Relatorio final.",
                FontSize = 12,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20
            });
            contextContent.Children.Add(contextBox);
            contextContent.Children.Add(CreateDialogHintCard("O contexto eh opcional. Sem ele, o arquivo continua organizado pelo tipo principal.", accentBrush));

            var form = new StackPanel();
            form.Children.Add(CreateDialogSectionCard("Arquivos selecionados", "Confirme o pacote antes de guardar tudo na mochila local.", accentBrush, summaryContent));
            form.Children.Add(CreateDialogSectionCard("Associacao principal", "Essa camada define onde o material vai aparecer primeiro quando voce consultar o hub.", accentBrush, choicePanel));
            form.Children.Add(CreateDialogSectionCard("Rotulo opcional", "Use um apelido curto para a trilha onde esse conjunto entra.", accentBrush, contextContent, new Thickness(0, 0, 0, 0)));

            var saveButton = CreateDialogActionButton("Guardar arquivos", accentBrush, Brushes.White, Brushes.Transparent, 148);
            saveButton.Click += (_, __) =>
            {
                result = new FilesHubAssociationSelection
                {
                    AssociationType = selectedType,
                    AssociationLabel = contextBox.Text?.Trim() ?? string.Empty
                };
                dialog.DialogResult = true;
                dialog.Close();
            };

            var cancelButton = CreateDialogActionButton("Cancelar", Brushes.Transparent, GetThemeBrush("PrimaryTextBrush"), GetThemeBrush("CardBorderBrush"));
            cancelButton.Click += (_, __) => dialog.Close();

            var root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            root.Children.Add(CreateDialogHeader("ARQUIVOS", "Vincular pacote", "Guarde seus materiais ja com o contexto certo para evitar uma lista solta e sem memoria de uso.", accentBrush));

            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = form
            };
            Grid.SetRow(scrollViewer, 1);
            root.Children.Add(scrollViewer);

            var actions = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 22, 0, 0)
            };
            actions.Children.Add(cancelButton);
            actions.Children.Add(saveButton);
            Grid.SetRow(actions, 2);
            root.Children.Add(actions);

            dialog.Content = CreateStyledDialogShell(root);
            return dialog.ShowDialog() == true ? result : null;
        }

        private FilesHubItem ArchiveFilesHubItem(string filePath, FilesHubAssociationSelection associationSelection)
        {
            var extension = IOPath.GetExtension(filePath);
            var destinationPath = CreateFilesHubArchiveFilePath(filePath);
            File.Copy(filePath, destinationPath, false);

            var fileInfo = new FileInfo(destinationPath);
            return new FilesHubItem
            {
                ItemId = Guid.NewGuid().ToString("N"),
                FileName = IOPath.GetFileName(filePath),
                StoredFilePath = destinationPath,
                AssociationType = string.IsNullOrWhiteSpace(associationSelection.AssociationType) ? "Projeto" : associationSelection.AssociationType,
                AssociationLabel = associationSelection.AssociationLabel,
                AddedAt = DateTime.Now,
                FileSizeBytes = fileInfo.Exists ? fileInfo.Length : 0,
                FileExtension = string.IsNullOrWhiteSpace(extension) ? "ARQ" : extension.TrimStart('.').ToUpperInvariant()
            };
        }

        private string CreateFilesHubArchiveFilePath(string originalFilePathOrName)
        {
            var archiveDirectory = GetFilesHubArchiveDirectory();
            Directory.CreateDirectory(archiveDirectory);

            var originalName = IOPath.GetFileName(originalFilePathOrName);
            var extension = IOPath.GetExtension(originalName);
            var safeName = SanitizeFileNameSegment(IOPath.GetFileNameWithoutExtension(originalName));
            var shortGuid = Guid.NewGuid().ToString("N").Substring(0, 8);
            var archivedFileName = $"{DateTime.Now:yyyyMMddHHmmss}-{shortGuid}-{safeName}{extension}";
            return IOPath.Combine(archiveDirectory, archivedFileName);
        }

        private string ArchiveBytesIntoFilesHub(string originalFileName, byte[] fileBytes)
        {
            var destinationPath = CreateFilesHubArchiveFilePath(originalFileName);
            File.WriteAllBytes(destinationPath, fileBytes);
            return destinationPath;
        }

        private bool IsFilesHubItemSynced(FilesHubItem item)
        {
            return item != null
                && !string.IsNullOrWhiteSpace(item.RemoteTeamId)
                && !string.IsNullOrWhiteSpace(item.RemoteAssetId)
                && !string.IsNullOrWhiteSpace(item.RemoteStorageReference);
        }

        private TeamWorkspaceInfo? FindTeamById(string? teamId)
        {
            if (string.IsNullOrWhiteSpace(teamId))
            {
                return null;
            }

            return _teamWorkspaces.FirstOrDefault(team =>
                string.Equals(team.TeamId, teamId, StringComparison.OrdinalIgnoreCase));
        }

        private FilesHubSyncSelection? ShowFilesHubSyncDialog(FilesHubItem item, TeamWorkspaceInfo? preferredTeam = null, string? preferredScope = null, string? preferredSummary = null)
        {
            var eligibleTeams = _teamWorkspaces
                .Where(CanCurrentUserUploadFiles)
                .OrderBy(team => team.TeamName)
                .ToList();

            if (eligibleTeams.Count == 0)
            {
                ShowStyledAlertDialog(
                    "ARQUIVOS",
                    "Sem equipe elegível",
                    "Nenhuma equipe disponível aceita sincronização remota com o seu papel atual. Entre em uma equipe com permissão de upload para publicar esse material.",
                    "Fechar",
                    GetThemeBrush("AccentBrush"));
                return null;
            }

            var accentBrush = GetThemeBrush("AccentBrush");
            var dialog = CreateStyledDialogWindow("Sincronizar no Firebase", 680, 620, 580);

            var teamBox = new ComboBox
            {
                ItemsSource = eligibleTeams,
                SelectedItem = preferredTeam != null && eligibleTeams.Any(team => string.Equals(team.TeamId, preferredTeam.TeamId, StringComparison.OrdinalIgnoreCase))
                    ? eligibleTeams.First(team => string.Equals(team.TeamId, preferredTeam.TeamId, StringComparison.OrdinalIgnoreCase))
                    : eligibleTeams.FirstOrDefault(),
                DisplayMemberPath = "TeamName",
                Height = 46,
                Margin = new Thickness(0, 8, 0, 0)
            };
            var permissionScopeBox = new ComboBox
            {
                Height = 46,
                Margin = new Thickness(0, 8, 0, 0),
                DisplayMemberPath = "Label",
                SelectedValuePath = "Value",
                ItemsSource = new[]
                {
                    new { Label = "Equipe", Value = "team" },
                    new { Label = "Curso", Value = "course" },
                    new { Label = "Liderança", Value = "leadership" },
                    new { Label = "Privado", Value = "private" }
                },
                SelectedValue = TeamPermissionService.NormalizePermissionScope(preferredScope)
            };
            var summaryBox = new TextBox
            {
                Text = preferredSummary ?? (IsFilesHubItemSynced(item) ? "Nova versão publicada a partir do hub local." : "Versão inicial publicada a partir do hub local."),
                Height = 96,
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(0, 8, 0, 0)
            };

            ApplyDialogInputStyle(teamBox);
            ApplyDialogInputStyle(permissionScopeBox);
            ApplyDialogInputStyle(summaryBox);

            FilesHubSyncSelection? result = null;

            var form = new StackPanel();
            form.Children.Add(CreateDialogSectionCard(
                "Arquivo local",
                "Você está publicando ou versionando um item que já vive no hub local do perfil.",
                accentBrush,
                new StackPanel
                {
                    Children =
                    {
                        new TextBlock
                        {
                            Text = item.FileName,
                            FontSize = 14,
                            FontWeight = FontWeights.Bold,
                            Foreground = GetThemeBrush("PrimaryTextBrush")
                        },
                        new TextBlock
                        {
                            Text = $"{item.FileExtension} • {FormatFilesHubSize(item.FileSizeBytes)}" + (string.IsNullOrWhiteSpace(item.AssociationLabel) ? string.Empty : $" • {item.AssociationLabel}"),
                            Margin = new Thickness(0, 6, 0, 0),
                            FontSize = 11,
                            Foreground = GetThemeBrush("SecondaryTextBrush"),
                            TextWrapping = TextWrapping.Wrap
                        }
                    }
                }));

            var destinationContent = new StackPanel();
            destinationContent.Children.Add(CreateDialogFieldLabel("Equipe destino"));
            destinationContent.Children.Add(teamBox);
            destinationContent.Children.Add(CreateDialogFieldLabel("Escopo do arquivo"));
            destinationContent.Children.Add(permissionScopeBox);
            form.Children.Add(CreateDialogSectionCard(
                "Destino remoto",
                "Escolha a equipe que receberá o arquivo e defina quem deve conseguir abrir o conteúdo remoto.",
                accentBrush,
                destinationContent));

            var summaryContent = new StackPanel();
            summaryContent.Children.Add(CreateDialogFieldLabel("Resumo da mudança"));
            summaryContent.Children.Add(summaryBox);
            summaryContent.Children.Add(CreateDialogHintCard(
                "Esse resumo vira trilha do versionamento dentro do material remoto da equipe.",
                accentBrush));
            form.Children.Add(CreateDialogSectionCard(
                "Histórico",
                "Registre em uma frase curta o que esta versão adiciona, substitui ou corrige.",
                accentBrush,
                summaryContent,
                new Thickness(0, 0, 0, 0)));

            var root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            root.Children.Add(CreateDialogHeader(
                "ARQUIVOS",
                IsFilesHubItemSynced(item) ? "Publicar nova versão" : "Sincronizar no Firebase",
                "O conteúdo continua no seu hub local, mas passa a ter uma cópia remota versionada dentro da equipe selecionada.",
                accentBrush));

            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = form
            };
            Grid.SetRow(scrollViewer, 1);
            root.Children.Add(scrollViewer);

            var cancelButton = CreateDialogActionButton("Cancelar", Brushes.Transparent, GetThemeBrush("PrimaryTextBrush"), GetThemeBrush("CardBorderBrush"), 110);
            cancelButton.Click += (_, __) => dialog.Close();
            var saveButton = CreateDialogActionButton("Sincronizar", accentBrush, Brushes.White, Brushes.Transparent, 132);
            saveButton.Click += (_, __) =>
            {
                if (teamBox.SelectedItem is not TeamWorkspaceInfo selectedTeam)
                {
                    ShowStyledAlertDialog("ARQUIVOS", "Equipe obrigatória", "Escolha a equipe que receberá a sincronização remota antes de continuar.", "Continuar", accentBrush);
                    return;
                }

                result = new FilesHubSyncSelection
                {
                    Team = selectedTeam,
                    PermissionScope = permissionScopeBox.SelectedValue?.ToString() ?? selectedTeam.DefaultFilePermissionScope,
                    ChangeSummary = string.IsNullOrWhiteSpace(summaryBox.Text)
                        ? "Atualização enviada pelo hub de arquivos."
                        : summaryBox.Text.Trim()
                };
                dialog.DialogResult = true;
                dialog.Close();
            };

            var footer = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 20, 0, 0),
                Children = { cancelButton, saveButton }
            };
            Grid.SetRow(footer, 2);
            root.Children.Add(footer);

            dialog.Content = CreateStyledDialogShell(root);
            return dialog.ShowDialog() == true ? result : null;
        }

        private async Task<(bool Success, TeamAssetInfo? Asset, string? ErrorMessage)> CreateOrUpdateTeamAssetAsync(
            TeamWorkspaceInfo team,
            string filePath,
            string category,
            string permissionScope,
            string changeSummary,
            TeamAssetInfo? existingAsset = null,
            string? description = null)
        {
            if (_teamService == null)
            {
                return (false, null, "Serviço de equipes não inicializado.");
            }

            if (!File.Exists(filePath))
            {
                return (false, null, "Arquivo local não encontrado para sincronização.");
            }

            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
            {
                return (false, null, "Arquivo local não está mais disponível.");
            }

            if (fileInfo.Length > MaxRemoteTeamAssetBytes)
            {
                return (false, null, $"{IOPath.GetFileName(filePath)} excede o limite de {FormatFilesHubSize(MaxRemoteTeamAssetBytes)} para sincronização remota via Firestore.");
            }

            var normalizedScope = TeamPermissionService.NormalizePermissionScope(permissionScope);
            var fileName = IOPath.GetFileName(filePath);
            var mimeType = GetMimeTypeFromFileName(fileName);
            var versionNumber = existingAsset == null ? 1 : Math.Max(1, existingAsset.Version + 1);
            var changedAt = DateTime.Now;
            var asset = existingAsset ?? new TeamAssetInfo
            {
                AssetId = Guid.NewGuid().ToString("N"),
                AddedByUserId = GetCurrentUserId(),
                AddedAt = changedAt,
                VersionHistory = new List<TeamAssetVersionInfo>()
            };

            var versionEntry = new TeamAssetVersionInfo
            {
                VersionNumber = versionNumber,
                ChangedByUserId = GetCurrentUserId(),
                ChangeSummary = string.IsNullOrWhiteSpace(changeSummary) ? "Atualização de arquivo" : changeSummary.Trim(),
                FileName = fileName,
                MimeType = mimeType,
                PermissionScope = normalizedScope,
                StorageKind = "firestore-document",
                SizeBytes = fileInfo.Length,
                ChangedAt = changedAt
            };

            var fileBytes = await File.ReadAllBytesAsync(filePath);
            var storageResult = await _teamService.SaveTeamAssetContentAsync(team.TeamId, asset.AssetId, versionEntry, fileBytes);
            if (!storageResult.Success)
            {
                return (false, null, storageResult.ErrorMessage ?? "Falha desconhecida ao salvar conteúdo remoto do arquivo.");
            }

            versionEntry.StorageReference = storageResult.StorageReference;
            asset.Category = string.IsNullOrWhiteSpace(category) ? ResolveAssetCategoryForFile(filePath) : category;
            asset.FileName = fileName;
            asset.PreviewImageDataUri = IsFilesHubImageExtension(GetFilesHubExtension(filePath, string.Empty))
                ? TryCreateCompressedImageDataUri(filePath, 420, 74) ?? string.Empty
                : string.Empty;
            asset.Description = string.IsNullOrWhiteSpace(description) ? asset.Description : description.Trim();
            asset.MimeType = mimeType;
            asset.PermissionScope = normalizedScope;
            asset.StorageKind = "firestore-document";
            asset.StorageReference = storageResult.StorageReference;
            asset.SizeBytes = fileInfo.Length;
            asset.Version = versionNumber;
            asset.LastSyncedAt = changedAt;
            asset.AddedByUserId = string.IsNullOrWhiteSpace(asset.AddedByUserId) ? GetCurrentUserId() : asset.AddedByUserId;
            asset.VersionHistory ??= new List<TeamAssetVersionInfo>();
            asset.VersionHistory.RemoveAll(version => version.VersionNumber == versionNumber);
            asset.VersionHistory.Add(versionEntry);
            asset.VersionHistory = asset.VersionHistory
                .OrderByDescending(version => version.VersionNumber)
                .ToList();

            return (true, asset, null);
        }

        private async Task<(bool Success, string LocalPath, string? ErrorMessage)> EnsureTeamAssetLocalCopyAsync(TeamWorkspaceInfo team, TeamAssetInfo asset, TeamAssetVersionInfo? version = null)
        {
            if (_teamService == null)
            {
                return (false, string.Empty, "Serviço de equipes não inicializado.");
            }

            var targetReference = version?.StorageReference ?? asset.StorageReference;
            if (string.IsNullOrWhiteSpace(targetReference))
            {
                return (false, string.Empty, "Esse material não possui conteúdo remoto disponível para download.");
            }

            var downloadResult = await _teamService.LoadTeamAssetContentAsync(targetReference);
            if (!downloadResult.Success || downloadResult.Payload == null)
            {
                return (false, string.Empty, downloadResult.ErrorMessage ?? "Não foi possível baixar a versão remota do arquivo.");
            }

            var payload = downloadResult.Payload;
            var cacheDirectory = BuildTeamAssetCacheDirectory(team, asset.AssetId);
            Directory.CreateDirectory(cacheDirectory);

            var resolvedFileName = string.IsNullOrWhiteSpace(payload.FileName)
                ? (string.IsNullOrWhiteSpace(asset.FileName) ? $"asset-{asset.AssetId}" : asset.FileName)
                : payload.FileName;
            var localPath = IOPath.Combine(
                cacheDirectory,
                $"v{payload.VersionNumber:D3}-{SanitizeFileNameSegment(IOPath.GetFileNameWithoutExtension(resolvedFileName))}{IOPath.GetExtension(resolvedFileName)}");

            await File.WriteAllBytesAsync(localPath, payload.Bytes);
            return (true, localPath, null);
        }

        private async Task<bool> RestoreFilesHubItemFromRemoteAsync(FilesHubItem item)
        {
            if (!IsFilesHubItemSynced(item) || _teamService == null)
            {
                return false;
            }

            var team = FindTeamById(item.RemoteTeamId);
            if (team == null)
            {
                team = await _teamService.GetTeamByIdAsync(item.RemoteTeamId);
                if (team == null)
                {
                    return false;
                }
            }

            var asset = team.Assets.FirstOrDefault(candidate =>
                string.Equals(candidate.AssetId, item.RemoteAssetId, StringComparison.OrdinalIgnoreCase));
            if (asset == null)
            {
                return false;
            }

            var localCopyResult = await EnsureTeamAssetLocalCopyAsync(team, asset);
            if (!localCopyResult.Success || string.IsNullOrWhiteSpace(localCopyResult.LocalPath))
            {
                return false;
            }

            item.StoredFilePath = ArchiveBytesIntoFilesHub(asset.FileName, await File.ReadAllBytesAsync(localCopyResult.LocalPath));
            item.FileName = asset.FileName;
            item.FileExtension = string.IsNullOrWhiteSpace(IOPath.GetExtension(asset.FileName))
                ? item.FileExtension
                : IOPath.GetExtension(asset.FileName).TrimStart('.').ToUpperInvariant();
            item.FileSizeBytes = new FileInfo(item.StoredFilePath).Length;
            item.RemoteLastSyncedAt = asset.LastSyncedAt;
            TrySaveFilesHubState(true);
            return true;
        }

        private async Task<bool> SynchronizeFilesHubItemAsync(FilesHubItem item, FilesHubSyncSelection syncSelection)
        {
            var team = syncSelection.Team;
            var existingAsset = string.IsNullOrWhiteSpace(item.RemoteAssetId)
                ? null
                : team.Assets.FirstOrDefault(asset => string.Equals(asset.AssetId, item.RemoteAssetId, StringComparison.OrdinalIgnoreCase));

            var syncResult = await CreateOrUpdateTeamAssetAsync(
                team,
                item.StoredFilePath,
                ResolveAssetCategoryForFile(item.StoredFilePath),
                syncSelection.PermissionScope,
                syncSelection.ChangeSummary,
                existingAsset,
                string.IsNullOrWhiteSpace(item.AssociationLabel)
                    ? $"Arquivo sincronizado a partir do hub ({item.AssociationType})."
                    : item.AssociationLabel);

            if (!syncResult.Success || syncResult.Asset == null)
            {
                ShowStyledAlertDialog(
                    "ARQUIVOS",
                    "Falha na sincronização",
                    syncResult.ErrorMessage ?? "Não foi possível publicar o arquivo remoto desta vez.",
                    "Fechar",
                    new SolidColorBrush(Color.FromRgb(220, 38, 38)));
                return false;
            }

            if (existingAsset == null)
            {
                team.Assets.Add(syncResult.Asset);
            }

            item.RemoteTeamId = team.TeamId;
            item.RemoteTeamName = team.TeamName;
            item.RemoteAssetId = syncResult.Asset.AssetId;
            item.RemotePermissionScope = syncResult.Asset.PermissionScope;
            item.RemoteStorageReference = syncResult.Asset.StorageReference;
            item.RemoteVersion = syncResult.Asset.Version;
            item.RemoteLastSyncedAt = syncResult.Asset.LastSyncedAt;

            _filesHubStatusMessage = existingAsset == null
                ? $"{item.FileName} foi sincronizado com {team.TeamName}."
                : $"Nova versão de {item.FileName} publicada em {team.TeamName}.";

            AddTeamNotification(
                team,
                existingAsset == null
                    ? $"Arquivo sincronizado do hub local: {item.FileName}."
                    : $"Nova versão do arquivo {item.FileName} publicada a partir do hub local.");
            SaveTeamWorkspace(team);
            TrySaveFilesHubState(true);
            RenderFilesHub();
            return true;
        }

        private (bool Confirmed, string PermissionScope, string ChangeSummary) ShowTeamAssetSyncOptionsDialog(
            TeamWorkspaceInfo team,
            string dialogTitle,
            string eyebrow,
            string description,
            string initialScope,
            string initialSummary)
        {
            var accentBrush = GetThemeBrush("AccentBrush");
            var dialog = CreateStyledDialogWindow(dialogTitle, 620, 520, 500);

            var permissionScopeBox = new ComboBox
            {
                Height = 46,
                Margin = new Thickness(0, 8, 0, 0),
                DisplayMemberPath = "Label",
                SelectedValuePath = "Value",
                ItemsSource = new[]
                {
                    new { Label = "Equipe", Value = "team" },
                    new { Label = "Curso", Value = "course" },
                    new { Label = "Liderança", Value = "leadership" },
                    new { Label = "Privado", Value = "private" }
                },
                SelectedValue = TeamPermissionService.NormalizePermissionScope(initialScope)
            };
            var summaryBox = new TextBox
            {
                Text = initialSummary,
                Height = 100,
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(0, 8, 0, 0)
            };
            ApplyDialogInputStyle(permissionScopeBox);
            ApplyDialogInputStyle(summaryBox);

            var form = new StackPanel();
            form.Children.Add(CreateDialogSectionCard(
                "Equipe alvo",
                $"Toda a sincronização vai para {team.TeamName}. Aqui você só ajusta o alcance do arquivo e o texto que ficará no histórico.",
                accentBrush,
                new StackPanel
                {
                    Children =
                    {
                        new TextBlock
                        {
                            Text = team.TeamName,
                            FontSize = 14,
                            FontWeight = FontWeights.Bold,
                            Foreground = GetThemeBrush("PrimaryTextBrush")
                        },
                        new TextBlock
                        {
                            Text = string.IsNullOrWhiteSpace(team.TemplateName)
                                ? $"{team.Course} • {team.ClassName}"
                                : $"{team.TemplateName} • {team.Course}",
                            Margin = new Thickness(0, 6, 0, 0),
                            FontSize = 11,
                            Foreground = GetThemeBrush("SecondaryTextBrush"),
                            TextWrapping = TextWrapping.Wrap
                        }
                    }
                }));

            var permissionContent = new StackPanel();
            permissionContent.Children.Add(CreateDialogFieldLabel("Escopo do arquivo"));
            permissionContent.Children.Add(permissionScopeBox);
            permissionContent.Children.Add(CreateDialogHintCard(
                "Escopos restritos controlam quem consegue abrir o conteúdo remoto do material versionado.",
                accentBrush));
            form.Children.Add(CreateDialogSectionCard(
                "Permissões",
                "Defina se o conteúdo deve ser aberto por toda a equipe, apenas liderança ou ficar privado para o autor e gestores.",
                accentBrush,
                permissionContent));

            var summaryContent = new StackPanel();
            summaryContent.Children.Add(CreateDialogFieldLabel("Resumo da alteração"));
            summaryContent.Children.Add(summaryBox);
            form.Children.Add(CreateDialogSectionCard(
                "Histórico da versão",
                "Esse resumo aparece no trilho de versões do asset para deixar claro o que mudou entre uploads.",
                accentBrush,
                summaryContent,
                new Thickness(0, 0, 0, 0)));

            var root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.Children.Add(CreateDialogHeader(eyebrow, dialogTitle, description, accentBrush));

            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = form
            };
            Grid.SetRow(scrollViewer, 1);
            root.Children.Add(scrollViewer);

            var cancelButton = CreateDialogActionButton("Cancelar", Brushes.Transparent, GetThemeBrush("PrimaryTextBrush"), GetThemeBrush("CardBorderBrush"), 110);
            var saveButton = CreateDialogActionButton("Confirmar", accentBrush, Brushes.White, Brushes.Transparent, 118);
            var footer = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 20, 0, 0),
                Children = { cancelButton, saveButton }
            };
            Grid.SetRow(footer, 2);
            root.Children.Add(footer);

            dialog.Content = CreateStyledDialogShell(root);

            var confirmed = false;
            cancelButton.Click += (_, __) => dialog.Close();
            saveButton.Click += (_, __) =>
            {
                confirmed = true;
                dialog.DialogResult = true;
                dialog.Close();
            };

            dialog.ShowDialog();
            return (
                confirmed,
                permissionScopeBox.SelectedValue?.ToString() ?? team.DefaultFilePermissionScope,
                string.IsNullOrWhiteSpace(summaryBox.Text) ? initialSummary : summaryBox.Text.Trim());
        }

        private FilesHubItem CreateTransientFilesHubItemForAsset(TeamWorkspaceInfo team, TeamAssetInfo asset, string localPath, TeamAssetVersionInfo? version = null)
        {
            var fileName = version?.FileName ?? asset.FileName;
            return new FilesHubItem
            {
                ItemId = Guid.NewGuid().ToString("N"),
                FileName = fileName,
                StoredFilePath = localPath,
                AssociationType = "Projeto",
                AssociationLabel = team.TeamName,
                AddedAt = version?.ChangedAt ?? asset.AddedAt,
                FileSizeBytes = new FileInfo(localPath).Length,
                FileExtension = IOPath.GetExtension(fileName).TrimStart('.').ToUpperInvariant(),
                RemoteTeamId = team.TeamId,
                RemoteTeamName = team.TeamName,
                RemoteAssetId = asset.AssetId,
                RemotePermissionScope = version?.PermissionScope ?? asset.PermissionScope,
                RemoteStorageReference = version?.StorageReference ?? asset.StorageReference,
                RemoteVersion = version?.VersionNumber ?? asset.Version,
                RemoteLastSyncedAt = asset.LastSyncedAt
            };
        }

        private async Task OpenTeamAssetPreviewAsync(TeamWorkspaceInfo team, TeamAssetInfo asset, TeamAssetVersionInfo? version = null)
        {
            if (!CanCurrentUserViewAsset(team, asset))
            {
                ShowStyledAlertDialog("ARQUIVOS", "Acesso restrito", "O escopo deste arquivo não permite abrir o conteúdo remoto com o seu papel atual.", "Fechar", GetThemeBrush("AccentBrush"));
                return;
            }

            var localCopyResult = await EnsureTeamAssetLocalCopyAsync(team, asset, version);
            if (!localCopyResult.Success)
            {
                ShowStyledAlertDialog("ARQUIVOS", "Falha ao abrir material", localCopyResult.ErrorMessage ?? "Não foi possível recuperar a versão remota do arquivo.", "Fechar", new SolidColorBrush(Color.FromRgb(220, 38, 38)));
                return;
            }

            ShowFilesHubItemPreviewDialog(CreateTransientFilesHubItemForAsset(team, asset, localCopyResult.LocalPath, version));
        }

        private async void OpenTeamAssetPreview_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not Tuple<TeamWorkspaceInfo, TeamAssetInfo> payload)
            {
                return;
            }

            await OpenTeamAssetPreviewAsync(payload.Item1, payload.Item2);
        }

        private void OpenTeamAssetHistory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not Tuple<TeamWorkspaceInfo, TeamAssetInfo> payload)
            {
                return;
            }

            var team = payload.Item1;
            var asset = payload.Item2;
            var versions = (asset.VersionHistory ?? new List<TeamAssetVersionInfo>())
                .Where(version => !string.IsNullOrWhiteSpace(version.StorageReference))
                .OrderByDescending(version => version.VersionNumber)
                .ToList();

            if (versions.Count == 0)
            {
                ShowStyledAlertDialog("ARQUIVOS", "Sem histórico", "Este material ainda não possui versões remotas suficientes para navegação do histórico.", "Fechar", GetThemeBrush("AccentBrush"));
                return;
            }

            var accentBrush = GetThemeBrush("AccentBrush");
            var dialog = CreateStyledDialogWindow($"Histórico • {asset.FileName}", 760, 680, 640, true);
            var root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.Children.Add(CreateDialogHeader(
                "ARQUIVOS",
                $"Histórico de {asset.FileName}",
                "Abra versões anteriores do material remoto e acompanhe quem publicou cada mudança.",
                accentBrush));

            var versionsHost = new StackPanel();
            foreach (var version in versions)
            {
                var card = new Border
                {
                    Margin = new Thickness(0, 0, 0, 12),
                    Padding = new Thickness(16),
                    Background = GetThemeBrush("MutedCardBackgroundBrush"),
                    BorderBrush = GetThemeBrush("CardBorderBrush"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(18)
                };

                var layout = new Grid();
                layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var info = new StackPanel();
                info.Children.Add(new TextBlock
                {
                    Text = $"Versão {version.VersionNumber}",
                    FontSize = 13,
                    FontWeight = FontWeights.Bold,
                    Foreground = GetThemeBrush("PrimaryTextBrush")
                });
                info.Children.Add(new TextBlock
                {
                    Text = string.IsNullOrWhiteSpace(version.ChangeSummary) ? "Sem resumo registrado." : version.ChangeSummary,
                    Margin = new Thickness(0, 6, 0, 0),
                    FontSize = 11,
                    Foreground = GetThemeBrush("SecondaryTextBrush"),
                    TextWrapping = TextWrapping.Wrap,
                    LineHeight = 18
                });
                var chips = new WrapPanel { Margin = new Thickness(0, 10, 0, 0) };
                chips.Children.Add(CreateStaticTeamChip(version.FileName, accentBrush, Brushes.White));
                chips.Children.Add(CreateStaticTeamChip(GetPermissionScopeLabel(version.PermissionScope), GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
                chips.Children.Add(CreateStaticTeamChip(FormatFilesHubSize(version.SizeBytes), GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
                chips.Children.Add(CreateStaticTeamChip(version.ChangedAt == default ? "Sem data" : version.ChangedAt.ToString("dd/MM HH:mm"), GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
                info.Children.Add(chips);
                layout.Children.Add(info);

                var openButton = CreateDialogActionButton("Abrir", accentBrush, Brushes.White, Brushes.Transparent, 96);
                openButton.Margin = new Thickness(12, 0, 0, 0);
                openButton.Click += async (_, __) => await OpenTeamAssetPreviewAsync(team, asset, version);
                Grid.SetColumn(openButton, 1);
                layout.Children.Add(openButton);

                card.Child = layout;
                versionsHost.Children.Add(card);
            }

            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = versionsHost
            };
            Grid.SetRow(scrollViewer, 1);
            root.Children.Add(scrollViewer);

            var closeButton = CreateDialogActionButton("Fechar", accentBrush, Brushes.White, Brushes.Transparent, 104);
            closeButton.Click += (_, __) => dialog.Close();
            var footer = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 18, 0, 0),
                Children = { closeButton }
            };
            Grid.SetRow(footer, 2);
            root.Children.Add(footer);

            dialog.Content = CreateStyledDialogShell(root);
            dialog.ShowDialog();
        }

        private async void UpdateTeamAssetVersion_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not Tuple<TeamWorkspaceInfo, TeamAssetInfo> payload)
            {
                return;
            }

            var team = payload.Item1;
            var asset = payload.Item2;
            if (!CanCurrentUserUploadFiles(team))
            {
                ShowStyledAlertDialog("ARQUIVOS", "Permissão insuficiente", "Seu papel atual não permite publicar nova versão deste material.", "Fechar", GetThemeBrush("AccentBrush"));
                return;
            }

            var dialog = new OpenFileDialog
            {
                Multiselect = false,
                Title = "Selecionar nova versão do arquivo",
                Filter = "Todos os arquivos|*.*"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            var options = ShowTeamAssetSyncOptionsDialog(
                team,
                "Publicar nova versão",
                "ARQUIVOS",
                "Atualize o escopo se necessário e registre o que mudou nesta nova versão do material.",
                asset.PermissionScope,
                $"Versão {asset.Version + 1} de {asset.FileName}.");
            if (!options.Confirmed)
            {
                return;
            }

            var updateResult = await CreateOrUpdateTeamAssetAsync(
                team,
                dialog.FileName,
                asset.Category,
                options.PermissionScope,
                options.ChangeSummary,
                asset,
                asset.Description);
            if (!updateResult.Success)
            {
                ShowStyledAlertDialog("ARQUIVOS", "Falha ao versionar", updateResult.ErrorMessage ?? "Não foi possível publicar a nova versão deste arquivo.", "Fechar", new SolidColorBrush(Color.FromRgb(220, 38, 38)));
                return;
            }

            AddTeamNotification(team, $"Nova versão publicada para {asset.FileName}.");
            SaveTeamWorkspace(team);
            RenderTeamWorkspace();
        }

        private void UpdateFilesHubBottomSpacing()
        {
            if (FilesHubScrollStack == null)
            {
                return;
            }

            FilesHubScrollStack.Margin = new Thickness(0, 0, 0, _showChoasIntroBubble ? 324 : 220);
        }

        private void UpdateFilesHubStatusPresentation()
        {
            if (FilesStatusText == null)
            {
                return;
            }

            FilesStatusText.Text = _filesHubStatusMessage;
            FilesStatusText.Visibility = string.IsNullOrWhiteSpace(_filesHubStatusMessage)
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private void RefreshChoasPresentation()
        {
            UpdateFilesHubBottomSpacing();
            UpdateFilesHubStatusPresentation();
            RenderFilesChoasCompanion();
        }

        private void RenderFilesHub()
        {
            EnsureFilesHubStateLoaded();

            if (!_filesHubState.IntroSeen)
            {
                _filesHubState.IntroSeen = true;
                _showChoasIntroBubble = true;
                TrySaveFilesHubState();
            }

            if (_filesHubState.ChoasMinimized)
            {
                _filesHubState.ChoasMinimized = false;
                TrySaveFilesHubState();
            }

            UpdateFilesHubBottomSpacing();
            UpdateFilesHubStatusPresentation();

            FilesHubSummaryHost.Children.Clear();
            FilesHubBodyHost.Children.Clear();

            var items = _filesHubState.Items
                .OrderByDescending(item => item.AddedAt)
                .ToList();
            var dominantType = items
                .GroupBy(item => string.IsNullOrWhiteSpace(item.AssociationType) ? "Projeto" : item.AssociationType)
                .OrderByDescending(group => group.Count())
                .ThenBy(group => GetFilesHubAssociationOrder(group.Key))
                .Select(group => group.Key)
                .FirstOrDefault() ?? "Projeto";
            var latestItem = items.FirstOrDefault();

            FilesHubSummaryHost.Children.Add(CreateBoardOverviewMetric(
                "Arquivos",
                items.Count.ToString(),
                items.Count == 0 ? "Nenhum material guardado ainda" : "Pacote local pronto para consulta",
                Color.FromRgb(14, 165, 233)));
            FilesHubSummaryHost.Children.Add(CreateBoardOverviewMetric(
                "Vinculo lider",
                items.Count == 0 ? "--" : dominantType,
                items.Count == 0 ? "Use o + para classificar seu primeiro item" : GetFilesHubAssociationDescription(dominantType),
                GetFilesHubAssociationAccentColor(dominantType)));
            FilesHubSummaryHost.Children.Add(CreateBoardOverviewMetric(
                "Ultimo item",
                latestItem == null ? "--" : FormatRelativeDate(latestItem.AddedAt),
                latestItem == null ? "CHOAS ainda sem memoria local" : latestItem.FileName,
                Color.FromRgb(16, 185, 129)));

            FilesHubBodyHost.Children.Add(CreateFilesHubOverviewBanner(items.Count));

            if (items.Count == 0)
            {
                FilesHubBodyHost.Children.Add(CreateFilesHubEmptyState());
            }
            else
            {
                foreach (var group in items
                    .GroupBy(item => string.IsNullOrWhiteSpace(item.AssociationType) ? "Projeto" : item.AssociationType)
                    .OrderBy(group => GetFilesHubAssociationOrder(group.Key))
                    .ThenBy(group => group.Key, StringComparer.OrdinalIgnoreCase))
                {
                    FilesHubBodyHost.Children.Add(CreateFilesHubSection(group.Key, group.ToList()));
                }
            }

            RenderFilesChoasCompanion();
        }

        private Border CreateFilesHubOverviewBanner(int itemCount)
        {
            var accentBrush = new SolidColorBrush(Color.FromRgb(14, 165, 233));
            var chips = new WrapPanel();
            chips.Children.Add(CreateStaticTeamChip("Projeto", CreateSoftAccentBrush(new SolidColorBrush(GetFilesHubAssociationAccentColor("Projeto")), 28), new SolidColorBrush(GetFilesHubAssociationAccentColor("Projeto"))));
            chips.Children.Add(CreateStaticTeamChip("Sessao", CreateSoftAccentBrush(new SolidColorBrush(GetFilesHubAssociationAccentColor("Sessao")), 28), new SolidColorBrush(GetFilesHubAssociationAccentColor("Sessao"))));
            chips.Children.Add(CreateStaticTeamChip("Trabalho", CreateSoftAccentBrush(new SolidColorBrush(GetFilesHubAssociationAccentColor("Trabalho")), 28), new SolidColorBrush(GetFilesHubAssociationAccentColor("Trabalho"))));
            chips.Children.Add(CreateStaticTeamChip("Atividade", CreateSoftAccentBrush(new SolidColorBrush(GetFilesHubAssociationAccentColor("Atividade")), 28), new SolidColorBrush(GetFilesHubAssociationAccentColor("Atividade"))));

            var contentGrid = new Grid();
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var textStack = new StackPanel();
            textStack.Children.Add(new TextBlock
            {
                Text = "Mochila do projeto",
                FontSize = 22,
                FontWeight = FontWeights.ExtraBold,
                Foreground = GetThemeBrush("PrimaryTextBrush"),
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 440
            });
            textStack.Children.Add(new TextBlock
            {
                Text = itemCount == 0
                    ? "Arraste o primeiro pacote para dentro da jornada e ja marque o tipo certo para nao virar um deposito generico."
                    : "Cada arquivo fica guardado com memoria de contexto, pronto para reaparecer no momento certo do fluxo.",
                Margin = new Thickness(0, 10, 0, 0),
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 440,
                LineHeight = 21
            });
            textStack.Children.Add(new Border
            {
                Margin = new Thickness(0, 14, 0, 0),
                Child = chips
            });
            contentGrid.Children.Add(textStack);

            var badge = new Border
            {
                Width = 92,
                Height = 92,
                Margin = new Thickness(20, 0, 0, 0),
                CornerRadius = new CornerRadius(24),
                Background = CreateSoftAccentBrush(accentBrush, 30),
                BorderBrush = accentBrush,
                BorderThickness = new Thickness(1),
                Child = new StackPanel
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = itemCount.ToString(),
                            FontSize = 28,
                            FontWeight = FontWeights.ExtraBold,
                            Foreground = accentBrush,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            TextAlignment = TextAlignment.Center
                        },
                        new TextBlock
                        {
                            Text = itemCount == 1 ? "item" : "itens",
                            FontSize = 11,
                            Foreground = GetThemeBrush("SecondaryTextBrush"),
                            HorizontalAlignment = HorizontalAlignment.Center,
                            TextAlignment = TextAlignment.Center
                        }
                    }
                }
            };
            Grid.SetColumn(badge, 1);
            contentGrid.Children.Add(badge);

            return new Border
            {
                Margin = new Thickness(0, 0, 0, 16),
                Padding = new Thickness(22),
                Background = GetThemeBrush("CardBackgroundBrush"),
                BorderBrush = accentBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(24),
                Child = contentGrid
            };
        }

        private Border CreateFilesHubEmptyState()
        {
            var accentBrush = GetThemeBrush("AccentBrush");
            var addButton = CreateFilesHubActionButton("Adicionar agora", accentBrush, Brushes.White, Brushes.Transparent, AddFilesHub_Click);

            return new Border
            {
                Background = GetThemeBrush("CardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(24),
                Padding = new Thickness(26),
                Child = new StackPanel
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Width = 420,
                    Children =
                    {
                        new PackIconMaterial
                        {
                            Kind = PackIconMaterialKind.FolderOutline,
                            Width = 34,
                            Height = 34,
                            Foreground = accentBrush,
                            HorizontalAlignment = HorizontalAlignment.Center
                        },
                        new TextBlock
                        {
                            Text = "Sua mochila ainda esta vazia",
                            Margin = new Thickness(0, 14, 0, 0),
                            FontSize = 22,
                            FontWeight = FontWeights.ExtraBold,
                            Foreground = GetThemeBrush("PrimaryTextBrush"),
                            TextAlignment = TextAlignment.Center
                        },
                        new TextBlock
                        {
                            Text = "Adicione imagens, PDFs, planilhas ou qualquer apoio de trabalho. Depois classifique como projeto, sessao, trabalho ou atividade para nao perder contexto.",
                            Margin = new Thickness(0, 10, 0, 18),
                            FontSize = 12,
                            Foreground = GetThemeBrush("SecondaryTextBrush"),
                            TextWrapping = TextWrapping.Wrap,
                            TextAlignment = TextAlignment.Center,
                            LineHeight = 20
                        },
                        addButton
                    }
                }
            };
        }

        private Border CreateFilesHubSection(string associationType, List<FilesHubItem> items)
        {
            var accentColor = GetFilesHubAssociationAccentColor(associationType);
            var accentBrush = new SolidColorBrush(accentColor);
            var itemStack = new StackPanel();

            foreach (var item in items.OrderByDescending(entry => entry.AddedAt))
            {
                itemStack.Children.Add(CreateFilesHubItemCard(item));
            }

            var metaWrap = new WrapPanel();
            metaWrap.Children.Add(CreateStaticTeamChip($"{items.Count} item(ns)", CreateSoftAccentBrush(accentBrush, 28), accentBrush));
            metaWrap.Children.Add(CreateStaticTeamChip(GetFilesHubAssociationDescription(associationType), GetThemeBrush("MutedCardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));

            return new Border
            {
                Margin = new Thickness(0, 0, 0, 16),
                Padding = new Thickness(20),
                Background = GetThemeBrush("CardBackgroundBrush"),
                BorderBrush = accentBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(22),
                Child = new StackPanel
                {
                    Children =
                    {
                        new Border
                        {
                            Width = 48,
                            Height = 5,
                            CornerRadius = new CornerRadius(999),
                            Background = accentBrush
                        },
                        new TextBlock
                        {
                            Text = associationType,
                            Margin = new Thickness(0, 12, 0, 0),
                            FontSize = 18,
                            FontWeight = FontWeights.Bold,
                            Foreground = GetThemeBrush("PrimaryTextBrush")
                        },
                        new Border
                        {
                            Margin = new Thickness(0, 10, 0, 8),
                            Child = metaWrap
                        },
                        itemStack
                    }
                }
            };
        }

        private Border CreateFilesHubItemCard(FilesHubItem item)
        {
            var accentColor = GetFilesHubAssociationAccentColor(item.AssociationType);
            var accentBrush = new SolidColorBrush(accentColor);
            var fileExists = File.Exists(item.StoredFilePath);
            var isSynced = IsFilesHubItemSynced(item);

            var layout = new Grid();
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var leftStack = new StackPanel();
            leftStack.Children.Add(new TextBlock
            {
                Text = item.FileName,
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                Foreground = GetThemeBrush("PrimaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            });

            var description = string.IsNullOrWhiteSpace(item.AssociationLabel)
                ? $"Contexto base: {GetFilesHubAssociationDescription(item.AssociationType)}"
                : $"Contexto vinculado: {item.AssociationLabel}";
            leftStack.Children.Add(new TextBlock
            {
                Text = description,
                Margin = new Thickness(0, 6, 0, 0),
                FontSize = 12,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20
            });

            var metaWrap = new WrapPanel { Margin = new Thickness(0, 12, 0, 0) };
            metaWrap.Children.Add(CreateStaticTeamChip(item.FileExtension, accentBrush, Brushes.White));
            metaWrap.Children.Add(CreateStaticTeamChip(FormatFilesHubSize(item.FileSizeBytes), GetThemeBrush("MutedCardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            metaWrap.Children.Add(CreateStaticTeamChip($"Guardado {FormatRelativeDate(item.AddedAt)}", GetThemeBrush("MutedCardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            if (isSynced)
            {
                metaWrap.Children.Add(CreateStaticTeamChip($"Sync v{Math.Max(1, item.RemoteVersion)}", GetThemeBrush("AccentMutedBrush"), GetThemeBrush("AccentBrush")));
                if (!string.IsNullOrWhiteSpace(item.RemoteTeamName))
                {
                    metaWrap.Children.Add(CreateStaticTeamChip(item.RemoteTeamName, GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
                }
                metaWrap.Children.Add(CreateStaticTeamChip(GetPermissionScopeLabel(item.RemotePermissionScope), GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            }
            if (!fileExists)
            {
                metaWrap.Children.Add(CreateStaticTeamChip(
                    isSynced ? "Cache local ausente • restaurável" : "Arquivo local indisponível",
                    new SolidColorBrush(Color.FromRgb(220, 38, 38)),
                    Brushes.White));
            }
            leftStack.Children.Add(metaWrap);
            layout.Children.Add(leftStack);

            var actions = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(18, 0, 0, 0)
            };

            var openButton = CreateFilesHubActionButton(fileExists ? "Visualizar" : "Indisponivel", accentBrush, Brushes.White, Brushes.Transparent, OpenFilesHubItem_Click, item, 112);
            openButton.Content = fileExists || isSynced ? "Visualizar" : "Indisponível";
            openButton.IsEnabled = fileExists || isSynced;
            var syncButton = CreateFilesHubActionButton(isSynced ? "Nova versão" : "Sincronizar", GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("AccentBrush"), GetThemeBrush("AccentBrush"), SyncFilesHubItem_Click, item, 122);
            var removeButton = CreateFilesHubActionButton("Remover", GetThemeBrush("MutedCardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush"), GetThemeBrush("CardBorderBrush"), RemoveFilesHubItem_Click, item, 104);
            actions.Children.Add(openButton);
            actions.Children.Add(syncButton);
            actions.Children.Add(removeButton);

            Grid.SetColumn(actions, 1);
            layout.Children.Add(actions);

            return new Border
            {
                Margin = new Thickness(0, 0, 0, 12),
                Padding = new Thickness(16),
                Background = GetThemeBrush("MutedCardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(18),
                Child = layout
            };
        }

        private Button CreateFilesHubActionButton(string label, Brush background, Brush foreground, Brush borderBrush, RoutedEventHandler clickHandler, object? tag = null, double minWidth = 124)
        {
            var button = new Button
            {
                Content = label,
                MinWidth = minWidth,
                Height = 38,
                Margin = new Thickness(0, 0, 10, 0),
                Padding = new Thickness(14, 8, 14, 8),
                Background = background,
                Foreground = foreground,
                BorderBrush = borderBrush,
                BorderThickness = borderBrush == Brushes.Transparent ? new Thickness(0) : new Thickness(1),
                FontWeight = FontWeights.SemiBold,
                Cursor = Cursors.Hand,
                Tag = tag
            };
            button.Click += clickHandler;
            return button;
        }

        private async void OpenFilesHubItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not FilesHubItem item)
            {
                return;
            }

            if (!File.Exists(item.StoredFilePath))
            {
                if (await RestoreFilesHubItemFromRemoteAsync(item) && File.Exists(item.StoredFilePath))
                {
                    _filesHubStatusMessage = $"{item.FileName} foi restaurado do Firebase para o hub local.";
                    RenderFilesHub();
                }
                if (!File.Exists(item.StoredFilePath))
                {
                    ShowStyledAlertDialog(
                        "ARQUIVOS",
                        "Arquivo indisponivel",
                        IsFilesHubItemSynced(item)
                            ? "O cache local não foi encontrado e a restauração remota falhou. Verifique a conectividade com o Firebase ou publique uma nova versão do arquivo."
                            : "O item continua listado, mas o arquivo local nao foi encontrado. Remova-o ou adicione novamente se precisar restaurar esse slot.",
                        "Fechar",
                        new SolidColorBrush(Color.FromRgb(220, 38, 38)));
                    return;
                }
            }

            try
            {
                ShowFilesHubItemPreviewDialog(item);
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"Falha ao abrir arquivo do hub: {ex.Message}");
                ShowStyledAlertDialog(
                    "ARQUIVOS",
                    "Nao foi possivel abrir",
                    "O app nao conseguiu montar a visualizacao interna desse item agora. Verifique se o arquivo continua intacto e tente novamente.",
                    "Fechar",
                    new SolidColorBrush(Color.FromRgb(234, 88, 12)));
            }
        }

        private async void SyncFilesHubItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not FilesHubItem item)
            {
                return;
            }

            if (IsFilesHubItemSynced(item))
            {
                var existingTeam = FindTeamById(item.RemoteTeamId);
                if (existingTeam == null)
                {
                    ShowStyledAlertDialog(
                        "ARQUIVOS",
                        "Equipe não carregada",
                        "A equipe remota vinculada a este arquivo não está disponível no momento. Recarregue suas equipes antes de publicar uma nova versão.",
                        "Fechar",
                        GetThemeBrush("AccentBrush"));
                    return;
                }

                if (!CanCurrentUserUploadFiles(existingTeam))
                {
                    ShowStyledAlertDialog(
                        "ARQUIVOS",
                        "Permissão insuficiente",
                        "Seu papel atual nessa equipe não permite publicar novas versões de arquivo.",
                        "Fechar",
                        GetThemeBrush("AccentBrush"));
                    return;
                }

                var replacementDialog = new OpenFileDialog
                {
                    Multiselect = false,
                    Title = "Escolher nova versão do arquivo",
                    Filter = "Todos os arquivos|*.*"
                };

                if (replacementDialog.ShowDialog() != true)
                {
                    return;
                }

                var replacementItem = ArchiveFilesHubItem(replacementDialog.FileName, new FilesHubAssociationSelection
                {
                    AssociationType = item.AssociationType,
                    AssociationLabel = item.AssociationLabel
                });

                try
                {
                    if (File.Exists(item.StoredFilePath))
                    {
                        File.Delete(item.StoredFilePath);
                    }
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine($"Falha ao substituir arquivo local no hub: {ex.Message}");
                }

                item.FileName = replacementItem.FileName;
                item.StoredFilePath = replacementItem.StoredFilePath;
                item.FileSizeBytes = replacementItem.FileSizeBytes;
                item.FileExtension = replacementItem.FileExtension;
                item.AddedAt = DateTime.Now;

                var syncSelection = ShowFilesHubSyncDialog(item, existingTeam, item.RemotePermissionScope, $"Versão {Math.Max(1, item.RemoteVersion) + 1} enviada pelo hub local.");
                if (syncSelection == null)
                {
                    TrySaveFilesHubState(true);
                    RenderFilesHub();
                    return;
                }

                await SynchronizeFilesHubItemAsync(item, syncSelection);
                return;
            }

            if (!File.Exists(item.StoredFilePath))
            {
                ShowStyledAlertDialog(
                    "ARQUIVOS",
                    "Arquivo local ausente",
                    "O arquivo precisa existir no hub local antes da primeira sincronização remota.",
                    "Fechar",
                    GetThemeBrush("AccentBrush"));
                return;
            }

            var selection = ShowFilesHubSyncDialog(item);
            if (selection == null)
            {
                return;
            }

            await SynchronizeFilesHubItemAsync(item, selection);
        }

        private void ShowFilesHubItemPreviewDialog(FilesHubItem item)
        {
            var accentBrush = new SolidColorBrush(GetFilesHubAssociationAccentColor(item.AssociationType));
            var dialog = CreateStyledDialogWindow(item.FileName, 980, 760, 620, true);

            var root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            root.Children.Add(CreateDialogHeader(
                "ARQUIVOS",
                item.FileName,
                "Visualizacao interna do arquivo dentro do hub, sem depender de aplicativo externo.",
                accentBrush));

            var metaWrap = new WrapPanel { Margin = new Thickness(0, 0, 0, 18) };
            metaWrap.Children.Add(CreateStaticTeamChip(item.FileExtension, accentBrush, Brushes.White));
            metaWrap.Children.Add(CreateStaticTeamChip(GetFilesHubPreviewLabel(item.StoredFilePath), GetThemeBrush("MutedCardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            metaWrap.Children.Add(CreateStaticTeamChip(FormatFilesHubSize(item.FileSizeBytes), GetThemeBrush("MutedCardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            metaWrap.Children.Add(CreateStaticTeamChip($"Guardado {FormatRelativeDate(item.AddedAt)}", GetThemeBrush("MutedCardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            if (!string.IsNullOrWhiteSpace(item.AssociationLabel))
            {
                metaWrap.Children.Add(CreateStaticTeamChip(item.AssociationLabel, CreateSoftAccentBrush(accentBrush, 28), accentBrush));
            }
            Grid.SetRow(metaWrap, 1);
            root.Children.Add(metaWrap);

            var previewHost = new Border
            {
                Background = GetThemeBrush("MutedCardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(22),
                Child = CreateFilesHubPreviewLoadingState()
            };
            Grid.SetRow(previewHost, 2);
            root.Children.Add(previewHost);

            var footer = new Border
            {
                Padding = new Thickness(0, 18, 0, 0),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(0, 1, 0, 0)
            };
            var closeButton = CreateDialogActionButton("Fechar", accentBrush, Brushes.White, Brushes.Transparent, 112);
            closeButton.Click += (_, __) => dialog.Close();
            footer.Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Children = { closeButton }
            };
            Grid.SetRow(footer, 3);
            root.Children.Add(footer);

            dialog.Content = CreateStyledDialogShell(root);
            dialog.Loaded += async (_, __) =>
            {
                try
                {
                    previewHost.Child = await CreateFilesHubPreviewContentAsync(item);
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine($"Falha ao montar preview interno: {ex.Message}");
                    previewHost.Child = CreateFilesHubPreviewMessage(
                        PackIconMaterialKind.AlertCircleOutline,
                        "Falha ao montar preview",
                        "O arquivo continua salvo, mas o visualizador interno nao conseguiu renderizar esse conteudo agora.");
                }
            };

            dialog.ShowDialog();
        }

        private UIElement CreateFilesHubPreviewLoadingState()
        {
            var stack = new StackPanel
            {
                Width = 280,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            stack.Children.Add(new PackIconMaterial
            {
                Kind = PackIconMaterialKind.FolderSearchOutline,
                Width = 32,
                Height = 32,
                Foreground = GetThemeBrush("AccentBrush"),
                HorizontalAlignment = HorizontalAlignment.Center
            });
            stack.Children.Add(new TextBlock
            {
                Text = "Preparando visualizacao...",
                Margin = new Thickness(0, 14, 0, 0),
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = GetThemeBrush("PrimaryTextBrush"),
                TextAlignment = TextAlignment.Center
            });
            stack.Children.Add(new TextBlock
            {
                Text = "O arquivo sera carregado dentro do aplicativo assim que o preview estiver pronto.",
                Margin = new Thickness(0, 8, 0, 0),
                FontSize = 12,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                LineHeight = 20
            });
            stack.Children.Add(new ProgressBar
            {
                Margin = new Thickness(0, 16, 0, 0),
                Height = 6,
                IsIndeterminate = true,
                Foreground = GetThemeBrush("AccentBrush")
            });

            return stack;
        }

        private async Task<UIElement> CreateFilesHubPreviewContentAsync(FilesHubItem item)
        {
            var extension = GetFilesHubExtension(item.StoredFilePath, item.FileExtension);

            if (IsFilesHubImageExtension(extension))
            {
                return CreateFilesHubImagePreview(item.StoredFilePath);
            }

            if (string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase))
            {
                return await CreateFilesHubPdfPreviewAsync(item.StoredFilePath);
            }

            if (IsFilesHubWordExtension(extension))
            {
                var content = await ExtractWordDocumentTextAsync(item.StoredFilePath);
                return CreateFilesHubTextPreview(content, wrapText: true, useMonospace: false);
            }

            if (IsFilesHubSpreadsheetExtension(extension))
            {
                var content = await ExtractSpreadsheetTextAsync(item.StoredFilePath);
                return CreateFilesHubTextPreview(content, wrapText: false, useMonospace: true);
            }

            if (IsFilesHubPresentationExtension(extension))
            {
                var content = await ExtractPresentationTextAsync(item.StoredFilePath);
                return CreateFilesHubTextPreview(content, wrapText: true, useMonospace: false);
            }

            if (IsFilesHubTextExtension(extension))
            {
                var content = await File.ReadAllTextAsync(item.StoredFilePath);
                return CreateFilesHubTextPreview(content, wrapText: ShouldWrapFilesHubText(extension), useMonospace: ShouldUseFilesHubMonospace(extension));
            }

            return CreateFilesHubPreviewMessage(
                PackIconMaterialKind.FileQuestionOutline,
                "Preview ainda nao disponivel",
                "Esse formato ainda nao tem visualizacao interna completa no hub. O arquivo continua guardado, mas o viewer atual cobre imagem, PDF, texto e documentos Office mais comuns.");
        }

        private UIElement CreateFilesHubImagePreview(string filePath)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(filePath, UriKind.Absolute);
            bitmap.EndInit();

            if (bitmap.CanFreeze)
            {
                bitmap.Freeze();
            }

            var image = new Image
            {
                Source = bitmap,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(24),
                CacheMode = new BitmapCache()
            };
            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);

            return new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = new Grid
                {
                    Background = GetThemeBrush("MutedCardBackgroundBrush"),
                    Children =
                    {
                        new Border
                        {
                            Margin = new Thickness(24),
                            Padding = new Thickness(12),
                            Background = GetThemeBrush("CardBackgroundBrush"),
                            BorderBrush = GetThemeBrush("CardBorderBrush"),
                            BorderThickness = new Thickness(1),
                            CornerRadius = new CornerRadius(20),
                            Child = image
                        }
                    }
                }
            };
        }

        private async Task<UIElement> CreateFilesHubPdfPreviewAsync(string filePath)
        {
            var webView = new WebView2
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Thickness(0)
            };

            try
            {
                await webView.EnsureCoreWebView2Async();

                if (webView.CoreWebView2 != null)
                {
                    webView.CoreWebView2.Settings.AreDevToolsEnabled = false;
                    webView.CoreWebView2.Settings.IsStatusBarEnabled = true;
                    webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
                }

                webView.Source = new Uri(filePath, UriKind.Absolute);
                return webView;
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"Falha ao inicializar preview PDF interno: {ex.Message}");
                return CreateFilesHubPreviewMessage(
                    PackIconMaterialKind.FilePdfBox,
                    "PDF indisponivel no viewer interno",
                    "O motor interno de visualizacao PDF nao conseguiu iniciar agora. Tente novamente com o WebView2 instalado e ativo no Windows.");
            }
        }

        private UIElement CreateFilesHubTextPreview(string content, bool wrapText, bool useMonospace)
        {
            var textBox = new TextBox
            {
                Text = string.IsNullOrWhiteSpace(content) ? "Nenhum conteudo legivel foi encontrado para este arquivo." : content,
                IsReadOnly = true,
                AcceptsReturn = true,
                AcceptsTab = true,
                TextWrapping = wrapText ? TextWrapping.Wrap : TextWrapping.NoWrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = wrapText ? ScrollBarVisibility.Disabled : ScrollBarVisibility.Auto,
                BorderThickness = new Thickness(0),
                Background = GetThemeBrush("CardBackgroundBrush"),
                Foreground = GetThemeBrush("PrimaryTextBrush"),
                FontSize = useMonospace ? 12 : 13,
                Padding = new Thickness(18)
            };

            textBox.FontFamily = useMonospace
                ? new FontFamily("Cascadia Mono")
                : ((TryFindResource("AppTextFontFamily") as FontFamily) ?? textBox.FontFamily);

            return textBox;
        }

        private UIElement CreateFilesHubPreviewMessage(PackIconMaterialKind iconKind, string title, string description)
        {
            return new Border
            {
                Padding = new Thickness(26),
                Background = GetThemeBrush("CardBackgroundBrush"),
                Child = new StackPanel
                {
                    Width = 420,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Children =
                    {
                        new PackIconMaterial
                        {
                            Kind = iconKind,
                            Width = 34,
                            Height = 34,
                            Foreground = GetThemeBrush("AccentBrush"),
                            HorizontalAlignment = HorizontalAlignment.Center
                        },
                        new TextBlock
                        {
                            Text = title,
                            Margin = new Thickness(0, 14, 0, 0),
                            FontSize = 20,
                            FontWeight = FontWeights.ExtraBold,
                            Foreground = GetThemeBrush("PrimaryTextBrush"),
                            TextAlignment = TextAlignment.Center,
                            TextWrapping = TextWrapping.Wrap
                        },
                        new TextBlock
                        {
                            Text = description,
                            Margin = new Thickness(0, 10, 0, 0),
                            FontSize = 12,
                            Foreground = GetThemeBrush("SecondaryTextBrush"),
                            TextWrapping = TextWrapping.Wrap,
                            TextAlignment = TextAlignment.Center,
                            LineHeight = 20
                        }
                    }
                }
            };
        }

        private string GetFilesHubExtension(string filePath, string fallbackExtension)
        {
            var extension = IOPath.GetExtension(filePath);
            if (!string.IsNullOrWhiteSpace(extension))
            {
                return extension.Trim().ToLowerInvariant();
            }

            return string.IsNullOrWhiteSpace(fallbackExtension)
                ? string.Empty
                : $".{fallbackExtension.Trim().TrimStart('.').ToLowerInvariant()}";
        }

        private string GetFilesHubPreviewLabel(string filePath)
        {
            var extension = GetFilesHubExtension(filePath, string.Empty);

            if (IsFilesHubImageExtension(extension))
            {
                return "Imagem";
            }

            if (string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase))
            {
                return "PDF";
            }

            if (IsFilesHubWordExtension(extension))
            {
                return "Documento";
            }

            if (IsFilesHubSpreadsheetExtension(extension))
            {
                return "Planilha";
            }

            if (IsFilesHubPresentationExtension(extension))
            {
                return "Apresentacao";
            }

            if (IsFilesHubTextExtension(extension))
            {
                return "Texto";
            }

            return "Arquivo local";
        }

        private bool IsFilesHubImageExtension(string extension)
        {
            return extension is ".png" or ".jpg" or ".jpeg" or ".bmp" or ".gif" or ".tif" or ".tiff";
        }

        private bool IsFilesHubWordExtension(string extension)
        {
            return extension is ".docx";
        }

        private bool IsFilesHubSpreadsheetExtension(string extension)
        {
            return extension is ".xlsx";
        }

        private bool IsFilesHubPresentationExtension(string extension)
        {
            return extension is ".pptx";
        }

        private bool IsFilesHubTextExtension(string extension)
        {
            return extension is ".txt" or ".md" or ".markdown" or ".json" or ".xml" or ".csv" or ".log" or ".yml" or ".yaml" or ".rules" or ".cs" or ".xaml" or ".csproj" or ".js" or ".ts" or ".html" or ".css" or ".sql";
        }

        private bool ShouldWrapFilesHubText(string extension)
        {
            return extension is ".txt" or ".md" or ".markdown" or ".log" or ".docx" or ".pptx";
        }

        private bool ShouldUseFilesHubMonospace(string extension)
        {
            return extension is ".json" or ".xml" or ".csv" or ".yml" or ".yaml" or ".rules" or ".cs" or ".xaml" or ".csproj" or ".js" or ".ts" or ".html" or ".css" or ".sql" or ".xlsx";
        }

        private async Task<string> ExtractWordDocumentTextAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                using var document = WordprocessingDocument.Open(filePath, false);
                var paragraphs = document.MainDocumentPart?.Document?.Body?
                    .Descendants<W.Paragraph>()
                    .Select(paragraph => string.Concat(paragraph.Descendants<W.Text>().Select(text => text.Text)).Trim())
                    .Where(text => !string.IsNullOrWhiteSpace(text))
                    .Take(600)
                    .ToList();

                if (paragraphs == null || paragraphs.Count == 0)
                {
                    return "Nenhum texto legivel foi encontrado neste documento Word.";
                }

                return string.Join(Environment.NewLine + Environment.NewLine, paragraphs);
            });
        }

        private async Task<string> ExtractSpreadsheetTextAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                using var document = SpreadsheetDocument.Open(filePath, false);
                var workbookPart = document.WorkbookPart;
                var firstSheet = workbookPart?.Workbook?.Sheets?.Elements<S.Sheet>().FirstOrDefault();
                if (workbookPart == null || firstSheet == null)
                {
                    return "Nenhum conteudo legivel foi encontrado nesta planilha.";
                }

                var sharedStringTable = workbookPart.SharedStringTablePart?.SharedStringTable;
                var worksheetPart = (WorksheetPart)workbookPart.GetPartById(firstSheet.Id!);
                var rows = worksheetPart.Worksheet.Descendants<S.Row>().Take(200);
                var lines = new List<string>();

                foreach (var row in rows)
                {
                    var values = row.Elements<S.Cell>()
                        .Take(24)
                        .Select(cell => GetSpreadsheetCellValue(cell, sharedStringTable))
                        .Where(value => !string.IsNullOrWhiteSpace(value))
                        .ToList();

                    if (values.Count > 0)
                    {
                        lines.Add(string.Join(" | ", values));
                    }
                }

                if (lines.Count == 0)
                {
                    return "Nenhum conteudo legivel foi encontrado nesta planilha.";
                }

                return string.Join(Environment.NewLine, lines);
            });
        }

        private string GetSpreadsheetCellValue(S.Cell cell, S.SharedStringTable? sharedStringTable)
        {
            var rawValue = cell.CellValue?.InnerText ?? cell.InnerText ?? string.Empty;

            if (cell.DataType == null)
            {
                return rawValue;
            }

            var dataType = cell.DataType.Value;

            if (dataType == S.CellValues.SharedString && int.TryParse(rawValue, out var sharedIndex) && sharedStringTable != null)
            {
                return sharedStringTable.ElementAtOrDefault(sharedIndex)?.InnerText ?? string.Empty;
            }

            if (dataType == S.CellValues.Boolean)
            {
                return rawValue == "1" ? "TRUE" : "FALSE";
            }

            if (dataType == S.CellValues.InlineString)
            {
                return cell.InnerText ?? string.Empty;
            }

            return rawValue;
        }

        private async Task<string> ExtractPresentationTextAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                using var document = PresentationDocument.Open(filePath, false);
                var slideParts = document.PresentationPart?.SlideParts?.Take(60).ToList();
                if (slideParts == null || slideParts.Count == 0)
                {
                    return "Nenhum texto legivel foi encontrado nesta apresentacao.";
                }

                var slides = new List<string>();
                for (var index = 0; index < slideParts.Count; index++)
                {
                    var slideText = string.Join(" ", slideParts[index].Slide.Descendants<A.Text>()
                        .Select(text => text.Text?.Trim())
                        .Where(text => !string.IsNullOrWhiteSpace(text)));

                    slides.Add(string.IsNullOrWhiteSpace(slideText)
                        ? $"Slide {index + 1}{Environment.NewLine}(Sem texto legivel neste slide)"
                        : $"Slide {index + 1}{Environment.NewLine}{slideText}");
                }

                return string.Join(Environment.NewLine + Environment.NewLine, slides);
            });
        }

        private void RemoveFilesHubItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not FilesHubItem item)
            {
                return;
            }

            if (!ShowStyledConfirmationDialog(
                "ARQUIVOS",
                "Remover arquivo",
                IsFilesHubItemSynced(item)
                    ? $"{item.FileName} sairá da mochila local e o cache arquivado será removido deste perfil. A cópia remota da equipe permanecerá ativa no Firebase."
                    : $"{item.FileName} saira da mochila local e o arquivo arquivado tambem sera removido do armazenamento local.",
                "Remover",
                new SolidColorBrush(Color.FromRgb(220, 38, 38))))
            {
                return;
            }

            _filesHubState.Items.RemoveAll(existing => string.Equals(existing.ItemId, item.ItemId, StringComparison.OrdinalIgnoreCase));

            try
            {
                if (File.Exists(item.StoredFilePath))
                {
                    File.Delete(item.StoredFilePath);
                }
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"Falha ao excluir arquivo local do hub: {ex.Message}");
            }

            _filesHubStatusMessage = IsFilesHubItemSynced(item)
                ? $"{item.FileName} foi removido do hub local. A versão remota continua disponível na equipe vinculada."
                : $"{item.FileName} foi removido da mochila local.";
            TrySaveFilesHubState(true);
            RenderFilesHub();
        }

        private int GetFilesHubAssociationOrder(string associationType)
        {
            return associationType.Trim().ToLowerInvariant() switch
            {
                "projeto" => 0,
                "sessao" => 1,
                "trabalho" => 2,
                "atividade" => 3,
                _ => 4
            };
        }

        private Color GetFilesHubAssociationAccentColor(string associationType)
        {
            return associationType.Trim().ToLowerInvariant() switch
            {
                "projeto" => Color.FromRgb(37, 99, 235),
                "sessao" => Color.FromRgb(14, 165, 233),
                "trabalho" => Color.FromRgb(16, 185, 129),
                "atividade" => Color.FromRgb(245, 158, 11),
                _ => Color.FromRgb(124, 58, 237)
            };
        }

        private string GetFilesHubAssociationDescription(string associationType)
        {
            return associationType.Trim().ToLowerInvariant() switch
            {
                "projeto" => "Base mais ampla da entrega ou do produto.",
                "sessao" => "Rodada, aula ou encontro com recorte especifico.",
                "trabalho" => "Entrega formal com objetivo fechado.",
                "atividade" => "Apoio pontual para tarefa ou checklist.",
                _ => "Material avulso aguardando uma trilha melhor."
            };
        }

        private string FormatFilesHubSize(long bytes)
        {
            if (bytes <= 0)
            {
                return "0 B";
            }

            var units = new[] { "B", "KB", "MB", "GB", "TB" };
            double value = bytes;
            var unitIndex = 0;
            while (value >= 1024 && unitIndex < units.Length - 1)
            {
                value /= 1024;
                unitIndex++;
            }

            return $"{value:0.#} {units[unitIndex]}";
        }

        private void RenderFilesChoasCompanion()
        {
            var accentBrush = GetThemeBrush("AccentBrush");
            var filesBrush = GetThemeBrush("FilesIconBrush");
            var accentMutedBrush = GetThemeBrush("AccentMutedBrush");
            var panelBackground = GetThemeBrush("CardBackgroundBrush");
            var mutedBackgroundBrush = GetThemeBrush("MutedCardBackgroundBrush");
            var borderBrush = GetThemeBrush("CardBorderBrush");
            var primaryTextBrush = GetThemeBrush("PrimaryTextBrush");
            var secondaryTextBrush = GetThemeBrush("SecondaryTextBrush");

            var panel = new Border
            {
                Width = 436,
                Padding = new Thickness(16),
                Background = panelBackground,
                BorderBrush = borderBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(18),
                SnapsToDevicePixels = true,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    BlurRadius = 18,
                    ShadowDepth = 6,
                    Opacity = 0.10,
                    Color = Color.FromRgb(15, 23, 42)
                }
            };

            var root = new StackPanel();
            root.Children.Add(new Border
            {
                Width = 42,
                Height = 4,
                CornerRadius = new CornerRadius(999),
                Background = accentBrush,
                Margin = new Thickness(0, 0, 0, 14)
            });

            var summaryGrid = new Grid();
            summaryGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            summaryGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            summaryGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var mascotContent = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var mascotImageHost = new Border
            {
                Width = 108,
                Height = 84,
                CornerRadius = new CornerRadius(16),
                Background = mutedBackgroundBrush,
                BorderBrush = borderBrush,
                BorderThickness = new Thickness(1),
                SnapsToDevicePixels = true,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    BlurRadius = 12,
                    ShadowDepth = 0,
                    Opacity = 0.08,
                    Color = Color.FromRgb(15, 23, 42)
                }
            };

            var mascotImage = new Image
            {
                Width = 92,
                Height = 72,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                CacheMode = new BitmapCache()
            };
            RenderOptions.SetBitmapScalingMode(mascotImage, BitmapScalingMode.LowQuality);

            if (!TryAttachChoasAnimation(mascotImage))
            {
                mascotImageHost.Child = new TextBlock
                {
                    Text = "CHOAS",
                    FontSize = 18,
                    FontWeight = FontWeights.ExtraBold,
                    Foreground = filesBrush,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Center
                };
            }
            else
            {
                mascotImageHost.Child = mascotImage;
            }

            mascotContent.Children.Add(mascotImageHost);
            mascotContent.Children.Add(new TextBlock
            {
                Text = "CHOAS",
                Margin = new Thickness(0, 8, 0, 0),
                FontSize = 13,
                FontWeight = FontWeights.ExtraBold,
                Foreground = primaryTextBrush,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center
            });
            mascotContent.Children.Add(new TextBlock
            {
                Text = "guia da mochila",
                FontSize = 10.5,
                Foreground = secondaryTextBrush,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center
            });

            var mascotButton = new Button
            {
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(0),
                Cursor = Cursors.Hand,
                ToolTip = "CHOAS",
                Content = mascotContent
            };
            mascotButton.Click += ChoasCompanion_Click;

            summaryGrid.Children.Add(mascotButton);

            var infoStack = new StackPanel
            {
                Margin = new Thickness(16, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            infoStack.Children.Add(new Border
            {
                Background = accentMutedBrush,
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                CornerRadius = new CornerRadius(999),
                Padding = new Thickness(10, 4, 10, 4),
                HorizontalAlignment = HorizontalAlignment.Left,
                Child = new TextBlock
                {
                    Text = "Assistente de arquivos",
                    FontSize = 10,
                    FontWeight = FontWeights.Bold,
                    Foreground = accentBrush
                }
            });
            infoStack.Children.Add(new TextBlock
            {
                Text = "CHOAS",
                Margin = new Thickness(0, 10, 0, 0),
                FontSize = 22,
                FontWeight = FontWeights.ExtraBold,
                Foreground = primaryTextBrush
            });
            infoStack.Children.Add(new TextBlock
            {
                Text = "consultor da mochila",
                Margin = new Thickness(0, 3, 0, 0),
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = filesBrush
            });
            infoStack.Children.Add(new TextBlock
            {
                Text = "Use o CHOAS para revisar onde cada arquivo entra e abrir a previa da camada inteligente quando precisar.",
                Margin = new Thickness(0, 10, 0, 0),
                FontSize = 12,
                Foreground = secondaryTextBrush,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20,
                MaxWidth = 218
            });
            var tags = new WrapPanel { Margin = new Thickness(0, 12, 0, 0) };
            tags.Children.Add(CreateStaticTeamChip("Arquivos", accentMutedBrush, accentBrush));
            tags.Children.Add(CreateStaticTeamChip("Guia rapido", mutedBackgroundBrush, primaryTextBrush));
            infoStack.Children.Add(tags);
            Grid.SetColumn(infoStack, 1);
            summaryGrid.Children.Add(infoStack);

            var openButton = new Button
            {
                Content = "Ver opcoes",
                MinWidth = 110,
                Height = 36,
                Padding = new Thickness(14, 8, 14, 8),
                Background = accentBrush,
                Foreground = Brushes.White,
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                FontWeight = FontWeights.SemiBold,
                Cursor = Cursors.Hand,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(16, 0, 0, 0)
            };
            openButton.Click += ChoasCompanion_Click;
            Grid.SetColumn(openButton, 2);
            summaryGrid.Children.Add(openButton);

            root.Children.Add(summaryGrid);

            if (_showChoasIntroBubble)
            {
                root.Children.Add(CreateChoasBubbleCard(accentBrush));
            }

            panel.Child = root;
            FilesChoasHost.Child = panel;
        }

        private Border CreateChoasBubbleCard(Brush accentBrush)
        {
            var primaryTextBrush = GetThemeBrush("PrimaryTextBrush");
            var secondaryTextBrush = GetThemeBrush("SecondaryTextBrush");
            var borderBrush = GetThemeBrush("CardBorderBrush");
            var mutedBackgroundBrush = GetThemeBrush("MutedCardBackgroundBrush");
            var accentMutedBrush = GetThemeBrush("AccentMutedBrush");
            var chips = new WrapPanel();
            chips.Children.Add(CreateStaticTeamChip("Projeto", accentMutedBrush, accentBrush));
            chips.Children.Add(CreateStaticTeamChip("Sessao", mutedBackgroundBrush, primaryTextBrush));
            chips.Children.Add(CreateStaticTeamChip("Trabalho", mutedBackgroundBrush, primaryTextBrush));
            chips.Children.Add(CreateStaticTeamChip("Atividade", mutedBackgroundBrush, primaryTextBrush));

            var bubbleGrid = new Grid();
            bubbleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            bubbleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var textStack = new StackPanel();
            textStack.Children.Add(new Border
            {
                Background = accentMutedBrush,
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                CornerRadius = new CornerRadius(999),
                Padding = new Thickness(10, 4, 10, 4),
                HorizontalAlignment = HorizontalAlignment.Left,
                Child = new TextBlock
                {
                    Text = "Guia rapido",
                    FontSize = 10,
                    FontWeight = FontWeights.Bold,
                    Foreground = accentBrush
                }
            });
            textStack.Children.Add(new TextBlock
            {
                Text = "Classifique antes de acumular.",
                Margin = new Thickness(0, 10, 0, 0),
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = primaryTextBrush,
                TextWrapping = TextWrapping.Wrap
            });
            textStack.Children.Add(new TextBlock
            {
                Text = "Projeto guarda a camada macro. Sessao cobre aula, encontro ou sprint. Trabalho segura o que pertence a uma entrega. Atividade serve para apoio rapido, checklist e rascunho.",
                Margin = new Thickness(0, 10, 0, 0),
                FontSize = 12,
                Foreground = secondaryTextBrush,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20
            });
            textStack.Children.Add(new Border
            {
                Margin = new Thickness(0, 14, 0, 0),
                Child = chips
            });
            bubbleGrid.Children.Add(textStack);

            var closeButton = new Button
            {
                Content = "Fechar",
                MinWidth = 72,
                Height = 34,
                Margin = new Thickness(12, 0, 0, 0),
                Padding = new Thickness(12, 6, 12, 6),
                Background = mutedBackgroundBrush,
                Foreground = primaryTextBrush,
                BorderBrush = borderBrush,
                BorderThickness = new Thickness(1),
                FontWeight = FontWeights.SemiBold,
                Cursor = Cursors.Hand
            };
            closeButton.Click += CloseChoasBubble_Click;
            Grid.SetColumn(closeButton, 1);
            bubbleGrid.Children.Add(closeButton);

            return new Border
            {
                Margin = new Thickness(0, 16, 0, 0),
                Padding = new Thickness(16),
                Background = mutedBackgroundBrush,
                BorderBrush = borderBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(18),
                Child = bubbleGrid
            };
        }

        private void CloseChoasBubble_Click(object sender, RoutedEventArgs e)
        {
            _showChoasIntroBubble = false;
            RefreshChoasPresentation();
        }

        private void ChoasCompanion_Click(object sender, RoutedEventArgs e)
        {
            var choice = ShowChoasChoiceDialog();
            if (string.Equals(choice, "Arquivos", StringComparison.OrdinalIgnoreCase))
            {
                _showChoasIntroBubble = true;
                _filesHubStatusMessage = "CHOAS reabriu o guia rapido da mochila.";
                RefreshChoasPresentation();
                return;
            }

            if (string.Equals(choice, "Obsseract.IA", StringComparison.OrdinalIgnoreCase))
            {
                ShowChoasWorkInProgressDialog();
            }
        }

        private void ShowChoasWorkInProgressDialog()
        {
            var accentBrush = GetThemeBrush("AccentBrush");
            var filesBrush = GetThemeBrush("FilesIconBrush");
            var dialog = CreateStyledDialogWindow("Obsseract.IA", 680, 500, 460);

            var root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            root.Children.Add(CreateDialogHeader(
                "CHOAS",
                "Obsseract.IA em preparo",
                "A camada inteligente do hub ainda esta sendo montada.",
                accentBrush));

            var statusChips = new WrapPanel();
            statusChips.Children.Add(CreateStaticTeamChip("W.I.P.", CreateSoftAccentBrush(filesBrush, 28), filesBrush));
            statusChips.Children.Add(CreateStaticTeamChip("Consulta inteligente", GetThemeBrush("MutedCardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));

            var overviewContent = new StackPanel();
            overviewContent.Children.Add(new TextBlock
            {
                Text = "Quando essa rota entrar, o CHOAS vai usar o contexto dos arquivos guardados para sugerir consultas, atalhos e agrupamentos mais certeiros.",
                FontSize = 12,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20
            });
            overviewContent.Children.Add(new Border
            {
                Margin = new Thickness(0, 14, 0, 0),
                Child = statusChips
            });

            var nextStepContent = new StackPanel();
            nextStepContent.Children.Add(new TextBlock
            {
                Text = "Por enquanto, o fluxo principal continua sendo o guia de Arquivos acionado pelo proprio CHOAS.",
                FontSize = 12,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20
            });

            var body = new StackPanel();
            body.Children.Add(CreateDialogSectionCard(
                "Estado atual",
                "Ainda nao disponivel nesta versao.",
                accentBrush,
                overviewContent));
            body.Children.Add(CreateDialogSectionCard(
                "Enquanto isso",
                "Use o hub para guardar, revisar e classificar seus materiais.",
                accentBrush,
                nextStepContent,
                new Thickness(0, 0, 0, 0)));

            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = body
            };
            Grid.SetRow(scrollViewer, 1);
            root.Children.Add(scrollViewer);

            var footer = new Border
            {
                Padding = new Thickness(0, 18, 0, 0),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(0, 1, 0, 0)
            };
            var closeButton = CreateDialogActionButton("Fechar", accentBrush, Brushes.White, Brushes.Transparent, 112);
            closeButton.Click += (_, __) => dialog.Close();
            footer.Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Children = { closeButton }
            };
            Grid.SetRow(footer, 2);
            root.Children.Add(footer);

            dialog.Content = CreateStyledDialogShell(root);
            dialog.ShowDialog();
        }

        private string? ShowChoasChoiceDialog()
        {
            var accentBrush = GetThemeBrush("AccentBrush");
            var filesBrush = GetThemeBrush("FilesIconBrush");
            var dialog = CreateStyledDialogWindow("CHOAS", 640, 460, 430);
            string? selectedChoice = null;

            var root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            root.Children.Add(CreateDialogHeader(
                "CHOAS",
                "O que voce quer abrir?",
                "Escolha entre revisar o guia dos arquivos ou ver a previa da camada inteligente.",
                accentBrush));

            var choicePanel = new WrapPanel();
            choicePanel.Children.Add(CreateChoasRouteButton(
                "Arquivos",
                "Reabre o guia rapido da mochila e destaca como cada material pode ser organizado.",
                accentBrush,
                PackIconMaterialKind.FolderOutline,
                (_, __) =>
                {
                    selectedChoice = "Arquivos";
                    dialog.DialogResult = true;
                    dialog.Close();
                },
                272));
            choicePanel.Children.Add(CreateChoasRouteButton(
                "Obsseract.IA",
                "Abre a tela W.I.P. da camada inteligente prevista para o hub.",
                filesBrush,
                PackIconMaterialKind.CardsOutline,
                (_, __) =>
                {
                    selectedChoice = "Obsseract.IA";
                    dialog.DialogResult = true;
                    dialog.Close();
                },
                272));

            var helperContent = new StackPanel();
            helperContent.Children.Add(new TextBlock
            {
                Text = "O GIF do CHOAS continua sendo o ponto de entrada. Arquivos reabre a explicacao do hub e Obsseract.IA mostra o que ainda esta em preparo.",
                FontSize = 12,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20
            });

            var body = new StackPanel();
            body.Children.Add(CreateDialogSectionCard(
                "Rotas disponiveis",
                "Escolha uma rota para continuar.",
                accentBrush,
                choicePanel));
            body.Children.Add(CreateDialogSectionCard(
                "Como o CHOAS funciona",
                "O mascote continua no hub para orientar sem competir com o restante da interface.",
                accentBrush,
                helperContent,
                new Thickness(0, 0, 0, 0)));

            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = body
            };
            Grid.SetRow(scrollViewer, 1);
            root.Children.Add(scrollViewer);

            var footer = new Border
            {
                Padding = new Thickness(0, 18, 0, 0),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(0, 1, 0, 0)
            };
            var cancelButton = CreateDialogActionButton("Fechar", Brushes.Transparent, GetThemeBrush("PrimaryTextBrush"), GetThemeBrush("CardBorderBrush"), 112);
            cancelButton.Click += (_, __) => dialog.Close();
            footer.Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Children = { cancelButton }
            };
            Grid.SetRow(footer, 2);
            root.Children.Add(footer);

            dialog.Content = CreateStyledDialogShell(root);
            return dialog.ShowDialog() == true ? selectedChoice : null;
        }
        private Button CreateChoasRouteButton(string title, string description, Brush accentBrush, PackIconMaterialKind iconKind, RoutedEventHandler onClick, double width = 272)
        {
            var card = new Grid();
            card.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            card.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            card.Children.Add(new Border
            {
                Width = 42,
                Height = 42,
                CornerRadius = new CornerRadius(14),
                Background = CreateSoftAccentBrush(accentBrush, 30),
                Child = new PackIconMaterial
                {
                    Kind = iconKind,
                    Width = 20,
                    Height = 20,
                    Foreground = accentBrush,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            });

            var textStack = new StackPanel
            {
                Margin = new Thickness(12, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            textStack.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = GetThemeBrush("PrimaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            });
            textStack.Children.Add(new TextBlock
            {
                Text = description,
                Margin = new Thickness(0, 5, 0, 0),
                FontSize = 11.5,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 19
            });
            Grid.SetColumn(textStack, 1);
            card.Children.Add(textStack);

            var button = new Button
            {
                Width = width,
                MinHeight = 88,
                Margin = new Thickness(0, 0, 12, 12),
                Padding = new Thickness(14),
                Background = GetThemeBrush("CardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                Cursor = Cursors.Hand,
                Content = card
            };
            button.Click += onClick;
            return button;
        }

        private BitmapImage? GetChoasGifSource()
        {
            var gifPath = ResolveChoasGifPath();
            if (string.IsNullOrWhiteSpace(gifPath))
            {
                _choasGifSource = null;
                _choasGifSourcePath = null;
                return null;
            }

            if (_choasGifSource != null && string.Equals(_choasGifSourcePath, gifPath, StringComparison.OrdinalIgnoreCase))
            {
                return _choasGifSource;
            }

            try
            {
                var source = new BitmapImage();
                source.BeginInit();
                source.CacheOption = BitmapCacheOption.OnLoad;
                source.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                source.DecodePixelWidth = 160;
                source.UriSource = new Uri(gifPath, UriKind.Absolute);
                source.EndInit();

                if (source.CanFreeze)
                {
                    source.Freeze();
                }

                _choasGifSource = source;
                _choasGifSourcePath = gifPath;
                return _choasGifSource;
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"Falha ao carregar cache do CHOAS.gif: {ex.Message}");
                _choasGifSource = null;
                _choasGifSourcePath = null;
                return null;
            }
        }

        private bool TryAttachChoasAnimation(Image image)
        {
            var source = GetChoasGifSource();
            if (source == null)
            {
                return false;
            }

            try
            {
                ImageBehavior.SetAnimatedSource(image, source);
                ImageBehavior.SetRepeatBehavior(image, RepeatBehavior.Forever);

                return true;
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"Falha ao iniciar CHOAS.gif: {ex.Message}");
                StopChoasAnimation();
                return false;
            }
        }

        private void StopChoasAnimation()
        {
            if (FilesChoasHost != null)
            {
                FilesChoasHost.Child = null;
            }
        }

        private string? ResolveChoasGifPath()
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var candidates = new[]
            {
                IOPath.Combine(baseDirectory, "img", "Archives", "CHOAS.gif"),
                IOPath.GetFullPath(IOPath.Combine(baseDirectory, "..", "..", "..", "img", "Archives", "CHOAS.gif")),
                IOPath.GetFullPath(IOPath.Combine(Environment.CurrentDirectory, "img", "Archives", "CHOAS.gif"))
            };

            return candidates.FirstOrDefault(candidate => File.Exists(candidate));
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
            TeamTemplateComboBox.ItemsSource = AcademicProjectTemplateCatalog.GetAll().OrderBy(item => item.Title).ToList();
            TeamTemplateComboBox.SelectedValuePath = nameof(AcademicProjectTemplateInfo.TemplateId);
            TeamMemberInput.ItemsSource = _teamMemberSearchResults;
            TeamMemberInput.AddHandler(TextBox.TextChangedEvent, new TextChangedEventHandler(TeamMemberInput_TextChanged));
            TeamNameTextBox.TextChanged += (_, __) => RenderDraftTeamLogoPreview();
            TeamCourseComboBox.SelectionChanged += (_, __) => SyncDraftTemplateSuggestion();
            TeamMemberRoleComboBox.SelectionChanged += (_, __) => UpdateTeamMemberRoleHint();

            TeamClassComboBox.Text = KnownTeamClasses.First();
            TeamClassIdTextBox.Text = "TURMA-PI-001";
            TeamAcademicTermTextBox.Text = ResolveCurrentAcademicTerm();
            TeamMemberRoleComboBox.SelectedIndex = 0;
            TeamCreationStatusText.Text = string.Empty;
            TeamJoinStatusText.Text = string.Empty;
            TeamEntryOptionsPopup.IsOpen = false;
            if (TeamListSearchStatusText != null)
            {
                TeamListSearchStatusText.Text = CurrentViewerCanUseProfessorDiscovery()
                    ? "Pesquise por nome da equipe, turma, curso, UC ou professor focal."
                    : "Filtre suas equipes por nome, turma, curso ou UC.";
            }

            SyncDraftTemplateSuggestion();
            UpdateTeamMemberRoleHint();
            RenderTeamMembersDraft();
            RenderTeamUcsDraft();
            RenderDraftTeamLogoPreview();
            RenderTeamsList();
            UpdateTeamsViewState();
        }

        private void SyncTeamDefaultsWithProfile(UserProfile profile)
        {
            TeamCourseComboBox.Text = string.IsNullOrWhiteSpace(profile.Course)
                ? TeamCourseComboBox.Text
                : profile.Course;

            if (string.IsNullOrWhiteSpace(TeamAcademicTermTextBox.Text))
            {
                TeamAcademicTermTextBox.Text = ResolveCurrentAcademicTerm();
            }

            if (string.IsNullOrWhiteSpace(TeamNameTextBox.Text))
            {
                TeamNameTextBox.Text = $"Equipe {profile.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "Projeto"}";
            }

            SyncDraftTemplateSuggestion();
            EnsureCurrentUserInTeamDraft();
        }

        private void EnsureCurrentUserInTeamDraft()
        {
            var currentUser = CreateCurrentUserInfo();
            if (currentUser == null)
            {
                return;
            }

            var existingMember = _draftTeamMembers.FirstOrDefault(member => string.Equals(member.UserId, currentUser.UserId, StringComparison.OrdinalIgnoreCase));
            if (existingMember != null)
            {
                existingMember.Name = currentUser.Name;
                existingMember.Email = currentUser.Email;
                existingMember.Phone = currentUser.Phone;
                existingMember.Registration = currentUser.Registration;
                existingMember.Course = currentUser.Course;
                existingMember.Nickname = currentUser.Nickname;
                existingMember.ProfessionalTitle = currentUser.ProfessionalTitle;
                existingMember.AcademicDepartment = currentUser.AcademicDepartment;
                existingMember.AcademicFocus = currentUser.AcademicFocus;
                existingMember.OfficeHours = currentUser.OfficeHours;
                existingMember.Role = currentUser.Role;
                return;
            }

            _draftTeamMembers.Insert(0, currentUser);
            RenderTeamMembersDraft();
        }

        private string ResolveCurrentAcademicTerm()
        {
            var now = DateTime.Now;
            var semester = now.Month <= 6 ? 1 : 2;
            return $"{now.Year}.{semester}";
        }

        private void SyncDraftTemplateSuggestion()
        {
            if (TeamTemplateComboBox == null)
            {
                return;
            }

            if (TeamTemplateComboBox.SelectedItem is AcademicProjectTemplateInfo)
            {
                return;
            }

            var suggestion = AcademicProjectTemplateCatalog.FindBestMatch(TeamCourseComboBox?.Text, _draftTeamUcs);
            if (suggestion != null)
            {
                TeamTemplateComboBox.SelectedValue = suggestion.TemplateId;
            }
            else if (TeamTemplateComboBox.Items.Count > 0)
            {
                TeamTemplateComboBox.SelectedIndex = 0;
            }
        }

        private string GetSelectedDraftMemberRole()
        {
            return "student";
        }

        private void UpdateTeamMemberRoleHint()
        {
            if (TeamMemberRoleHintText == null)
            {
                return;
            }

            var role = GetSelectedDraftMemberRole();
            TeamMemberRoleHintText.Text = role switch
            {
                "leader" => "Liderança discente é definida depois, pelo professor focal, para não misturar governança com entrada inicial na equipe.",
                _ => "Novos participantes entram como alunos. Depois o professor focal pode promover alguém da equipe para liderança discente."
            };
        }

        private UserInfo CloneUserInfo(UserInfo source, string? overrideRole = null)
        {
            return new UserInfo
            {
                UserId = source.UserId,
                Name = source.Name,
                Email = source.Email,
                Registration = source.Registration,
                Course = source.Course,
                Phone = source.Phone,
                Nickname = source.Nickname,
                ProfessionalTitle = source.ProfessionalTitle,
                AcademicDepartment = source.AcademicDepartment,
                AcademicFocus = source.AcademicFocus,
                OfficeHours = source.OfficeHours,
                Bio = source.Bio,
                Skills = source.Skills,
                ProgrammingLanguages = source.ProgrammingLanguages,
                PortfolioLink = source.PortfolioLink,
                LinkedInLink = source.LinkedInLink,
                Role = TeamPermissionService.NormalizeRole(string.IsNullOrWhiteSpace(overrideRole) ? source.Role : overrideRole),
                AvatarBody = source.AvatarBody,
                AvatarHair = source.AvatarHair,
                AvatarHat = source.AvatarHat,
                AvatarAccessory = source.AvatarAccessory,
                AvatarClothing = source.AvatarClothing
            };
        }

        private void SelectDraftTeamLogo_Click(object sender, RoutedEventArgs e)
        {
            var logoAsset = CreateDraftTeamLogoAsset();
            if (logoAsset == null)
            {
                return;
            }

            _draftTeamLogoAsset = logoAsset;
            RenderDraftTeamLogoPreview();
            TeamCreationStatusText.Text = "Logo preparado. O recorte será salvo junto com a equipe.";
        }

        private void ClearDraftTeamLogo_Click(object sender, RoutedEventArgs e)
        {
            _draftTeamLogoAsset = null;
            RenderDraftTeamLogoPreview();
            TeamCreationStatusText.Text = "Logo removido do rascunho da equipe.";
        }

        private void EditTeamLogo_Click(object? sender, RoutedEventArgs e)
        {
            if (_activeTeamWorkspace == null)
            {
                return;
            }

            var logoAsset = CreateDraftTeamLogoAsset();
            if (logoAsset == null)
            {
                return;
            }

            ReplaceTeamLogoAsset(_activeTeamWorkspace, logoAsset);
            AddTeamNotification(_activeTeamWorkspace, "Logo do projeto atualizado.");
            SaveTeamWorkspace(_activeTeamWorkspace);
            RenderTeamWorkspace();
        }

        private void RemoveTeamLogo_Click(object? sender, RoutedEventArgs e)
        {
            if (_activeTeamWorkspace == null)
            {
                return;
            }

            var existingLogo = GetTeamLogoAsset(_activeTeamWorkspace);
            if (existingLogo == null)
            {
                return;
            }

            _activeTeamWorkspace.Assets.RemoveAll(asset => string.Equals(asset.Category, "logo", StringComparison.OrdinalIgnoreCase));
            AddTeamNotification(_activeTeamWorkspace, "Logo do projeto removido.");
            SaveTeamWorkspace(_activeTeamWorkspace);
            RenderTeamWorkspace();
        }

        private void RenderDraftTeamLogoPreview()
        {
            if (TeamLogoPreviewHost == null)
            {
                return;
            }

            TeamLogoPreviewHost.Content = CreateTeamLogoBadge(
                _draftTeamLogoAsset?.PreviewImageDataUri,
                TeamNameTextBox?.Text,
                82,
                circular: true,
                fontSize: 24,
                borderThickness: 0);

            if (SelectTeamLogoButton != null)
            {
                SelectTeamLogoButton.Content = _draftTeamLogoAsset == null ? "Escolher logo" : "Trocar logo";
            }

            if (ClearTeamLogoButton != null)
            {
                ClearTeamLogoButton.IsEnabled = _draftTeamLogoAsset != null;
                ClearTeamLogoButton.Opacity = _draftTeamLogoAsset == null ? 0.58 : 1;
            }
        }

        private TeamAssetInfo? CreateDraftTeamLogoAsset()
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = false,
                Title = "Selecionar logo do projeto",
                Filter = "Imagens|*.png;*.jpg;*.jpeg;*.bmp;*.webp"
            };

            if (dialog.ShowDialog() != true)
            {
                return null;
            }

            var croppedDataUri = ShowTeamLogoCropperDialog(dialog.FileName);
            if (string.IsNullOrWhiteSpace(croppedDataUri))
            {
                return null;
            }

            return new TeamAssetInfo
            {
                AssetId = Guid.NewGuid().ToString("N"),
                Category = "logo",
                FileName = IOPath.GetFileName(dialog.FileName),
                PreviewImageDataUri = croppedDataUri,
                AddedByUserId = _currentProfile?.UserId ?? string.Empty,
                AddedAt = DateTime.Now
            };
        }

        private string? ShowTeamLogoCropperDialog(string filePath)
        {
            return ShowImageCropperDialog(
                filePath,
                eyebrow: "LOGO",
                dialogTitle: "Ajustar logo do projeto",
                headerTitle: "Enquadre a identidade visual do projeto",
                description: "Arraste a imagem, aplique zoom e confira como o logo vai aparecer em bolinhas e cards quadrados do app antes de salvar.",
                workspaceHint: "Posicione o foco principal dentro da área guia. O recorte salvo sempre sai em formato quadrado para manter definição no app inteiro.",
                previewDescription: "A primeira mostra o comportamento em bolinhas. A segunda confirma leitura em áreas quadradas e cards do projeto.",
                firstPreviewLabel: "Modo circular",
                firstPreviewCircular: true,
                secondPreviewLabel: "Modo quadrado",
                secondPreviewCircular: false,
                tipText: "Dica: deixe o elemento mais importante no centro da guia circular. Assim o logo continua legível tanto nos resumos quanto no perfil do projeto.",
                saveButtonLabel: "Aplicar logo",
                accentColor: Color.FromRgb(236, 72, 153),
                outputSize: TeamLogoOutputSize,
                quality: TeamLogoJpegQuality,
                invalidImageTitle: "Imagem inválida",
                invalidImageMessage: "Não foi possível abrir a imagem selecionada para o recorte do projeto.",
                exportErrorTitle: "Falha ao gerar recorte",
                exportErrorMessage: "O recorte não pôde ser exportado. Tente ajustar a imagem novamente.");
        }

        private string? ShowProfileGalleryCropperDialog(string filePath)
        {
            return ShowImageCropperDialog(
                filePath,
                eyebrow: "GALERIA",
                dialogTitle: "Ajustar imagem da galeria",
                headerTitle: "Prepare a imagem do currículo",
                description: "Use o mesmo recorte guiado do projeto para encaixar a imagem da galeria com acabamento profissional antes de publicar no currículo.",
                workspaceHint: "A galeria do currículo usa thumbnails 1:1. Centralize o conteúdo principal para a miniatura continuar forte mesmo em tamanhos menores.",
                previewDescription: "A primeira visualização simula a miniatura da galeria. A segunda mostra o mesmo recorte em um card maior do perfil.",
                firstPreviewLabel: "Thumb da galeria",
                firstPreviewCircular: false,
                secondPreviewLabel: "Card do currículo",
                secondPreviewCircular: false,
                tipText: "Dica: fotos, artes e entregas com contraste forte no centro costumam funcionar melhor no mosaico da galeria.",
                saveButtonLabel: "Aplicar imagem",
                accentColor: GetThemeBrush("AccentBrush").Color,
                outputSize: ProfileGalleryImageMaxSide,
                quality: 82,
                invalidImageTitle: "Imagem inválida",
                invalidImageMessage: "Não foi possível abrir a imagem selecionada para a galeria do currículo.",
                exportErrorTitle: "Falha ao gerar recorte",
                exportErrorMessage: "O recorte da imagem da galeria não pôde ser exportado. Tente novamente.");
        }

        private string? ShowImageCropperDialog(
            string filePath,
            string eyebrow,
            string dialogTitle,
            string headerTitle,
            string description,
            string workspaceHint,
            string previewDescription,
            string firstPreviewLabel,
            bool firstPreviewCircular,
            string secondPreviewLabel,
            bool secondPreviewCircular,
            string tipText,
            string saveButtonLabel,
            Color accentColor,
            int outputSize,
            int quality,
            string invalidImageTitle,
            string invalidImageMessage,
            string exportErrorTitle,
            string exportErrorMessage)
        {
            var source = TryLoadBitmapSourceFromFile(filePath);
            if (source == null)
            {
                ShowStyledAlertDialog(eyebrow, invalidImageTitle, invalidImageMessage, "Fechar", new SolidColorBrush(Color.FromRgb(220, 38, 38)));
                return null;
            }

            const double cropViewportSize = 360;
            var accentBrush = new SolidColorBrush(accentColor);
            var dialog = CreateStyledDialogWindow(dialogTitle, 1040, 800, 740, true);
            string? croppedDataUri = null;

            var root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            root.Children.Add(CreateDialogHeader(eyebrow, headerTitle, description, accentBrush));

            var contentGrid = new Grid { Margin = new Thickness(0, 4, 0, 0) };
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.45, GridUnitType.Star) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(18) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.95, GridUnitType.Star) });
            Grid.SetRow(contentGrid, 1);
            root.Children.Add(contentGrid);

            var workspaceCard = new Border
            {
                Background = GetThemeBrush("MutedCardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(24),
                Padding = new Thickness(18)
            };

            var workspaceStack = new StackPanel();
            workspaceStack.Children.Add(new TextBlock
            {
                Text = workspaceHint,
                FontSize = 12,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20
            });

            var cropViewport = new Canvas
            {
                Width = cropViewportSize,
                Height = cropViewportSize,
                Background = Brushes.White,
                ClipToBounds = true
            };

            var cropImage = new Image
            {
                Source = source,
                Stretch = Stretch.Fill,
                Cursor = Cursors.SizeAll,
                SnapsToDevicePixels = true
            };
            cropViewport.Children.Add(cropImage);

            var cropStage = new Grid
            {
                Width = cropViewportSize,
                Height = cropViewportSize,
                Margin = new Thickness(0, 18, 0, 16),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var cropFrame = new Border
            {
                Background = Brushes.White,
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(28),
                Child = cropViewport
            };
            cropStage.Children.Add(cropFrame);
            cropStage.Children.Add(new Border
            {
                CornerRadius = new CornerRadius(28),
                BorderBrush = accentBrush,
                BorderThickness = new Thickness(2),
                Background = new SolidColorBrush(Color.FromArgb(12, accentColor.R, accentColor.G, accentColor.B)),
                IsHitTestVisible = false
            });
            cropStage.Children.Add(new System.Windows.Shapes.Ellipse
            {
                Width = cropViewportSize - 58,
                Height = cropViewportSize - 58,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Stroke = accentBrush,
                StrokeThickness = 2,
                StrokeDashArray = new DoubleCollection { 10, 8 },
                Opacity = 0.52,
                IsHitTestVisible = false
            });
            workspaceStack.Children.Add(cropStage);

            workspaceStack.Children.Add(new TextBlock
            {
                Text = "Zoom",
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            });

            var zoomSlider = new Slider
            {
                Minimum = 1,
                Maximum = 3.2,
                Value = 1,
                Margin = new Thickness(0, 10, 0, 0),
                TickFrequency = 0.05,
                IsSnapToTickEnabled = false
            };
            workspaceStack.Children.Add(zoomSlider);

            var zoomHints = new DockPanel { Margin = new Thickness(0, 6, 0, 0) };
            var zoomRightHint = new TextBlock
            {
                Text = "Aproximar",
                FontSize = 11,
                Foreground = GetThemeBrush("TertiaryTextBrush"),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            zoomHints.Children.Add(zoomRightHint);
            DockPanel.SetDock(zoomRightHint, Dock.Right);
            zoomHints.Children.Add(new TextBlock
            {
                Text = "Enquadramento original",
                FontSize = 11,
                Foreground = GetThemeBrush("TertiaryTextBrush")
            });
            workspaceStack.Children.Add(zoomHints);

            var helperChips = new WrapPanel { Margin = new Thickness(0, 14, 0, 0) };
            helperChips.Children.Add(CreateStaticTeamChip("Recorte final em 1:1", GetThemeBrush("AccentMutedBrush"), accentBrush));
            helperChips.Children.Add(CreateStaticTeamChip("Previews ao lado", GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            helperChips.Children.Add(CreateStaticTeamChip("Arraste com o mouse", GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            workspaceStack.Children.Add(helperChips);

            workspaceCard.Child = workspaceStack;
            contentGrid.Children.Add(workspaceCard);

            var previewCard = new Border
            {
                Background = GetThemeBrush("MutedCardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(24),
                Padding = new Thickness(18)
            };
            Grid.SetColumn(previewCard, 2);

            Border CreatePreviewSurface(bool circular)
            {
                return new Border
                {
                    Width = 126,
                    Height = 126,
                    CornerRadius = circular ? new CornerRadius(63) : new CornerRadius(26),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    BorderBrush = GetThemeBrush("CardBorderBrush"),
                    BorderThickness = new Thickness(1),
                    Background = new VisualBrush(cropViewport) { Stretch = Stretch.UniformToFill },
                    ClipToBounds = true
                };
            }

            var previewStack = new StackPanel();
            previewStack.Children.Add(new TextBlock
            {
                Text = "Pré-visualizações",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            });
            previewStack.Children.Add(new TextBlock
            {
                Text = previewDescription,
                Margin = new Thickness(0, 8, 0, 0),
                FontSize = 12,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20
            });
            previewStack.Children.Add(new TextBlock
            {
                Text = firstPreviewLabel,
                Margin = new Thickness(0, 18, 0, 10),
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            });
            previewStack.Children.Add(CreatePreviewSurface(firstPreviewCircular));
            previewStack.Children.Add(new TextBlock
            {
                Text = secondPreviewLabel,
                Margin = new Thickness(0, 22, 0, 10),
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            });
            previewStack.Children.Add(CreatePreviewSurface(secondPreviewCircular));
            previewStack.Children.Add(new Border
            {
                Margin = new Thickness(0, 24, 0, 0),
                Padding = new Thickness(14),
                CornerRadius = new CornerRadius(18),
                Background = GetThemeBrush("CardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                Child = new TextBlock
                {
                    Text = tipText,
                    FontSize = 11,
                    Foreground = GetThemeBrush("SecondaryTextBrush"),
                    TextWrapping = TextWrapping.Wrap,
                    LineHeight = 18
                }
            });
            previewCard.Child = previewStack;
            contentGrid.Children.Add(previewCard);

            var actions = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 22, 0, 0)
            };
            Grid.SetRow(actions, 2);

            var resetButton = CreateDialogActionButton("Centralizar", Brushes.Transparent, GetThemeBrush("PrimaryTextBrush"), GetThemeBrush("CardBorderBrush"), 120);
            var cancelButton = CreateDialogActionButton("Cancelar", Brushes.Transparent, GetThemeBrush("PrimaryTextBrush"), GetThemeBrush("CardBorderBrush"), 118);
            var saveButton = CreateDialogActionButton(saveButtonLabel, accentBrush, Brushes.White, Brushes.Transparent, 138);
            actions.Children.Add(resetButton);
            actions.Children.Add(cancelButton);
            actions.Children.Add(saveButton);
            root.Children.Add(actions);

            var baseScale = Math.Max(cropViewportSize / source.PixelWidth, cropViewportSize / source.PixelHeight);
            var previousWidth = 0d;
            var previousHeight = 0d;
            var currentLeft = 0d;
            var currentTop = 0d;
            Point? dragStart = null;
            var dragOriginLeft = 0d;
            var dragOriginTop = 0d;

            void ClampPosition(double width, double height)
            {
                currentLeft = width <= cropViewportSize
                    ? (cropViewportSize - width) / 2
                    : Math.Min(0, Math.Max(cropViewportSize - width, currentLeft));

                currentTop = height <= cropViewportSize
                    ? (cropViewportSize - height) / 2
                    : Math.Min(0, Math.Max(cropViewportSize - height, currentTop));
            }

            void ApplyCropState(bool resetPosition)
            {
                var width = source.PixelWidth * baseScale * zoomSlider.Value;
                var height = source.PixelHeight * baseScale * zoomSlider.Value;

                if (resetPosition || previousWidth <= 0 || previousHeight <= 0)
                {
                    currentLeft = (cropViewportSize - width) / 2;
                    currentTop = (cropViewportSize - height) / 2;
                }
                else
                {
                    var focusRatioX = (cropViewportSize / 2 - currentLeft) / previousWidth;
                    var focusRatioY = (cropViewportSize / 2 - currentTop) / previousHeight;
                    currentLeft = cropViewportSize / 2 - (focusRatioX * width);
                    currentTop = cropViewportSize / 2 - (focusRatioY * height);
                }

                ClampPosition(width, height);
                cropImage.Width = width;
                cropImage.Height = height;
                Canvas.SetLeft(cropImage, currentLeft);
                Canvas.SetTop(cropImage, currentTop);
                previousWidth = width;
                previousHeight = height;
            }

            cropFrame.MouseLeftButtonDown += (_, args) =>
            {
                dragStart = args.GetPosition(cropFrame);
                dragOriginLeft = currentLeft;
                dragOriginTop = currentTop;
                cropFrame.CaptureMouse();
            };

            cropFrame.MouseMove += (_, args) =>
            {
                if (dragStart == null || !cropFrame.IsMouseCaptured)
                {
                    return;
                }

                var currentPoint = args.GetPosition(cropFrame);
                currentLeft = dragOriginLeft + (currentPoint.X - dragStart.Value.X);
                currentTop = dragOriginTop + (currentPoint.Y - dragStart.Value.Y);
                ClampPosition(cropImage.Width, cropImage.Height);
                Canvas.SetLeft(cropImage, currentLeft);
                Canvas.SetTop(cropImage, currentTop);
            };

            void ReleaseDrag()
            {
                dragStart = null;
                cropFrame.ReleaseMouseCapture();
            }

            cropFrame.MouseLeftButtonUp += (_, __) => ReleaseDrag();
            cropFrame.MouseLeave += (_, __) =>
            {
                if (cropFrame.IsMouseCaptured && Mouse.LeftButton != MouseButtonState.Pressed)
                {
                    ReleaseDrag();
                }
            };

            cropFrame.MouseWheel += (_, args) =>
            {
                zoomSlider.Value = Math.Max(zoomSlider.Minimum, Math.Min(zoomSlider.Maximum, zoomSlider.Value + (args.Delta > 0 ? 0.08 : -0.08)));
            };

            zoomSlider.ValueChanged += (_, __) => ApplyCropState(resetPosition: false);

            resetButton.Click += (_, __) =>
            {
                zoomSlider.Value = 1;
                ApplyCropState(resetPosition: true);
            };

            cancelButton.Click += (_, __) => dialog.Close();
            saveButton.Click += (_, __) =>
            {
                croppedDataUri = CreateJpegDataUriFromVisual(cropViewport, outputSize, quality);
                if (string.IsNullOrWhiteSpace(croppedDataUri))
                {
                    ShowStyledAlertDialog(eyebrow, exportErrorTitle, exportErrorMessage, "Fechar", new SolidColorBrush(Color.FromRgb(220, 38, 38)));
                    return;
                }

                dialog.DialogResult = true;
                dialog.Close();
            };

            dialog.Content = CreateStyledDialogShell(root);
            dialog.Loaded += (_, __) => ApplyCropState(resetPosition: true);

            return dialog.ShowDialog() == true ? croppedDataUri : null;
        }

        private BitmapSource? TryLoadBitmapSourceFromFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return null;
            }

            try
            {
                using var stream = File.OpenRead(filePath);
                var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                var frame = decoder.Frames.FirstOrDefault();
                if (frame != null && frame.CanFreeze)
                {
                    frame.Freeze();
                }

                return frame;
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeamLogo] Falha ao carregar imagem: {ex.Message}");
                return null;
            }
        }

        private string? CreateJpegDataUriFromVisual(FrameworkElement element, int outputSize, int quality)
        {
            try
            {
                var drawingVisual = new DrawingVisual();
                using (var drawingContext = drawingVisual.RenderOpen())
                {
                    drawingContext.DrawRectangle(Brushes.White, null, new Rect(0, 0, outputSize, outputSize));
                    drawingContext.DrawRectangle(new VisualBrush(element) { Stretch = Stretch.Fill }, null, new Rect(0, 0, outputSize, outputSize));
                }

                var renderBitmap = new RenderTargetBitmap(outputSize, outputSize, 96, 96, PixelFormats.Pbgra32);
                renderBitmap.Render(drawingVisual);

                var encoder = new JpegBitmapEncoder
                {
                    QualityLevel = quality
                };
                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

                using var memoryStream = new MemoryStream();
                encoder.Save(memoryStream);
                return $"data:image/jpeg;base64,{Convert.ToBase64String(memoryStream.ToArray())}";
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeamLogo] Falha ao exportar recorte do logo: {ex.Message}");
                return null;
            }
        }

        private TeamAssetInfo? GetTeamLogoAsset(TeamWorkspaceInfo? team)
        {
            return team?.Assets?
                .Where(asset => string.Equals(asset.Category, "logo", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(asset.PreviewImageDataUri))
                .OrderByDescending(asset => asset.AddedAt)
                .FirstOrDefault();
        }

        private void ReplaceTeamLogoAsset(TeamWorkspaceInfo team, TeamAssetInfo logoAsset)
        {
            team.Assets.RemoveAll(asset => string.Equals(asset.Category, "logo", StringComparison.OrdinalIgnoreCase));
            team.Assets.Add(logoAsset);
        }

        private string GetTeamInitials(string? teamName)
        {
            var parts = (teamName ?? string.Empty)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Take(2)
                .Select(part => char.ToUpperInvariant(part[0]).ToString())
                .ToList();

            return parts.Count == 0 ? "PI" : string.Concat(parts);
        }

        private Border CreateTeamLogoBadge(TeamWorkspaceInfo? team, double size, bool circular, double fontSize = 20, double borderThickness = 1)
        {
            return CreateTeamLogoBadge(GetTeamLogoAsset(team)?.PreviewImageDataUri, team?.TeamName, size, circular, fontSize, borderThickness);
        }

        private Border CreateTeamLogoBadge(string? imageDataUri, string? fallbackText, double size, bool circular, double fontSize = 20, double borderThickness = 1)
        {
            var imageSource = TryCreateImageSourceFromDataUri(imageDataUri);

            return new Border
            {
                Width = size,
                Height = size,
                CornerRadius = circular ? new CornerRadius(size / 2) : new CornerRadius(Math.Max(16, size * 0.24)),
                Background = GetThemeBrush("MutedCardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(borderThickness),
                ClipToBounds = true,
                Child = imageSource != null
                    ? new Image
                    {
                        Source = imageSource,
                        Stretch = Stretch.UniformToFill
                    }
                    : new Grid
                    {
                        Children =
                        {
                            new Border
                            {
                                Background = GetThemeBrush("AccentMutedBrush")
                            },
                            new TextBlock
                            {
                                Text = GetTeamInitials(fallbackText),
                                FontFamily = GetAppDisplayFontFamily(),
                                FontSize = fontSize,
                                FontWeight = FontWeights.ExtraBold,
                                Foreground = GetThemeBrush("AccentBrush"),
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Center
                            }
                        }
                    }
            };
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

            var normalizedRole = TeamPermissionService.NormalizeRole(_currentProfile.Role);

            return new UserInfo
            {
                UserId = string.IsNullOrWhiteSpace(_currentProfile.UserId) ? "current-user" : _currentProfile.UserId,
                Name = _currentProfile.Name,
                Email = _currentProfile.Email,
                Phone = _currentProfile.Phone,
                Registration = _currentProfile.Registration,
                Course = _currentProfile.Course,
                Role = normalizedRole,
                Nickname = _currentProfile.Nickname,
                ProfessionalTitle = _currentProfile.ProfessionalTitle,
                AcademicDepartment = _currentProfile.AcademicDepartment,
                AcademicFocus = _currentProfile.AcademicFocus,
                OfficeHours = _currentProfile.OfficeHours,
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
                new TeamTaskColumnInfo { Title = "Backlog", AccentColor = Color.FromRgb(59, 130, 246), Cards = new List<TeamTaskCardInfo>() },
                new TeamTaskColumnInfo { Title = "Em andamento", AccentColor = Color.FromRgb(245, 158, 11), Cards = new List<TeamTaskCardInfo>() },
                new TeamTaskColumnInfo { Title = "Revisao", AccentColor = Color.FromRgb(168, 85, 247), Cards = new List<TeamTaskCardInfo>() },
                new TeamTaskColumnInfo { Title = "Concluido", AccentColor = Color.FromRgb(16, 185, 129), Cards = new List<TeamTaskCardInfo>() }
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
            if (team.CreatedAt == default) team.CreatedAt = DateTime.Now;
            if (team.UpdatedAt == default) team.UpdatedAt = DateTime.Now;
            if (!team.LastRealtimeSyncAt.HasValue) team.LastRealtimeSyncAt = team.UpdatedAt;
            if (string.IsNullOrWhiteSpace(team.AcademicTerm)) team.AcademicTerm = ResolveCurrentAcademicTerm();
            team.DefaultFilePermissionScope = string.IsNullOrWhiteSpace(team.DefaultFilePermissionScope) ? "team" : team.DefaultFilePermissionScope;
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

            team.Members = team.Members
                .Where(member => member != null && !string.IsNullOrWhiteSpace(member.UserId))
                .GroupBy(member => member.UserId, StringComparer.OrdinalIgnoreCase)
                .Select(group =>
                {
                    var member = group.First();
                    member.Role = TeamPermissionService.NormalizeRole(member.Role);
                    return member;
                })
                .OrderBy(member => member.Name)
                .ToList();

            if (!string.IsNullOrWhiteSpace(team.FocalProfessorUserId) &&
                string.IsNullOrWhiteSpace(team.FocalProfessorName))
            {
                team.FocalProfessorName = team.Members
                    .FirstOrDefault(member => string.Equals(member.UserId, team.FocalProfessorUserId, StringComparison.OrdinalIgnoreCase))
                    ?.Name ?? string.Empty;
            }

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

            team.Ucs = team.Ucs
                .Where(uc => !string.IsNullOrWhiteSpace(uc))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(uc => uc)
                .ToList();

            team.Assets ??= new List<TeamAssetInfo>();
            team.Milestones ??= new List<TeamMilestoneInfo>();
            team.Notifications ??= new List<TeamNotificationInfo>();
            team.ChatMessages ??= new List<TeamChatMessageInfo>();
            team.AccessRules ??= new List<TeamAccessRuleInfo>();
            team.SemesterTimeline ??= new List<TeamTimelineItemInfo>();
            team.ProjectProgress = Math.Max(0, Math.Min(100, team.ProjectProgress));
            team.ProjectStatus = string.IsNullOrWhiteSpace(team.ProjectStatus) ? "Planejamento" : team.ProjectStatus;
            team.AccessRules = TeamPermissionService.NormalizeAccessRules(team.AccessRules);

            foreach (var asset in team.Assets)
            {
                asset.PermissionScope = string.IsNullOrWhiteSpace(asset.PermissionScope) ? team.DefaultFilePermissionScope : asset.PermissionScope;
                asset.StorageKind = string.IsNullOrWhiteSpace(asset.StorageKind) ? "firestore-preview" : asset.StorageKind;
                asset.MimeType = string.IsNullOrWhiteSpace(asset.MimeType) ? GetMimeTypeFromFileName(asset.FileName) : asset.MimeType;
                asset.VersionHistory ??= new List<TeamAssetVersionInfo>();
                if (asset.Version <= 0)
                {
                    asset.Version = 1;
                }

                foreach (var version in asset.VersionHistory)
                {
                    version.FileName = string.IsNullOrWhiteSpace(version.FileName) ? asset.FileName : version.FileName;
                    version.MimeType = string.IsNullOrWhiteSpace(version.MimeType) ? asset.MimeType : version.MimeType;
                    version.PermissionScope = string.IsNullOrWhiteSpace(version.PermissionScope) ? asset.PermissionScope : version.PermissionScope;
                    version.StorageKind = string.IsNullOrWhiteSpace(version.StorageKind) ? asset.StorageKind : version.StorageKind;
                    version.SizeBytes = version.SizeBytes <= 0 ? asset.SizeBytes : version.SizeBytes;
                }
            }

            foreach (var milestone in team.Milestones)
            {
                milestone.CreatedByUserId = string.IsNullOrWhiteSpace(milestone.CreatedByUserId) ? team.CreatedBy : milestone.CreatedByUserId;
                milestone.OwnerUserId = string.IsNullOrWhiteSpace(milestone.OwnerUserId) ? milestone.CreatedByUserId : milestone.OwnerUserId;
                milestone.MentionedUserIds ??= new List<string>();
                milestone.Comments ??= new List<TeamCommentInfo>();
                milestone.Attachments ??= new List<TeamAttachmentInfo>();
                if (milestone.UpdatedAt == default)
                {
                    milestone.UpdatedAt = milestone.CreatedAt == default ? DateTime.Now : milestone.CreatedAt;
                }
                milestone.UpdatedByUserId = string.IsNullOrWhiteSpace(milestone.UpdatedByUserId) ? milestone.CreatedByUserId : milestone.UpdatedByUserId;
            }

            foreach (var column in team.TaskColumns ?? Enumerable.Empty<TeamTaskColumnInfo>())
            {
                foreach (var card in column.Cards)
                {
                    card.ColumnId = string.IsNullOrWhiteSpace(card.ColumnId) ? column.Id : card.ColumnId;
                    card.RequiredRole = TeamPermissionService.NormalizeRole(card.RequiredRole);
                    card.CreatedByUserId = string.IsNullOrWhiteSpace(card.CreatedByUserId) ? team.CreatedBy : card.CreatedByUserId;
                    card.MentionedUserIds ??= new List<string>();
                    card.Comments ??= new List<TeamCommentInfo>();
                    card.Attachments ??= new List<TeamAttachmentInfo>();
                    if (card.UpdatedAt == default)
                    {
                        card.UpdatedAt = card.CreatedAt == default ? DateTime.Now : card.CreatedAt;
                    }
                    card.UpdatedByUserId = string.IsNullOrWhiteSpace(card.UpdatedByUserId) ? card.CreatedByUserId : card.UpdatedByUserId;
                }
            }

            if (team.TaskColumns == null || team.TaskColumns.Count == 0) team.TaskColumns = CreateDefaultTeamColumns();
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

            if (team.Milestones.Count == 0) team.Milestones = CreateDefaultMilestones();
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

        private string GetCurrentUserRole(TeamWorkspaceInfo? team = null)
        {
            if (team != null)
            {
                var memberRole = team.Members
                    .FirstOrDefault(member => string.Equals(member.UserId, GetCurrentUserId(), StringComparison.OrdinalIgnoreCase))
                    ?.Role;

                if (!string.IsNullOrWhiteSpace(memberRole))
                {
                    return TeamPermissionService.NormalizeRole(memberRole);
                }
            }

            return TeamPermissionService.NormalizeRole(_currentProfile?.Role);
        }

        private bool CanCurrentUserManageMembers(TeamWorkspaceInfo team)
        {
            return TeamPermissionService.CanManageMembers(team, GetCurrentUserRole(team));
        }

        private bool CanCurrentUserAddMembers(TeamWorkspaceInfo team)
        {
            return TeamPermissionService.CanAddMembers(team, GetCurrentUserRole(team), GetCurrentUserId());
        }

        private bool CanCurrentUserAssignLeadership(TeamWorkspaceInfo team)
        {
            return TeamPermissionService.CanAssignLeadership(team, GetCurrentUserRole(team));
        }

        private bool CanCurrentUserEditProjectSettings(TeamWorkspaceInfo team)
        {
            return IsCurrentUserTeamCreator(team)
                || TeamPermissionService.CanEditProjectSettings(team, GetCurrentUserRole(team));
        }

        private bool CanCurrentUserDeleteTeam(TeamWorkspaceInfo team)
        {
            return TeamPermissionService.CanDeleteTeam(team, GetCurrentUserRole(team), GetCurrentUserId());
        }

        private bool CanCurrentUserComment(TeamWorkspaceInfo team)
        {
            return TeamPermissionService.CanComment(team, GetCurrentUserRole(team));
        }

        private bool CanCurrentUserUploadFiles(TeamWorkspaceInfo team)
        {
            return TeamPermissionService.CanUploadFiles(team, GetCurrentUserRole(team));
        }

        private bool CanCurrentUserReviewDeliverables(TeamWorkspaceInfo team)
        {
            return TeamPermissionService.CanReviewDeliverables(team, GetCurrentUserRole(team));
        }

        private bool CanCurrentUserViewAsset(TeamWorkspaceInfo team, TeamAssetInfo asset)
        {
            return TeamPermissionService.CanViewAsset(
                team,
                GetCurrentUserRole(team),
                asset.PermissionScope,
                GetCurrentUserId(),
                asset.AddedByUserId);
        }

        private List<UserInfo> GetFacultyMembers(TeamWorkspaceInfo team)
        {
            return (team.Members ?? new List<UserInfo>())
                .Where(member => TeamPermissionService.IsFacultyRole(member.Role))
                .OrderBy(member => member.Name)
                .ToList();
        }

        private List<UserInfo> GetStudentTeamMembers(TeamWorkspaceInfo team)
        {
            return (team.Members ?? new List<UserInfo>())
                .Where(member => !TeamPermissionService.IsFacultyRole(member.Role))
                .OrderBy(member => member.Name)
                .ToList();
        }

        private List<UserInfo> GetTaskAssignableMembers(TeamWorkspaceInfo team)
        {
            return GetStudentTeamMembers(team)
                .Where(member => TeamPermissionService.CanBeTaskAssignee(member.Role))
                .ToList();
        }

        private List<UserInfo> GetStudentLeaders(TeamWorkspaceInfo team)
        {
            return GetStudentTeamMembers(team)
                .Where(member => TeamPermissionService.IsLeaderLike(member.Role))
                .ToList();
        }

        private string GetPermissionScopeLabel(string? permissionScope)
        {
            return TeamPermissionService.NormalizePermissionScope(permissionScope) switch
            {
                "course" => "Curso",
                "leadership" => "Liderança",
                "private" => "Privado",
                _ => "Equipe"
            };
        }

        private string ResolveAssetCategoryForFile(string filePath, string fallbackCategory = "documentos")
        {
            var extension = GetFilesHubExtension(filePath, string.Empty);
            if (IsFilesHubImageExtension(extension))
            {
                return "imagens";
            }

            return string.IsNullOrWhiteSpace(fallbackCategory) ? "documentos" : fallbackCategory;
        }

        private string GetMimeTypeFromFileName(string fileName)
        {
            var extension = IOPath.GetExtension(fileName ?? string.Empty).Trim().ToLowerInvariant();
            return extension switch
            {
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".bmp" => "image/bmp",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".pdf" => "application/pdf",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".txt" => "text/plain",
                ".md" => "text/markdown",
                ".markdown" => "text/markdown",
                ".json" => "application/json",
                ".csv" => "text/csv",
                ".xml" => "application/xml",
                ".yml" => "application/yaml",
                ".yaml" => "application/yaml",
                ".rules" => "text/plain",
                ".cs" => "text/plain",
                ".xaml" => "application/xml",
                _ => "application/octet-stream"
            };
        }

        private string BuildTeamAssetCacheDirectory(TeamWorkspaceInfo team, string assetId)
        {
            return IOPath.Combine(GetFilesHubRootDirectory(), "remote-cache", SanitizeFileNameSegment(team.TeamId), SanitizeFileNameSegment(assetId));
        }

        private int CalculateTeamProgressPercentage(TeamWorkspaceInfo team)
        {
            return Math.Max(0, Math.Min(100, team.ProjectProgress));
        }

        private string GetNextTeamDeadlineLabel(TeamWorkspaceInfo team)
        {
            if (team.ProjectDeadline.HasValue) return FormatRelativeDate(team.ProjectDeadline.Value);

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
            team.Notifications.Insert(0, new TeamNotificationInfo { Message = message, CreatedAt = DateTime.Now });
            if (team.Notifications.Count > 10) team.Notifications = team.Notifications.Take(10).ToList();
        }

        private string FormatRelativeDate(DateTime date)
        {
            var difference = date.Date - DateTime.Today;
            if (difference.TotalDays == 0) return "Hoje";
            if (difference.TotalDays == 1) return "Amanha";
            if (difference.TotalDays == -1) return "Ontem";
            return date.ToString("dd/MM");
        }

        private static bool IsCompletedTaskColumn(TeamTaskColumnInfo column)
        {
            return column.Title.Contains("Conclu", StringComparison.OrdinalIgnoreCase)
                || column.Title.Contains("Done", StringComparison.OrdinalIgnoreCase)
                || column.Title.Contains("Final", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsCompletedMilestone(TeamMilestoneInfo milestone)
        {
            return string.Equals(milestone.Status, "Concluida", StringComparison.OrdinalIgnoreCase);
        }

        private List<CalendarAgendaItem> BuildCalendarAgendaItems(IReadOnlyList<TeamWorkspaceInfo> teams)
        {
            var items = new List<CalendarAgendaItem>();

            foreach (var team in teams)
            {
                if (team.ProjectDeadline.HasValue)
                {
                    items.Add(new CalendarAgendaItem
                    {
                        Team = team,
                        KindLabel = "Projeto",
                        Title = "Prazo principal do projeto",
                        Subtitle = $"{team.TeamName} • {team.ProjectStatus}",
                        Notes = string.IsNullOrWhiteSpace(team.Course)
                            ? "Entrega macro da equipe para o semestre atual."
                            : $"Entrega macro vinculada a {team.Course}.",
                        StatusLabel = team.ProjectStatus,
                        DueDate = team.ProjectDeadline.Value,
                        AccentColor = Color.FromRgb(249, 115, 22),
                        IconKind = PackIconMaterialKind.FlagCheckered,
                        IsOverdue = team.ProjectDeadline.Value.Date < DateTime.Today
                    });
                }

                foreach (var milestone in team.Milestones
                    .Where(item => item.DueDate.HasValue && !IsCompletedMilestone(item))
                    .OrderBy(item => item.DueDate))
                {
                    items.Add(new CalendarAgendaItem
                    {
                        Team = team,
                        KindLabel = "Marco",
                        Title = milestone.Title,
                        Subtitle = $"{team.TeamName} • Entrega planejada",
                        Notes = milestone.Notes,
                        StatusLabel = milestone.Status,
                        DueDate = milestone.DueDate!.Value,
                        AccentColor = Color.FromRgb(124, 58, 237),
                        IconKind = PackIconMaterialKind.Target,
                        IsOverdue = milestone.DueDate.Value.Date < DateTime.Today
                    });
                }

                foreach (var item in team.TaskColumns
                    .Where(column => !IsCompletedTaskColumn(column))
                    .SelectMany(column => column.Cards.Select(card => new { Column = column, Card = card }))
                    .Where(item => item.Card.DueDate.HasValue)
                    .OrderBy(item => item.Card.DueDate))
                {
                    items.Add(new CalendarAgendaItem
                    {
                        Team = team,
                        KindLabel = "Tarefa",
                        Title = item.Card.Title,
                        Subtitle = $"{team.TeamName} • {item.Column.Title}",
                        Notes = item.Card.Description,
                        StatusLabel = string.IsNullOrWhiteSpace(item.Card.Priority) ? item.Column.Title : $"Prioridade {item.Card.Priority}",
                        DueDate = item.Card.DueDate!.Value,
                        AccentColor = item.Column.AccentColor == default ? Color.FromRgb(37, 99, 235) : item.Column.AccentColor,
                        IconKind = PackIconMaterialKind.TextBoxCheckOutline,
                        IsOverdue = item.Card.DueDate.Value.Date < DateTime.Today
                    });
                }

                foreach (var timelineItem in team.SemesterTimeline
                    .Where(item => item.EndsAt.HasValue && !string.Equals(item.Status, "Concluido", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(item => item.EndsAt))
                {
                    items.Add(new CalendarAgendaItem
                    {
                        Team = team,
                        KindLabel = "Timeline",
                        Title = timelineItem.Title,
                        Subtitle = $"{team.TeamName} • {timelineItem.Category}",
                        Notes = timelineItem.Description,
                        StatusLabel = timelineItem.Status,
                        DueDate = timelineItem.EndsAt!.Value,
                        AccentColor = Color.FromRgb(16, 185, 129),
                        IconKind = PackIconMaterialKind.TimelineClockOutline,
                        IsOverdue = timelineItem.EndsAt.Value.Date < DateTime.Today
                    });
                }
            }

            return items
                .OrderBy(item => item.DueDate.Date)
                .ThenBy(item => item.Team.TeamName)
                .ThenBy(item => item.Title)
                .ToList();
        }

        private List<CalendarAgendaItem> ApplyCalendarAgendaFilters(IReadOnlyList<CalendarAgendaItem> agendaItems)
        {
            var endDate = DateTime.Today.AddDays(Math.Max(1, _calendarFilterWindowDays));

            return agendaItems
                .Where(item => string.IsNullOrWhiteSpace(_calendarFilterTeamId) || string.Equals(item.Team.TeamId, _calendarFilterTeamId, StringComparison.OrdinalIgnoreCase))
                .Where(item => string.Equals(_calendarFilterKind, "Todos", StringComparison.OrdinalIgnoreCase) || string.Equals(item.KindLabel, _calendarFilterKind, StringComparison.OrdinalIgnoreCase))
                .Where(item => item.DueDate.Date <= endDate)
                .Where(CalendarAgendaStatusMatches)
                .OrderBy(item => item.DueDate)
                .ThenBy(item => item.Team.TeamName)
                .ThenBy(item => item.Title)
                .ToList();
        }

        private bool CalendarAgendaStatusMatches(CalendarAgendaItem item)
        {
            return _calendarFilterStatus switch
            {
                "Em risco" => item.IsOverdue,
                "Hoje" => item.DueDate.Date == DateTime.Today,
                "Proximos 7 dias" => !item.IsOverdue && item.DueDate.Date <= DateTime.Today.AddDays(7),
                "Com revisão" => item.StatusLabel.Contains("Revis", StringComparison.OrdinalIgnoreCase),
                _ => true
            };
        }

        private Border CreateCalendarFiltersCard(IReadOnlyList<TeamWorkspaceInfo> teams, IReadOnlyList<CalendarAgendaItem> filteredAgendaItems, int totalAgendaCount)
        {
            var card = new Border
            {
                Background = GetThemeBrush("CardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(20),
                Padding = new Thickness(18),
                Margin = new Thickness(0, 0, 0, 18)
            };

            var stack = new StackPanel();
            stack.Children.Add(new TextBlock
            {
                Text = "Filtros e exportação",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            });
            stack.Children.Add(new TextBlock
            {
                Text = $"A visão atual mostra {filteredAgendaItems.Count} de {totalAgendaCount} compromisso(s). Ajuste a janela e exporte a semana para acompanhamento externo.",
                Margin = new Thickness(0, 6, 0, 0),
                FontSize = 11,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 18
            });

            var selectors = new WrapPanel { Margin = new Thickness(0, 14, 0, 0) };
            selectors.Children.Add(CreateCalendarFilterSelector(
                "Equipe",
                new[] { (Label: "Todas", Value: string.Empty) }.Concat(teams.Select(team => (team.TeamName, team.TeamId))).ToList(),
                _calendarFilterTeamId,
                value =>
                {
                    _calendarFilterTeamId = value;
                    RenderCalendarAgenda();
                }));
            selectors.Children.Add(CreateCalendarFilterSelector(
                "Tipo",
                new[] { ("Todos", "Todos"), ("Projeto", "Projeto"), ("Marco", "Marco"), ("Tarefa", "Tarefa"), ("Timeline", "Timeline") },
                _calendarFilterKind,
                value =>
                {
                    _calendarFilterKind = string.IsNullOrWhiteSpace(value) ? "Todos" : value;
                    RenderCalendarAgenda();
                }));
            selectors.Children.Add(CreateCalendarFilterSelector(
                "Status",
                new[] { ("Todos", "Todos"), ("Em risco", "Em risco"), ("Hoje", "Hoje"), ("Próximos 7 dias", "Proximos 7 dias"), ("Com revisão", "Com revisão") },
                _calendarFilterStatus,
                value =>
                {
                    _calendarFilterStatus = string.IsNullOrWhiteSpace(value) ? "Todos" : value;
                    RenderCalendarAgenda();
                }));
            selectors.Children.Add(CreateCalendarFilterSelector(
                "Janela",
                new[] { ("7 dias", "7"), ("14 dias", "14"), ("30 dias", "30"), ("90 dias", "90") },
                _calendarFilterWindowDays.ToString(CultureInfo.InvariantCulture),
                value =>
                {
                    _calendarFilterWindowDays = int.TryParse(value, out var days) ? days : 14;
                    RenderCalendarAgenda();
                }));
            stack.Children.Add(selectors);

            var actions = new WrapPanel { Margin = new Thickness(0, 14, 0, 0) };
            actions.Children.Add(CreateSidebarButton("Exportar Excel", Color.FromRgb(16, 185, 129), (s, e) => ExportCalendarAgendaToExcel(), PackIconMaterialKind.FileDocumentOutline));
            actions.Children.Add(CreateSidebarButton("Exportar PDF", Color.FromRgb(220, 38, 38), (s, e) => ExportCalendarAgendaToPdf(), PackIconMaterialKind.FilePdfBox));
            actions.Children.Add(CreateSidebarButton("Limpar filtros", Color.FromRgb(100, 116, 139), (s, e) =>
            {
                _calendarFilterTeamId = string.Empty;
                _calendarFilterKind = "Todos";
                _calendarFilterStatus = "Todos";
                _calendarFilterWindowDays = 14;
                RenderCalendarAgenda();
            }, PackIconMaterialKind.FilterOffOutline));
            stack.Children.Add(actions);

            card.Child = stack;
            return card;
        }

        private Border CreateCalendarFilterSelector(string label, IEnumerable<(string Label, string Value)> options, string selectedValue, Action<string> onChanged)
        {
            var container = new Border
            {
                Background = GetThemeBrush("MutedCardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(16),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 0, 10, 10),
                MinWidth = 188
            };

            var stack = new StackPanel();
            stack.Children.Add(new TextBlock
            {
                Text = label,
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = GetThemeBrush("SecondaryTextBrush")
            });

            var combo = new ComboBox
            {
                Height = 40,
                Margin = new Thickness(0, 8, 0, 0),
                DisplayMemberPath = "Label",
                SelectedValuePath = "Value",
                ItemsSource = options.Select(item => new { item.Label, item.Value }).ToList(),
                SelectedValue = selectedValue
            };
            ApplyDialogInputStyle(combo);
            combo.SelectionChanged += (s, e) =>
            {
                if (combo.SelectedValue is string value)
                {
                    onChanged(value);
                }
            };

            stack.Children.Add(combo);
            container.Child = stack;
            return container;
        }

        private Border CreateCalendarFilteredEmptyState()
        {
            return CreateSearchSlideInfoCard(
                "Nenhum compromisso nessa visão",
                "Ajuste equipe, tipo, status ou janela de datas para voltar a enxergar itens na agenda filtrada.");
        }

        private List<CalendarAgendaItem> GetWeeklyAgendaExportItems()
        {
            var limitDate = DateTime.Today.AddDays(7);
            return (_lastCalendarAgendaItems ?? new List<CalendarAgendaItem>())
                .Where(item => item.DueDate.Date <= limitDate)
                .OrderBy(item => item.DueDate)
                .ThenBy(item => item.Team.TeamName)
                .ThenBy(item => item.Title)
                .ToList();
        }

        private void ExportCalendarAgendaToExcel()
        {
            var items = GetWeeklyAgendaExportItems();
            if (items.Count == 0)
            {
                ShowStyledAlertDialog("AGENDA", "Nada para exportar", "A semana filtrada não possui compromissos para exportação em Excel.", "Fechar", GetThemeBrush("AccentBrush"));
                return;
            }

            var dialog = new SaveFileDialog
            {
                FileName = $"agenda-semanal-{DateTime.Today:yyyyMMdd}.xlsx",
                Filter = "Planilha Excel|*.xlsx",
                DefaultExt = ".xlsx"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            using var document = SpreadsheetDocument.Create(dialog.FileName, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook);
            var workbookPart = document.AddWorkbookPart();
            workbookPart.Workbook = new S.Workbook();

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            var sheetData = new S.SheetData();
            worksheetPart.Worksheet = new S.Worksheet(sheetData);

            var sheets = workbookPart.Workbook.AppendChild(new S.Sheets());
            sheets.Append(new S.Sheet
            {
                Id = workbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "Agenda Semanal"
            });

            sheetData.Append(CreateSpreadsheetRow(new[]
            {
                "Equipe",
                "Turma",
                "Semestre",
                "Tipo",
                "Título",
                "Prazo",
                "Status",
                "Template",
                "Observações"
            }));

            foreach (var item in items)
            {
                sheetData.Append(CreateSpreadsheetRow(new[]
                {
                    item.Team.TeamName,
                    item.Team.ClassName,
                    item.Team.AcademicTerm,
                    item.KindLabel,
                    item.Title,
                    item.DueDate.ToString("dd/MM/yyyy"),
                    item.StatusLabel,
                    item.Team.TemplateName,
                    item.Notes
                }));
            }

            workbookPart.Workbook.Save();
            ShowStyledAlertDialog("AGENDA", "Exportação concluída", $"A agenda semanal foi exportada para:\n{dialog.FileName}", "Fechar", GetThemeBrush("AccentBrush"));
        }

        private S.Row CreateSpreadsheetRow(IEnumerable<string> values)
        {
            var row = new S.Row();
            foreach (var value in values)
            {
                row.AppendChild(new S.Cell
                {
                    DataType = S.CellValues.String,
                    CellValue = new S.CellValue(value ?? string.Empty)
                });
            }

            return row;
        }

        private void ExportCalendarAgendaToPdf()
        {
            var items = GetWeeklyAgendaExportItems();
            if (items.Count == 0)
            {
                ShowStyledAlertDialog("AGENDA", "Nada para exportar", "A semana filtrada não possui compromissos para exportação em PDF.", "Fechar", GetThemeBrush("AccentBrush"));
                return;
            }

            var dialog = new SaveFileDialog
            {
                FileName = $"agenda-semanal-{DateTime.Today:yyyyMMdd}.pdf",
                Filter = "Documento PDF|*.pdf",
                DefaultExt = ".pdf"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            var lines = new List<string>
            {
                "Agenda semanal academica",
                $"Gerada em {DateTime.Now:dd/MM/yyyy HH:mm}",
                string.Empty
            };

            foreach (var item in items)
            {
                lines.Add($"[{item.KindLabel}] {item.Team.TeamName} - {item.Title}");
                lines.Add($"Turma: {item.Team.ClassName} | Prazo: {item.DueDate:dd/MM/yyyy} | Status: {item.StatusLabel}");
                if (!string.IsNullOrWhiteSpace(item.Notes))
                {
                    lines.Add($"Notas: {TruncateAgendaText(item.Notes, 140)}");
                }
                lines.Add(string.Empty);
            }

            WriteBasicPdfDocument(dialog.FileName, lines);
            ShowStyledAlertDialog("AGENDA", "Exportação concluída", $"A agenda semanal foi exportada para:\n{dialog.FileName}", "Fechar", GetThemeBrush("AccentBrush"));
        }

        private void WriteBasicPdfDocument(string filePath, IReadOnlyList<string> lines)
        {
            var safeLines = (lines ?? Array.Empty<string>())
                .Select(SanitizePdfText)
                .ToList();

            var pages = new List<List<string>>();
            const int linesPerPage = 38;
            for (var index = 0; index < safeLines.Count; index += linesPerPage)
            {
                pages.Add(safeLines.Skip(index).Take(linesPerPage).ToList());
            }

            if (pages.Count == 0)
            {
                pages.Add(new List<string> { "Agenda semanal vazia." });
            }

            var objects = new List<string>();
            objects.Add("<< /Type /Catalog /Pages 2 0 R >>");

            var pageObjectIds = new List<int>();
            var contentObjectIds = new List<int>();
            const int fontObjectId = 3;

            var nextObjectId = 4;
            foreach (var _ in pages)
            {
                pageObjectIds.Add(nextObjectId++);
                contentObjectIds.Add(nextObjectId++);
            }

            var kids = string.Join(" ", pageObjectIds.Select(id => $"{id} 0 R"));
            objects.Add($"<< /Type /Pages /Count {pages.Count} /Kids [ {kids} ] >>");
            objects.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");

            for (var pageIndex = 0; pageIndex < pages.Count; pageIndex++)
            {
                var pageObjectId = pageObjectIds[pageIndex];
                var contentObjectId = contentObjectIds[pageIndex];
                objects.Add($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 {fontObjectId} 0 R >> >> /Contents {contentObjectId} 0 R >>");

                var contentBuilder = new StringBuilder();
                contentBuilder.AppendLine("BT");
                contentBuilder.AppendLine("/F1 12 Tf");
                contentBuilder.AppendLine("40 800 Td");

                var isFirstLine = true;
                foreach (var line in pages[pageIndex])
                {
                    if (isFirstLine)
                    {
                        contentBuilder.AppendLine($"({EscapePdfText(line)}) Tj");
                        isFirstLine = false;
                    }
                    else
                    {
                        contentBuilder.AppendLine("0 -18 Td");
                        contentBuilder.AppendLine($"({EscapePdfText(line)}) Tj");
                    }
                }

                contentBuilder.AppendLine("ET");
                var contentStream = contentBuilder.ToString();
                objects.Add($"<< /Length {Encoding.ASCII.GetByteCount(contentStream)} >>\nstream\n{contentStream}endstream");
            }

            using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            using var writer = new StreamWriter(stream, Encoding.ASCII);
            writer.WriteLine("%PDF-1.4");

            var offsets = new List<long> { 0 };
            for (var objectIndex = 0; objectIndex < objects.Count; objectIndex++)
            {
                writer.Flush();
                offsets.Add(stream.Position);
                writer.WriteLine($"{objectIndex + 1} 0 obj");
                writer.WriteLine(objects[objectIndex]);
                writer.WriteLine("endobj");
            }

            writer.Flush();
            var xrefPosition = stream.Position;
            writer.WriteLine($"xref\n0 {objects.Count + 1}");
            writer.WriteLine("0000000000 65535 f ");
            foreach (var offset in offsets.Skip(1))
            {
                writer.WriteLine($"{offset:D10} 00000 n ");
            }
            writer.WriteLine("trailer");
            writer.WriteLine($"<< /Size {objects.Count + 1} /Root 1 0 R >>");
            writer.WriteLine("startxref");
            writer.WriteLine(xrefPosition.ToString(CultureInfo.InvariantCulture));
            writer.WriteLine("%%EOF");
            writer.Flush();
        }

        private string SanitizePdfText(string value)
        {
            var normalized = (value ?? string.Empty).Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder();
            foreach (var character in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                builder.Append(character <= 127 ? character : '?');
            }

            return builder.ToString().Normalize(NormalizationForm.FormC);
        }

        private string EscapePdfText(string value)
        {
            return (value ?? string.Empty)
                .Replace("\\", "\\\\")
                .Replace("(", "\\(")
                .Replace(")", "\\)");
        }

        private int GetCalendarTeamRiskScore(TeamWorkspaceInfo team, IReadOnlyList<CalendarAgendaItem> items)
        {
            var overdueCount = items.Count(item =>
                string.Equals(item.Team.TeamId, team.TeamId, StringComparison.OrdinalIgnoreCase)
                && item.IsOverdue);
            var dueSoonCount = items.Count(item =>
                string.Equals(item.Team.TeamId, team.TeamId, StringComparison.OrdinalIgnoreCase)
                && !item.IsOverdue
                && item.DueDate.Date <= DateTime.Today.AddDays(7));
            var unassignedCount = team.TaskColumns
                .Where(column => !IsCompletedTaskColumn(column))
                .SelectMany(column => column.Cards)
                .Count(card => card.AssignedUserIds.Count == 0);

            return (overdueCount * 4) + (dueSoonCount * 2) + Math.Min(2, unassignedCount);
        }

        private TeamWorkspaceInfo? GetCalendarFocusTeam(IReadOnlyList<TeamWorkspaceInfo> teams, IReadOnlyList<CalendarAgendaItem> items)
        {
            if (_activeTeamWorkspace != null)
            {
                var active = teams.FirstOrDefault(team =>
                    string.Equals(team.TeamId, _activeTeamWorkspace.TeamId, StringComparison.OrdinalIgnoreCase));
                if (active != null)
                {
                    return active;
                }
            }

            return teams
                .OrderByDescending(team => GetCalendarTeamRiskScore(team, items))
                .ThenBy(team => items
                    .Where(item => string.Equals(item.Team.TeamId, team.TeamId, StringComparison.OrdinalIgnoreCase))
                    .Select(item => item.DueDate)
                    .DefaultIfEmpty(DateTime.MaxValue)
                    .Min())
                .FirstOrDefault();
        }

        private string GetAgendaDueCountdownText(DateTime dueDate)
        {
            var days = (dueDate.Date - DateTime.Today).Days;
            if (days == 0)
            {
                return "Entrega hoje";
            }

            if (days == 1)
            {
                return "Entrega amanha";
            }

            if (days > 1)
            {
                return $"Em {days} dias";
            }

            var overdueDays = Math.Abs(days);
            return overdueDays == 1 ? "1 dia de atraso" : $"{overdueDays} dias de atraso";
        }

        private string GetAgendaDateHeading(DateTime date)
        {
            var culture = CultureInfo.GetCultureInfo("pt-BR");
            var difference = (date.Date - DateTime.Today).Days;

            if (difference == 0)
            {
                return $"Hoje • {date:dd/MM}";
            }

            if (difference == 1)
            {
                return $"Amanha • {date:dd/MM}";
            }

            return $"{culture.TextInfo.ToTitleCase(date.ToString("dddd", culture))} • {date:dd/MM}";
        }

        private string TruncateAgendaText(string text, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var normalized = text.Trim();
            return normalized.Length <= maxLength
                ? normalized
                : normalized.Substring(0, Math.Max(0, maxLength - 3)) + "...";
        }

        private void RenderCalendarAgenda()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(RenderCalendarAgenda);
                return;
            }

            CalendarAgendaHost.Children.Clear();

            var teams = _teamWorkspaces
                .Select(EnsureTeamWorkspaceDefaults)
                .OrderBy(team => team.TeamName)
                .ToList();
            var agendaItems = BuildCalendarAgendaItems(teams);
            var filteredAgendaItems = ApplyCalendarAgendaFilters(agendaItems);
            var overdueCount = filteredAgendaItems.Count(item => item.IsOverdue);
            _lastCalendarAgendaTeams = teams;
            _lastCalendarAgendaItems = filteredAgendaItems;

            CalendarStatusText.Text = teams.Count == 0
                ? "Crie ou entre em uma equipe para acompanhar prazos, marcos e tarefas em uma agenda consolidada."
                : $"{teams.Count} equipe(s) monitorada(s), {filteredAgendaItems.Count} compromisso(s) na visão atual e {overdueCount} ponto(s) em risco imediato.";

            if (teams.Count == 0)
            {
                CalendarAgendaHost.Children.Add(CreateCalendarEmptyState());
                return;
            }

            CalendarAgendaHost.Children.Add(CreateCalendarHeroCard(teams, filteredAgendaItems));
            CalendarAgendaHost.Children.Add(CreateCalendarFiltersCard(teams, filteredAgendaItems, agendaItems.Count));

            if (filteredAgendaItems.Count == 0)
            {
                CalendarAgendaHost.Children.Add(CreateCalendarFilteredEmptyState());
                return;
            }

            CalendarAgendaHost.Children.Add(CreateCalendarWeekStripCard(filteredAgendaItems));

            var mainGrid = new Grid { Margin = new Thickness(0, 0, 0, 18) };
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.7, GridUnitType.Star) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(18) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var leftStack = new StackPanel();
            leftStack.Children.Add(CreateCalendarPrioritySection(filteredAgendaItems));
            leftStack.Children.Add(CreateCalendarTimelineSection(filteredAgendaItems));
            mainGrid.Children.Add(leftStack);

            var rightStack = new StackPanel();
            rightStack.Children.Add(CreateCalendarTeamRadarSection(teams, filteredAgendaItems));
            rightStack.Children.Add(CreateCalendarRecentActivitySection(teams));
            rightStack.Children.Add(CreateCalendarGuidanceSection(teams, filteredAgendaItems));
            Grid.SetColumn(rightStack, 2);
            mainGrid.Children.Add(rightStack);

            CalendarAgendaHost.Children.Add(mainGrid);
        }

        private Border CreateCalendarEmptyState()
        {
            var card = new Border
            {
                Background = GetThemeBrush("CardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(22),
                Padding = new Thickness(26),
                Margin = new Thickness(0, 0, 0, 18)
            };

            var stack = new StackPanel();
            stack.Children.Add(new Border
            {
                Background = GetThemeBrush("AccentMutedBrush"),
                CornerRadius = new CornerRadius(999),
                Padding = new Thickness(10, 5, 10, 5),
                HorizontalAlignment = HorizontalAlignment.Left,
                Child = new TextBlock
                {
                    Text = "Agenda profissional",
                    FontSize = 10,
                    FontWeight = FontWeights.Bold,
                    Foreground = GetThemeBrush("AccentBrush")
                }
            });
            stack.Children.Add(new TextBlock
            {
                Text = "Sua agenda vai nascer aqui.",
                Margin = new Thickness(0, 14, 0, 0),
                FontFamily = GetAppDisplayFontFamily(),
                FontSize = 24,
                FontWeight = FontWeights.ExtraBold,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            });
            stack.Children.Add(new TextBlock
            {
                Text = "Assim que uma equipe for criada ou vinculada, o painel passa a consolidar prazo principal, milestones, tarefas com vencimento e sinais de risco do semestre.",
                Margin = new Thickness(0, 10, 0, 0),
                FontSize = 12,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20
            });

            var chips = new WrapPanel { Margin = new Thickness(0, 14, 0, 0) };
            chips.Children.Add(CreateStaticTeamChip("Projeto integrador", GetThemeBrush("AccentMutedBrush"), GetThemeBrush("AccentBrush")));
            chips.Children.Add(CreateStaticTeamChip("Marcos", GetThemeBrush("MutedCardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            chips.Children.Add(CreateStaticTeamChip("Tarefas com prazo", GetThemeBrush("MutedCardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            stack.Children.Add(chips);

            var openButton = new Button
            {
                Content = "Abrir area de equipes",
                Width = 190,
                Height = 42,
                Margin = new Thickness(0, 18, 0, 0),
                Background = GetThemeBrush("AccentBrush"),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontWeight = FontWeights.SemiBold,
                Cursor = Cursors.Hand
            };
            openButton.Click += (_, __) => ShowTeamsSection();
            stack.Children.Add(openButton);

            card.Child = stack;
            return card;
        }

        private Border CreateCalendarHeroCard(IReadOnlyList<TeamWorkspaceInfo> teams, IReadOnlyList<CalendarAgendaItem> agendaItems)
        {
            var overdueCount = agendaItems.Count(item => item.IsOverdue);
            var nextSevenCount = agendaItems.Count(item => !item.IsOverdue && item.DueDate.Date <= DateTime.Today.AddDays(7));
            var milestoneCount = teams.Sum(team => team.Milestones.Count(item => !IsCompletedMilestone(item)));
            var openTasks = teams.Sum(team => team.TaskColumns
                .Where(column => !IsCompletedTaskColumn(column))
                .Sum(column => column.Cards.Count));
            var focusTeam = GetCalendarFocusTeam(teams, agendaItems);

            string narrative;
            if (overdueCount > 0)
            {
                narrative = $"Existem {overdueCount} item(ns) fora do prazo. Vale concentrar a proxima reuniao nas equipes marcadas em risco antes de assumir novas frentes.";
            }
            else if (nextSevenCount > 0)
            {
                narrative = $"A proxima janela academica concentra {nextSevenCount} compromisso(s) nos proximos 7 dias. O painel abaixo ajuda a distribuir foco entre entregas, revisoes e board.";
            }
            else
            {
                narrative = "A agenda esta respirando. Use esse folego para revisar marcos, validar qualidade das entregas e distribuir responsabilidades antes da proxima rodada.";
            }

            var card = new Border
            {
                Background = GetThemeBrush("CardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(24),
                Padding = new Thickness(22),
                Margin = new Thickness(0, 0, 0, 18)
            };

            var stack = new StackPanel();
            var header = new Grid();
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var textStack = new StackPanel();
            textStack.Children.Add(new Border
            {
                Background = CreateSoftAccentBrush(GetThemeBrush("CalendarIconBrush"), 26),
                CornerRadius = new CornerRadius(999),
                Padding = new Thickness(10, 5, 10, 5),
                HorizontalAlignment = HorizontalAlignment.Left,
                Child = new TextBlock
                {
                    Text = "Painel do semestre",
                    FontSize = 10,
                    FontWeight = FontWeights.Bold,
                    Foreground = GetThemeBrush("CalendarIconBrush")
                }
            });
            textStack.Children.Add(new TextBlock
            {
                Text = "Agenda central das equipes",
                Margin = new Thickness(0, 12, 0, 0),
                FontFamily = GetAppDisplayFontFamily(),
                FontSize = 24,
                FontWeight = FontWeights.ExtraBold,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            });
            textStack.Children.Add(new TextBlock
            {
                Text = narrative,
                Margin = new Thickness(0, 10, 0, 0),
                FontSize = 12,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20,
                MaxWidth = 760
            });

            var chipRow = new WrapPanel { Margin = new Thickness(0, 14, 0, 0) };
            chipRow.Children.Add(CreateStaticTeamChip($"{teams.Count} equipe(s)", GetThemeBrush("AccentMutedBrush"), GetThemeBrush("AccentBrush")));
            chipRow.Children.Add(CreateStaticTeamChip($"{agendaItems.Count} prazos rastreados", GetThemeBrush("MutedCardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            chipRow.Children.Add(CreateStaticTeamChip(overdueCount == 0 ? "Sem atrasos hoje" : $"{overdueCount} em risco", overdueCount == 0 ? CreateSoftAccentBrush(new SolidColorBrush(Color.FromRgb(16, 185, 129)), 28) : CreateSoftAccentBrush(new SolidColorBrush(Color.FromRgb(220, 38, 38)), 28), overdueCount == 0 ? new SolidColorBrush(Color.FromRgb(16, 185, 129)) : new SolidColorBrush(Color.FromRgb(220, 38, 38))));
            textStack.Children.Add(chipRow);
            header.Children.Add(textStack);

            if (focusTeam != null)
            {
                var openButton = CreateCalendarOpenTeamButton(
                    focusTeam,
                    _activeTeamWorkspace != null && string.Equals(_activeTeamWorkspace.TeamId, focusTeam.TeamId, StringComparison.OrdinalIgnoreCase)
                        ? "Abrir equipe ativa"
                        : "Abrir equipe foco");
                openButton.Margin = new Thickness(16, 0, 0, 0);
                Grid.SetColumn(openButton, 1);
                header.Children.Add(openButton);
            }

            stack.Children.Add(header);

            var metrics = new WrapPanel { Margin = new Thickness(0, 18, 0, 0) };
            metrics.Children.Add(CreateBoardOverviewMetric("Equipes", teams.Count.ToString(), "Base acompanhada", Color.FromRgb(37, 99, 235)));
            metrics.Children.Add(CreateBoardOverviewMetric("Marcos", milestoneCount.ToString(), "Entregas abertas", Color.FromRgb(124, 58, 237)));
            metrics.Children.Add(CreateBoardOverviewMetric("Board", openTasks.ToString(), "Tarefas ainda ativas", Color.FromRgb(14, 165, 233)));
            metrics.Children.Add(CreateBoardOverviewMetric("Janela 7d", nextSevenCount.ToString(), "Compromissos proximos", Color.FromRgb(245, 158, 11)));
            metrics.Children.Add(CreateBoardOverviewMetric("Em risco", overdueCount.ToString(), "Atrasos e urgencias", Color.FromRgb(220, 38, 38)));
            stack.Children.Add(metrics);

            if (focusTeam != null)
            {
                stack.Children.Add(CreateCalendarQuickActionsCard(focusTeam));
            }

            card.Child = stack;
            return card;
        }

        private Border CreateCalendarQuickActionsCard(TeamWorkspaceInfo focusTeam)
        {
            var accentBrush = GetThemeBrush("AccentBrush");
            var card = new Border
            {
                Background = GetThemeBrush("MutedCardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(20),
                Padding = new Thickness(18),
                Margin = new Thickness(0, 18, 0, 0)
            };

            var stack = new StackPanel();
            stack.Children.Add(new Border
            {
                Background = CreateSoftAccentBrush(accentBrush, 24),
                CornerRadius = new CornerRadius(999),
                Padding = new Thickness(10, 5, 10, 5),
                HorizontalAlignment = HorizontalAlignment.Left,
                Child = new TextBlock
                {
                    Text = "Acoes rapidas da agenda",
                    FontSize = 10,
                    FontWeight = FontWeights.Bold,
                    Foreground = accentBrush
                }
            });
            stack.Children.Add(new TextBlock
            {
                Text = $"Aplicando em {focusTeam.TeamName}",
                Margin = new Thickness(0, 12, 0, 0),
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            });
            stack.Children.Add(new TextBlock
            {
                Text = "Abra um novo marco, ajuste o prazo principal ou crie uma tarefa do board sem sair da leitura executiva do calendario.",
                Margin = new Thickness(0, 6, 0, 0),
                FontSize = 11,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 18
            });

            var chips = new WrapPanel { Margin = new Thickness(0, 12, 0, 0) };
            chips.Children.Add(CreateStaticTeamChip(focusTeam.TeamName, GetThemeBrush("AccentMutedBrush"), GetThemeBrush("AccentBrush")));
            chips.Children.Add(CreateStaticTeamChip($"Status {focusTeam.ProjectStatus}", GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            chips.Children.Add(CreateStaticTeamChip($"Prazo {GetNextTeamDeadlineLabel(focusTeam)}", GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            stack.Children.Add(chips);

            var actions = new WrapPanel { Margin = new Thickness(0, 14, 0, 0) };
            actions.Children.Add(CreateSidebarButton("Nova entrega", Color.FromRgb(124, 58, 237), (s, e) => OpenAddMilestoneDialog(focusTeam), PackIconMaterialKind.FlagCheckered));
            actions.Children.Add(CreateSidebarButton("Ajustar prazo principal", Color.FromRgb(14, 165, 233), (s, e) => OpenProjectManagementDialog(focusTeam), PackIconMaterialKind.CalendarEdit));
            actions.Children.Add(CreateSidebarButton("Nova tarefa", Color.FromRgb(37, 99, 235), (s, e) => OpenCreateTaskDialog(focusTeam), PackIconMaterialKind.TextBoxPlusOutline));
            stack.Children.Add(actions);

            card.Child = stack;
            return card;
        }

        private Border CreateCalendarWeekStripCard(IReadOnlyList<CalendarAgendaItem> agendaItems)
        {
            var culture = CultureInfo.GetCultureInfo("pt-BR");
            var card = new Border
            {
                Background = GetThemeBrush("CardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(20),
                Padding = new Thickness(18),
                Margin = new Thickness(0, 0, 0, 18)
            };

            var stack = new StackPanel();
            stack.Children.Add(new TextBlock
            {
                Text = "Janela dos proximos 7 dias",
                FontFamily = GetAppDisplayFontFamily(),
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            });
            stack.Children.Add(new TextBlock
            {
                Text = "Leia a semana como um quadro de carga academica: hoje, amanha e os proximos checkpoints distribuidos por dia.",
                Margin = new Thickness(0, 6, 0, 14),
                FontSize = 11,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            });

            var grid = new UniformGrid
            {
                Columns = 7
            };

            for (var offset = 0; offset < 7; offset++)
            {
                var date = DateTime.Today.AddDays(offset);
                var dueCount = agendaItems.Count(item => item.DueDate.Date == date.Date);
                var isToday = offset == 0;
                var hasItems = dueCount > 0;

                grid.Children.Add(new Border
                {
                    Background = isToday
                        ? GetThemeBrush("AccentMutedBrush")
                        : hasItems
                            ? GetThemeBrush("CardBackgroundBrush")
                            : GetThemeBrush("MutedCardBackgroundBrush"),
                    BorderBrush = isToday
                        ? GetThemeBrush("AccentBrush")
                        : hasItems
                            ? GetThemeBrush("CardBorderBrush")
                            : GetThemeBrush("CardBorderBrush"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(16),
                    Padding = new Thickness(12),
                    Margin = new Thickness(0, 0, 10, 0),
                    Child = new StackPanel
                    {
                        Children =
                        {
                            new TextBlock
                            {
                                Text = date.ToString("ddd", culture).ToUpperInvariant(),
                                FontSize = 10,
                                FontWeight = FontWeights.Bold,
                                Foreground = isToday ? GetThemeBrush("AccentBrush") : GetThemeBrush("TertiaryTextBrush")
                            },
                            new TextBlock
                            {
                                Text = date.ToString("dd/MM"),
                                Margin = new Thickness(0, 8, 0, 0),
                                FontSize = 18,
                                FontWeight = FontWeights.ExtraBold,
                                Foreground = GetThemeBrush("PrimaryTextBrush")
                            },
                            new TextBlock
                            {
                                Text = hasItems ? (dueCount == 1 ? "1 compromisso" : $"{dueCount} compromissos") : "Sem entregas",
                                Margin = new Thickness(0, 8, 0, 0),
                                FontSize = 11,
                                Foreground = hasItems ? GetThemeBrush("PrimaryTextBrush") : GetThemeBrush("SecondaryTextBrush"),
                                TextWrapping = TextWrapping.Wrap
                            }
                        }
                    }
                });
            }

            stack.Children.Add(grid);
            card.Child = stack;
            return card;
        }

        private Border CreateCalendarPrioritySection(IReadOnlyList<CalendarAgendaItem> agendaItems)
        {
            var border = CreateSidebarSection("Foco imediato", "O que precisa entrar na conversa da proxima daily, reuniao de orientacao ou alinhamento da equipe.");
            var content = (StackPanel)border.Child;

            var urgentItems = agendaItems
                .Where(item => item.IsOverdue || item.DueDate.Date <= DateTime.Today.AddDays(2))
                .Take(6)
                .ToList();

            var overview = new WrapPanel { Margin = new Thickness(0, 12, 0, 4) };
            overview.Children.Add(CreateSidebarMiniMetric("Atrasados", agendaItems.Count(item => item.IsOverdue).ToString(), Color.FromRgb(220, 38, 38)));
            overview.Children.Add(CreateSidebarMiniMetric("Hoje", agendaItems.Count(item => item.DueDate.Date == DateTime.Today).ToString(), Color.FromRgb(245, 158, 11)));
            overview.Children.Add(CreateSidebarMiniMetric("Amanha", agendaItems.Count(item => item.DueDate.Date == DateTime.Today.AddDays(1)).ToString(), Color.FromRgb(37, 99, 235)));
            content.Children.Add(overview);

            if (urgentItems.Count == 0)
            {
                content.Children.Add(new Border
                {
                    Background = CreateSoftAccentBrush(new SolidColorBrush(Color.FromRgb(16, 185, 129)), 26),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(16, 185, 129)),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(16),
                    Padding = new Thickness(14),
                    Margin = new Thickness(0, 10, 0, 0),
                    Child = new TextBlock
                    {
                        Text = "Sem urgencias imediatas. O painel de timeline abaixo pode ser usado para preparar a proxima semana com calma.",
                        FontSize = 11,
                        Foreground = new SolidColorBrush(Color.FromRgb(21, 128, 61)),
                        TextWrapping = TextWrapping.Wrap,
                        LineHeight = 18
                    }
                });
                return border;
            }

            foreach (var item in urgentItems)
            {
                content.Children.Add(CreateCalendarAgendaItemCard(item, emphasize: true));
            }

            return border;
        }

        private Border CreateCalendarTimelineSection(IReadOnlyList<CalendarAgendaItem> agendaItems)
        {
            var border = CreateSidebarSection("Linha do tempo", "Sequencia de entregas e tarefas futuras para visualizar a cadencia academica sem abrir equipe por equipe.");
            var content = (StackPanel)border.Child;

            var timelineItems = agendaItems
                .Where(item => item.DueDate.Date >= DateTime.Today)
                .Take(12)
                .ToList();

            if (timelineItems.Count == 0)
            {
                content.Children.Add(new TextBlock
                {
                    Text = "Nao ha compromissos futuros com data definida ainda. Vale abrir as equipes e configurar prazo principal, milestones e vencimentos no board.",
                    FontSize = 11,
                    Margin = new Thickness(0, 12, 0, 0),
                    Foreground = GetThemeBrush("SecondaryTextBrush"),
                    TextWrapping = TextWrapping.Wrap,
                    LineHeight = 18
                });
                return border;
            }

            foreach (var group in timelineItems.GroupBy(item => item.DueDate.Date).OrderBy(group => group.Key))
            {
                content.Children.Add(new TextBlock
                {
                    Text = GetAgendaDateHeading(group.Key),
                    Margin = new Thickness(0, 14, 0, 2),
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                    Foreground = GetThemeBrush("PrimaryTextBrush")
                });

                foreach (var item in group)
                {
                    content.Children.Add(CreateCalendarAgendaItemCard(item));
                }
            }

            return border;
        }

        private Border CreateCalendarTeamRadarSection(IReadOnlyList<TeamWorkspaceInfo> teams, IReadOnlyList<CalendarAgendaItem> agendaItems)
        {
            var border = CreateSidebarSection("Radar das equipes", "Panorama rapido de carga, risco e proximo movimento recomendado por equipe.");
            var content = (StackPanel)border.Child;

            foreach (var item in teams
                .Select(team => new
                {
                    Team = team,
                    Overdue = agendaItems.Count(agendaItem => string.Equals(agendaItem.Team.TeamId, team.TeamId, StringComparison.OrdinalIgnoreCase) && agendaItem.IsOverdue),
                    Upcoming = agendaItems.Count(agendaItem => string.Equals(agendaItem.Team.TeamId, team.TeamId, StringComparison.OrdinalIgnoreCase) && !agendaItem.IsOverdue && agendaItem.DueDate.Date <= DateTime.Today.AddDays(7)),
                    RiskScore = GetCalendarTeamRiskScore(team, agendaItems)
                })
                .OrderByDescending(item => item.RiskScore)
                .ThenBy(item => item.Team.TeamName)
                .Take(6))
            {
                var isActive = _activeTeamWorkspace != null && string.Equals(_activeTeamWorkspace.TeamId, item.Team.TeamId, StringComparison.OrdinalIgnoreCase);
                var statusColor = item.Overdue > 0
                    ? Color.FromRgb(220, 38, 38)
                    : item.Upcoming > 2
                        ? Color.FromRgb(245, 158, 11)
                        : Color.FromRgb(16, 185, 129);
                var statusLabel = item.Overdue > 0
                    ? "Em risco"
                    : item.Upcoming > 2
                        ? "Semana cheia"
                        : "Estavel";

                var card = new Border
                {
                    Background = isActive ? CreateSoftAccentBrush(GetThemeBrush("AccentBrush"), 20) : GetThemeBrush("MutedCardBackgroundBrush"),
                    BorderBrush = isActive ? GetThemeBrush("AccentBrush") : GetThemeBrush("CardBorderBrush"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(16),
                    Padding = new Thickness(14),
                    Margin = new Thickness(0, 10, 0, 0)
                };

                var stack = new StackPanel();
                stack.Children.Add(new TextBlock
                {
                    Text = item.Team.TeamName,
                    FontSize = 13,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = GetThemeBrush("PrimaryTextBrush")
                });
                stack.Children.Add(new TextBlock
                {
                    Text = $"{item.Team.Course} • {item.Team.ClassName}",
                    Margin = new Thickness(0, 4, 0, 0),
                    FontSize = 11,
                    Foreground = GetThemeBrush("SecondaryTextBrush"),
                    TextWrapping = TextWrapping.Wrap
                });

                var chips = new WrapPanel { Margin = new Thickness(0, 10, 0, 0) };
                chips.Children.Add(CreateStaticTeamChip(statusLabel, new SolidColorBrush(statusColor), Brushes.White));
                chips.Children.Add(CreateStaticTeamChip($"Progresso {CalculateTeamProgressPercentage(item.Team)}%", GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
                chips.Children.Add(CreateStaticTeamChip($"Proximo {GetNextTeamDeadlineLabel(item.Team)}", GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
                chips.Children.Add(CreateStaticTeamChip(BuildTeamBalanceLabel(item.Team), GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
                stack.Children.Add(chips);

                stack.Children.Add(new TextBlock
                {
                    Text = item.Overdue > 0
                        ? $"{item.Overdue} item(ns) atrasado(s) pedem intervencao imediata."
                        : item.Upcoming > 0
                            ? $"{item.Upcoming} compromisso(s) entram na janela da semana."
                            : "Sem alerta imediato; bom momento para fortalecer documentacao e checkpoints.",
                    Margin = new Thickness(0, 10, 0, 0),
                    FontSize = 11,
                    Foreground = GetThemeBrush("SecondaryTextBrush"),
                    TextWrapping = TextWrapping.Wrap,
                    LineHeight = 18
                });

                var openButton = CreateCalendarOpenTeamButton(item.Team);
                openButton.Margin = new Thickness(0, 12, 0, 0);
                stack.Children.Add(openButton);

                card.Child = stack;
                content.Children.Add(card);
            }

            return border;
        }

        private Border CreateCalendarRecentActivitySection(IReadOnlyList<TeamWorkspaceInfo> teams)
        {
            var border = CreateSidebarSection("Movimentacoes recentes", "Ultimos sinais do workspace compartilhado: board, marcos, materiais e ajustes de equipe.");
            var content = (StackPanel)border.Child;

            var recentNotifications = teams
                .SelectMany(team => team.Notifications.Select(notification => new { Team = team, Notification = notification }))
                .OrderByDescending(item => item.Notification.CreatedAt)
                .Take(6)
                .ToList();

            if (recentNotifications.Count == 0)
            {
                content.Children.Add(new TextBlock
                {
                    Text = "Nenhuma movimentacao registrada ainda. Assim que o board, as entregas ou os materiais forem atualizados, o historico aparece aqui.",
                    FontSize = 11,
                    Margin = new Thickness(0, 12, 0, 0),
                    Foreground = GetThemeBrush("SecondaryTextBrush"),
                    TextWrapping = TextWrapping.Wrap,
                    LineHeight = 18
                });
                return border;
            }

            foreach (var item in recentNotifications)
            {
                var card = new Border
                {
                    Background = GetThemeBrush("MutedCardBackgroundBrush"),
                    BorderBrush = GetThemeBrush("CardBorderBrush"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(14),
                    Padding = new Thickness(12),
                    Margin = new Thickness(0, 10, 0, 0)
                };

                var stack = new StackPanel();
                stack.Children.Add(new TextBlock
                {
                    Text = item.Team.TeamName,
                    FontSize = 11,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = GetThemeBrush("AccentBrush")
                });
                stack.Children.Add(new TextBlock
                {
                    Text = item.Notification.Message,
                    Margin = new Thickness(0, 6, 0, 0),
                    FontSize = 11,
                    Foreground = GetThemeBrush("PrimaryTextBrush"),
                    TextWrapping = TextWrapping.Wrap,
                    LineHeight = 18
                });
                stack.Children.Add(new TextBlock
                {
                    Text = item.Notification.CreatedAt.ToString("dd/MM 'as' HH:mm"),
                    Margin = new Thickness(0, 8, 0, 0),
                    FontSize = 10,
                    Foreground = GetThemeBrush("TertiaryTextBrush")
                });

                var openButton = CreateCalendarOpenTeamButton(item.Team, "Abrir equipe");
                openButton.Margin = new Thickness(0, 10, 0, 0);
                stack.Children.Add(openButton);

                card.Child = stack;
                content.Children.Add(card);
            }

            return border;
        }

        private Border CreateCalendarGuidanceSection(IReadOnlyList<TeamWorkspaceInfo> teams, IReadOnlyList<CalendarAgendaItem> agendaItems)
        {
            var border = CreateSidebarSection("Leitura rapida", "Sugestoes automaticas para orientar a proxima rodada de organizacao do projeto integrador.");
            var content = (StackPanel)border.Child;

            var teamsWithoutDeadline = teams.Count(team => !team.ProjectDeadline.HasValue);
            var teamsWithoutMilestones = teams.Count(team => team.Milestones.Count == 0);
            var teamsWithUnassignedTasks = teams.Count(team => team.TaskColumns
                .Where(column => !IsCompletedTaskColumn(column))
                .SelectMany(column => column.Cards)
                .Any(card => card.AssignedUserIds.Count == 0));
            var overdueCount = agendaItems.Count(item => item.IsOverdue);

            var insights = new List<(Color Accent, string Text)>();
            if (overdueCount > 0)
            {
                insights.Add((Color.FromRgb(220, 38, 38), $"Ataque primeiro os {overdueCount} item(ns) atrasados. Eles afetam a leitura real de progresso do semestre."));
            }
            if (teamsWithoutDeadline > 0)
            {
                insights.Add((Color.FromRgb(245, 158, 11), $"{teamsWithoutDeadline} equipe(s) ainda nao possuem prazo principal. Definir essa data melhora radar, priorizacao e percepcao de risco."));
            }
            if (teamsWithoutMilestones > 0)
            {
                insights.Add((Color.FromRgb(124, 58, 237), $"{teamsWithoutMilestones} equipe(s) estao sem milestones. Quebre o semestre em checkpoints menores para orientar alunos e orientadores."));
            }
            if (teamsWithUnassignedTasks > 0)
            {
                insights.Add((Color.FromRgb(37, 99, 235), $"{teamsWithUnassignedTasks} equipe(s) possuem tarefas sem responsavel. Distribuir dono por card evita apagao perto da entrega."));
            }
            if (insights.Count == 0)
            {
                insights.Add((Color.FromRgb(16, 185, 129), "Agenda equilibrada: sem atrasos imediatos e com estrutura minima de prazo. Bom momento para revisar qualidade, narrativa e apresentacao final."));
            }

            foreach (var insight in insights.Take(4))
            {
                content.Children.Add(CreateCalendarInsightRow(insight.Text, insight.Accent));
            }

            return border;
        }

        private Border CreateCalendarInsightRow(string text, Color accent)
        {
            return new Border
            {
                Background = GetThemeBrush("MutedCardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(14),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 10, 0, 0),
                Child = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        new Border
                        {
                            Width = 10,
                            Height = 10,
                            Margin = new Thickness(0, 4, 10, 0),
                            Background = new SolidColorBrush(accent),
                            CornerRadius = new CornerRadius(999),
                            VerticalAlignment = VerticalAlignment.Top
                        },
                        new TextBlock
                        {
                            Text = text,
                            FontSize = 11,
                            Foreground = GetThemeBrush("PrimaryTextBrush"),
                            TextWrapping = TextWrapping.Wrap,
                            LineHeight = 18,
                            MaxWidth = 360
                        }
                    }
                }
            };
        }

        private Border CreateCalendarAgendaItemCard(CalendarAgendaItem item, bool emphasize = false)
        {
            var accent = item.IsOverdue ? Color.FromRgb(220, 38, 38) : item.AccentColor;
            var accentBrush = new SolidColorBrush(accent);
            var background = item.IsOverdue
                ? new SolidColorBrush(Color.FromRgb(255, 247, 237))
                : emphasize
                    ? CreateSoftAccentBrush(accentBrush, 18)
                    : GetThemeBrush("MutedCardBackgroundBrush");

            var card = new Border
            {
                Background = background,
                BorderBrush = accentBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(16),
                Padding = new Thickness(14),
                Margin = new Thickness(0, 10, 0, 0)
            };

            var layout = new Grid();
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var iconBadge = new Border
            {
                Width = 42,
                Height = 42,
                Background = CreateSoftAccentBrush(accentBrush, 24),
                BorderBrush = accentBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(14),
                Child = CreateMaterialIcon(item.IconKind, accentBrush, 18),
                VerticalAlignment = VerticalAlignment.Top
            };
            layout.Children.Add(iconBadge);

            var content = new StackPanel();
            content.Children.Add(new TextBlock
            {
                Text = item.Title,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = GetThemeBrush("PrimaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            });
            content.Children.Add(new TextBlock
            {
                Text = item.Subtitle,
                Margin = new Thickness(0, 4, 0, 0),
                FontSize = 11,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            });

            var notes = TruncateAgendaText(item.Notes, 160);
            if (!string.IsNullOrWhiteSpace(notes))
            {
                content.Children.Add(new TextBlock
                {
                    Text = notes,
                    Margin = new Thickness(0, 8, 0, 0),
                    FontSize = 11,
                    Foreground = GetThemeBrush("SecondaryTextBrush"),
                    TextWrapping = TextWrapping.Wrap,
                    LineHeight = 18,
                    MaxWidth = 540
                });
            }

            var chips = new WrapPanel { Margin = new Thickness(0, 10, 0, 0) };
            chips.Children.Add(CreateStaticTeamChip(item.KindLabel, CreateSoftAccentBrush(accentBrush, 24), accentBrush));
            chips.Children.Add(CreateStaticTeamChip($"{GetAgendaDueCountdownText(item.DueDate)} • {item.DueDate:dd/MM}", item.IsOverdue ? new SolidColorBrush(Color.FromRgb(220, 38, 38)) : GetThemeBrush("CardBackgroundBrush"), item.IsOverdue ? Brushes.White : GetThemeBrush("PrimaryTextBrush")));
            if (!string.IsNullOrWhiteSpace(item.StatusLabel))
            {
                chips.Children.Add(CreateStaticTeamChip(item.StatusLabel, GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            }
            content.Children.Add(chips);

            Grid.SetColumn(content, 2);
            layout.Children.Add(content);

            var openButton = CreateCalendarOpenTeamButton(item.Team, emphasize ? "Abrir equipe" : "Abrir");
            openButton.Margin = new Thickness(14, 0, 0, 0);
            Grid.SetColumn(openButton, 3);
            layout.Children.Add(openButton);

            card.Child = layout;
            return card;
        }

        private Button CreateCalendarOpenTeamButton(TeamWorkspaceInfo team, string text = "Abrir equipe")
        {
            var button = new Button
            {
                Content = text,
                Tag = team,
                MinWidth = 110,
                Height = 36,
                Padding = new Thickness(14, 8, 14, 8),
                Background = GetThemeBrush("AccentBrush"),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontWeight = FontWeights.SemiBold,
                Cursor = Cursors.Hand,
                VerticalAlignment = VerticalAlignment.Center
            };
            button.Click += OpenTeamWorkspaceFromCalendar_Click;
            return button;
        }

        private Border CreateDraftChip(string text, Brush background, Brush foreground, Action? onRemove = null)
        {
            var content = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            content.Children.Add(new TextBlock { Text = text, Foreground = foreground, FontSize = 12, FontWeight = FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center });

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
            var content = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            content.Children.Add(new Border
            {
                Width = 28,
                Height = 28,
                CornerRadius = new CornerRadius(14),
                Margin = new Thickness(0, 0, 8, 0),
                ClipToBounds = true,
                Child = CreateUserAvatarVisual(member, 28)
            });

            var labels = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            labels.Children.Add(new TextBlock
            {
                Text = string.IsNullOrWhiteSpace(member.Name) ? member.Email : member.Name,
                Foreground = foreground,
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center
            });

            var roleLabel = TeamPermissionService.GetRoleLabel(member.Role);
            labels.Children.Add(new TextBlock
            {
                Text = roleLabel,
                Foreground = foreground,
                Opacity = 0.78,
                FontSize = 10,
                Margin = new Thickness(0, 2, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            });

            content.Children.Add(labels);

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
                    Text = "Nenhum participante adicionado ainda.",
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
                _teamWorkspaceRenderSequence++;
                TeamWorkspaceHost.Content = null;
                UpdateTeamsViewState();
                return;
            }

            EnsureTeamWorkspaceDefaults(_activeTeamWorkspace);
            UpdateTeamsViewState();
            QueueTeamWorkspaceRender();
        }

        private async void QueueTeamWorkspaceRender()
        {
            var team = _activeTeamWorkspace;
            if (team == null)
            {
                return;
            }

            var renderSequence = ++_teamWorkspaceRenderSequence;
            await Dispatcher.Yield(DispatcherPriority.Background);

            if (renderSequence != _teamWorkspaceRenderSequence || !ReferenceEquals(team, _activeTeamWorkspace))
            {
                return;
            }

            TeamWorkspaceHost.Content = CreateTeamWorkspaceContent(team);
        }

        private UIElement CreateTeamWorkspaceLoadingState(TeamWorkspaceInfo team)
        {
            return new Border
            {
                Margin = new Thickness(22),
                Padding = new Thickness(28),
                CornerRadius = new CornerRadius(24),
                Background = GetThemeBrush("CardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                Child = new StackPanel
                {
                    Children =
                    {
                        new TextBlock
                        {
                            Text = $"Abrindo {team.TeamName}",
                            FontSize = 22,
                            FontWeight = FontWeights.ExtraBold,
                            Foreground = GetThemeBrush("PrimaryTextBrush")
                        },
                        new TextBlock
                        {
                            Text = "Montando cards, métricas e atividades da equipe em segundo plano.",
                            Margin = new Thickness(0, 10, 0, 0),
                            FontSize = 12,
                            Foreground = GetThemeBrush("SecondaryTextBrush"),
                            TextWrapping = TextWrapping.Wrap
                        }
                    }
                }
            };
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
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var logoBadge = CreateTeamLogoBadge(team, 84, circular: true, fontSize: 24);
            logoBadge.Margin = new Thickness(0, 0, 18, 0);
            logoBadge.VerticalAlignment = VerticalAlignment.Center;
            grid.Children.Add(logoBadge);

            var titleStack = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center
            };
            titleStack.Children.Add(new TextBlock
            {
                Text = team.TeamName,
                FontFamily = GetAppDisplayFontFamily(),
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

            var metadataWrap = new WrapPanel { Margin = new Thickness(0, 10, 0, 0) };
            metadataWrap.Children.Add(CreateStaticTeamChip(
                string.IsNullOrWhiteSpace(team.AcademicTerm) ? "Sem semestre" : $"Semestre {team.AcademicTerm}",
                GetThemeBrush("AccentMutedBrush"),
                GetThemeBrush("AccentBrush")));
            if (!string.IsNullOrWhiteSpace(team.TemplateName))
            {
                metadataWrap.Children.Add(CreateStaticTeamChip(team.TemplateName, GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            }
            metadataWrap.Children.Add(CreateStaticTeamChip(
                team.LastRealtimeSyncAt.HasValue
                    ? $"Sync {team.LastRealtimeSyncAt.Value:dd/MM HH:mm}"
                    : "Sync pendente",
                GetThemeBrush("CardBackgroundBrush"),
                GetThemeBrush("PrimaryTextBrush")));
            metadataWrap.Children.Add(CreateStaticTeamChip(BuildTeamProfessorFocusLabel(team), GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            metadataWrap.Children.Add(CreateStaticTeamChip(BuildTeamLeadershipLabel(team), GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            metadataWrap.Children.Add(CreateStaticTeamChip(BuildTeamBalanceLabel(team), GetThemeBrush("MutedCardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            titleStack.Children.Add(metadataWrap);
            Grid.SetColumn(titleStack, 1);
            grid.Children.Add(titleStack);

            var actions = new WrapPanel
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top
            };

            actions.Children.Add(CreateTeamWorkspaceActionButton("Copiar codigo", Color.FromRgb(14, 165, 233), CopyTeamCode_Click, PackIconMaterialKind.ContentCopy));
            actions.Children.Add(CreateTeamWorkspaceActionButton(GetTeamLogoAsset(team) == null ? "Adicionar logo" : "Trocar logo", Color.FromRgb(236, 72, 153), EditTeamLogo_Click, PackIconMaterialKind.ImageOutline));
            if (GetTeamLogoAsset(team) != null)
            {
                actions.Children.Add(CreateTeamWorkspaceActionButton("Remover logo", Color.FromRgb(100, 116, 139), RemoveTeamLogo_Click, PackIconMaterialKind.DeleteOutline));
            }
            if (CanCurrentUserAddMembers(team))
            {
                actions.Children.Add(CreateTeamWorkspaceActionButton("Adicionar membro", Color.FromRgb(37, 99, 235), (s, e) => OpenAddTeamMemberDialog(team), PackIconMaterialKind.AccountPlusOutline));
            }
            if (CanCurrentUserAssignLeadership(team))
            {
                actions.Children.Add(CreateTeamWorkspaceActionButton("Definir líder", Color.FromRgb(124, 58, 237), (s, e) => OpenAssignTeamLeaderDialog(team), PackIconMaterialKind.AccountStarOutline));
            }
            if (CanCurrentUserManageMembers(team))
            {
                actions.Children.Add(CreateTeamWorkspaceActionButton("Remover membro", Color.FromRgb(245, 158, 11), (s, e) => OpenRemoveTeamMemberDialog(team), PackIconMaterialKind.AccountRemoveOutline));
            }
            if (CanCurrentUserDeleteTeam(team))
            {
                actions.Children.Add(CreateTeamWorkspaceActionButton("Apagar equipe", Color.FromRgb(220, 38, 38), DeleteActiveTeamWorkspace, PackIconMaterialKind.DeleteOutline));
            }
            actions.Children.Add(CreateTeamWorkspaceActionButton("Fechar", Color.FromRgb(100, 116, 139), CloseTeamWorkspace_Click, PackIconMaterialKind.Close));

            Grid.SetColumn(actions, 2);
            grid.Children.Add(actions);

            border.Child = grid;
            return border;
        }

        private UIElement CreateTeamWorkspaceMetrics(TeamWorkspaceInfo team)
        {
            var overdueCount = team.TaskColumns.SelectMany(column => column.Cards).Count(card => card.DueDate.HasValue && card.DueDate.Value.Date < DateTime.Today);
            var completedMilestones = team.Milestones.Count(milestone => string.Equals(milestone.Status, "Concluida", StringComparison.OrdinalIgnoreCase));
            var studentCount = GetStudentTeamMembers(team).Count;
            var facultyCount = GetFacultyMembers(team).Count;

            var wrap = new WrapPanel
            {
                Margin = new Thickness(22, 18, 22, 18)
            };

            wrap.Children.Add(CreateTeamMetricCard("Discentes", $"{studentCount}", "Execução do projeto", Color.FromRgb(37, 99, 235)));
            wrap.Children.Add(CreateTeamMetricCard("Docência", $"{facultyCount}", "Orientação e governança", Color.FromRgb(124, 58, 237)));
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

        private Button CreateTeamWorkspaceActionButton(string text, Color backgroundColor, RoutedEventHandler onClick, PackIconMaterialKind? iconKind = null)
        {
            var backgroundBrush = new SolidColorBrush(backgroundColor);
            var button = new Button
            {
                Content = iconKind.HasValue
                    ? CreateIconLabelContent(text, iconKind.Value, Brushes.White, Brushes.White, 14, 12, FontWeights.SemiBold)
                    : text,
                Background = backgroundBrush,
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

            var dragInfo = new TeamTaskDragInfo
            {
                Team = team,
                Column = column,
                Card = card,
                CompactMode = compactMode
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
                Tag = dragInfo
            };
            cardBorder.MouseMove += TeamTaskCard_MouseMove;
            cardBorder.GiveFeedback += BoardDragPreview_GiveFeedback;

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

            if (card.EstimatedHours > 0)
            {
                chips.Children.Add(CreateStaticTeamChip($"{card.EstimatedHours}h", GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            }

            if (card.WorkloadPoints > 0)
            {
                chips.Children.Add(CreateStaticTeamChip($"{card.WorkloadPoints} pts", GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            }

            if (!string.IsNullOrWhiteSpace(card.RequiredRole) && !string.Equals(card.RequiredRole, "student", StringComparison.OrdinalIgnoreCase))
            {
                chips.Children.Add(CreateStaticTeamChip(TeamPermissionService.GetRoleLabel(card.RequiredRole), GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            }

            if (card.RequiresProfessorReview)
            {
                chips.Children.Add(CreateStaticTeamChip("Revisão docente", GetThemeBrush("AccentMutedBrush"), GetThemeBrush("AccentBrush")));
            }

            if (card.Comments.Count > 0)
            {
                chips.Children.Add(CreateStaticTeamChip($"{card.Comments.Count} comentário(s)", GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            }

            if (card.Attachments.Count > 0)
            {
                chips.Children.Add(CreateStaticTeamChip($"{card.Attachments.Count} anexo(s)", GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
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
            footer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            footer.Children.Add(new TextBlock
            {
                Text = $"Criado em {card.CreatedAt:dd/MM}",
                FontSize = 10,
                Foreground = GetThemeBrush("TertiaryTextBrush")
            });

            if (CanCurrentUserComment(team) || CanCurrentUserUploadFiles(team))
            {
                var collaborateButton = new Button
                {
                    Content = "Colaborar",
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Foreground = GetThemeBrush("AccentBrush"),
                    FontSize = 10,
                    FontWeight = FontWeights.SemiBold,
                    Cursor = Cursors.Hand,
                    Padding = new Thickness(6, 0, 0, 0),
                    Tag = Tuple.Create(team, column, card)
                };
                collaborateButton.Click += OpenTaskCollaboration_Click;
                Grid.SetColumn(collaborateButton, 1);
                footer.Children.Add(collaborateButton);
            }

            if (CanCurrentUserEditProjectSettings(team))
            {
                var editButton = new Button
                {
                    Content = "Editar",
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Foreground = GetThemeBrush("AccentBrush"),
                    FontSize = 10,
                    FontWeight = FontWeights.SemiBold,
                    Cursor = Cursors.Hand,
                    Padding = new Thickness(8, 0, 0, 0),
                    Tag = Tuple.Create(team, column, card)
                };
                editButton.Click += EditTeamTask_Click;
                Grid.SetColumn(editButton, 2);
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
                Grid.SetColumn(deleteButton, 3);
                footer.Children.Add(deleteButton);
            }

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
                Padding = new Thickness(14),
                AllowDrop = true,
                Tag = Tuple.Create(team, title)
            };
            border.DragOver += CsdColumn_DragOver;
            border.Drop += CsdColumn_Drop;

            var stack = new StackPanel();
            stack.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(accent)
            });

            for (var noteIndex = 0; noteIndex < notes.Count; noteIndex++)
            {
                var note = notes[noteIndex];
                var noteCard = new Border
                {
                    Background = GetThemeBrush("MutedCardBackgroundBrush"),
                    BorderBrush = GetThemeBrush("CardBorderBrush"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(14),
                    Padding = new Thickness(12),
                    Margin = new Thickness(0, 10, 0, 0),
                    Cursor = Cursors.Hand,
                    Tag = new CsdNoteDragInfo
                    {
                        Team = team,
                        SourceBucket = title,
                        Note = note,
                        SourceIndex = noteIndex
                    }
                };
                noteCard.MouseMove += CsdNoteCard_MouseMove;
                noteCard.GiveFeedback += BoardDragPreview_GiveFeedback;

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

            var notes = GetCsdNotesBucket(team, bucket);

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
            stack.Children.Add(CreateTeamAcademicTimelineSection(team));
            stack.Children.Add(CreateTeamMilestonesSection(team));
            stack.Children.Add(CreateTeamWorkloadSection(team));
            stack.Children.Add(CreateProjectChatSection(team));
            stack.Children.Add(CreateTeamMembersSection(team));
            stack.Children.Add(CreateTeamAssetsSection(team));
            stack.Children.Add(CreateTeamProfessorNotesSection(team));
            stack.Children.Add(CreateTeamNotificationsSection(team));
            return stack;
        }

        private Border CreateProjectManagementSection(TeamWorkspaceInfo team)
        {
            var border = CreateSidebarSection("Gestao do projeto", "Defina o andamento geral e o prazo principal da equipe.");
            var content = (StackPanel)border.Child;

            content.Children.Add(CreateStaticTeamChip($"Status: {team.ProjectStatus}", GetThemeBrush("AccentMutedBrush"), GetThemeBrush("AccentBrush")));
            if (!string.IsNullOrWhiteSpace(team.TemplateName))
            {
                content.Children.Add(CreateStaticTeamChip(team.TemplateName, GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            }
            if (!string.IsNullOrWhiteSpace(team.AcademicTerm))
            {
                content.Children.Add(CreateStaticTeamChip($"Semestre {team.AcademicTerm}", GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            }

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

            if (!string.IsNullOrWhiteSpace(team.TeacherNotes))
            {
                content.Children.Add(new TextBlock
                {
                    Text = team.TeacherNotes,
                    Margin = new Thickness(0, 10, 0, 0),
                    FontSize = 11,
                    Foreground = GetThemeBrush("SecondaryTextBrush"),
                    TextWrapping = TextWrapping.Wrap,
                    LineHeight = 18
                });
            }

            if (CanCurrentUserEditProjectSettings(team))
            {
                content.Children.Add(CreateSidebarButton("Atualizar progresso e prazo", Color.FromRgb(14, 165, 233), (s, e) => OpenProjectManagementDialog(team), PackIconMaterialKind.Refresh));
            }
            else
            {
                content.Children.Add(CreateDialogHintCard(
                    "A leitura global do projeto fica com a liderança discente e a docência focal. Alunos sem esse papel continuam na execução e colaboração diária.",
                    GetThemeBrush("AccentBrush"),
                    new Thickness(0, 12, 0, 0)));
            }

            return border;
        }

        private Border CreateTeamAcademicTimelineSection(TeamWorkspaceInfo team)
        {
            var border = CreateSidebarSection("Timeline acadêmica", "Macrofases do semestre e checkpoints que estruturam a jornada da equipe.");
            var content = (StackPanel)border.Child;

            if (team.SemesterTimeline.Count == 0)
            {
                content.Children.Add(new TextBlock
                {
                    Text = "Nenhuma timeline foi configurada ainda. Escolha um template ou registre fases do semestre para enxergar a cadência acadêmica.",
                    Margin = new Thickness(0, 12, 0, 0),
                    FontSize = 11,
                    Foreground = GetThemeBrush("SecondaryTextBrush"),
                    TextWrapping = TextWrapping.Wrap,
                    LineHeight = 18
                });
                return border;
            }

            foreach (var item in team.SemesterTimeline.OrderBy(entry => entry.StartsAt ?? DateTime.MaxValue).Take(5))
            {
                var card = new Border
                {
                    Background = GetThemeBrush("MutedCardBackgroundBrush"),
                    BorderBrush = GetThemeBrush("CardBorderBrush"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(16),
                    Padding = new Thickness(12),
                    Margin = new Thickness(0, 10, 0, 0)
                };

                var stack = new StackPanel();
                stack.Children.Add(new TextBlock
                {
                    Text = item.Title,
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = GetThemeBrush("PrimaryTextBrush")
                });
                stack.Children.Add(new TextBlock
                {
                    Text = string.IsNullOrWhiteSpace(item.Description) ? "Sem descrição adicional." : item.Description,
                    Margin = new Thickness(0, 6, 0, 0),
                    FontSize = 11,
                    Foreground = GetThemeBrush("SecondaryTextBrush"),
                    TextWrapping = TextWrapping.Wrap,
                    LineHeight = 18
                });

                var chips = new WrapPanel { Margin = new Thickness(0, 8, 0, 0) };
                chips.Children.Add(CreateStaticTeamChip(item.Category, GetThemeBrush("AccentMutedBrush"), GetThemeBrush("AccentBrush")));
                chips.Children.Add(CreateStaticTeamChip(item.Status, GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
                if (item.StartsAt.HasValue || item.EndsAt.HasValue)
                {
                    chips.Children.Add(CreateStaticTeamChip(
                        $"{item.StartsAt:dd/MM} → {item.EndsAt:dd/MM}",
                        GetThemeBrush("CardBackgroundBrush"),
                        GetThemeBrush("PrimaryTextBrush")));
                }
                stack.Children.Add(chips);
                card.Child = stack;
                content.Children.Add(card);
            }

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

            content.Children.Add(CreateSidebarButton("Abrir chat do projeto", Color.FromRgb(37, 99, 235), (s, e) => OpenProjectChatDialog(team), PackIconMaterialKind.MessageTextOutline));
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
            content.Children.Add(CreateSidebarButton("Adicionar entrega", Color.FromRgb(124, 58, 237), (s, e) => OpenAddMilestoneDialog(team), PackIconMaterialKind.FlagCheckered));

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
                if (milestone.RequiresProfessorReview)
                {
                    badges.Children.Add(CreateStaticTeamChip("Revisão docente", GetThemeBrush("AccentMutedBrush"), GetThemeBrush("AccentBrush")));
                }
                if (milestone.Comments.Count > 0)
                {
                    badges.Children.Add(CreateStaticTeamChip($"{milestone.Comments.Count} comentário(s)", GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
                }
                if (milestone.Attachments.Count > 0)
                {
                    badges.Children.Add(CreateStaticTeamChip($"{milestone.Attachments.Count} anexo(s)", GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
                }
                info.Children.Add(badges);

                var actions = new WrapPanel
                {
                    Margin = new Thickness(0, 8, 0, 0)
                };

                if (CanCurrentUserComment(team) || CanCurrentUserUploadFiles(team))
                {
                    var collaborateButton = new Button
                    {
                        Content = "Colaboração",
                        Background = Brushes.Transparent,
                        Foreground = GetThemeBrush("AccentBrush"),
                        BorderThickness = new Thickness(0),
                        Cursor = Cursors.Hand,
                        Padding = new Thickness(0),
                        FontWeight = FontWeights.SemiBold,
                        Tag = Tuple.Create(team, milestone)
                    };
                    collaborateButton.Click += OpenMilestoneCollaboration_Click;
                    actions.Children.Add(collaborateButton);
                }

                if (CanCurrentUserReviewDeliverables(team))
                {
                    var toggleButton = new Button
                    {
                        Content = isDone ? "Reabrir" : "Concluir",
                        Background = Brushes.Transparent,
                        Foreground = GetThemeBrush("AccentBrush"),
                        BorderThickness = new Thickness(0),
                        Cursor = Cursors.Hand,
                        Padding = new Thickness(actions.Children.Count == 0 ? 0 : 12, 0, 0, 0),
                        FontWeight = FontWeights.SemiBold,
                        Tag = Tuple.Create(team, milestone)
                    };
                    toggleButton.Click += ToggleMilestoneStatus_Click;
                    actions.Children.Add(toggleButton);
                }

                if (CanCurrentUserEditProjectSettings(team))
                {
                    var removeButton = new Button
                    {
                        Content = "Remover",
                        Background = Brushes.Transparent,
                        Foreground = new SolidColorBrush(Color.FromRgb(220, 38, 38)),
                        BorderThickness = new Thickness(0),
                        Cursor = Cursors.Hand,
                        Padding = new Thickness(actions.Children.Count == 0 ? 0 : 12, 0, 0, 0),
                        FontWeight = FontWeights.SemiBold,
                        Tag = Tuple.Create(team, milestone)
                    };
                    removeButton.Click += DeleteMilestone_Click;
                    actions.Children.Add(removeButton);
                }

                info.Children.Add(actions);

                Grid.SetColumn(info, 2);
                rowStack.Children.Add(info);
                row.Child = rowStack;
                content.Children.Add(row);
            }

            return border;
        }

        private Border CreateTeamWorkloadSection(TeamWorkspaceInfo team)
        {
            var border = CreateSidebarSection("Carga por membro", "Leitura de distribuição de tarefas para orientar equilíbrio, risco e mentoria.");
            var content = (StackPanel)border.Child;
            var workload = AcademicRiskEngine.BuildMemberWorkload(team).Take(5).ToList();

            if (workload.Count == 0)
            {
                content.Children.Add(new TextBlock
                {
                    Text = "Ainda não há tarefas atribuídas o suficiente para medir carga da equipe.",
                    Margin = new Thickness(0, 12, 0, 0),
                    FontSize = 11,
                    Foreground = GetThemeBrush("SecondaryTextBrush"),
                    TextWrapping = TextWrapping.Wrap,
                    LineHeight = 18
                });
                return border;
            }

            foreach (var item in workload)
            {
                var accent = item.Level switch
                {
                    "Alta" => Color.FromRgb(220, 38, 38),
                    "Media" => Color.FromRgb(245, 158, 11),
                    _ => Color.FromRgb(16, 185, 129)
                };

                var row = new Border
                {
                    Background = GetThemeBrush("MutedCardBackgroundBrush"),
                    BorderBrush = new SolidColorBrush(accent),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(14),
                    Padding = new Thickness(12),
                    Margin = new Thickness(0, 10, 0, 0)
                };

                var stack = new StackPanel();
                stack.Children.Add(new TextBlock
                {
                    Text = item.Name,
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = GetThemeBrush("PrimaryTextBrush")
                });
                stack.Children.Add(new TextBlock
                {
                    Text = $"{item.Role} • {item.Summary}",
                    Margin = new Thickness(0, 4, 0, 0),
                    FontSize = 11,
                    Foreground = GetThemeBrush("SecondaryTextBrush"),
                    TextWrapping = TextWrapping.Wrap,
                    LineHeight = 18
                });

                var chips = new WrapPanel { Margin = new Thickness(0, 8, 0, 0) };
                chips.Children.Add(CreateStaticTeamChip($"Carga {item.Level}", new SolidColorBrush(accent), Brushes.White));
                chips.Children.Add(CreateStaticTeamChip($"{item.WorkloadPoints} pts", GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
                chips.Children.Add(CreateStaticTeamChip($"{item.EstimatedHours}h", GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
                if (item.OverdueTasks > 0)
                {
                    chips.Children.Add(CreateStaticTeamChip($"{item.OverdueTasks} atraso(s)", GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
                }
                stack.Children.Add(chips);

                row.Child = stack;
                content.Children.Add(row);
            }

            return border;
        }

        private Border CreateTeamMembersSection(TeamWorkspaceInfo team)
        {
            var border = CreateSidebarSection("Pessoas e papéis", "Execução fica com estudantes; docência focal entra para orientar, revisar e organizar a governança.");
            var content = (StackPanel)border.Child;

            content.Children.Add(CreateDialogHintCard(
                $"{BuildTeamBalanceLabel(team)}. O professor focal define a liderança e cuida de remoções; a equipe discente segue na execução diária.",
                GetThemeBrush("AccentBrush"),
                new Thickness(0, 12, 0, 0)));

            var studentMembers = GetStudentTeamMembers(team);
            var facultyMembers = GetFacultyMembers(team);

            content.Children.Add(new TextBlock
            {
                Text = "Equipe discente",
                Margin = new Thickness(0, 14, 0, 8),
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            });

            var studentWrap = new WrapPanel();
            foreach (var member in studentMembers)
            {
                studentWrap.Children.Add(CreateMemberChip(
                    member,
                    GetThemeBrush("AccentMutedBrush"),
                    GetThemeBrush("AccentBrush"),
                    CanCurrentUserManageMembers(team) && team.Members.Count > 1
                        ? () => RemoveMemberFromActiveTeam(member)
                        : null));
            }

            if (studentWrap.Children.Count == 0)
            {
                content.Children.Add(new TextBlock
                {
                    Text = "Nenhum aluno foi vinculado ainda a esta equipe.",
                    FontSize = 11,
                    Foreground = GetThemeBrush("SecondaryTextBrush"),
                    TextWrapping = TextWrapping.Wrap
                });
            }
            else
            {
                content.Children.Add(studentWrap);
            }

            content.Children.Add(new TextBlock
            {
                Text = "Orientação docente",
                Margin = new Thickness(0, 14, 0, 8),
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            });

            if (facultyMembers.Count == 0)
            {
                content.Children.Add(new TextBlock
                {
                    Text = "Nenhum docente vinculado ainda. Use a descoberta docente ou a supervisão focal para orientar a equipe sem misturar execução e mentoria.",
                    FontSize = 11,
                    Foreground = GetThemeBrush("SecondaryTextBrush"),
                    TextWrapping = TextWrapping.Wrap,
                    LineHeight = 18
                });
            }
            else
            {
                var facultyWrap = new WrapPanel();
                foreach (var member in facultyMembers)
                {
                    facultyWrap.Children.Add(CreateMemberChip(
                        member,
                        GetThemeBrush("MutedCardBackgroundBrush"),
                        GetThemeBrush("PrimaryTextBrush"),
                        CanCurrentUserManageMembers(team) && team.Members.Count > 1
                            ? () => RemoveMemberFromActiveTeam(member)
                            : null));
                }

                content.Children.Add(facultyWrap);
            }

            var actions = new WrapPanel();
            if (CanCurrentUserAddMembers(team))
            {
                actions.Children.Add(CreateSidebarButton("Adicionar integrante", Color.FromRgb(37, 99, 235), (s, e) => OpenAddTeamMemberDialog(team), PackIconMaterialKind.AccountPlusOutline));
            }
            if (CanCurrentUserAssignLeadership(team) && studentMembers.Count > 0)
            {
                actions.Children.Add(CreateSidebarButton("Definir liderança", Color.FromRgb(124, 58, 237), (s, e) => OpenAssignTeamLeaderDialog(team), PackIconMaterialKind.AccountStarOutline));
            }
            if (CanCurrentUserManageMembers(team) && team.Members.Count > 1)
            {
                actions.Children.Add(CreateSidebarButton("Remover integrante", Color.FromRgb(245, 158, 11), (s, e) => OpenRemoveTeamMemberDialog(team), PackIconMaterialKind.AccountRemoveOutline));
            }

            if (actions.Children.Count > 0)
            {
                content.Children.Add(actions);
            }
            else
            {
                content.Children.Add(CreateDialogHintCard(
                    "Você pode acompanhar a composição da equipe, mas a governança de pessoas fica com o professor focal e a entrada inicial fica com o criador da equipe.",
                    GetThemeBrush("AccentBrush"),
                    new Thickness(0, 12, 0, 0)));
            }
            return border;
        }

        private Border CreateTeamAssetsSection(TeamWorkspaceInfo team)
        {
            var border = CreateSidebarSection("Materiais e planos", "Arquivos e artefatos da equipe ficam centralizados aqui.");
            var content = (StackPanel)border.Child;
            var visibleAssets = team.Assets
                .Where(asset => CanCurrentUserViewAsset(team, asset))
                .OrderByDescending(asset => asset.LastSyncedAt ?? asset.AddedAt)
                .ToList();

            var logoCount = visibleAssets.Count(item => string.Equals(item.Category, "logo", StringComparison.OrdinalIgnoreCase));
            var imagesCount = visibleAssets.Count(item => string.Equals(item.Category, "imagens", StringComparison.OrdinalIgnoreCase));
            var documentsCount = visibleAssets.Count(item => string.Equals(item.Category, "documentos", StringComparison.OrdinalIgnoreCase));
            var plansCount = visibleAssets.Count(item => string.Equals(item.Category, "planos", StringComparison.OrdinalIgnoreCase));
            var syncedCount = visibleAssets.Count(item => item.LastSyncedAt.HasValue);

            var stats = new WrapPanel { Margin = new Thickness(0, 12, 0, 4) };
            stats.Children.Add(CreateSidebarMiniMetric("Logo", logoCount.ToString(), Color.FromRgb(236, 72, 153)));
            stats.Children.Add(CreateSidebarMiniMetric("Imagens", imagesCount.ToString(), Color.FromRgb(14, 165, 233)));
            stats.Children.Add(CreateSidebarMiniMetric("Docs", documentsCount.ToString(), Color.FromRgb(37, 99, 235)));
            stats.Children.Add(CreateSidebarMiniMetric("Planos", plansCount.ToString(), Color.FromRgb(16, 185, 129)));
            stats.Children.Add(CreateSidebarMiniMetric("Sync", syncedCount.ToString(), Color.FromRgb(124, 58, 237)));
            content.Children.Add(stats);

            if (CanCurrentUserUploadFiles(team))
            {
                var actions = new WrapPanel();
                actions.Children.Add(CreateSidebarButton(logoCount == 0 ? "Logo" : "Trocar logo", Color.FromRgb(236, 72, 153), (s, e) => AddTeamAsset("logo"), PackIconMaterialKind.ImageOutline));
                actions.Children.Add(CreateSidebarButton("Imagens", Color.FromRgb(14, 165, 233), (s, e) => AddTeamAsset("imagens"), PackIconMaterialKind.ImageOutline));
                actions.Children.Add(CreateSidebarButton("Documentos", Color.FromRgb(37, 99, 235), (s, e) => AddTeamAsset("documentos"), PackIconMaterialKind.FileDocumentOutline));
                actions.Children.Add(CreateSidebarButton("Planos", Color.FromRgb(16, 185, 129), (s, e) => AddTeamAsset("planos"), PackIconMaterialKind.ClipboardTextOutline));
                content.Children.Add(actions);
            }
            else
            {
                content.Children.Add(CreateDialogHintCard(
                    "Seu papel atual pode consultar materiais liberados, mas não publicar novos arquivos ou versões nesta equipe.",
                    GetThemeBrush("AccentBrush"),
                    new Thickness(0, 12, 0, 0)));
            }

            if (visibleAssets.Count == 0)
            {
                content.Children.Add(new TextBlock
                {
                    Text = team.Assets.Count == 0
                        ? "Nenhum material foi anexado ainda."
                        : "Existem materiais na equipe, mas o escopo atual impede abrir o conteúdo remoto com o seu papel.",
                    FontSize = 11,
                    Margin = new Thickness(0, 12, 0, 0),
                    Foreground = GetThemeBrush("SecondaryTextBrush"),
                    TextWrapping = TextWrapping.Wrap,
                    LineHeight = 18
                });
            }
            else
            {
                foreach (var asset in visibleAssets.Take(6))
                {
                    content.Children.Add(CreateAssetSidebarCard(team, asset));
                }
            }

            return border;
        }

        private Border CreateAssetSidebarCard(TeamWorkspaceInfo team, TeamAssetInfo asset)
        {
            var accent = asset.Category.ToLowerInvariant() switch
            {
                "logo" => Color.FromRgb(236, 72, 153),
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

            var layout = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            if (!string.IsNullOrWhiteSpace(asset.PreviewImageDataUri))
            {
                var preview = CreateTeamLogoBadge(
                    asset.PreviewImageDataUri,
                    asset.FileName,
                    56,
                    circular: string.Equals(asset.Category, "logo", StringComparison.OrdinalIgnoreCase),
                    fontSize: 14,
                    borderThickness: 0);
                preview.Margin = new Thickness(0, 0, 12, 0);
                layout.Children.Add(preview);
            }

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
            stack.Children.Add(new TextBlock
            {
                Text = $"Versão {asset.Version} • Escopo {GetPermissionScopeLabel(asset.PermissionScope)}" + (asset.LastSyncedAt.HasValue ? $" • Sync {asset.LastSyncedAt.Value:dd/MM HH:mm}" : string.Empty),
                FontSize = 10,
                Margin = new Thickness(0, 4, 0, 0),
                Foreground = GetThemeBrush("TertiaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            });

            var actions = new WrapPanel
            {
                Margin = new Thickness(0, 10, 0, 0)
            };

            var openButton = new Button
            {
                Content = "Abrir",
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = GetThemeBrush("AccentBrush"),
                FontSize = 10,
                FontWeight = FontWeights.SemiBold,
                Cursor = Cursors.Hand,
                Padding = new Thickness(0),
                Tag = Tuple.Create(team, asset)
            };
            openButton.Click += OpenTeamAssetPreview_Click;
            actions.Children.Add(openButton);

            if (asset.VersionHistory.Any(version => !string.IsNullOrWhiteSpace(version.StorageReference)))
            {
                var historyButton = new Button
                {
                    Content = "Histórico",
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Foreground = GetThemeBrush("PrimaryTextBrush"),
                    FontSize = 10,
                    FontWeight = FontWeights.SemiBold,
                    Cursor = Cursors.Hand,
                    Padding = new Thickness(12, 0, 0, 0),
                    Tag = Tuple.Create(team, asset)
                };
                historyButton.Click += OpenTeamAssetHistory_Click;
                actions.Children.Add(historyButton);
            }

            if (CanCurrentUserUploadFiles(team) && !string.Equals(asset.Category, "logo", StringComparison.OrdinalIgnoreCase))
            {
                var versionButton = new Button
                {
                    Content = "Nova versão",
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Foreground = GetThemeBrush("PrimaryTextBrush"),
                    FontSize = 10,
                    FontWeight = FontWeights.SemiBold,
                    Cursor = Cursors.Hand,
                    Padding = new Thickness(12, 0, 0, 0),
                    Tag = Tuple.Create(team, asset)
                };
                versionButton.Click += UpdateTeamAssetVersion_Click;
                actions.Children.Add(versionButton);
            }

            stack.Children.Add(actions);
            layout.Children.Add(stack);
            card.Child = layout;
            return card;
        }

        private Border CreateTeamProfessorNotesSection(TeamWorkspaceInfo team)
        {
            var border = CreateSidebarSection("Mentoria e IA acadêmica", "Notas docentes, brief automático e orientação de próxima ação para a equipe.");
            var content = (StackPanel)border.Child;

            var identityWrap = new WrapPanel { Margin = new Thickness(0, 12, 0, 0) };
            identityWrap.Children.Add(CreateStaticTeamChip(BuildTeamProfessorFocusLabel(team), GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            identityWrap.Children.Add(CreateStaticTeamChip(BuildTeamLeadershipLabel(team), GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            content.Children.Add(identityWrap);

            var brief = AcademicRiskEngine.BuildAcademicAssistantBrief(team);
            content.Children.Add(new TextBlock
            {
                Text = string.IsNullOrWhiteSpace(team.TeacherNotes) ? "Sem observação docente fixa até agora." : team.TeacherNotes,
                Margin = new Thickness(0, 12, 0, 0),
                FontSize = 11,
                Foreground = GetThemeBrush("PrimaryTextBrush"),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 18
            });
            content.Children.Add(new Border
            {
                Background = GetThemeBrush("MutedCardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(14),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 12, 0, 0),
                Child = new TextBlock
                {
                    Text = brief,
                    FontSize = 11,
                    Foreground = GetThemeBrush("SecondaryTextBrush"),
                    TextWrapping = TextWrapping.Wrap,
                    LineHeight = 18
                }
            });

            return border;
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
                FontFamily = GetAppDisplayFontFamily(),
                FontSize = 15,
                FontWeight = FontWeights.Bold,
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

        private Button CreateSidebarButton(string text, Color color, RoutedEventHandler onClick, PackIconMaterialKind? iconKind = null)
        {
            var backgroundBrush = new SolidColorBrush(color);
            var button = new Button
            {
                Content = iconKind.HasValue
                    ? CreateIconLabelContent(text, iconKind.Value, Brushes.White, Brushes.White, 14, 12, FontWeights.SemiBold)
                    : text,
                Background = backgroundBrush,
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

            var teamsToRender = ResolveVisibleTeamListResults();
            if (teamsToRender.Count == 0)
            {
                var activeQuery = NormalizeTeamValue(TeamListSearchBox?.Text ?? string.Empty);
                TeamListPanel.Children.Add(CreateTeamsListDiscoveryHintCard(
                    string.IsNullOrWhiteSpace(activeQuery)
                        ? CurrentViewerCanUseProfessorDiscovery()
                            ? "Use a busca desta aba para localizar equipes e projetos de qualquer aluno, mesmo que ainda não estejam carregados localmente."
                            : "Nenhuma equipe carregada ainda para sua conta."
                        : $"Nenhuma equipe encontrada para \"{activeQuery}\"."));

                RenderProfileProjectSelection(_currentProfile);
                RenderCalendarAgenda();
                return;
            }

            foreach (var team in teamsToRender)
            {
                TeamListPanel.Children.Add(CreateTeamListItem(team));
            }

            RenderProfileProjectSelection(_currentProfile);
            RenderCalendarAgenda();
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
            var identityRow = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            var logoBadge = CreateTeamLogoBadge(team, 56, circular: false, fontSize: 18);
            logoBadge.Margin = new Thickness(0, 0, 14, 0);
            identityRow.Children.Add(logoBadge);

            var headingStack = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center
            };
            headingStack.Children.Add(new TextBlock
            {
                Text = team.TeamName,
                FontFamily = GetAppDisplayFontFamily(),
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = GetThemeBrush("PrimaryTextBrush"),
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 360
            });
            headingStack.Children.Add(new TextBlock
            {
                Text = $"{team.Course} • {team.ClassName} • ID {team.ClassId}",
                FontSize = 11,
                Margin = new Thickness(0, 4, 0, 0),
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            });
            identityRow.Children.Add(headingStack);
            textStack.Children.Add(identityRow);
            textStack.Children.Add(new TextBlock
            {
                Text = $"{BuildTeamBalanceLabel(team)} • {team.Ucs.Count} UC(s)",
                FontSize = 11,
                Margin = new Thickness(0, 10, 0, 0),
                Foreground = GetThemeBrush("TertiaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            });
            var metaWrap = new WrapPanel
            {
                Margin = new Thickness(0, 10, 0, 0)
            };
            metaWrap.Children.Add(CreateStaticTeamChip($"Codigo {team.TeamId}", GetThemeBrush("AccentMutedBrush"), GetThemeBrush("AccentBrush")));
            metaWrap.Children.Add(CreateStaticTeamChip($"Progresso {progress}%", GetThemeBrush("MutedCardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            metaWrap.Children.Add(CreateStaticTeamChip($"Proximo prazo {GetNextTeamDeadlineLabel(team)}", GetThemeBrush("MutedCardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            metaWrap.Children.Add(CreateStaticTeamChip(BuildTeamLeadershipLabel(team), GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
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
            team.UpdatedAt = DateTime.Now;
            team.LastRealtimeSyncAt = DateTime.Now;
            _userAcademicPortfolioCache.Clear();

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

            if (_activeTeamWorkspace != null && string.Equals(_activeTeamWorkspace.TeamId, team.TeamId, StringComparison.OrdinalIgnoreCase))
            {
                _activeTeamWorkspace = team;
            }

            RenderProfessorDashboard();
            RenderCalendarAgenda();
        }

        private void TrackTeamWorkspaceLocally(TeamWorkspaceInfo team, bool refreshVisuals = true)
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

            _userAcademicPortfolioCache.Clear();

            if (!refreshVisuals)
            {
                return;
            }

            UpdateTeamsViewState();
            RenderTeamsList();
            RenderProfessorDashboard();
            RenderCalendarAgenda();
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
            var titleIconKind = mode switch
            {
                ConnectionSectionMode.IncomingRequests => PackIconMaterialKind.AccountPlusOutline,
                ConnectionSectionMode.Notifications => PackIconMaterialKind.BellOutline,
                ConnectionSectionMode.OutgoingRequests => PackIconMaterialKind.SendOutline,
                _ => PackIconMaterialKind.AccountGroupOutline
            };
            var titleIconBrush = mode switch
            {
                ConnectionSectionMode.IncomingRequests => new SolidColorBrush(Color.FromRgb(14, 165, 233)),
                ConnectionSectionMode.Notifications => new SolidColorBrush(Color.FromRgb(245, 158, 11)),
                ConnectionSectionMode.OutgoingRequests => new SolidColorBrush(Color.FromRgb(124, 58, 237)),
                _ => new SolidColorBrush(Color.FromRgb(16, 185, 129))
            };

            var titleRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };
            var titleIcon = CreateMaterialIcon(titleIconKind, titleIconBrush, 18);
            titleIcon.Margin = new Thickness(0, 0, 10, 0);
            titleRow.Children.Add(titleIcon);
            titleRow.Children.Add(new TextBlock
            {
                Text = title,
                FontFamily = GetAppDisplayFontFamily(),
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            });
            stack.Children.Add(titleRow);
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
            TeamListCard.Visibility = (_teamWorkspaces.Count > 0 || CurrentViewerCanUseProfessorDiscovery())
                ? Visibility.Visible
                : Visibility.Collapsed;
            TeamWorkspaceCard.Visibility = _activeTeamWorkspace == null
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private void PopulateProfessionalProfileFields(UserProfile profile)
        {
            NormalizeProfileCollections(profile);
            AccountNameText.Text = profile.Name;
            NicknameTextBox.Text = profile.Nickname;
            ProfessionalTitleTextBox.Text = profile.ProfessionalTitle;
            AcademicDepartmentTextBox.Text = profile.AcademicDepartment;
            AcademicFocusTextBox.Text = profile.AcademicFocus;
            OfficeHoursTextBox.Text = profile.OfficeHours;
            BioTextBox.Text = profile.Bio;
            SkillsTextBox.Text = profile.Skills;
            PortfolioLinkTextBox.Text = profile.PortfolioLink;
            LinkedInLinkTextBox.Text = profile.LinkedInLink;
            UpdateRoleAwareShellState(profile);
            SetProgrammingLanguages(profile.ProgrammingLanguages);
            RefreshAvatarUi(profile);
            RenderProfileGalleryEditor(profile);
            RenderProfileProjectSelection(profile);
            ProfessionalProfileStatusText.Text = string.Empty;
        }

        private sealed class ProfileGalleryCardEntry
        {
            public ProfileGalleryCardEntry(string entryId, IReadOnlyList<ProfileGalleryImage> images, bool isAlbum)
            {
                EntryId = entryId;
                Images = images ?? Array.Empty<ProfileGalleryImage>();
                IsAlbum = isAlbum;
                CoverImage = Images
                    .OrderByDescending(item => item.AddedAt == default ? DateTime.MinValue : item.AddedAt)
                    .FirstOrDefault() ?? new ProfileGalleryImage();

                AddedAt = Images.Count == 0
                    ? DateTime.Now
                    : Images.Max(item => item.AddedAt == default ? DateTime.Now : item.AddedAt);

                var titleSource = isAlbum
                    ? Images.Select(item => item.GalleryAlbumTitle).FirstOrDefault(text => !string.IsNullOrWhiteSpace(text))
                    : CoverImage.Title;
                if (string.IsNullOrWhiteSpace(titleSource) && isAlbum)
                {
                    titleSource = Images.Select(item => item.Title).FirstOrDefault(text => !string.IsNullOrWhiteSpace(text));
                }

                var descriptionSource = isAlbum
                    ? Images.Select(item => item.GalleryAlbumDescription).FirstOrDefault(text => !string.IsNullOrWhiteSpace(text))
                    : CoverImage.Description;
                if (string.IsNullOrWhiteSpace(descriptionSource) && isAlbum)
                {
                    descriptionSource = Images.Select(item => item.Description).FirstOrDefault(text => !string.IsNullOrWhiteSpace(text));
                }

                Title = string.IsNullOrWhiteSpace(titleSource)
                    ? (isAlbum ? "Galeria do evento" : "Imagem do currículo")
                    : titleSource.Trim();
                Description = descriptionSource?.Trim() ?? string.Empty;
                CountLabel = Images.Count == 1 ? "1 foto" : $"{Images.Count} fotos";
                DateLabel = AddedAt == default ? "Sem data" : $"{AddedAt:dd/MM/yyyy}";
            }

            public string EntryId { get; }
            public IReadOnlyList<ProfileGalleryImage> Images { get; }
            public ProfileGalleryImage CoverImage { get; }
            public bool IsAlbum { get; }
            public string Title { get; }
            public string Description { get; }
            public string CountLabel { get; }
            public string DateLabel { get; }
            public DateTime AddedAt { get; }
        }

        private void NormalizeProfileCollections(UserProfile profile)
        {
            profile.GalleryImages ??= new List<ProfileGalleryImage>();
            profile.FeaturedProjectIds ??= new List<string>();

            profile.GalleryImages = profile.GalleryImages
                .Where(item => item != null && !string.IsNullOrWhiteSpace(item.ImageDataUri))
                .Select(item => new ProfileGalleryImage
                {
                    ImageId = string.IsNullOrWhiteSpace(item.ImageId) ? Guid.NewGuid().ToString("N") : item.ImageId,
                    Title = item.Title?.Trim() ?? string.Empty,
                    Description = item.Description?.Trim() ?? string.Empty,
                    GalleryAlbumId = item.GalleryAlbumId?.Trim() ?? string.Empty,
                    GalleryAlbumTitle = item.GalleryAlbumTitle?.Trim() ?? string.Empty,
                    GalleryAlbumDescription = item.GalleryAlbumDescription?.Trim() ?? string.Empty,
                    ImageDataUri = item.ImageDataUri,
                    AddedAt = item.AddedAt == default ? DateTime.Now : item.AddedAt
                })
                .OrderByDescending(item => item.AddedAt)
                .Take(MaxProfileGalleryImages)
                .ToList();

            profile.FeaturedProjectIds = profile.FeaturedProjectIds
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Select(item => item.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private void MarkProfessionalProfileDirty(string message)
        {
            ProfessionalProfileStatusText.Text = message;
            ProfessionalProfileStatusText.Foreground = GetThemeBrush("SecondaryTextBrush");
        }

        private List<ProfileGalleryCardEntry> BuildProfileGalleryEntries(UserProfile profile)
        {
            var orderedImages = (profile.GalleryImages ?? new List<ProfileGalleryImage>())
                .OrderByDescending(item => item.AddedAt == default ? DateTime.MinValue : item.AddedAt)
                .ToList();

            var entries = new List<ProfileGalleryCardEntry>();
            var processedAlbums = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var image in orderedImages)
            {
                var albumId = image.GalleryAlbumId?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(albumId))
                {
                    entries.Add(new ProfileGalleryCardEntry(image.ImageId, new[] { image }, false));
                    continue;
                }

                if (!processedAlbums.Add(albumId))
                {
                    continue;
                }

                var albumImages = orderedImages
                    .Where(item => string.Equals(item.GalleryAlbumId?.Trim(), albumId, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(item => item.AddedAt == default ? DateTime.Now : item.AddedAt)
                    .ToList();
                entries.Add(new ProfileGalleryCardEntry(albumId, albumImages, true));
            }

            return entries
                .OrderByDescending(entry => entry.AddedAt)
                .ToList();
        }

        private void RenderProfileGalleryEditor(UserProfile? profile)
        {
            if (ProfileGalleryHost == null || ProfileGalleryStatusText == null || AddProfileGalleryButton == null || AddProfileGalleryAlbumButton == null)
            {
                return;
            }

            ProfileGalleryHost.Children.Clear();

            if (profile == null)
            {
                ProfileGalleryStatusText.Text = "Carregue um perfil para gerenciar a galeria do currículo.";
                AddProfileGalleryButton.IsEnabled = false;
                AddProfileGalleryAlbumButton.IsEnabled = false;
                return;
            }

            NormalizeProfileCollections(profile);
            var entries = BuildProfileGalleryEntries(profile);
            var remainingSlots = Math.Max(0, MaxProfileGalleryImages - profile.GalleryImages.Count);
            AddProfileGalleryButton.IsEnabled = remainingSlots > 0;
            AddProfileGalleryAlbumButton.IsEnabled = remainingSlots > 0;
            ProfileGalleryStatusText.Text = profile.GalleryImages.Count == 0
                ? $"Você pode manter até {MaxProfileGalleryImages} imagens no currículo, em fotos soltas ou blocos por evento."
                : $"{profile.GalleryImages.Count} imagem(ns) distribuída(s) em {entries.Count} bloco(s). Restam {remainingSlots} vaga(s).";

            if (entries.Count == 0)
            {
                ProfileGalleryHost.Children.Add(new Border
                {
                    Width = 264,
                    Padding = new Thickness(14),
                    Background = GetThemeBrush("CardBackgroundBrush"),
                    BorderBrush = GetThemeBrush("CardBorderBrush"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(14),
                    Child = new TextBlock
                    {
                        Text = "Nenhuma imagem adicionada ainda. Use os botões acima para publicar fotos avulsas ou uma galeria em pasta para eventos e entregas.",
                        FontSize = 11,
                        Foreground = GetThemeBrush("SecondaryTextBrush"),
                        TextWrapping = TextWrapping.Wrap,
                        LineHeight = 18
                    }
                });
                return;
            }

            foreach (var entry in entries)
            {
                ProfileGalleryHost.Children.Add(CreateProfileGalleryEditorCard(entry));
            }
        }

        private Border CreateGalleryPreviewTile(ImageSource? source, CornerRadius cornerRadius, string? overlayText = null)
        {
            var grid = new Grid();
            if (source == null)
            {
                grid.Children.Add(new TextBlock
                {
                    Text = "Prévia indisponível",
                    FontSize = 11,
                    Foreground = GetThemeBrush("SecondaryTextBrush"),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(14)
                });
            }
            else
            {
                grid.Children.Add(new Image
                {
                    Source = source,
                    Stretch = Stretch.UniformToFill,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                });
            }

            if (!string.IsNullOrWhiteSpace(overlayText))
            {
                grid.Children.Add(new Border
                {
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(10),
                    Padding = new Thickness(10, 5, 10, 5),
                    Background = new SolidColorBrush(Color.FromArgb(216, 15, 23, 42)),
                    CornerRadius = new CornerRadius(999),
                    Child = new TextBlock
                    {
                        Text = overlayText,
                        FontSize = 10,
                        FontWeight = FontWeights.SemiBold,
                        Foreground = Brushes.White
                    }
                });
            }

            return new Border
            {
                Background = GetThemeBrush("MutedCardBackgroundBrush"),
                CornerRadius = cornerRadius,
                ClipToBounds = true,
                Child = grid
            };
        }

        private UIElement CreateProfileGalleryEditorPreviewSurface(ProfileGalleryCardEntry entry)
        {
            if (!entry.IsAlbum)
            {
                return CreateGalleryPreviewTile(
                    TryCreateImageSourceFromDataUri(entry.CoverImage.ImageDataUri),
                    new CornerRadius(18, 18, 0, 0));
            }

            var root = new Grid
            {
                Background = GetThemeBrush("MutedCardBackgroundBrush")
            };

            var collage = new UniformGrid
            {
                Rows = 2,
                Columns = 2,
                Margin = new Thickness(6)
            };

            var previewImages = entry.Images
                .OrderBy(item => item.AddedAt == default ? DateTime.Now : item.AddedAt)
                .Select(item => TryCreateImageSourceFromDataUri(item.ImageDataUri))
                .Take(4)
                .ToList();

            for (var index = 0; index < 4; index++)
            {
                var overlay = index == 3 && entry.Images.Count > 4 ? $"+{entry.Images.Count - 4}" : null;
                collage.Children.Add(CreateGalleryPreviewTile(
                    index < previewImages.Count ? previewImages[index] : null,
                    new CornerRadius(12),
                    overlay));
            }

            root.Children.Add(collage);
            root.Children.Add(new Border
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(12),
                Padding = new Thickness(10, 5, 10, 5),
                Background = new SolidColorBrush(Color.FromArgb(212, 15, 23, 42)),
                CornerRadius = new CornerRadius(999),
                Child = new TextBlock
                {
                    Text = entry.CountLabel,
                    FontSize = 10,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = Brushes.White
                }
            });

            return new Border
            {
                Height = 142,
                CornerRadius = new CornerRadius(18, 18, 0, 0),
                Background = GetThemeBrush("MutedCardBackgroundBrush"),
                ClipToBounds = true,
                Child = root
            };
        }

        private Border CreateProfileGalleryEditorCard(ProfileGalleryCardEntry entry)
        {
            var card = new Border
            {
                Width = entry.IsAlbum ? 248 : 156,
                Margin = new Thickness(0, 0, 12, 12),
                Background = GetThemeBrush("CardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(18)
            };

            var root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var previewButton = new Button
            {
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(0),
                Cursor = Cursors.Hand,
                Tag = entry,
                Content = CreateProfileGalleryEditorPreviewSurface(entry)
            };
            previewButton.Click += PreviewProfileGalleryImage_Click;
            root.Children.Add(previewButton);

            var footer = new Grid
            {
                Margin = new Thickness(12, 10, 12, 12)
            };
            footer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            footer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var textStack = new StackPanel();
            textStack.Children.Add(new TextBlock
            {
                Text = entry.Title,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = GetThemeBrush("PrimaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            });
            if (!string.IsNullOrWhiteSpace(entry.Description))
            {
                textStack.Children.Add(new TextBlock
                {
                    Text = entry.Description,
                    Margin = new Thickness(0, 4, 0, 0),
                    FontSize = 10,
                    Foreground = GetThemeBrush("SecondaryTextBrush"),
                    TextWrapping = TextWrapping.Wrap,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    MaxHeight = 30
                });
            }
            textStack.Children.Add(new TextBlock
            {
                Text = entry.IsAlbum ? $"{entry.CountLabel} • {entry.DateLabel}" : entry.DateLabel,
                Margin = new Thickness(0, 4, 0, 0),
                FontSize = 10,
                Foreground = GetThemeBrush("TertiaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            });
            footer.Children.Add(textStack);

            var removeButton = new Button
            {
                Content = entry.IsAlbum ? "Remover bloco" : "Remover",
                Height = 28,
                Margin = new Thickness(10, 0, 0, 0),
                Padding = new Thickness(10, 4, 10, 4),
                Background = GetThemeBrush("MutedCardBackgroundBrush"),
                Foreground = GetThemeBrush("PrimaryTextBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                Cursor = Cursors.Hand,
                Tag = entry,
                FontSize = 10,
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Top
            };
            removeButton.Click += RemoveProfileGalleryImage_Click;
            Grid.SetColumn(removeButton, 1);
            footer.Children.Add(removeButton);

            Grid.SetRow(footer, 1);
            root.Children.Add(footer);
            card.Child = root;
            return card;
        }

        private List<GalleryViewerItem> BuildProfileGalleryViewerItems(ProfileGalleryCardEntry entry)
        {
            IEnumerable<ProfileGalleryImage> sourceImages;

            if (entry.IsAlbum)
            {
                sourceImages = entry.Images
                    .OrderBy(item => item.AddedAt == default ? DateTime.Now : item.AddedAt);
            }
            else
            {
                sourceImages = (_currentProfile?.GalleryImages ?? entry.Images.ToList())
                    .Where(item => string.IsNullOrWhiteSpace(item.GalleryAlbumId))
                    .OrderByDescending(item => item.AddedAt == default ? DateTime.MinValue : item.AddedAt);
            }

            var orderedImages = sourceImages.ToList();
            return orderedImages
                .Select((image, index) => CreateProfileGalleryViewerItem(image, entry.IsAlbum ? entry : null, index, orderedImages.Count))
                .Where(item => item != null)
                .Cast<GalleryViewerItem>()
                .ToList();
        }

        private void PreviewProfileGalleryImage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: ProfileGalleryCardEntry entry })
            {
                return;
            }

            var viewerItems = BuildProfileGalleryViewerItems(entry);
            if (viewerItems.Count == 0)
            {
                return;
            }

            var initialIndex = entry.IsAlbum
                ? 0
                : viewerItems.FindIndex(item => string.Equals(item.ItemId, entry.CoverImage.ImageId, StringComparison.OrdinalIgnoreCase));
            if (initialIndex < 0)
            {
                initialIndex = 0;
            }

            var viewer = new GalleryImageViewerWindow(
                viewerItems,
                initialIndex,
                this,
                GetThemeBrush("AccentBrush").Color,
                allowAdjustment: true,
                contextLabel: entry.IsAlbum ? "Sua galeria em pasta" : "Sua galeria profissional");
            viewer.ShowDialog();
        }

        private GalleryViewerItem? CreateProfileGalleryViewerItem(ProfileGalleryImage image, ProfileGalleryCardEntry? ownerEntry = null, int index = 0, int totalCount = 1)
        {
            var source = TryCreateImageSourceFromDataUri(image.ImageDataUri);
            if (source == null)
            {
                return null;
            }

            var isAlbumItem = ownerEntry?.IsAlbum == true;
            var title = isAlbumItem
                ? ownerEntry!.Title
                : string.IsNullOrWhiteSpace(image.Title) ? "Imagem do currículo" : image.Title;

            string subtitle;
            string description;
            if (isAlbumItem)
            {
                var imageLabel = string.IsNullOrWhiteSpace(image.Title)
                    ? $"Foto {index + 1} de {Math.Max(1, totalCount)}"
                    : $"{image.Title} • {index + 1} de {Math.Max(1, totalCount)}";
                subtitle = $"{ownerEntry!.CountLabel} • {imageLabel}";
                description = !string.IsNullOrWhiteSpace(image.Description) ? image.Description : ownerEntry.Description;
            }
            else
            {
                subtitle = image.AddedAt == default ? "Galeria do currículo" : $"Galeria profissional • adicionada em {image.AddedAt:dd/MM/yyyy}";
                description = image.Description;
            }

            return new GalleryViewerItem(image.ImageId, source, title, subtitle, description);
        }

        private UIElement CreateProfileGalleryMetadataPreview(IReadOnlyList<ImageSource> previewSources, bool isAlbum)
        {
            var safePreviewSources = previewSources ?? Array.Empty<ImageSource>();
            var previews = safePreviewSources
                .Where(source => source != null)
                .Cast<ImageSource>()
                .Take(4)
                .ToList();

            if (previews.Count == 0)
            {
                return CreateGalleryPreviewTile(null, new CornerRadius(24));
            }

            if (!isAlbum || previews.Count == 1)
            {
                return CreateGalleryPreviewTile(previews[0], new CornerRadius(24));
            }

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.35, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var hero = CreateGalleryPreviewTile(previews[0], new CornerRadius(24, 0, 0, 24));
            Grid.SetRowSpan(hero, 2);
            grid.Children.Add(hero);

            var topRight = CreateGalleryPreviewTile(previews.ElementAtOrDefault(1), new CornerRadius(0, 24, 0, 0));
            Grid.SetColumn(topRight, 1);
            grid.Children.Add(topRight);

            var extraCount = safePreviewSources.Count > 3 ? $"+{safePreviewSources.Count - 3}" : null;
            var bottomRight = CreateGalleryPreviewTile(previews.ElementAtOrDefault(2), new CornerRadius(0, 0, 24, 0), extraCount);
            Grid.SetColumn(bottomRight, 1);
            Grid.SetRow(bottomRight, 1);
            grid.Children.Add(bottomRight);

            return new Border
            {
                CornerRadius = new CornerRadius(24),
                ClipToBounds = true,
                Child = grid
            };
        }

        private (bool Confirmed, string Title, string Description) ShowProfileGalleryMetadataDialog(IReadOnlyList<ImageSource> previewSources, bool isAlbum)
        {
            var accentBrush = GetThemeBrush("AccentBrush");
            var dialog = CreateStyledDialogWindow(
                isAlbum ? "Detalhes da galeria em pasta" : "Detalhes da imagem",
                980,
                760,
                700,
                true);

            var root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            root.Children.Add(CreateDialogHeader(
                "GALERIA",
                isAlbum ? "Organize um bloco de fotos" : "Nomeie a imagem do currículo",
                isAlbum
                    ? "Use título e descrição para identificar o evento ou a entrega. Esse conjunto será exibido como um único bloco no perfil, com slideshow ao abrir."
                    : "Título e descrição são opcionais. Se você deixar vazio, a galeria usa um rótulo neutro sem expor o nome original do arquivo.",
                accentBrush));

            var content = new Grid { Margin = new Thickness(0, 6, 0, 0) };
            content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(360) });
            content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            Grid.SetRow(content, 1);

            var previewStack = new StackPanel();
            previewStack.Children.Add(new Border
            {
                Height = 286,
                Background = GetThemeBrush("MutedCardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(24),
                Child = CreateProfileGalleryMetadataPreview(previewSources, isAlbum)
            });
            previewStack.Children.Add(new TextBlock
            {
                Text = isAlbum
                    ? $"{previewSources.Count} arquivo(s) pronto(s) para este bloco da galeria."
                    : "A prévia mostra exatamente o recorte que vai para o currículo.",
                Margin = new Thickness(4, 12, 4, 0),
                FontSize = 11,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 18
            });
            content.Children.Add(previewStack);

            var formScroll = new ScrollViewer
            {
                Margin = new Thickness(22, 0, 0, 0),
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Content = new StackPanel()
            };
            Grid.SetColumn(formScroll, 1);

            var fields = (StackPanel)formScroll.Content;
            fields.Children.Add(CreateDialogFieldLabel(isAlbum ? "Título da pasta ou evento" : "Título opcional"));
            var titleBox = new TextBox
            {
                Height = 46,
                Margin = new Thickness(0, 6, 0, 16)
            };
            ApplyDialogInputStyle(titleBox);
            fields.Children.Add(titleBox);

            fields.Children.Add(CreateDialogFieldLabel(isAlbum ? "Descrição do bloco" : "Descrição opcional"));
            var descriptionBox = new TextBox
            {
                MinHeight = 190,
                Margin = new Thickness(0, 6, 0, 0),
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            ApplyDialogInputStyle(descriptionBox);
            fields.Children.Add(descriptionBox);

            fields.Children.Add(new Border
            {
                Margin = new Thickness(0, 18, 0, 0),
                Padding = new Thickness(14),
                Background = GetThemeBrush("MutedCardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(18),
                Child = new TextBlock
                {
                    Text = isAlbum
                        ? "Sugestão profissional: use o nome do evento, disciplina ou entrega no título e deixe a descrição para explicar contexto, stack, papel no projeto ou resultado alcançado."
                        : "Sugestão profissional: use um título curto para destacar a entrega e a descrição para explicar contexto, tecnologia ou etapa do projeto.",
                    FontSize = 11,
                    Foreground = GetThemeBrush("SecondaryTextBrush"),
                    TextWrapping = TextWrapping.Wrap,
                    LineHeight = 18
                }
            });

            content.Children.Add(formScroll);
            root.Children.Add(content);

            var actions = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 20, 0, 0)
            };
            Grid.SetRow(actions, 2);

            var cancelButton = CreateDialogActionButton("Cancelar", Brushes.Transparent, GetThemeBrush("PrimaryTextBrush"), GetThemeBrush("CardBorderBrush"), 112);
            var saveButton = CreateDialogActionButton(
                isAlbum ? "Criar bloco da galeria" : "Adicionar à galeria",
                accentBrush,
                Brushes.White,
                Brushes.Transparent,
                isAlbum ? 180 : 156);
            actions.Children.Add(cancelButton);
            actions.Children.Add(saveButton);
            root.Children.Add(actions);

            cancelButton.Click += (_, __) => dialog.Close();
            saveButton.Click += (_, __) =>
            {
                dialog.DialogResult = true;
                dialog.Close();
            };

            dialog.Content = CreateStyledDialogShell(root);
            var confirmed = dialog.ShowDialog() == true;
            return (confirmed, titleBox.Text?.Trim() ?? string.Empty, descriptionBox.Text?.Trim() ?? string.Empty);
        }

        private void ShowProfileGallerySkippedFilesAlert(IReadOnlyCollection<string> skippedFiles)
        {
            if (skippedFiles == null || skippedFiles.Count == 0)
            {
                return;
            }

            ShowStyledAlertDialog(
                "CURRÍCULO",
                "Algumas imagens foram ignoradas",
                $"Estas imagens não foram adicionadas porque o recorte foi cancelado ou a preparação falhou: {string.Join(", ", skippedFiles.Take(4))}{(skippedFiles.Count > 4 ? "..." : string.Empty)}",
                "Fechar",
                new SolidColorBrush(Color.FromRgb(234, 88, 12)));
        }

        private void RemoveProfileGalleryImage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentProfile == null || sender is not Button { Tag: ProfileGalleryCardEntry entry })
            {
                return;
            }

            if (entry.IsAlbum)
            {
                _currentProfile.GalleryImages.RemoveAll(item => string.Equals(item.GalleryAlbumId, entry.EntryId, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                _currentProfile.GalleryImages.RemoveAll(item => string.Equals(item.ImageId, entry.EntryId, StringComparison.OrdinalIgnoreCase));
            }

            RenderProfileGalleryEditor(_currentProfile);
            MarkProfessionalProfileDirty(entry.IsAlbum
                ? "Bloco da galeria removido. Clique em salvar para persistir as mudanças do currículo."
                : "Galeria atualizada. Clique em salvar para persistir as mudanças do currículo.");
        }

        private void AddProfileGalleryImages_Click(object sender, RoutedEventArgs e)
        {
            if (_currentProfile == null)
            {
                return;
            }

            NormalizeProfileCollections(_currentProfile);
            var remainingSlots = MaxProfileGalleryImages - _currentProfile.GalleryImages.Count;
            if (remainingSlots <= 0)
            {
                ShowStyledAlertDialog(
                    "CURRÍCULO",
                    "Galeria cheia",
                    $"A galeria do currículo aceita até {MaxProfileGalleryImages} imagens por perfil. Remova uma imagem para adicionar outra.",
                    "Fechar",
                    GetThemeBrush("AccentBrush"));
                return;
            }

            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                Title = "Selecionar imagens da galeria do currículo",
                Filter = "Imagens|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.webp"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            var addedCount = 0;
            var skippedFiles = new List<string>();
            foreach (var filePath in dialog.FileNames.Take(remainingSlots))
            {
                var dataUri = ShowProfileGalleryCropperDialog(filePath);
                if (string.IsNullOrWhiteSpace(dataUri))
                {
                    skippedFiles.Add(IOPath.GetFileName(filePath));
                    continue;
                }

                var previewSource = TryCreateImageSourceFromDataUri(dataUri);
                var metadata = ShowProfileGalleryMetadataDialog(
                    previewSource == null ? Array.Empty<ImageSource>() : new[] { previewSource },
                    false);
                if (!metadata.Confirmed)
                {
                    skippedFiles.Add(IOPath.GetFileName(filePath));
                    continue;
                }

                _currentProfile.GalleryImages.Add(new ProfileGalleryImage
                {
                    ImageId = Guid.NewGuid().ToString("N"),
                    Title = metadata.Title,
                    Description = metadata.Description,
                    GalleryAlbumId = string.Empty,
                    GalleryAlbumTitle = string.Empty,
                    GalleryAlbumDescription = string.Empty,
                    ImageDataUri = dataUri,
                    AddedAt = DateTime.Now
                });
                addedCount++;
            }

            NormalizeProfileCollections(_currentProfile);
            RenderProfileGalleryEditor(_currentProfile);

            var skippedByLimit = Math.Max(0, dialog.FileNames.Length - remainingSlots);
            if (addedCount > 0)
            {
                MarkProfessionalProfileDirty(skippedByLimit > 0
                    ? $"{addedCount} imagem(ns) adicionada(s). {skippedByLimit} ficou(aram) de fora por limite da galeria. Salve o perfil para publicar."
                    : $"{addedCount} imagem(ns) adicionada(s) ao currículo. Salve o perfil para publicar.");
            }

            ShowProfileGallerySkippedFilesAlert(skippedFiles);
        }

        private void AddProfileGalleryAlbum_Click(object sender, RoutedEventArgs e)
        {
            if (_currentProfile == null)
            {
                return;
            }

            NormalizeProfileCollections(_currentProfile);
            var remainingSlots = MaxProfileGalleryImages - _currentProfile.GalleryImages.Count;
            if (remainingSlots <= 0)
            {
                ShowStyledAlertDialog(
                    "CURRÍCULO",
                    "Galeria cheia",
                    $"A galeria do currículo aceita até {MaxProfileGalleryImages} imagens por perfil. Remova uma imagem para adicionar outra.",
                    "Fechar",
                    GetThemeBrush("AccentBrush"));
                return;
            }

            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                Title = "Selecionar imagens para a galeria em pasta",
                Filter = "Imagens|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.webp"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            var croppedItems = new List<(string FileName, string DataUri, ImageSource? PreviewSource)>();
            var skippedFiles = new List<string>();

            foreach (var filePath in dialog.FileNames.Take(remainingSlots))
            {
                var dataUri = ShowProfileGalleryCropperDialog(filePath);
                if (string.IsNullOrWhiteSpace(dataUri))
                {
                    skippedFiles.Add(IOPath.GetFileName(filePath));
                    continue;
                }

                croppedItems.Add((
                    IOPath.GetFileName(filePath),
                    dataUri,
                    TryCreateImageSourceFromDataUri(dataUri)));
            }

            if (croppedItems.Count == 0)
            {
                ShowProfileGallerySkippedFilesAlert(skippedFiles);
                return;
            }

            var albumPreviews = croppedItems
                .Where(item => item.PreviewSource != null)
                .Select(item => item.PreviewSource!)
                .ToList();
            var metadata = ShowProfileGalleryMetadataDialog(albumPreviews, true);
            if (!metadata.Confirmed)
            {
                return;
            }

            var albumId = Guid.NewGuid().ToString("N");
            var batchStart = DateTime.Now;
            for (var index = 0; index < croppedItems.Count; index++)
            {
                _currentProfile.GalleryImages.Add(new ProfileGalleryImage
                {
                    ImageId = Guid.NewGuid().ToString("N"),
                    Title = string.Empty,
                    Description = string.Empty,
                    GalleryAlbumId = albumId,
                    GalleryAlbumTitle = metadata.Title,
                    GalleryAlbumDescription = metadata.Description,
                    ImageDataUri = croppedItems[index].DataUri,
                    AddedAt = batchStart.AddMilliseconds(index)
                });
            }

            NormalizeProfileCollections(_currentProfile);
            RenderProfileGalleryEditor(_currentProfile);

            var skippedByLimit = Math.Max(0, dialog.FileNames.Length - remainingSlots);
            MarkProfessionalProfileDirty(skippedByLimit > 0
                ? $"Bloco com {croppedItems.Count} foto(s) criado. {skippedByLimit} arquivo(s) ficaram de fora por limite da galeria. Salve o perfil para publicar."
                : $"Bloco com {croppedItems.Count} foto(s) criado na galeria do currículo. Salve o perfil para publicar.");

            ShowProfileGallerySkippedFilesAlert(skippedFiles);
        }

        private void RenderProfileProjectSelection(UserProfile? profile)
        {
            if (ProfileProjectHighlightsHost == null || ProfileProjectHighlightsStatusText == null)
            {
                return;
            }

            ProfileProjectHighlightsHost.Children.Clear();

            if (profile == null)
            {
                ProfileProjectHighlightsStatusText.Text = "Carregue um perfil para vincular projetos ao currículo.";
                return;
            }

            NormalizeProfileCollections(profile);

            if (_teamWorkspaces.Count == 0)
            {
                ProfileProjectHighlightsStatusText.Text = "Você ainda não participa de equipes carregadas no app para vincular ao currículo.";
                ProfileProjectHighlightsHost.Children.Add(new Border
                {
                    Padding = new Thickness(14),
                    Background = GetThemeBrush("CardBackgroundBrush"),
                    BorderBrush = GetThemeBrush("CardBorderBrush"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(14),
                    Child = new TextBlock
                    {
                        Text = "Assim que suas equipes forem carregadas, você poderá marcar quais projetos entram no perfil profissional.",
                        FontSize = 11,
                        Foreground = GetThemeBrush("SecondaryTextBrush"),
                        TextWrapping = TextWrapping.Wrap,
                        LineHeight = 18
                    }
                });
                return;
            }

            var selectedCount = _teamWorkspaces.Count(team => profile.FeaturedProjectIds.Contains(team.TeamId, StringComparer.OrdinalIgnoreCase));
            ProfileProjectHighlightsStatusText.Text = selectedCount == 0
                ? "Nenhum projeto foi marcado ainda para aparecer no currículo."
                : $"{selectedCount} projeto(s) marcado(s) para aparecer no perfil.";

            foreach (var team in _teamWorkspaces.OrderBy(item => item.TeamName))
            {
                ProfileProjectHighlightsHost.Children.Add(CreateProfileProjectSelectionCard(profile, team));
            }
        }

        private Border CreateProfileProjectSelectionCard(UserProfile profile, TeamWorkspaceInfo team)
        {
            var isSelected = profile.FeaturedProjectIds.Contains(team.TeamId, StringComparer.OrdinalIgnoreCase);
            var projectImagesCount = team.Assets.Count(asset => !string.IsNullOrWhiteSpace(asset.PreviewImageDataUri));

            var root = new Grid();
            root.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            root.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var selector = new CheckBox
            {
                IsChecked = isSelected,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 2, 14, 0),
                Tag = team.TeamId
            };
            selector.Checked += ProfileProjectHighlightToggle_Changed;
            selector.Unchecked += ProfileProjectHighlightToggle_Changed;
            root.Children.Add(selector);

            var logoBadge = CreateTeamLogoBadge(team, 52, circular: false, fontSize: 16, borderThickness: 0);
            logoBadge.Margin = new Thickness(0, 0, 14, 0);
            logoBadge.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetColumn(logoBadge, 1);
            root.Children.Add(logoBadge);

            var content = new StackPanel();
            content.Children.Add(new TextBlock
            {
                Text = team.TeamName,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = GetThemeBrush("PrimaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            });
            content.Children.Add(new TextBlock
            {
                Text = $"{team.Course} • {team.ClassName} • {BuildTeamBalanceLabel(team)}",
                Margin = new Thickness(0, 5, 0, 0),
                FontSize = 11,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            });

            var metaWrap = new WrapPanel { Margin = new Thickness(0, 10, 0, 0) };
            metaWrap.Children.Add(CreateStaticTeamChip($"Status {team.ProjectStatus}", CreateSoftAccentBrush(GetThemeBrush("AccentBrush"), 24), GetThemeBrush("AccentBrush")));
            metaWrap.Children.Add(CreateStaticTeamChip($"Progresso {CalculateTeamProgressPercentage(team)}%", GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            metaWrap.Children.Add(CreateStaticTeamChip(projectImagesCount == 0 ? "Sem imagens do projeto" : $"{projectImagesCount} imagem(ns) do projeto", GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            content.Children.Add(metaWrap);

            Grid.SetColumn(content, 2);
            root.Children.Add(content);

            return new Border
            {
                Margin = new Thickness(0, 0, 0, 10),
                Padding = new Thickness(14),
                Background = GetThemeBrush("CardBackgroundBrush"),
                BorderBrush = isSelected ? GetThemeBrush("AccentBrush") : GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(isSelected ? 2 : 1),
                CornerRadius = new CornerRadius(14),
                Child = root
            };
        }

        private void ProfileProjectHighlightToggle_Changed(object sender, RoutedEventArgs e)
        {
            if (_currentProfile == null || sender is not CheckBox { Tag: string teamId })
            {
                return;
            }

            _currentProfile.FeaturedProjectIds.RemoveAll(item => string.Equals(item, teamId, StringComparison.OrdinalIgnoreCase));
            if (((CheckBox)sender).IsChecked == true)
            {
                _currentProfile.FeaturedProjectIds.Add(teamId);
            }

            NormalizeProfileCollections(_currentProfile);
            RenderProfileProjectSelection(_currentProfile);
            MarkProfessionalProfileDirty("Projetos do currículo atualizados. Clique em salvar para publicar as mudanças.");
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
                var endpoint = AppConfig.BuildFirestoreDocumentUrl($"users/{userId}");
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
                    Role = TeamPermissionService.NormalizeRole(GetFirestoreStringValue(fields, "role")),
                    AcademicDepartment = GetFirestoreStringValue(fields, "academicDepartment"),
                    AcademicFocus = GetFirestoreStringValue(fields, "academicFocus"),
                    OfficeHours = GetFirestoreStringValue(fields, "officeHours"),
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
                var endpoint = AppConfig.BuildFirestoreDocumentUrl($"users/{userId}");
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
                    Role = TeamPermissionService.NormalizeRole(GetFirestoreStringValue(fields, "role")),
                    AcademicDepartment = GetFirestoreStringValue(fields, "academicDepartment"),
                    AcademicFocus = GetFirestoreStringValue(fields, "academicFocus"),
                    OfficeHours = GetFirestoreStringValue(fields, "officeHours"),
                    ProfessorAccessLevel = GetFirestoreStringValue(fields, "professorAccessLevel"),
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
                    AvatarClothing = GetFirestoreStringValue(fields, "avatarClothing"),
                    GalleryImages = GetFirestoreProfileGalleryImages(fields, "galleryImages"),
                    FeaturedProjectIds = GetFirestoreStringListValue(fields, "featuredProjectIds")
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

        private bool CurrentViewerCanUseProfessorDiscovery()
        {
            return TeamPermissionService.CanUseProfessorDashboard(_currentProfile);
        }

        private bool CurrentViewerCanClaimFocalProfessor()
        {
            return TeamPermissionService.CanClaimFocalProfessorRole(_currentProfile);
        }

        private List<TeamWorkspaceInfo> GetLocalTeamsForUser(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return new List<TeamWorkspaceInfo>();
            }

            return _teamWorkspaces
                .Where(team => team.Members.Any(member => string.Equals(member.UserId, userId, StringComparison.OrdinalIgnoreCase)))
                .Select(EnsureTeamWorkspaceDefaults)
                .GroupBy(team => team.TeamId, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .OrderByDescending(team => team.UpdatedAt)
                .ThenBy(team => team.TeamName)
                .ToList();
        }

        private Task<List<TeamWorkspaceInfo>> LoadAcademicPortfolioCachedAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Task.FromResult(new List<TeamWorkspaceInfo>());
            }

            return _userAcademicPortfolioCache.GetOrAdd(userId, LoadAcademicPortfolioByUserIdAsync);
        }

        private async Task<List<TeamWorkspaceInfo>> LoadAcademicPortfolioByUserIdAsync(string userId)
        {
            var localTeams = GetLocalTeamsForUser(userId);
            if (_teamService == null)
            {
                return localTeams;
            }

            if (string.Equals(userId, GetCurrentUserId(), StringComparison.OrdinalIgnoreCase))
            {
                return localTeams;
            }

            if (!CurrentViewerCanUseProfessorDiscovery())
            {
                return localTeams;
            }

            try
            {
                var remoteTeams = await _teamService.LoadTeamsForUserAsync(userId);
                await EnrichTeamMembersAvatarsAsync(remoteTeams);

                return remoteTeams
                    .Concat(localTeams)
                    .GroupBy(team => team.TeamId, StringComparer.OrdinalIgnoreCase)
                    .Select(group => EnsureTeamWorkspaceDefaults(group.OrderByDescending(team => team.UpdatedAt).First()))
                    .OrderByDescending(team => team.UpdatedAt)
                    .ThenBy(team => team.TeamName)
                    .ToList();
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[AcademicPortfolio] Falha ao carregar equipes de '{userId}': {ex.Message}");
                return localTeams;
            }
        }

        private async Task EnrichUsersWithAcademicPortfolioAsync(IEnumerable<UserInfo> users)
        {
            var targets = (users ?? Enumerable.Empty<UserInfo>())
                .Where(user => user != null && !string.IsNullOrWhiteSpace(user.UserId))
                .ToList();

            if (targets.Count == 0 || !CurrentViewerCanUseProfessorDiscovery())
            {
                return;
            }

            var portfolioResults = await Task.WhenAll(targets.Select(async user => new
            {
                User = user,
                Teams = await LoadAcademicPortfolioCachedAsync(user.UserId)
            }));

            foreach (var result in portfolioResults)
            {
                result.User.AcademicProjects = result.Teams;
            }
        }

        private async Task<List<TeamWorkspaceInfo>> LoadSearchSlideTeamMatchesAsync(string query, int maxResults = 10)
        {
            var localTeams = GetLocalTeamSearchMatches(query, maxResults);
            if (_teamService == null || !CurrentViewerCanUseProfessorDiscovery())
            {
                return localTeams;
            }

            try
            {
                var remoteTeams = await _teamService.SearchTeamsAsync(query, maxResults: maxResults);
                return remoteTeams
                    .Concat(localTeams)
                    .GroupBy(team => team.TeamId, StringComparer.OrdinalIgnoreCase)
                    .Select(group => EnsureTeamWorkspaceDefaults(group.OrderByDescending(team => team.UpdatedAt).First()))
                    .OrderByDescending(team => team.UpdatedAt)
                    .ThenBy(team => team.TeamName)
                    .Take(Math.Max(1, maxResults))
                    .ToList();
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[SearchSlideTeams] Falha ao buscar equipes remotamente: {ex.Message}");
                return localTeams;
            }
        }

        private List<TeamWorkspaceInfo> GetLocalTeamSearchMatches(string query, int maxResults = 8)
        {
            var normalizedQuery = NormalizeTeamValue(query);
            if (string.IsNullOrWhiteSpace(normalizedQuery))
            {
                return new List<TeamWorkspaceInfo>();
            }

            return _teamWorkspaces
                .Select(EnsureTeamWorkspaceDefaults)
                .Where(team =>
                    team.TeamName.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)
                    || team.Course.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)
                    || team.ClassName.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)
                    || team.ClassId.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)
                    || team.TeamId.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)
                    || team.Ucs.Any(uc => uc.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrWhiteSpace(team.TemplateName) && team.TemplateName.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrWhiteSpace(team.FocalProfessorName) && team.FocalProfessorName.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase))
                    || (team.ProfessorSupervisorNames?.Any(name => name.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)) ?? false))
                .OrderByDescending(team => team.UpdatedAt)
                .ThenBy(team => team.TeamName)
                .Take(Math.Max(1, maxResults))
                .ToList();
        }

        private List<TeamWorkspaceInfo> ResolveVisibleTeamListResults()
        {
            var activeQuery = NormalizeTeamValue(TeamListSearchBox?.Text ?? string.Empty);
            if (string.IsNullOrWhiteSpace(activeQuery))
            {
                return _teamWorkspaces
                    .Select(EnsureTeamWorkspaceDefaults)
                    .OrderBy(item => item.TeamName)
                    .ToList();
            }

            return (_teamListSearchResults ?? new List<TeamWorkspaceInfo>())
                .Select(EnsureTeamWorkspaceDefaults)
                .GroupBy(team => team.TeamId, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .OrderBy(team => team.TeamName)
                .ToList();
        }

        private Border CreateTeamsListDiscoveryHintCard(string message)
        {
            return CreateSearchSlideInfoCard("Pesquisa de equipes", message);
        }

        private List<TeamWorkspaceInfo> ResolveFeaturedProjectsFromPortfolio(UserProfile profile, IEnumerable<TeamWorkspaceInfo>? portfolioTeams)
        {
            var featuredIds = (profile?.FeaturedProjectIds ?? new List<string>())
                .Where(projectId => !string.IsNullOrWhiteSpace(projectId))
                .Select(projectId => projectId.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (featuredIds.Count == 0)
            {
                return new List<TeamWorkspaceInfo>();
            }

            return (portfolioTeams ?? Enumerable.Empty<TeamWorkspaceInfo>())
                .Where(team => featuredIds.Contains(team.TeamId, StringComparer.OrdinalIgnoreCase))
                .GroupBy(team => team.TeamId, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .OrderBy(team => team.TeamName)
                .ToList();
        }

        private void RefreshSearchSlideTeams(TeamWorkspaceInfo updatedTeam)
        {
            foreach (var user in _searchSlideResults)
            {
                if (user.AcademicProjects == null || user.AcademicProjects.Count == 0)
                {
                    continue;
                }

                for (var index = 0; index < user.AcademicProjects.Count; index++)
                {
                    if (string.Equals(user.AcademicProjects[index].TeamId, updatedTeam.TeamId, StringComparison.OrdinalIgnoreCase))
                    {
                        user.AcademicProjects[index] = updatedTeam;
                    }
                }
            }
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
            profile.Role = string.IsNullOrWhiteSpace(profile.Role) ? TeamPermissionService.NormalizeRole(summaryUser.Role) : TeamPermissionService.NormalizeRole(profile.Role);
            profile.AcademicDepartment = string.IsNullOrWhiteSpace(profile.AcademicDepartment) ? summaryUser.AcademicDepartment : profile.AcademicDepartment;
            profile.AcademicFocus = string.IsNullOrWhiteSpace(profile.AcademicFocus) ? summaryUser.AcademicFocus : profile.AcademicFocus;
            profile.OfficeHours = string.IsNullOrWhiteSpace(profile.OfficeHours) ? summaryUser.OfficeHours : profile.OfficeHours;
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
            NormalizeProfileCollections(profile);

            var accessiblePortfolioTeams = (summaryUser.AcademicProjects ?? new List<TeamWorkspaceInfo>())
                .Where(team => team != null)
                .GroupBy(team => team.TeamId, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();

            if (accessiblePortfolioTeams.Count == 0)
            {
                accessiblePortfolioTeams = await LoadAcademicPortfolioCachedAsync(profile.UserId);
            }

            var accessibleFeaturedProjects = ResolveFeaturedProjectsFromPortfolio(profile, accessiblePortfolioTeams);
            if (accessibleFeaturedProjects.Count == 0 && (profile.FeaturedProjectIds?.Count ?? 0) > 0)
            {
                accessibleFeaturedProjects = await LoadAccessibleFeaturedProjectsAsync(profile);
            }

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

            var dialog = new UserProfileViewWindow(
                profile,
                CreateUserAvatarVisual(avatarUser, 108, true),
                accessibleFeaturedProjects,
                accessiblePortfolioTeams,
                _currentProfile,
                team =>
                {
                    TrackTeamWorkspaceLocally(team);
                    OpenTeamWorkspace(team, navigateToTeams: true);
                },
                CurrentViewerCanClaimFocalProfessor() ? AssignCurrentProfessorAsFocalAsync : null)
            {
                Owner = this
            };

            dialog.ShowDialog();
        }

        private async Task<List<TeamWorkspaceInfo>> LoadAccessibleFeaturedProjectsAsync(UserProfile profile)
        {
            var projects = new List<TeamWorkspaceInfo>();
            if (profile == null || _teamService == null)
            {
                return projects;
            }

            var featuredProjectIds = (profile.FeaturedProjectIds ?? new List<string>())
                .Where(projectId => !string.IsNullOrWhiteSpace(projectId))
                .Select(projectId => projectId.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (featuredProjectIds.Count == 0)
            {
                return projects;
            }

            var loadTasks = featuredProjectIds
                .Select(async projectId =>
                {
                    try
                    {
                        return await _teamService.GetTeamByIdAsync(projectId);
                    }
                    catch (Exception ex)
                    {
                        DebugHelper.WriteLine($"[ProfileProjects] Falha ao carregar projeto destacado '{projectId}': {ex.Message}");
                        return null;
                    }
                })
                .ToArray();

            var loadedProjects = await Task.WhenAll(loadTasks);
            foreach (var project in loadedProjects)
            {
                if (project != null)
                {
                    projects.Add(project);
                }
            }

            return projects;
        }

        private async Task<TeamWorkspaceInfo?> AssignCurrentProfessorAsFocalAsync(TeamWorkspaceInfo team)
        {
            if (team == null)
            {
                return null;
            }

            if (!CurrentViewerCanClaimFocalProfessor())
            {
                ShowStyledAlertDialog("DOCENTE", "Ação indisponível", "Somente perfis com papel de professor podem assumir a supervisão focal da equipe.", "Fechar", GetThemeBrush("AccentBrush"));
                return null;
            }

            if (_teamService == null)
            {
                ShowStyledAlertDialog("DOCENTE", "Serviço indisponível", "O serviço de equipes ainda não foi inicializado para registrar a supervisão focal.", "Fechar", GetThemeBrush("AccentBrush"));
                return null;
            }

            var professor = CreateCurrentUserInfo();
            if (professor == null || string.IsNullOrWhiteSpace(professor.UserId))
            {
                return null;
            }

            professor.Role = "professor";
            EnsureTeamWorkspaceDefaults(team);

            var changed = false;
            var existingMember = team.Members.FirstOrDefault(member => string.Equals(member.UserId, professor.UserId, StringComparison.OrdinalIgnoreCase));
            if (existingMember == null)
            {
                team.Members.Add(CloneUserInfo(professor, "professor"));
                changed = true;
            }
            else
            {
                existingMember.Name = professor.Name;
                existingMember.Email = professor.Email;
                existingMember.Phone = professor.Phone;
                existingMember.Registration = professor.Registration;
                existingMember.Course = professor.Course;
                existingMember.Nickname = professor.Nickname;
                existingMember.ProfessionalTitle = professor.ProfessionalTitle;
                existingMember.AcademicDepartment = professor.AcademicDepartment;
                existingMember.AcademicFocus = professor.AcademicFocus;
                existingMember.OfficeHours = professor.OfficeHours;
                existingMember.AvatarBody = professor.AvatarBody;
                existingMember.AvatarHair = professor.AvatarHair;
                existingMember.AvatarHat = professor.AvatarHat;
                existingMember.AvatarAccessory = professor.AvatarAccessory;
                existingMember.AvatarClothing = professor.AvatarClothing;
                if (!string.Equals(existingMember.Role, "professor", StringComparison.OrdinalIgnoreCase))
                {
                    existingMember.Role = "professor";
                    changed = true;
                }
            }

            team.ProfessorSupervisorUserIds ??= new List<string>();
            team.ProfessorSupervisorNames ??= new List<string>();

            if (!team.ProfessorSupervisorUserIds.Contains(professor.UserId, StringComparer.OrdinalIgnoreCase))
            {
                team.ProfessorSupervisorUserIds.Add(professor.UserId);
                changed = true;
            }

            if (!team.ProfessorSupervisorNames.Contains(professor.Name, StringComparer.OrdinalIgnoreCase))
            {
                team.ProfessorSupervisorNames.Add(professor.Name);
                changed = true;
            }

            if (!string.Equals(team.FocalProfessorUserId, professor.UserId, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(team.FocalProfessorName, professor.Name, StringComparison.OrdinalIgnoreCase))
            {
                team.FocalProfessorUserId = professor.UserId;
                team.FocalProfessorName = professor.Name;
                changed = true;
            }

            if (!changed)
            {
                return team;
            }

            AddTeamNotification(team, $"{professor.Name} assumiu a supervisão focal da equipe.");

            var saveResult = await _teamService.UpdateProfessorFocusAsync(team);
            if (!saveResult.Success)
            {
                ShowStyledAlertDialog(
                    "DOCENTE",
                    "Falha ao assumir foco",
                    $"Não foi possível registrar a supervisão focal desta equipe.\n\n{saveResult.ErrorMessage}\n\nLogs: {DebugHelper.GetLogFilePath()}",
                    "Fechar",
                    GetThemeBrush("AccentBrush"));
                return null;
            }

            SaveTeamWorkspace(team, persistInBackground: false);
            RefreshSearchSlideTeams(team);
            return team;
        }

        private string GetFirestoreStringValue(JsonElement fields, string fieldName)
        {
            return fields.TryGetProperty(fieldName, out var field) && field.TryGetProperty("stringValue", out var value)
                ? value.GetString() ?? string.Empty
                : string.Empty;
        }

        private List<string> GetFirestoreStringListValue(JsonElement fields, string fieldName)
        {
            var values = new List<string>();
            if (!fields.TryGetProperty(fieldName, out var field) ||
                !field.TryGetProperty("arrayValue", out var arrayValue) ||
                !arrayValue.TryGetProperty("values", out var items) ||
                items.ValueKind != JsonValueKind.Array)
            {
                return values;
            }

            foreach (var item in items.EnumerateArray())
            {
                if (item.TryGetProperty("stringValue", out var value))
                {
                    var text = value.GetString();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        values.Add(text.Trim());
                    }
                }
            }

            return values;
        }

        private List<ProfileGalleryImage> GetFirestoreProfileGalleryImages(JsonElement fields, string fieldName)
        {
            var images = new List<ProfileGalleryImage>();
            if (!fields.TryGetProperty(fieldName, out var field) ||
                !field.TryGetProperty("arrayValue", out var arrayValue) ||
                !arrayValue.TryGetProperty("values", out var items) ||
                items.ValueKind != JsonValueKind.Array)
            {
                return images;
            }

            foreach (var item in items.EnumerateArray())
            {
                if (!item.TryGetProperty("mapValue", out var mapValue) ||
                    !mapValue.TryGetProperty("fields", out var mapFields))
                {
                    continue;
                }

                images.Add(new ProfileGalleryImage
                {
                    ImageId = GetFirestoreStringValue(mapFields, "imageId"),
                    Title = GetFirestoreStringValue(mapFields, "title"),
                    Description = GetFirestoreStringValue(mapFields, "description"),
                    GalleryAlbumId = GetFirestoreStringValue(mapFields, "galleryAlbumId"),
                    GalleryAlbumTitle = GetFirestoreStringValue(mapFields, "galleryAlbumTitle"),
                    GalleryAlbumDescription = GetFirestoreStringValue(mapFields, "galleryAlbumDescription"),
                    ImageDataUri = GetFirestoreStringValue(mapFields, "imageDataUri"),
                    AddedAt = GetFirestoreTimestampValue(mapFields, "addedAt")
                });
            }

            return images;
        }

        private DateTime GetFirestoreTimestampValue(JsonElement fields, string fieldName)
        {
            if (fields.TryGetProperty(fieldName, out var field) &&
                field.TryGetProperty("timestampValue", out var value) &&
                DateTime.TryParse(value.GetString(), out var parsed))
            {
                return parsed.ToLocalTime();
            }

            return DateTime.MinValue;
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
                .Where(member => !string.IsNullOrWhiteSpace(member.UserId))
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

                member.Name = string.IsNullOrWhiteSpace(user.Name) ? member.Name : user.Name;
                member.Email = string.IsNullOrWhiteSpace(user.Email) ? member.Email : user.Email;
                member.Registration = string.IsNullOrWhiteSpace(user.Registration) ? member.Registration : user.Registration;
                member.Course = string.IsNullOrWhiteSpace(user.Course) ? member.Course : user.Course;
                member.AvatarBody = user.AvatarBody;
                member.AvatarHair = user.AvatarHair;
                member.AvatarHat = user.AvatarHat;
                member.AvatarAccessory = user.AvatarAccessory;
                member.AvatarClothing = user.AvatarClothing;
            }
        }

        private void SyncCurrentUserAvatarAcrossTeams(UserProfile profile)
        {
            if (string.IsNullOrWhiteSpace(profile.UserId))
            {
                return;
            }

            var updatedAny = false;

            foreach (var team in _teamWorkspaces)
            {
                foreach (var member in team.Members)
                {
                    if (!string.Equals(member.UserId, profile.UserId, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    member.Name = string.IsNullOrWhiteSpace(profile.Name) ? member.Name : profile.Name;
                    member.Email = string.IsNullOrWhiteSpace(profile.Email) ? member.Email : profile.Email;
                    member.Registration = string.IsNullOrWhiteSpace(profile.Registration) ? member.Registration : profile.Registration;
                    member.Course = string.IsNullOrWhiteSpace(profile.Course) ? member.Course : profile.Course;
                    member.AvatarBody = profile.AvatarBody;
                    member.AvatarHair = profile.AvatarHair;
                    member.AvatarHat = profile.AvatarHat;
                    member.AvatarAccessory = profile.AvatarAccessory;
                    member.AvatarClothing = profile.AvatarClothing;
                    updatedAny = true;
                }
            }

            foreach (var member in _draftTeamMembers)
            {
                if (!string.Equals(member.UserId, profile.UserId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                member.Name = string.IsNullOrWhiteSpace(profile.Name) ? member.Name : profile.Name;
                member.Email = string.IsNullOrWhiteSpace(profile.Email) ? member.Email : profile.Email;
                member.Registration = string.IsNullOrWhiteSpace(profile.Registration) ? member.Registration : profile.Registration;
                member.Course = string.IsNullOrWhiteSpace(profile.Course) ? member.Course : profile.Course;
                member.AvatarBody = profile.AvatarBody;
                member.AvatarHair = profile.AvatarHair;
                member.AvatarHat = profile.AvatarHat;
                member.AvatarAccessory = profile.AvatarAccessory;
                member.AvatarClothing = profile.AvatarClothing;
                updatedAny = true;
            }

            if (!updatedAny)
            {
                return;
            }

            RenderTeamMembersDraft();
            RenderTeamsList();
            if (_activeTeamWorkspace != null)
            {
                RenderTeamWorkspace();
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
            if (string.IsNullOrWhiteSpace(folder) || string.IsNullOrWhiteSpace(option))
            {
                return null;
            }

            var cacheKey = $"{folder}/{option}";
            return _avatarImageCache.GetOrAdd(cacheKey, _ =>
            {
                try
                {
                    return CreateFrozenBitmapImage(new Uri($"pack://application:,,,/img/avatar/{folder}/{option}.png", UriKind.Absolute));
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine($"[Avatar] Falha ao carregar recurso {folder}/{option}.png: {ex.Message}");
                    return null;
                }
            });
        }

        private static BitmapImage CreateFrozenBitmapImage(Uri uri)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = uri;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
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
            const string avatarEditorReadyHint = "Ao aplicar, o personagem é atualizado na tela e salvo no Firebase imediatamente.";

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
                TextWrapping = TextWrapping.Wrap,
                Text = "Preparando prévia do personagem..."
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
                Text = "Preparando opções do personagem...",
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

            dialog.Loaded += async (_, __) =>
            {
                await Dispatcher.Yield(DispatcherPriority.Background);
                RenderEditorOptions();
                RenderEditorPreview();
                footerHint.Text = avatarEditorReadyHint;
            };

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
            SyncDraftTemplateSuggestion();
            EnsureCurrentUserInTeamDraft();
            UpdateTeamMemberRoleHint();
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

            var roleLabel = TeamPermissionService.GetRoleLabel(GetSelectedDraftMemberRole());
            TeamCreationStatusText.Text = $"Participante selecionado: {selectedUser.Name}. Clique em Adicionar participante para incluir como {roleLabel.ToLowerInvariant()}.";
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
            TeamCreationStatusText.Text = "Buscando participantes no Firebase...";

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
                    TeamCreationStatusText.Text = $"{results.Count} participantes encontrados. Selecione um nas prévias.";
                    TeamMemberInput.IsDropDownOpen = true;
                    return;
                }

                if (results.Count == 1)
                {
                    TeamCreationStatusText.Text = "1 participante encontrado. Selecione a prévia e clique em Adicionar participante.";
                    TeamMemberInput.IsDropDownOpen = true;
                    return;
                }

                TeamCreationStatusText.Text = "Nenhum participante encontrado com esse critério.";
                TeamMemberInput.IsDropDownOpen = false;
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[TeamMemberInput_TextChanged ERROR] {ex.Message}");
                TeamCreationStatusText.Text = "Não foi possível buscar participantes agora.";
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
                TeamCreationStatusText.Text = "Esse participante já faz parte da equipe.";
                ClearTeamMemberSearchUi();
                return;
            }

            var draftMember = CloneUserInfo(member, GetSelectedDraftMemberRole());
            _draftTeamMembers.Add(draftMember);
            RenderTeamMembersDraft();
            ClearTeamMemberSearchUi();
            TeamCreationStatusText.Text = $"{draftMember.Name} adicionado com sucesso como {TeamPermissionService.GetRoleLabel(draftMember.Role).ToLowerInvariant()}.";
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
            var academicTerm = NormalizeTeamValue(TeamAcademicTermTextBox.Text);
            var selectedTemplate = TeamTemplateComboBox.SelectedItem as AcademicProjectTemplateInfo;

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
                AcademicTerm = string.IsNullOrWhiteSpace(academicTerm) ? ResolveCurrentAcademicTerm() : academicTerm,
                TemplateId = selectedTemplate?.TemplateId ?? string.Empty,
                TemplateName = selectedTemplate?.Title ?? string.Empty,
                CreatedBy = GetCurrentUserId(),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                LastRealtimeSyncAt = DateTime.Now,
                ProjectProgress = 0,
                ProjectDeadline = null,
                ProjectStatus = "Planejamento",
                TeacherNotes = selectedTemplate?.ProfessorGuidance ?? string.Empty,
                FocalProfessorUserId = CurrentViewerCanClaimFocalProfessor() ? GetCurrentUserId() : string.Empty,
                FocalProfessorName = CurrentViewerCanClaimFocalProfessor() ? (_currentProfile?.Name ?? string.Empty) : string.Empty,
                ProfessorSupervisorUserIds = CurrentViewerCanClaimFocalProfessor() && !string.IsNullOrWhiteSpace(GetCurrentUserId())
                    ? new List<string> { GetCurrentUserId() }
                    : new List<string>(),
                ProfessorSupervisorNames = CurrentViewerCanClaimFocalProfessor() && !string.IsNullOrWhiteSpace(_currentProfile?.Name)
                    ? new List<string> { _currentProfile?.Name ?? string.Empty }
                    : new List<string>(),
                DefaultFilePermissionScope = TeamPermissionService.IsProfessorLike(_currentProfile?.Role) ? "course" : "team",
                Members = _draftTeamMembers
                    .GroupBy(item => item.UserId, StringComparer.OrdinalIgnoreCase)
                    .Select(group => group.First())
                    .OrderBy(item => item.Name)
                    .ToList(),
                Ucs = _draftTeamUcs
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(item => item)
                    .ToList(),
                AccessRules = TeamPermissionService.CreateDefaultAccessRules(),
                SemesterTimeline = new List<TeamTimelineItemInfo>(),
                Milestones = CreateDefaultMilestones(),
                Assets = _draftTeamLogoAsset == null
                    ? new List<TeamAssetInfo>()
                    : new List<TeamAssetInfo>
                    {
                        new TeamAssetInfo
                        {
                            AssetId = Guid.NewGuid().ToString("N"),
                            Category = "logo",
                            FileName = _draftTeamLogoAsset.FileName,
                            PreviewImageDataUri = _draftTeamLogoAsset.PreviewImageDataUri,
                            AddedByUserId = _draftTeamLogoAsset.AddedByUserId,
                            AddedAt = DateTime.Now
                        }
                    },
                TaskColumns = CreateDefaultTeamColumns(),
                Notifications = new List<TeamNotificationInfo>(),
                ChatMessages = new List<TeamChatMessageInfo>(),
                CsdBoard = CreateDefaultCsdBoard()
            };

            if (selectedTemplate != null)
            {
                AcademicProjectTemplateCatalog.ApplyToTeam(teamWorkspace, selectedTemplate, GetCurrentUserId());
            }

            AddTeamNotification(
                teamWorkspace,
                selectedTemplate == null
                    ? "Equipe criada com estrutura personalizada."
                    : $"Equipe criada com o modelo acadêmico {selectedTemplate.Title}.");

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

            OpenTeamWorkspace(team, navigateToTeams: false);
        }

        private void OpenTeamWorkspaceFromCalendar_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not TeamWorkspaceInfo team)
            {
                return;
            }

            OpenTeamWorkspace(team, navigateToTeams: true);
        }

        private void OpenTeamWorkspace(TeamWorkspaceInfo team, bool navigateToTeams)
        {
            _activeTeamWorkspace = team;
            _activeTeamBoardView = TeamBoardView.Trello;
            TeamWorkspaceHost.Content = CreateTeamWorkspaceLoadingState(team);

            if (navigateToTeams)
            {
                ShowTeamsSection();
            }

            RenderTeamWorkspace();
        }

        private void ShowProfessorDashboardSection()
        {
            ResetNavigation();
            ProfessorDashboardContent.Visibility = Visibility.Visible;
            RenderProfessorDashboard();
        }

        private void ShowTeamsSection()
        {
            ResetNavigation();
            TeamsContent.Visibility = Visibility.Visible;
            RenderTeamsList();
        }

        private async void TeamListSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var query = NormalizeTeamValue(TeamListSearchBox?.Text ?? string.Empty);
            if (string.IsNullOrWhiteSpace(query))
            {
                _teamListSearchVersion++;
                _teamListSearchResults = new List<TeamWorkspaceInfo>();
                if (TeamListSearchStatusText != null)
                {
                    TeamListSearchStatusText.Text = CurrentViewerCanUseProfessorDiscovery()
                        ? "Pesquise por nome da equipe, turma, curso, UC ou professor focal."
                        : "Filtre suas equipes por nome, turma, curso ou UC.";
                }
                RenderTeamsList();
                return;
            }

            var currentVersion = ++_teamListSearchVersion;
            if (TeamListSearchStatusText != null)
            {
                TeamListSearchStatusText.Text = CurrentViewerCanUseProfessorDiscovery()
                    ? "Buscando equipes no ambiente local e no Firestore..."
                    : "Filtrando suas equipes carregadas...";
            }

            await Task.Delay(220);
            if (currentVersion != _teamListSearchVersion)
            {
                return;
            }

            _teamListSearchResults = await LoadSearchSlideTeamMatchesAsync(query, maxResults: 40);
            if (currentVersion != _teamListSearchVersion)
            {
                return;
            }

            if (TeamListSearchStatusText != null)
            {
                TeamListSearchStatusText.Text = _teamListSearchResults.Count == 0
                    ? $"Nenhuma equipe encontrada para \"{query}\"."
                    : _teamListSearchResults.Count == 1
                        ? "1 equipe encontrada."
                        : $"{_teamListSearchResults.Count} equipes encontradas.";
            }

            RenderTeamsList();
        }

        private void RenderProfessorDashboard()
        {
            if (ProfessorDashboardHost == null || ProfessorDashboardStatusText == null)
            {
                return;
            }

            ProfessorDashboardHost.Children.Clear();

            if (!TeamPermissionService.CanUseProfessorDashboard(_currentProfile))
            {
                ProfessorDashboardStatusText.Text = "O dashboard docente é exibido apenas para perfis com papel de professor orientador.";
                ProfessorDashboardHost.Children.Add(CreateSearchSlideInfoCard(
                    "Sem acesso docente",
                    "Atualize o cadastro como professor orientador para acompanhar risco, marcos e carga de várias equipes ao mesmo tempo."));
                return;
            }

            var teams = _teamWorkspaces
                .Select(EnsureTeamWorkspaceDefaults)
                .OrderBy(team => team.Course)
                .ThenBy(team => team.ClassName)
                .ThenBy(team => team.TeamName)
                .ToList();

            if (teams.Count == 0)
            {
                ProfessorDashboardStatusText.Text = "Nenhuma equipe vinculada ainda para leitura executiva.";
                ProfessorDashboardHost.Children.Add(CreateSearchSlideInfoCard(
                    "Aguardando equipes",
                    "Assim que equipes forem criadas ou vinculadas, o painel docente passa a consolidar risco, atrasos, checkpoints e sinais de carga por turma."));
                return;
            }

            var snapshot = AcademicRiskEngine.BuildProfessorDashboard(teams);
            ProfessorDashboardStatusText.Text = $"{snapshot.Teams.Count} equipe(s) em leitura, {snapshot.HighRiskTeams} em risco alto e {snapshot.OverdueItems} item(ns) atrasados no consolidado.";

            ProfessorDashboardHost.Children.Add(CreateProfessorDashboardHero(snapshot));

            foreach (var group in snapshot.Teams.GroupBy(item => $"{item.Course} • {item.ClassName}"))
            {
                ProfessorDashboardHost.Children.Add(new TextBlock
                {
                    Text = group.Key,
                    Margin = new Thickness(0, 18, 0, 10),
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = GetThemeBrush("PrimaryTextBrush")
                });

                foreach (var risk in group)
                {
                    var team = teams.First(current => string.Equals(current.TeamId, risk.TeamId, StringComparison.OrdinalIgnoreCase));
                    ProfessorDashboardHost.Children.Add(CreateProfessorDashboardTeamCard(team, risk));
                }
            }
        }

        private Border CreateProfessorDashboardHero(ProfessorDashboardSnapshot snapshot)
        {
            var card = new Border
            {
                Background = GetThemeBrush("CardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(22),
                Padding = new Thickness(20),
                Margin = new Thickness(0, 0, 0, 10)
            };

            var stack = new StackPanel();
            stack.Children.Add(new TextBlock
            {
                Text = "Leitura executiva por turma",
                FontFamily = GetAppDisplayFontFamily(),
                FontSize = 22,
                FontWeight = FontWeights.ExtraBold,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            });
            stack.Children.Add(new TextBlock
            {
                Text = "O consolidado docente mistura radar de atraso, carga dos membros e checkpoints para orientar a próxima intervenção acadêmica.",
                Margin = new Thickness(0, 8, 0, 0),
                FontSize = 12,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20
            });

            var metrics = new WrapPanel { Margin = new Thickness(0, 14, 0, 0) };
            metrics.Children.Add(CreateTeamMetricCard("Turmas", snapshot.Teams.Select(item => item.ClassName).Distinct(StringComparer.OrdinalIgnoreCase).Count().ToString(), "Agrupamentos ativos", Color.FromRgb(37, 99, 235)));
            metrics.Children.Add(CreateTeamMetricCard("Risco alto", snapshot.HighRiskTeams.ToString(), "Intervenção imediata", Color.FromRgb(220, 38, 38)));
            metrics.Children.Add(CreateTeamMetricCard("Risco moderado", snapshot.ModerateRiskTeams.ToString(), "Acompanhamento próximo", Color.FromRgb(245, 158, 11)));
            metrics.Children.Add(CreateTeamMetricCard("Atrasos", snapshot.OverdueItems.ToString(), "Pendências abertas", Color.FromRgb(124, 58, 237)));
            metrics.Children.Add(CreateTeamMetricCard("Próx. 7 dias", snapshot.UpcomingItems.ToString(), "Janela crítica", Color.FromRgb(16, 185, 129)));
            stack.Children.Add(metrics);

            card.Child = stack;
            return card;
        }

        private Border CreateProfessorDashboardTeamCard(TeamWorkspaceInfo team, TeamRiskSnapshot risk)
        {
            var workload = AcademicRiskEngine.BuildMemberWorkload(team).Take(4).ToList();
            var accent = string.Equals(risk.Level, "Alto", StringComparison.OrdinalIgnoreCase)
                ? Color.FromRgb(220, 38, 38)
                : string.Equals(risk.Level, "Moderado", StringComparison.OrdinalIgnoreCase)
                    ? Color.FromRgb(245, 158, 11)
                    : Color.FromRgb(16, 185, 129);

            var card = new Border
            {
                Background = GetThemeBrush("MutedCardBackgroundBrush"),
                BorderBrush = new SolidColorBrush(accent),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(20),
                Padding = new Thickness(18),
                Margin = new Thickness(0, 0, 0, 14)
            };

            var stack = new StackPanel();
            stack.Children.Add(new TextBlock
            {
                Text = team.TeamName,
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            });
            stack.Children.Add(new TextBlock
            {
                Text = string.IsNullOrWhiteSpace(team.TemplateName)
                    ? $"{team.Course} • {team.ClassName} • {team.AcademicTerm}"
                    : $"{team.Course} • {team.ClassName} • {team.TemplateName}",
                Margin = new Thickness(0, 4, 0, 0),
                FontSize = 11,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            });

            var chips = new WrapPanel { Margin = new Thickness(0, 10, 0, 0) };
            chips.Children.Add(CreateStaticTeamChip($"Risco {risk.Level}", new SolidColorBrush(accent), Brushes.White));
            chips.Children.Add(CreateStaticTeamChip($"{risk.OverdueItems} atraso(s)", GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            chips.Children.Add(CreateStaticTeamChip($"{risk.UpcomingSevenDays} na janela 7d", GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            chips.Children.Add(CreateStaticTeamChip($"{risk.PendingMilestones} marcos ativos", GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            chips.Children.Add(CreateStaticTeamChip(BuildTeamProfessorFocusLabel(team), GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            stack.Children.Add(chips);

            stack.Children.Add(new TextBlock
            {
                Text = risk.Summary,
                Margin = new Thickness(0, 10, 0, 0),
                FontSize = 11,
                Foreground = GetThemeBrush("PrimaryTextBrush"),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 18
            });
            stack.Children.Add(new TextBlock
            {
                Text = risk.Recommendation,
                Margin = new Thickness(0, 8, 0, 0),
                FontSize = 11,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 18
            });

            if (workload.Count > 0)
            {
                var workloadWrap = new WrapPanel { Margin = new Thickness(0, 12, 0, 0) };
                foreach (var member in workload)
                {
                    workloadWrap.Children.Add(CreateStaticTeamChip(
                        $"{member.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? member.Name}: {member.Level}",
                        GetThemeBrush("CardBackgroundBrush"),
                        GetThemeBrush("PrimaryTextBrush")));
                }
                stack.Children.Add(workloadWrap);
            }

            var actions = new WrapPanel { Margin = new Thickness(0, 14, 0, 0) };
            actions.Children.Add(CreateCalendarOpenTeamButton(team, "Abrir equipe"));

            var openAgendaButton = new Button
            {
                Content = "Abrir agenda",
                Background = GetThemeBrush("CardBackgroundBrush"),
                Foreground = GetThemeBrush("PrimaryTextBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(14, 8, 14, 8),
                Margin = new Thickness(8, 0, 0, 0),
                Cursor = Cursors.Hand,
                FontWeight = FontWeights.SemiBold,
                Tag = team.TeamId
            };
            openAgendaButton.Click += (_, __) =>
            {
                _calendarFilterTeamId = team.TeamId;
                ResetNavigation();
                CalendarContent.Visibility = Visibility.Visible;
                RenderCalendarAgenda();
            };
            actions.Children.Add(openAgendaButton);

            if (CurrentViewerCanClaimFocalProfessor() && !string.Equals(team.FocalProfessorUserId, GetCurrentUserId(), StringComparison.OrdinalIgnoreCase))
            {
                var claimButton = new Button
                {
                    Content = "Assumir foco docente",
                    Background = GetThemeBrush("CardBackgroundBrush"),
                    Foreground = GetThemeBrush("PrimaryTextBrush"),
                    BorderBrush = GetThemeBrush("CardBorderBrush"),
                    BorderThickness = new Thickness(1),
                    Padding = new Thickness(14, 8, 14, 8),
                    Margin = new Thickness(8, 0, 0, 0),
                    Cursor = Cursors.Hand,
                    FontWeight = FontWeights.SemiBold,
                    Tag = team
                };
                claimButton.Click += async (_, __) =>
                {
                    var updatedTeam = await AssignCurrentProfessorAsFocalAsync(team);
                    if (updatedTeam == null)
                    {
                        return;
                    }

                    ProfessorDashboardStatusText.Text = $"{updatedTeam.TeamName} agora está sob sua supervisão focal.";
                    ProfessorDashboardStatusText.Foreground = new SolidColorBrush(Color.FromRgb(21, 128, 61));
                    RenderProfessorDashboard();
                };
                actions.Children.Add(claimButton);
            }

            stack.Children.Add(actions);
            card.Child = stack;
            return card;
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
            if (e.LeftButton != MouseButtonState.Pressed || sender is not Border border || border.Tag is not TeamTaskDragInfo dragInfo)
            {
                return;
            }

            if (!CanCurrentUserEditProjectSettings(dragInfo.Team))
            {
                return;
            }

            _draggedTeamTaskCard = dragInfo.Card;
            ShowBoardDragPreview(border, CreateTaskCardDragPreview(dragInfo));

            try
            {
                DragDrop.DoDragDrop(border, new DataObject(typeof(TeamTaskCardInfo), dragInfo.Card), DragDropEffects.Move);
            }
            finally
            {
                HideBoardDragPreview();
                _draggedTeamTaskCard = null;
            }
        }

        private void CsdNoteCard_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || sender is not Border border || border.Tag is not CsdNoteDragInfo dragInfo)
            {
                return;
            }

            _draggedCsdNote = dragInfo;
            ShowBoardDragPreview(border, CreateCsdNoteDragPreview(dragInfo));

            try
            {
                DragDrop.DoDragDrop(border, new DataObject(typeof(CsdNoteDragInfo), dragInfo), DragDropEffects.Move);
            }
            finally
            {
                HideBoardDragPreview();
                _draggedCsdNote = null;
            }
        }

        private void BoardDragPreview_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            if (_boardDragPreviewPopup?.IsOpen == true)
            {
                e.UseDefaultCursors = false;
                Mouse.SetCursor(Cursors.Hand);
                UpdateBoardDragPreviewPosition();
                e.Handled = true;
                return;
            }

            e.UseDefaultCursors = true;
        }

        private void CsdColumn_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(typeof(CsdNoteDragInfo)) ? DragDropEffects.Move : DragDropEffects.None;
            e.Handled = true;
        }

        private void CsdColumn_Drop(object sender, DragEventArgs e)
        {
            if (sender is not Border border ||
                border.Tag is not Tuple<TeamWorkspaceInfo, string> targetInfo ||
                !e.Data.GetDataPresent(typeof(CsdNoteDragInfo)))
            {
                return;
            }

            var draggedNote = e.Data.GetData(typeof(CsdNoteDragInfo)) as CsdNoteDragInfo ?? _draggedCsdNote;
            if (draggedNote == null)
            {
                return;
            }

            var targetTeam = targetInfo.Item1;
            var targetBucket = targetInfo.Item2;
            var sourceNotes = GetCsdNotesBucket(draggedNote.Team, draggedNote.SourceBucket);
            var targetNotes = GetCsdNotesBucket(targetTeam, targetBucket);

            if (ReferenceEquals(sourceNotes, targetNotes))
            {
                return;
            }

            if (!TryRemoveCsdNote(sourceNotes, draggedNote))
            {
                return;
            }

            targetNotes.Add(draggedNote.Note);
            AddTeamNotification(targetTeam, $"Nota movida de {draggedNote.SourceBucket.ToLowerInvariant()} para {targetBucket.ToLowerInvariant()}.");
            SaveTeamWorkspace(targetTeam);
            RenderTeamWorkspace();
            e.Handled = true;
        }

        private void ShowBoardDragPreview(FrameworkElement sourceElement, FrameworkElement previewContent)
        {
            HideBoardDragPreview();

            var initialScale = 0.9;
            var targetScale = 1d;
            _boardDragPreviewScaleTransform = new ScaleTransform(initialScale, initialScale);
            _boardDragSourceElement = sourceElement;
            _boardDragSourceOriginalOpacity = sourceElement.Opacity;

            sourceElement.BeginAnimation(OpacityProperty, new DoubleAnimation(_boardDragSourceOriginalOpacity, 0.18, TimeSpan.FromMilliseconds(90))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            });

            _boardDragPreviewVisual = new Border
            {
                Child = previewContent,
                Background = Brushes.Transparent,
                Opacity = 0.98,
                IsHitTestVisible = false,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new TransformGroup
                {
                    Children =
                    {
                        _boardDragPreviewScaleTransform,
                        new RotateTransform(-3)
                    }
                },
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    BlurRadius = 22,
                    Direction = 270,
                    Opacity = 0.36,
                    ShadowDepth = 10,
                    Color = Colors.Black
                },
                CacheMode = new BitmapCache()
            };

            _boardDragPreviewPopup = new Popup
            {
                AllowsTransparency = true,
                IsHitTestVisible = false,
                Placement = PlacementMode.AbsolutePoint,
                StaysOpen = true,
                PopupAnimation = PopupAnimation.None,
                Child = _boardDragPreviewVisual
            };

            UpdateBoardDragPreviewPosition();
            _boardDragPreviewPopup.IsOpen = true;

            var easing = new CubicEase { EasingMode = EasingMode.EaseOut };
            _boardDragPreviewScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(initialScale, targetScale, TimeSpan.FromMilliseconds(150))
            {
                EasingFunction = easing
            });

            _boardDragPreviewScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(initialScale, targetScale, TimeSpan.FromMilliseconds(150))
            {
                EasingFunction = easing
            });
        }

        private void UpdateBoardDragPreviewPosition()
        {
            if (_boardDragPreviewPopup == null)
            {
                return;
            }

            var cursorPosition = GetCursorScreenDipPosition();
            _boardDragPreviewPopup.HorizontalOffset = cursorPosition.X + 26;
            _boardDragPreviewPopup.VerticalOffset = cursorPosition.Y + 20;
        }

        private void HideBoardDragPreview()
        {
            if (_boardDragSourceElement != null)
            {
                _boardDragSourceElement.BeginAnimation(OpacityProperty, new DoubleAnimation(_boardDragSourceElement.Opacity, _boardDragSourceOriginalOpacity, TimeSpan.FromMilliseconds(110))
                {
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                });
            }

            if (_boardDragPreviewPopup != null)
            {
                _boardDragPreviewPopup.IsOpen = false;
                _boardDragPreviewPopup.Child = null;
            }

            _boardDragPreviewPopup = null;
            _boardDragPreviewVisual = null;
            _boardDragPreviewScaleTransform = null;
            _boardDragSourceElement = null;
            _boardDragSourceOriginalOpacity = 1;
        }

        private FrameworkElement CreateTaskCardDragPreview(TeamTaskDragInfo dragInfo)
        {
            var card = dragInfo.Card;
            var team = dragInfo.Team;
            var column = dragInfo.Column;
            var isOverdue = card.DueDate.HasValue && card.DueDate.Value.Date < DateTime.Today;
            var priorityColor = card.Priority switch
            {
                "Alta" => Color.FromRgb(220, 38, 38),
                "Baixa" => Color.FromRgb(16, 185, 129),
                _ => Color.FromRgb(245, 158, 11)
            };
            var assignedMembers = team.Members
                .Where(member => card.AssignedUserIds.Contains(member.UserId))
                .ToList();

            var previewStack = new StackPanel();
            previewStack.Children.Add(new Border
            {
                Width = 44,
                Height = 5,
                CornerRadius = new CornerRadius(999),
                Background = new SolidColorBrush(column.AccentColor),
                Margin = new Thickness(0, 0, 0, 10)
            });
            previewStack.Children.Add(new TextBlock
            {
                Text = column.Title.ToUpperInvariant(),
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(column.AccentColor),
                Margin = new Thickness(0, 0, 0, 8)
            });
            previewStack.Children.Add(new TextBlock
            {
                Text = card.Title,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                TextWrapping = TextWrapping.Wrap,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            });

            if (!string.IsNullOrWhiteSpace(card.Description))
            {
                previewStack.Children.Add(new TextBlock
                {
                    Text = card.Description,
                    FontSize = 11,
                    TextWrapping = TextWrapping.Wrap,
                    MaxWidth = 232,
                    Margin = new Thickness(0, 8, 0, 0),
                    Foreground = GetThemeBrush("SecondaryTextBrush")
                });
            }

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

            previewStack.Children.Add(chips);

            if (assignedMembers.Count > 0)
            {
                previewStack.Children.Add(new TextBlock
                {
                    Text = assignedMembers.Count == 1 ? "Responsavel" : "Responsaveis",
                    FontSize = 10,
                    Margin = new Thickness(0, 10, 0, 6),
                    Foreground = GetThemeBrush("TertiaryTextBrush")
                });
                previewStack.Children.Add(CreateTaskAssigneesPanel(assignedMembers, compactMode: true));
            }

            previewStack.Children.Add(new TextBlock
            {
                Text = $"Criado em {card.CreatedAt:dd/MM}",
                FontSize = 10,
                Margin = new Thickness(0, 10, 0, 0),
                Foreground = GetThemeBrush("TertiaryTextBrush")
            });

            return new Border
            {
                Width = 260,
                MaxWidth = 260,
                Background = isOverdue
                    ? new SolidColorBrush(Color.FromRgb(254, 242, 242))
                    : Brushes.White,
                BorderBrush = new SolidColorBrush(isOverdue ? Color.FromRgb(220, 38, 38) : column.AccentColor),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(18),
                Padding = new Thickness(16),
                Child = previewStack
            };
        }

        private FrameworkElement CreateCsdNoteDragPreview(CsdNoteDragInfo dragInfo)
        {
            var accent = new SolidColorBrush(GetCsdBucketAccentColor(dragInfo.SourceBucket));

            var stack = new StackPanel();
            stack.Children.Add(new TextBlock
            {
                Text = dragInfo.SourceBucket.ToUpperInvariant(),
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = accent,
                Margin = new Thickness(0, 0, 0, 8)
            });
            stack.Children.Add(new TextBlock
            {
                Text = dragInfo.Note,
                FontSize = 12,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 212,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            });

            return new Border
            {
                Width = 240,
                MaxWidth = 240,
                Background = GetThemeBrush("MutedCardBackgroundBrush"),
                BorderBrush = accent,
                BorderThickness = new Thickness(1.5),
                CornerRadius = new CornerRadius(16),
                Padding = new Thickness(14),
                Child = stack
            };
        }

        private List<string> GetCsdNotesBucket(TeamWorkspaceInfo team, string bucket)
        {
            return bucket switch
            {
                "Suposicoes" => team.CsdBoard.Assumptions,
                "Duvidas" => team.CsdBoard.Doubts,
                _ => team.CsdBoard.Certainties
            };
        }

        private Color GetCsdBucketAccentColor(string bucket)
        {
            return bucket switch
            {
                "Suposicoes" => Color.FromRgb(245, 158, 11),
                "Duvidas" => Color.FromRgb(168, 85, 247),
                _ => Color.FromRgb(37, 99, 235)
            };
        }

        private bool TryRemoveCsdNote(List<string> notes, CsdNoteDragInfo draggedNote)
        {
            if (draggedNote.SourceIndex >= 0 &&
                draggedNote.SourceIndex < notes.Count &&
                string.Equals(notes[draggedNote.SourceIndex], draggedNote.Note, StringComparison.Ordinal))
            {
                notes.RemoveAt(draggedNote.SourceIndex);
                return true;
            }

            var fallbackIndex = notes.FindIndex(note => string.Equals(note, draggedNote.Note, StringComparison.Ordinal));
            if (fallbackIndex < 0)
            {
                return false;
            }

            notes.RemoveAt(fallbackIndex);
            return true;
        }

        private Point GetCursorScreenDipPosition()
        {
            if (!GetCursorPos(out var nativePoint))
            {
                return new Point(0, 0);
            }

            var screenPoint = new Point(nativePoint.X, nativePoint.Y);
            var source = PresentationSource.FromVisual(this);

            if (source?.CompositionTarget == null)
            {
                return screenPoint;
            }

            return source.CompositionTarget.TransformFromDevice.Transform(screenPoint);
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

            if (!CanCurrentUserEditProjectSettings(_activeTeamWorkspace))
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
            draggedCard.ColumnId = targetColumn.Id;
            draggedCard.UpdatedAt = DateTime.Now;
            draggedCard.UpdatedByUserId = GetCurrentUserId();
            targetColumn.Cards.Add(draggedCard);
            AddTeamNotification(_activeTeamWorkspace, $"Tarefa \"{draggedCard.Title}\" movida para {targetColumn.Title}.");
            SaveTeamWorkspace(_activeTeamWorkspace);
            RenderTeamWorkspace();
        }

        private void OpenCreateTaskDialog(TeamWorkspaceInfo team)
        {
            if (!CanCurrentUserEditProjectSettings(team))
            {
                ShowStyledAlertDialog("TAREFA", "Permissão insuficiente", "Seu papel atual pode colaborar nas tarefas, mas não criar novas entradas neste board.", "Fechar", GetThemeBrush("AccentBrush"));
                return;
            }

            var dialog = CreateTaskDialog(team, null, null);
            if (dialog is not { } taskDialog)
            {
                return;
            }

            var targetColumn = team.TaskColumns.FirstOrDefault(column => column.Id == taskDialog.TargetColumnId) ?? team.TaskColumns.First();
            taskDialog.Card.ColumnId = targetColumn.Id;
            taskDialog.Card.CreatedByUserId = string.IsNullOrWhiteSpace(taskDialog.Card.CreatedByUserId) ? GetCurrentUserId() : taskDialog.Card.CreatedByUserId;
            taskDialog.Card.UpdatedAt = DateTime.Now;
            taskDialog.Card.UpdatedByUserId = GetCurrentUserId();
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

            if (!CanCurrentUserEditProjectSettings(team))
            {
                ShowStyledAlertDialog("TAREFA", "Permissão insuficiente", "Seu papel atual não permite editar a estrutura das tarefas desta equipe.", "Fechar", GetThemeBrush("AccentBrush"));
                return;
            }

            var dialog = CreateTaskDialog(team, column, card);
            if (dialog is not { } taskDialog)
            {
                return;
            }

            taskDialog.Card.ColumnId = taskDialog.TargetColumnId;
            taskDialog.Card.CreatedByUserId = string.IsNullOrWhiteSpace(taskDialog.Card.CreatedByUserId) ? GetCurrentUserId() : taskDialog.Card.CreatedByUserId;
            taskDialog.Card.UpdatedAt = DateTime.Now;
            taskDialog.Card.UpdatedByUserId = GetCurrentUserId();

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

            if (!CanCurrentUserEditProjectSettings(team))
            {
                ShowStyledAlertDialog("TAREFA", "Permissão insuficiente", "Seu papel atual não permite excluir tarefas desta equipe.", "Fechar", GetThemeBrush("AccentBrush"));
                return;
            }

            if (MessageBox.Show($"Excluir a tarefa \"{card.Title}\"?", "Excluir tarefa", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            column.Cards.RemoveAll(item => item.Id == card.Id);
            AddTeamNotification(team, $"Tarefa removida: {card.Title}.");
            SaveTeamWorkspace(team);
            RenderTeamWorkspace();
        }

        private void OpenTaskCollaboration_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not Tuple<TeamWorkspaceInfo, TeamTaskColumnInfo, TeamTaskCardInfo> payload)
            {
                return;
            }

            var team = payload.Item1;
            var column = payload.Item2;
            var card = payload.Item3;
            OpenCollaborationDialogCore(
                team,
                "TAREFA",
                card.Title,
                $"{column.Title} • {card.Priority}" + (card.DueDate.HasValue ? $" • prazo {card.DueDate.Value:dd/MM}" : string.Empty),
                card.Comments,
                card.Attachments,
                card.MentionedUserIds,
                () =>
                {
                    card.UpdatedAt = DateTime.Now;
                    card.UpdatedByUserId = GetCurrentUserId();
                });
        }

        private void OpenMilestoneCollaboration_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not Tuple<TeamWorkspaceInfo, TeamMilestoneInfo> payload)
            {
                return;
            }

            var team = payload.Item1;
            var milestone = payload.Item2;
            OpenCollaborationDialogCore(
                team,
                "ENTREGA",
                milestone.Title,
                $"{milestone.Status}" + (milestone.DueDate.HasValue ? $" • prazo {milestone.DueDate.Value:dd/MM}" : string.Empty),
                milestone.Comments,
                milestone.Attachments,
                milestone.MentionedUserIds,
                () =>
                {
                    milestone.UpdatedAt = DateTime.Now;
                    milestone.UpdatedByUserId = GetCurrentUserId();
                });
        }

        private async Task<(bool Success, TeamAttachmentInfo? Attachment, TeamAssetInfo? Asset, string? ErrorMessage)> CreateTeamAttachmentFromFileAsync(
            TeamWorkspaceInfo team,
            string filePath,
            string entityTitle,
            string permissionScope)
        {
            var assetResult = await CreateOrUpdateTeamAssetAsync(
                team,
                filePath,
                ResolveAssetCategoryForFile(filePath, "documentos"),
                permissionScope,
                $"Anexo registrado em {entityTitle}.",
                null,
                $"Anexo vinculado a {entityTitle}.");
            if (!assetResult.Success || assetResult.Asset == null)
            {
                return (false, null, null, assetResult.ErrorMessage ?? "Não foi possível sincronizar o anexo remotamente.");
            }

            var asset = assetResult.Asset;
            return (true, new TeamAttachmentInfo
            {
                AttachmentId = Guid.NewGuid().ToString("N"),
                AssetId = asset.AssetId,
                FileName = asset.FileName,
                PreviewImageDataUri = asset.PreviewImageDataUri,
                PermissionScope = asset.PermissionScope,
                Version = asset.Version,
                AddedByUserId = GetCurrentUserId(),
                AddedAt = DateTime.Now
            }, asset, null);
        }

        private void OpenCollaborationDialogCore(
            TeamWorkspaceInfo team,
            string eyebrow,
            string entityTitle,
            string entitySubtitle,
            List<TeamCommentInfo> comments,
            List<TeamAttachmentInfo> attachments,
            List<string> mentionedUserIds,
            Action? markEntityUpdated = null)
        {
            if (!CanCurrentUserComment(team) && !CanCurrentUserUploadFiles(team))
            {
                ShowStyledAlertDialog(eyebrow, "Permissão insuficiente", "Seu papel atual não permite comentar ou anexar materiais neste item.", "Fechar", GetThemeBrush("AccentBrush"));
                return;
            }

            var accentBrush = GetThemeBrush("AccentBrush");
            var dialog = CreateStyledDialogWindow($"Colaboração • {entityTitle}", 860, 900, 760, true);
            var commentBox = new TextBox
            {
                MinHeight = 124,
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(0, 8, 0, 0)
            };
            var permissionScopeBox = new ComboBox
            {
                Height = 46,
                Margin = new Thickness(0, 8, 0, 0),
                DisplayMemberPath = "Label",
                SelectedValuePath = "Value",
                ItemsSource = new[]
                {
                    new { Label = "Equipe", Value = "team" },
                    new { Label = "Curso", Value = "course" },
                    new { Label = "Liderança", Value = "leadership" },
                    new { Label = "Privado", Value = "private" }
                },
                SelectedValue = team.DefaultFilePermissionScope
            };
            ApplyDialogInputStyle(commentBox);
            ApplyDialogInputStyle(permissionScopeBox);

            var selectedMentionIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var pendingAttachments = new List<PendingAttachmentFile>();
            var selectedMentionsWrap = new WrapPanel { Margin = new Thickness(0, 10, 0, 0) };
            var mentionPanel = new WrapPanel { Margin = new Thickness(0, 12, 0, 0) };
            var pendingAttachmentsHost = new StackPanel { Margin = new Thickness(0, 12, 0, 0) };
            var attachmentsHost = new StackPanel();
            var commentsHost = new StackPanel();
            var composerStateText = new TextBlock
            {
                Margin = new Thickness(0, 10, 0, 0),
                FontSize = 11,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            };

            void RefreshComposerState()
            {
                composerStateText.Text = $"{commentBox.Text.Trim().Length} caractere(s) • {selectedMentionIds.Count} menção(ões) • {pendingAttachments.Count} anexo(s) pendente(s).";
            }

            void RenderSelectedMentions()
            {
                selectedMentionsWrap.Children.Clear();
                foreach (var member in team.Members.Where(member => selectedMentionIds.Contains(member.UserId)).OrderBy(member => member.Name))
                {
                    selectedMentionsWrap.Children.Add(CreateStaticTeamChip(
                        GetTeamMemberChipLabel(member),
                        GetThemeBrush("AccentMutedBrush"),
                        GetThemeBrush("AccentBrush")));
                }

                selectedMentionsWrap.Visibility = selectedMentionsWrap.Children.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
                RefreshComposerState();
            }

            void RenderMentionSelector()
            {
                mentionPanel.Children.Clear();
                foreach (var member in team.Members.OrderBy(member => member.Name))
                {
                    var localMember = member;
                    mentionPanel.Children.Add(CreateDialogMemberChoiceCard(
                        localMember,
                        selectedMentionIds.Contains(localMember.UserId),
                        (_, __) =>
                        {
                            if (!selectedMentionIds.Add(localMember.UserId))
                            {
                                selectedMentionIds.Remove(localMember.UserId);
                            }

                            RenderMentionSelector();
                            RenderSelectedMentions();
                        }));
                }
            }

            void RenderPendingAttachments()
            {
                pendingAttachmentsHost.Children.Clear();
                if (pendingAttachments.Count == 0)
                {
                    pendingAttachmentsHost.Children.Add(new TextBlock
                    {
                        Text = "Nenhum anexo preparado ainda. Os arquivos só sobem para o remoto quando você salvar o comentário.",
                        FontSize = 11,
                        Foreground = GetThemeBrush("SecondaryTextBrush"),
                        TextWrapping = TextWrapping.Wrap,
                        LineHeight = 18
                    });
                    RefreshComposerState();
                    return;
                }

                foreach (var attachment in pendingAttachments.ToList())
                {
                    var card = new Border
                    {
                        Margin = new Thickness(0, 0, 0, 8),
                        Padding = new Thickness(12),
                        Background = GetThemeBrush("CardBackgroundBrush"),
                        BorderBrush = GetThemeBrush("CardBorderBrush"),
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(14)
                    };

                    var row = new Grid();
                    row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    row.Children.Add(new TextBlock
                    {
                        Text = attachment.FileName,
                        FontSize = 11,
                        FontWeight = FontWeights.SemiBold,
                        Foreground = GetThemeBrush("PrimaryTextBrush"),
                        TextWrapping = TextWrapping.Wrap
                    });

                    var removeButton = new Button
                    {
                        Content = "Remover",
                        Background = Brushes.Transparent,
                        BorderThickness = new Thickness(0),
                        Foreground = new SolidColorBrush(Color.FromRgb(220, 38, 38)),
                        FontSize = 10,
                        FontWeight = FontWeights.SemiBold,
                        Cursor = Cursors.Hand,
                        Padding = new Thickness(10, 0, 0, 0)
                    };
                    removeButton.Click += (_, __) =>
                    {
                        pendingAttachments.Remove(attachment);
                        RenderPendingAttachments();
                    };
                    Grid.SetColumn(removeButton, 1);
                    row.Children.Add(removeButton);
                    card.Child = row;
                    pendingAttachmentsHost.Children.Add(card);
                }

                RefreshComposerState();
            }

            void RenderExistingAttachments()
            {
                attachmentsHost.Children.Clear();
                if (attachments.Count == 0)
                {
                    attachmentsHost.Children.Add(new TextBlock
                    {
                        Text = "Nenhum anexo foi publicado ainda neste item.",
                        FontSize = 11,
                        Foreground = GetThemeBrush("SecondaryTextBrush"),
                        TextWrapping = TextWrapping.Wrap,
                        LineHeight = 18
                    });
                    return;
                }

                foreach (var attachment in attachments.OrderByDescending(item => item.AddedAt))
                {
                    var relatedAsset = team.Assets.FirstOrDefault(asset => string.Equals(asset.AssetId, attachment.AssetId, StringComparison.OrdinalIgnoreCase));
                    var canOpen = relatedAsset != null && CanCurrentUserViewAsset(team, relatedAsset);
                    var card = new Border
                    {
                        Margin = new Thickness(0, 0, 0, 10),
                        Padding = new Thickness(14),
                        Background = GetThemeBrush("CardBackgroundBrush"),
                        BorderBrush = GetThemeBrush("CardBorderBrush"),
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(16)
                    };

                    var row = new Grid();
                    row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                    var info = new StackPanel();
                    info.Children.Add(new TextBlock
                    {
                        Text = attachment.FileName,
                        FontSize = 12,
                        FontWeight = FontWeights.SemiBold,
                        Foreground = GetThemeBrush("PrimaryTextBrush"),
                        TextWrapping = TextWrapping.Wrap
                    });
                    var chips = new WrapPanel { Margin = new Thickness(0, 8, 0, 0) };
                    chips.Children.Add(CreateStaticTeamChip($"v{Math.Max(1, attachment.Version)}", accentBrush, Brushes.White));
                    chips.Children.Add(CreateStaticTeamChip(GetPermissionScopeLabel(attachment.PermissionScope), GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
                    chips.Children.Add(CreateStaticTeamChip(attachment.AddedAt == default ? "Sem data" : attachment.AddedAt.ToString("dd/MM HH:mm"), GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
                    info.Children.Add(chips);
                    row.Children.Add(info);

                    if (canOpen)
                    {
                        var openButton = CreateDialogActionButton("Abrir", accentBrush, Brushes.White, Brushes.Transparent, 92);
                        openButton.Margin = new Thickness(12, 0, 0, 0);
                        openButton.Click += async (_, __) => await OpenTeamAssetPreviewAsync(team, relatedAsset!);
                        Grid.SetColumn(openButton, 1);
                        row.Children.Add(openButton);
                    }

                    card.Child = row;
                    attachmentsHost.Children.Add(card);
                }
            }

            void RenderComments()
            {
                commentsHost.Children.Clear();
                if (comments.Count == 0)
                {
                    commentsHost.Children.Add(new TextBlock
                    {
                        Text = "Nenhum comentário registrado ainda. Use o composer acima para abrir a trilha de colaboração deste item.",
                        FontSize = 11,
                        Foreground = GetThemeBrush("SecondaryTextBrush"),
                        TextWrapping = TextWrapping.Wrap,
                        LineHeight = 18
                    });
                    return;
                }

                foreach (var comment in comments.OrderByDescending(item => item.CreatedAt))
                {
                    commentsHost.Children.Add(CreateTeamCommentTimelineCard(team, comment));
                }
            }

            commentBox.TextChanged += (_, __) => RefreshComposerState();

            var addAttachmentButton = CreateDialogActionButton("Adicionar anexo", Brushes.Transparent, GetThemeBrush("PrimaryTextBrush"), GetThemeBrush("CardBorderBrush"), 144);
            addAttachmentButton.Click += (_, __) =>
            {
                if (!CanCurrentUserUploadFiles(team))
                {
                    ShowStyledAlertDialog(eyebrow, "Upload bloqueado", "Seu papel atual não permite anexar arquivos neste item.", "Fechar", accentBrush);
                    return;
                }

                var openDialog = new OpenFileDialog
                {
                    Multiselect = true,
                    Title = $"Selecionar anexos para {entityTitle}",
                    Filter = "Todos os arquivos|*.*"
                };

                if (openDialog.ShowDialog() != true)
                {
                    return;
                }

                foreach (var filePath in openDialog.FileNames)
                {
                    if (pendingAttachments.Any(existing => string.Equals(existing.FilePath, filePath, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    pendingAttachments.Add(new PendingAttachmentFile
                    {
                        FilePath = filePath,
                        FileName = IOPath.GetFileName(filePath),
                        PreviewImageDataUri = IsFilesHubImageExtension(GetFilesHubExtension(filePath, string.Empty))
                            ? TryCreateCompressedImageDataUri(filePath, 220, 70) ?? string.Empty
                            : string.Empty
                    });
                }

                RenderPendingAttachments();
            };

            var summaryContent = new StackPanel();
            var summaryChips = new WrapPanel();
            summaryChips.Children.Add(CreateStaticTeamChip(team.TeamName, GetThemeBrush("AccentMutedBrush"), GetThemeBrush("AccentBrush")));
            summaryChips.Children.Add(CreateStaticTeamChip($"{comments.Count} comentário(s)", GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            summaryChips.Children.Add(CreateStaticTeamChip($"{attachments.Count} anexo(s)", GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            if (!string.IsNullOrWhiteSpace(entitySubtitle))
            {
                summaryChips.Children.Add(CreateStaticTeamChip(entitySubtitle, GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            }
            summaryContent.Children.Add(summaryChips);
            summaryContent.Children.Add(new TextBlock
            {
                Text = "Comentários e anexos usam a mesma trilha remota dos materiais da equipe, com histórico de versão e respeito ao escopo escolhido no upload.",
                Margin = new Thickness(0, 12, 0, 0),
                FontSize = 12,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20
            });

            var composerContent = new StackPanel();
            composerContent.Children.Add(CreateDialogFieldLabel("Comentário"));
            composerContent.Children.Add(commentBox);
            composerContent.Children.Add(composerStateText);
            composerContent.Children.Add(CreateDialogFieldLabel("Escopo dos anexos desta publicação"));
            composerContent.Children.Add(permissionScopeBox);
            composerContent.Children.Add(CreateDialogFieldLabel("Menções"));
            composerContent.Children.Add(selectedMentionsWrap);
            composerContent.Children.Add(mentionPanel);
            if (CanCurrentUserUploadFiles(team))
            {
                composerContent.Children.Add(new Border
                {
                    Margin = new Thickness(0, 14, 0, 0),
                    Child = addAttachmentButton
                });
            }
            composerContent.Children.Add(pendingAttachmentsHost);

            var attachmentsContent = new StackPanel();
            attachmentsContent.Children.Add(attachmentsHost);

            var commentsContent = new StackPanel();
            commentsContent.Children.Add(commentsHost);

            var form = new StackPanel();
            form.Children.Add(CreateDialogSectionCard("Visão geral", "Este item já carrega contagem de comentários, menções e anexos no board principal.", accentBrush, summaryContent));
            form.Children.Add(CreateDialogSectionCard("Nova colaboração", "Escreva o comentário, marque membros e escolha os anexos que devem subir para o remoto ao salvar.", accentBrush, composerContent));
            form.Children.Add(CreateDialogSectionCard("Anexos publicados", "Todo arquivo publicado aqui também entra na trilha de materiais da equipe com o escopo configurado.", accentBrush, attachmentsContent));
            form.Children.Add(CreateDialogSectionCard("Timeline", "Leitura rápida dos comentários já registrados neste item.", accentBrush, commentsContent, new Thickness(0, 0, 0, 0)));

            var root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.Children.Add(CreateDialogHeader(eyebrow, $"Colaboração • {entityTitle}", "Centralize comentários, menções e anexos sem sair do fluxo visual do workspace acadêmico.", accentBrush));

            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = form
            };
            Grid.SetRow(scrollViewer, 1);
            root.Children.Add(scrollViewer);

            var cancelButton = CreateDialogActionButton("Fechar", Brushes.Transparent, GetThemeBrush("PrimaryTextBrush"), GetThemeBrush("CardBorderBrush"), 104);
            cancelButton.Click += (_, __) => dialog.Close();
            var saveButton = CreateDialogActionButton("Publicar", accentBrush, Brushes.White, Brushes.Transparent, 112);
            saveButton.Click += async (_, __) =>
            {
                var commentText = commentBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(commentText) && pendingAttachments.Count == 0)
                {
                    ShowStyledAlertDialog(eyebrow, "Nada para publicar", "Escreva um comentário ou prepare ao menos um anexo antes de publicar.", "Continuar", accentBrush);
                    return;
                }

                if (!string.IsNullOrWhiteSpace(commentText) && !CanCurrentUserComment(team))
                {
                    ShowStyledAlertDialog(eyebrow, "Comentário bloqueado", "Seu papel atual não permite comentar neste item.", "Fechar", accentBrush);
                    return;
                }

                if (pendingAttachments.Count > 0 && !CanCurrentUserUploadFiles(team))
                {
                    ShowStyledAlertDialog(eyebrow, "Upload bloqueado", "Seu papel atual não permite anexar arquivos neste item.", "Fechar", accentBrush);
                    return;
                }

                var createdAttachments = new List<TeamAttachmentInfo>();
                foreach (var pendingAttachment in pendingAttachments)
                {
                    var attachmentResult = await CreateTeamAttachmentFromFileAsync(
                        team,
                        pendingAttachment.FilePath,
                        entityTitle,
                        permissionScopeBox.SelectedValue?.ToString() ?? team.DefaultFilePermissionScope);
                    if (!attachmentResult.Success || attachmentResult.Attachment == null || attachmentResult.Asset == null)
                    {
                        ShowStyledAlertDialog(eyebrow, "Falha ao anexar", attachmentResult.ErrorMessage ?? "Não foi possível sincronizar um dos anexos desta publicação.", "Fechar", new SolidColorBrush(Color.FromRgb(220, 38, 38)));
                        return;
                    }

                    if (!team.Assets.Any(asset => string.Equals(asset.AssetId, attachmentResult.Asset.AssetId, StringComparison.OrdinalIgnoreCase)))
                    {
                        team.Assets.Add(attachmentResult.Asset);
                    }

                    attachments.Add(attachmentResult.Attachment);
                    createdAttachments.Add(attachmentResult.Attachment);
                }

                comments.Add(new TeamCommentInfo
                {
                    CommentId = Guid.NewGuid().ToString("N"),
                    AuthorUserId = GetCurrentUserId(),
                    AuthorName = string.IsNullOrWhiteSpace(_currentProfile?.Name) ? "Equipe" : _currentProfile!.Name,
                    Content = commentText,
                    MentionedUserIds = selectedMentionIds.ToList(),
                    AttachmentFileNames = createdAttachments.Select(attachment => attachment.FileName).ToList(),
                    CreatedAt = DateTime.Now
                });

                foreach (var mentionId in selectedMentionIds)
                {
                    if (!mentionedUserIds.Contains(mentionId, StringComparer.OrdinalIgnoreCase))
                    {
                        mentionedUserIds.Add(mentionId);
                    }
                }

                AddTeamNotification(team, $"Novo comentário em {entityTitle}.");
                if (selectedMentionIds.Count > 0)
                {
                    AddTeamNotification(team, $"{selectedMentionIds.Count} menção(ões) registradas em {entityTitle}.");
                }

                markEntityUpdated?.Invoke();
                SaveTeamWorkspace(team);
                dialog.DialogResult = true;
                dialog.Close();
                RenderTeamWorkspace();
            };

            var footer = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 18, 0, 0),
                Children = { cancelButton, saveButton }
            };
            Grid.SetRow(footer, 2);
            root.Children.Add(footer);

            dialog.Content = CreateStyledDialogShell(root);

            RenderSelectedMentions();
            RenderMentionSelector();
            RenderPendingAttachments();
            RenderExistingAttachments();
            RenderComments();
            RefreshComposerState();

            dialog.ShowDialog();
        }

        private Border CreateTeamCommentTimelineCard(TeamWorkspaceInfo team, TeamCommentInfo comment)
        {
            var card = new Border
            {
                Margin = new Thickness(0, 0, 0, 10),
                Padding = new Thickness(14),
                Background = GetThemeBrush("CardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(16)
            };

            var stack = new StackPanel();
            stack.Children.Add(new TextBlock
            {
                Text = string.IsNullOrWhiteSpace(comment.AuthorName) ? "Equipe" : comment.AuthorName,
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            });
            stack.Children.Add(new TextBlock
            {
                Text = comment.CreatedAt == default ? "Sem data" : comment.CreatedAt.ToString("dd/MM HH:mm"),
                Margin = new Thickness(0, 4, 0, 0),
                FontSize = 10,
                Foreground = GetThemeBrush("TertiaryTextBrush")
            });
            stack.Children.Add(new TextBlock
            {
                Text = string.IsNullOrWhiteSpace(comment.Content) ? "(Comentário publicado apenas com anexos)" : comment.Content,
                Margin = new Thickness(0, 10, 0, 0),
                FontSize = 11,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 18
            });

            var chips = new WrapPanel { Margin = new Thickness(0, 10, 0, 0) };
            foreach (var mentionId in comment.MentionedUserIds ?? new List<string>())
            {
                var mentionName = team.Members.FirstOrDefault(member => string.Equals(member.UserId, mentionId, StringComparison.OrdinalIgnoreCase))?.Name;
                chips.Children.Add(CreateStaticTeamChip($"@{(string.IsNullOrWhiteSpace(mentionName) ? mentionId : mentionName)}", GetThemeBrush("AccentMutedBrush"), GetThemeBrush("AccentBrush")));
            }
            foreach (var attachmentName in comment.AttachmentFileNames ?? new List<string>())
            {
                chips.Children.Add(CreateStaticTeamChip(attachmentName, GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            }

            if (chips.Children.Count > 0)
            {
                stack.Children.Add(chips);
            }

            card.Child = stack;
            return card;
        }

        private (TeamTaskCardInfo Card, string TargetColumnId)? CreateTaskDialog(TeamWorkspaceInfo team, TeamTaskColumnInfo? currentColumn, TeamTaskCardInfo? existingCard)
        {
            var dialog = CreateStyledDialogWindow(existingCard == null ? "Nova tarefa" : "Editar tarefa", 660, 820, 760);
            var accentBrush = GetThemeBrush("AccentBrush");
            var selectedPriority = existingCard?.Priority ?? "Media";
            var assignableMembers = GetTaskAssignableMembers(team);
            var assignableMemberIds = new HashSet<string>(assignableMembers.Select(member => member.UserId), StringComparer.OrdinalIgnoreCase);
            var selectedAssignedUserIds = new HashSet<string>((existingCard?.AssignedUserIds ?? new List<string>())
                .Where(assignableMemberIds.Contains), StringComparer.OrdinalIgnoreCase);

            var root = new Grid { Margin = new Thickness(6) };
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            root.Children.Add(CreateDialogHeader(
                "BOARD",
                existingCard == null ? "Montar nova tarefa do board" : "Refinar tarefa do board",
                existingCard == null
                    ? "Organize contexto, prioridade, etapa e responsáveis em um formulário mais editorial e coerente com o workspace."
                    : "Ajuste a tarefa sem perder consistência visual entre os modos Trello e Kanban.",
                accentBrush));

            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Margin = new Thickness(0, 4, 0, 0)
            };
            Grid.SetRow(scrollViewer, 1);
            root.Children.Add(scrollViewer);

            var form = new StackPanel();
            scrollViewer.Content = form;

            var titleBox = new TextBox
            {
                Text = existingCard?.Title ?? string.Empty,
                Height = 48,
                FontSize = 14,
                Margin = new Thickness(0, 8, 0, 0)
            };
            var descriptionBox = new TextBox
            {
                Text = existingCard?.Description ?? string.Empty,
                Height = 136,
                FontSize = 13,
                Padding = new Thickness(12),
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(0, 8, 0, 0)
            };
            var dueDatePicker = new DatePicker
            {
                SelectedDate = existingCard?.DueDate,
                Height = 48,
                Margin = new Thickness(0, 8, 0, 0)
            };
            var columnBox = new ComboBox
            {
                ItemsSource = team.TaskColumns,
                DisplayMemberPath = "Title",
                SelectedItem = currentColumn ?? team.TaskColumns.FirstOrDefault(),
                Height = 48,
                Margin = new Thickness(0, 8, 0, 0)
            };
            var estimatedHoursBox = new TextBox
            {
                Text = existingCard != null && existingCard.EstimatedHours > 0 ? existingCard.EstimatedHours.ToString(CultureInfo.InvariantCulture) : string.Empty,
                Height = 48,
                FontSize = 13,
                Margin = new Thickness(0, 8, 0, 0)
            };
            var workloadPointsBox = new TextBox
            {
                Text = existingCard != null && existingCard.WorkloadPoints > 0 ? existingCard.WorkloadPoints.ToString(CultureInfo.InvariantCulture) : string.Empty,
                Height = 48,
                FontSize = 13,
                Margin = new Thickness(0, 8, 0, 0)
            };
            var requiredRoleBox = new ComboBox
            {
                Height = 48,
                Margin = new Thickness(0, 8, 0, 0),
                DisplayMemberPath = "Label",
                SelectedValuePath = "Value",
                ItemsSource = new[]
                {
                    new { Label = "Aluno", Value = "student" },
                    new { Label = "Líder", Value = "leader" }
                },
                SelectedValue = TeamPermissionService.NormalizeExecutionRole(existingCard?.RequiredRole)
            };
            var professorReviewCheckBox = new CheckBox
            {
                Content = "Exigir revisão do professor orientador",
                IsChecked = existingCard?.RequiresProfessorReview == true,
                Margin = new Thickness(0, 12, 0, 0),
                Foreground = GetThemeBrush("PrimaryTextBrush"),
                FontWeight = FontWeights.SemiBold
            };

            ApplyDialogInputStyle(titleBox);
            ApplyDialogInputStyle(descriptionBox);
            ApplyDialogInputStyle(dueDatePicker);
            ApplyDialogInputStyle(columnBox);
            ApplyDialogInputStyle(estimatedHoursBox);
            ApplyDialogInputStyle(workloadPointsBox);
            ApplyDialogInputStyle(requiredRoleBox);

            var priorityPanel = new WrapPanel { Margin = new Thickness(0, 14, 0, 0) };
            var selectedAssigneesLabel = new TextBlock
            {
                FontSize = 11,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            };
            var selectedAssigneesWrap = new WrapPanel { Margin = new Thickness(0, 10, 0, 0) };
            var assigneePanel = new WrapPanel { Margin = new Thickness(0, 14, 0, 0) };

            void RenderPriorityChoices()
            {
                priorityPanel.Children.Clear();

                foreach (var option in new[]
                {
                    (Label: "Alta", Description: "Entrega crítica e urgente", Brush: (Brush)new SolidColorBrush(Color.FromRgb(220, 38, 38))),
                    (Label: "Media", Description: "Fluxo principal do sprint", Brush: (Brush)new SolidColorBrush(Color.FromRgb(245, 158, 11))),
                    (Label: "Baixa", Description: "Pode aguardar a próxima janela", Brush: (Brush)new SolidColorBrush(Color.FromRgb(16, 185, 129)))
                })
                {
                    var currentOption = option;
                    priorityPanel.Children.Add(CreateDialogChoiceCard(
                        currentOption.Label,
                        currentOption.Description,
                        currentOption.Brush,
                        string.Equals(selectedPriority, currentOption.Label, StringComparison.OrdinalIgnoreCase),
                        (s, e) =>
                        {
                            selectedPriority = currentOption.Label;
                            RenderPriorityChoices();
                        }));
                }
            }

            void RenderAssigneeSelector()
            {
                assigneePanel.Children.Clear();
                selectedAssigneesWrap.Children.Clear();

                var selectedMembers = assignableMembers
                    .Where(member => selectedAssignedUserIds.Contains(member.UserId))
                    .OrderBy(member => member.Name)
                    .ToList();

                if (selectedMembers.Count == 0)
                {
                    selectedAssigneesLabel.Text = assignableMembers.Count == 0
                        ? "Ainda não há alunos ou liderança discente disponíveis para receber esta tarefa."
                        : "Nenhum responsável selecionado. A tarefa será criada sem atribuição direta.";
                    selectedAssigneesWrap.Visibility = Visibility.Collapsed;
                }
                else
                {
                    selectedAssigneesLabel.Text = selectedMembers.Count == 1
                        ? "1 responsável já receberá atualização desta tarefa."
                        : $"{selectedMembers.Count} responsáveis receberão atualização desta tarefa.";
                    selectedAssigneesWrap.Visibility = Visibility.Visible;

                    foreach (var member in selectedMembers)
                    {
                        selectedAssigneesWrap.Children.Add(CreateAssigneeAvatarPill(member, compactMode: true));
                    }
                }

                if (assignableMembers.Count == 0)
                {
                    assigneePanel.Children.Add(CreateDialogHintCard(
                        "Professor orientador e coordenação não entram como executores do projeto. Adicione alunos ou defina uma liderança discente para atribuir esta tarefa.",
                        accentBrush,
                        new Thickness(0, 0, 0, 0)));
                    return;
                }

                foreach (var member in assignableMembers.OrderBy(item => item.Name))
                {
                    var currentMember = member;
                    assigneePanel.Children.Add(CreateDialogMemberChoiceCard(
                        currentMember,
                        selectedAssignedUserIds.Contains(currentMember.UserId),
                        (s, e) =>
                        {
                            if (!selectedAssignedUserIds.Add(currentMember.UserId))
                            {
                                selectedAssignedUserIds.Remove(currentMember.UserId);
                            }

                            RenderAssigneeSelector();
                        }));
                }
            }

            var summaryContent = new StackPanel();
            var summaryWrap = new WrapPanel();
            summaryWrap.Children.Add(CreateStaticTeamChip(team.TeamName, GetThemeBrush("AccentMutedBrush"), GetThemeBrush("AccentBrush")));
            summaryWrap.Children.Add(CreateStaticTeamChip($"{team.TaskColumns.Count} etapas", GetThemeBrush("MutedCardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            summaryWrap.Children.Add(CreateStaticTeamChip($"{assignableMembers.Count} executores disponíveis", GetThemeBrush("MutedCardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            summaryWrap.Children.Add(CreateStaticTeamChip(existingCard == null ? "Modo criacao" : "Modo edicao", GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            summaryContent.Children.Add(summaryWrap);
            summaryContent.Children.Add(new TextBlock
            {
                Text = "O resultado deste formulário alimenta os dois modos de visualização do board, então o conteúdo precisa continuar claro em listas compactas e em cards amplos.",
                Margin = new Thickness(0, 12, 0, 0),
                FontSize = 12,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20
            });
            form.Children.Add(CreateDialogSectionCard("Contexto do board", "Uma visão rápida antes de configurar a tarefa.", accentBrush, summaryContent));

            var identityContent = new StackPanel();
            identityContent.Children.Add(CreateDialogFieldLabel("Titulo da tarefa"));
            identityContent.Children.Add(titleBox);
            identityContent.Children.Add(CreateDialogFieldLabel("Descricao"));
            identityContent.Children.Add(descriptionBox);
            identityContent.Children.Add(CreateDialogHintCard(
                "Prefira um título objetivo e uma descrição curta com verbo de ação, entrega esperada e critério de pronto.",
                accentBrush));
            form.Children.Add(CreateDialogSectionCard(
                "Identidade da tarefa",
                "O card precisa continuar legível no Trello, no Kanban e no preview de arraste.",
                accentBrush,
                identityContent));

            var planningContent = new StackPanel();
            planningContent.Children.Add(CreateDialogFieldLabel("Prioridade"));
            planningContent.Children.Add(priorityPanel);

            var planningGrid = new Grid { Margin = new Thickness(0, 16, 0, 0) };
            planningGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            planningGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(16) });
            planningGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var dueDateStack = new StackPanel();
            dueDateStack.Children.Add(CreateDialogFieldLabel("Prazo"));
            dueDateStack.Children.Add(dueDatePicker);
            planningGrid.Children.Add(dueDateStack);

            var columnStack = new StackPanel();
            columnStack.Children.Add(CreateDialogFieldLabel("Etapa / coluna"));
            columnStack.Children.Add(columnBox);
            Grid.SetColumn(columnStack, 2);
            planningGrid.Children.Add(columnStack);

            planningContent.Children.Add(planningGrid);

            var workloadGrid = new Grid { Margin = new Thickness(0, 16, 0, 0) };
            workloadGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            workloadGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(16) });
            workloadGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var estimatedHoursStack = new StackPanel();
            estimatedHoursStack.Children.Add(CreateDialogFieldLabel("Horas estimadas"));
            estimatedHoursStack.Children.Add(estimatedHoursBox);
            workloadGrid.Children.Add(estimatedHoursStack);

            var workloadPointsStack = new StackPanel();
            workloadPointsStack.Children.Add(CreateDialogFieldLabel("Pontos de carga"));
            workloadPointsStack.Children.Add(workloadPointsBox);
            Grid.SetColumn(workloadPointsStack, 2);
            workloadGrid.Children.Add(workloadPointsStack);
            planningContent.Children.Add(workloadGrid);

            var roleGrid = new Grid { Margin = new Thickness(0, 16, 0, 0) };
            roleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            roleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(16) });
            roleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var roleStack = new StackPanel();
            roleStack.Children.Add(CreateDialogFieldLabel("Papel recomendado"));
            roleStack.Children.Add(requiredRoleBox);
            roleGrid.Children.Add(roleStack);

            var reviewStack = new StackPanel();
            reviewStack.Children.Add(CreateDialogFieldLabel("Validação"));
            reviewStack.Children.Add(professorReviewCheckBox);
            Grid.SetColumn(reviewStack, 2);
            roleGrid.Children.Add(reviewStack);
            planningContent.Children.Add(roleGrid);

            planningContent.Children.Add(CreateDialogHintCard(
                "Professor orientador revisa e guia o fluxo, mas a execução continua com alunos e liderança discente. Use a revisão docente para manter essa separação visível.",
                accentBrush));
            form.Children.Add(CreateDialogSectionCard(
                "Planejamento e fluxo",
                "Essas definições orientam o posicionamento do card dentro do board e do sprint.",
                accentBrush,
                planningContent));

            var assigneeContent = new StackPanel();
            assigneeContent.Children.Add(selectedAssigneesLabel);
            assigneeContent.Children.Add(selectedAssigneesWrap);
            assigneeContent.Children.Add(assigneePanel);
            form.Children.Add(CreateDialogSectionCard(
                "Responsaveis e notificacoes",
                "Selecione quem precisa receber contexto visual e rastreamento dessa tarefa.",
                accentBrush,
                assigneeContent,
                new Thickness(0, 0, 0, 8)));

            RenderPriorityChoices();
            RenderAssigneeSelector();

            var footer = new Border
            {
                Padding = new Thickness(0, 16, 0, 0),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(0, 1, 0, 0)
            };
            Grid.SetRow(footer, 2);
            root.Children.Add(footer);

            var footerButtons = new WrapPanel { HorizontalAlignment = HorizontalAlignment.Right };
            var cancelButton = CreateDialogActionButton("Cancelar", Brushes.Transparent, GetThemeBrush("PrimaryTextBrush"), GetThemeBrush("CardBorderBrush"), 110);
            var saveButton = CreateDialogActionButton(existingCard == null ? "Salvar tarefa" : "Salvar alteracoes", GetThemeBrush("AccentBrush"), Brushes.White, Brushes.Transparent, 162);
            footerButtons.Children.Add(cancelButton);
            footerButtons.Children.Add(saveButton);
            footer.Child = footerButtons;
            dialog.Content = CreateStyledDialogShell(root);

            TeamTaskCardInfo? resultCard = null;
            string? resultColumnId = null;

            cancelButton.Click += (s, e) => dialog.Close();
            saveButton.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(titleBox.Text) || columnBox.SelectedItem is not TeamTaskColumnInfo selectedColumn)
                {
                    ShowStyledAlertDialog("TAREFA", "Campos obrigatórios", "Preencha ao menos o título e a etapa da tarefa antes de confirmar.", "Continuar", accentBrush);
                    return;
                }

                resultCard = existingCard ?? new TeamTaskCardInfo();
                resultCard.Title = titleBox.Text.Trim();
                resultCard.Description = descriptionBox.Text.Trim();
                resultCard.Priority = selectedPriority;
                resultCard.DueDate = dueDatePicker.SelectedDate;
                resultCard.EstimatedHours = int.TryParse(estimatedHoursBox.Text, out var estimatedHours) ? Math.Max(0, estimatedHours) : 0;
                resultCard.WorkloadPoints = int.TryParse(workloadPointsBox.Text, out var workloadPoints) ? Math.Max(0, workloadPoints) : 0;
                resultCard.RequiredRole = TeamPermissionService.NormalizeExecutionRole(requiredRoleBox.SelectedValue?.ToString());
                resultCard.RequiresProfessorReview = professorReviewCheckBox.IsChecked == true;
                resultCard.AssignedUserIds = selectedAssignedUserIds.ToList();
                resultCard.ColumnId = selectedColumn.Id;
                resultCard.CreatedAt = resultCard.CreatedAt == default ? DateTime.Now : resultCard.CreatedAt;
                resultCard.CreatedByUserId = string.IsNullOrWhiteSpace(resultCard.CreatedByUserId) ? GetCurrentUserId() : resultCard.CreatedByUserId;
                resultCard.UpdatedAt = DateTime.Now;
                resultCard.UpdatedByUserId = GetCurrentUserId();
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
            var dialog = CreateStyledDialogWindow("Nova nota CSD", 620, 580, 540);
            var accentBrush = GetThemeBrush("AccentBrush");
            var selectedBucket = "Certezas";

            var root = new Grid { Margin = new Thickness(6) };
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            root.Children.Add(CreateDialogHeader(
                "CSD",
                "Registrar nota na matriz do projeto",
                "Escolha o quadrante correto e escreva uma nota curta, acionável e clara para o time.",
                accentBrush));

            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Margin = new Thickness(0, 4, 0, 0)
            };
            Grid.SetRow(scrollViewer, 1);
            root.Children.Add(scrollViewer);

            var form = new StackPanel();
            scrollViewer.Content = form;

            var bucketPanel = new WrapPanel { Margin = new Thickness(0, 14, 0, 0) };
            var bucketStateText = new TextBlock
            {
                FontSize = 11,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            };
            var noteBox = new TextBox
            {
                Height = 220,
                FontSize = 13,
                Padding = new Thickness(12),
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(0, 8, 0, 0)
            };
            ApplyDialogInputStyle(noteBox);

            var noteMetaText = new TextBlock
            {
                Margin = new Thickness(0, 10, 0, 0),
                FontSize = 11,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            };
            var bucketHintText = new TextBlock
            {
                FontSize = 12,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20
            };
            var bucketHintCard = new Border
            {
                Margin = new Thickness(0, 14, 0, 0),
                Padding = new Thickness(14),
                CornerRadius = new CornerRadius(16),
                BorderThickness = new Thickness(1),
                Child = bucketHintText
            };

            string GetBucketDescription(string bucket)
            {
                return bucket switch
                {
                    "Suposicoes" => "Use para hipóteses, caminhos ainda não validados e ideias que dependem de confirmação.",
                    "Duvidas" => "Use para perguntas abertas, riscos, bloqueios ou incertezas que exigem investigação.",
                    _ => "Use para fatos já confirmados, decisões fechadas e aprendizados que o time já consolidou."
                };
            }

            void UpdateNoteMeta()
            {
                var noteLength = noteBox.Text.Trim().Length;
                noteMetaText.Text = noteLength == 0
                    ? "Comece com uma frase clara. O ideal é uma nota direta, sem parágrafo longo."
                    : $"{noteLength} caractere(s). Mantendo a nota objetiva, o card continua legível na matriz e no preview de arraste.";
            }

            void RenderBucketSelector()
            {
                bucketPanel.Children.Clear();

                foreach (var option in new[]
                {
                    (Label: "Certezas", Description: "Fato validado", Brush: (Brush)new SolidColorBrush(GetCsdBucketAccentColor("Certezas"))),
                    (Label: "Suposicoes", Description: "Hipotese em teste", Brush: (Brush)new SolidColorBrush(GetCsdBucketAccentColor("Suposicoes"))),
                    (Label: "Duvidas", Description: "Pergunta ou risco", Brush: (Brush)new SolidColorBrush(GetCsdBucketAccentColor("Duvidas")))
                })
                {
                    var currentOption = option;
                    bucketPanel.Children.Add(CreateDialogChoiceCard(
                        currentOption.Label,
                        currentOption.Description,
                        currentOption.Brush,
                        string.Equals(selectedBucket, currentOption.Label, StringComparison.OrdinalIgnoreCase),
                        (s, e) =>
                        {
                            selectedBucket = currentOption.Label;
                            RenderBucketSelector();
                        }));
                }

                var currentAccent = new SolidColorBrush(GetCsdBucketAccentColor(selectedBucket));
                bucketStateText.Text = $"Destino atual: {selectedBucket}. {GetBucketDescription(selectedBucket)}";
                bucketHintCard.Background = CreateSoftAccentBrush(currentAccent, 26);
                bucketHintCard.BorderBrush = currentAccent;
                bucketHintText.Text = GetBucketDescription(selectedBucket);
            }

            noteBox.TextChanged += (s, e) => UpdateNoteMeta();

            var overviewContent = new StackPanel();
            var overviewWrap = new WrapPanel();
            overviewWrap.Children.Add(CreateStaticTeamChip($"Certezas {team.CsdBoard.Certainties.Count}", CreateSoftAccentBrush(new SolidColorBrush(GetCsdBucketAccentColor("Certezas")), 26), new SolidColorBrush(GetCsdBucketAccentColor("Certezas"))));
            overviewWrap.Children.Add(CreateStaticTeamChip($"Suposicoes {team.CsdBoard.Assumptions.Count}", CreateSoftAccentBrush(new SolidColorBrush(GetCsdBucketAccentColor("Suposicoes")), 26), new SolidColorBrush(GetCsdBucketAccentColor("Suposicoes"))));
            overviewWrap.Children.Add(CreateStaticTeamChip($"Duvidas {team.CsdBoard.Doubts.Count}", CreateSoftAccentBrush(new SolidColorBrush(GetCsdBucketAccentColor("Duvidas")), 26), new SolidColorBrush(GetCsdBucketAccentColor("Duvidas"))));
            overviewContent.Children.Add(overviewWrap);
            overviewContent.Children.Add(new TextBlock
            {
                Text = "A matriz CSD funciona melhor quando cada nota entra no quadrante certo logo no início. Isso reduz retrabalho e melhora a leitura do grupo.",
                Margin = new Thickness(0, 12, 0, 0),
                FontSize = 12,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20
            });
            form.Children.Add(CreateDialogSectionCard("Panorama atual", "Veja o volume das notas antes de registrar uma nova entrada.", accentBrush, overviewContent));

            var bucketContent = new StackPanel();
            bucketContent.Children.Add(bucketStateText);
            bucketContent.Children.Add(bucketPanel);
            form.Children.Add(CreateDialogSectionCard(
                "Destino da nota",
                "Cada quadrante responde a um tipo diferente de percepção do projeto.",
                accentBrush,
                bucketContent));

            var noteContent = new StackPanel();
            noteContent.Children.Add(CreateDialogFieldLabel("Conteudo da nota"));
            noteContent.Children.Add(noteBox);
            noteContent.Children.Add(noteMetaText);
            noteContent.Children.Add(bucketHintCard);
            form.Children.Add(CreateDialogSectionCard(
                "Conteudo e contexto",
                "Escreva uma nota objetiva o suficiente para ser entendida rápido, mas clara o suficiente para orientar a ação do time.",
                accentBrush,
                noteContent,
                new Thickness(0, 0, 0, 8)));

            RenderBucketSelector();
            UpdateNoteMeta();

            var footer = new Border
            {
                Padding = new Thickness(0, 16, 0, 0),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(0, 1, 0, 0)
            };
            Grid.SetRow(footer, 2);
            var footerButtons = new WrapPanel { HorizontalAlignment = HorizontalAlignment.Right };
            var cancelButton = CreateDialogActionButton("Cancelar", Brushes.Transparent, GetThemeBrush("PrimaryTextBrush"), GetThemeBrush("CardBorderBrush"), 110);
            var addButton = CreateDialogActionButton("Adicionar a matriz", GetThemeBrush("AccentBrush"), Brushes.White, Brushes.Transparent, 170);
            footerButtons.Children.Add(cancelButton);
            footerButtons.Children.Add(addButton);
            footer.Child = footerButtons;
            root.Children.Add(footer);
            dialog.Content = CreateStyledDialogShell(root);

            cancelButton.Click += (s, e) => dialog.Close();

            addButton.Click += (s, e) =>
            {
                var note = noteBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(note))
                {
                    ShowStyledAlertDialog("CSD", "Nota vazia", "Digite a nota antes de confirmar a inclusão no quadro CSD.", "Continuar", accentBrush);
                    return;
                }

                switch (selectedBucket)
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

        private Border CreateDialogSectionCard(string title, string description, Brush accentBrush, UIElement content, Thickness? margin = null)
        {
            var stack = new StackPanel();
            stack.Children.Add(new Border
            {
                Width = 46,
                Height = 5,
                CornerRadius = new CornerRadius(999),
                Background = accentBrush,
                Margin = new Thickness(0, 0, 0, 12)
            });
            stack.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = GetThemeBrush("PrimaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            });

            if (!string.IsNullOrWhiteSpace(description))
            {
                stack.Children.Add(new TextBlock
                {
                    Text = description,
                    Margin = new Thickness(0, 8, 0, 0),
                    FontSize = 12,
                    Foreground = GetThemeBrush("SecondaryTextBrush"),
                    TextWrapping = TextWrapping.Wrap,
                    LineHeight = 20
                });
            }

            stack.Children.Add(new Border
            {
                Margin = new Thickness(0, 16, 0, 0),
                Child = content
            });

            return new Border
            {
                Margin = margin ?? new Thickness(0, 0, 0, 16),
                Padding = new Thickness(18),
                Background = GetThemeBrush("MutedCardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(22),
                Child = stack
            };
        }

        private Button CreateDialogChoiceCard(string label, string description, Brush accentBrush, bool isSelected, RoutedEventHandler onClick, double width = 174)
        {
            var contentGrid = new Grid();
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var textStack = new StackPanel();
            textStack.Children.Add(new TextBlock
            {
                Text = label,
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = isSelected ? accentBrush : GetThemeBrush("PrimaryTextBrush")
            });
            textStack.Children.Add(new TextBlock
            {
                Text = description,
                Margin = new Thickness(0, 5, 0, 0),
                FontSize = 11,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            });
            contentGrid.Children.Add(textStack);

            if (isSelected)
            {
                var stateBadge = new Border
                {
                    Width = 28,
                    Height = 28,
                    CornerRadius = new CornerRadius(14),
                    Background = accentBrush,
                    Child = new PackIconMaterial
                    {
                        Kind = PackIconMaterialKind.Check,
                        Width = 16,
                        Height = 16,
                        Foreground = Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                };
                Grid.SetColumn(stateBadge, 1);
                contentGrid.Children.Add(stateBadge);
            }

            var button = new Button
            {
                Width = width,
                MinHeight = 82,
                Margin = new Thickness(0, 0, 12, 12),
                Padding = new Thickness(14),
                Background = isSelected ? CreateSoftAccentBrush(accentBrush, 34) : GetThemeBrush("CardBackgroundBrush"),
                BorderBrush = isSelected ? accentBrush : GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(isSelected ? 2 : 1),
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                Cursor = Cursors.Hand,
                Content = contentGrid
            };
            button.Click += onClick;
            return button;
        }

        private Button CreateDialogMemberChoiceCard(UserInfo member, bool isSelected, RoutedEventHandler onClick)
        {
            var accentBrush = GetThemeBrush("AccentBrush");
            var subtitle = string.IsNullOrWhiteSpace(member.Email)
                ? (string.IsNullOrWhiteSpace(member.Course) ? "Membro da equipe" : member.Course)
                : member.Email;

            var contentGrid = new Grid();
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            contentGrid.Children.Add(new Border
            {
                Width = 36,
                Height = 36,
                CornerRadius = new CornerRadius(18),
                ClipToBounds = true,
                Child = CreateUserAvatarVisual(member, 36, true)
            });

            var textStack = new StackPanel
            {
                Margin = new Thickness(12, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            textStack.Children.Add(new TextBlock
            {
                Text = GetTeamMemberChipLabel(member),
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = GetThemeBrush("PrimaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            });
            textStack.Children.Add(new TextBlock
            {
                Text = subtitle,
                Margin = new Thickness(0, 4, 0, 0),
                FontSize = 10,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            });
            Grid.SetColumn(textStack, 1);
            contentGrid.Children.Add(textStack);

            if (isSelected)
            {
                var stateBadge = new Border
                {
                    Width = 30,
                    Height = 30,
                    CornerRadius = new CornerRadius(15),
                    Background = accentBrush,
                    Child = new PackIconMaterial
                    {
                        Kind = PackIconMaterialKind.Check,
                        Width = 16,
                        Height = 16,
                        Foreground = Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                };
                Grid.SetColumn(stateBadge, 2);
                contentGrid.Children.Add(stateBadge);
            }

            var button = new Button
            {
                Width = 262,
                MinHeight = 76,
                Margin = new Thickness(0, 0, 12, 12),
                Padding = new Thickness(12),
                Background = isSelected ? CreateSoftAccentBrush(accentBrush, 30) : GetThemeBrush("CardBackgroundBrush"),
                BorderBrush = isSelected ? accentBrush : GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(isSelected ? 2 : 1),
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Center,
                Cursor = Cursors.Hand,
                Content = contentGrid
            };
            button.Click += onClick;
            return button;
        }

        private Border CreateDialogHintCard(string text, Brush accentBrush, Thickness? margin = null)
        {
            return new Border
            {
                Margin = margin ?? new Thickness(0, 14, 0, 0),
                Padding = new Thickness(14),
                Background = CreateSoftAccentBrush(accentBrush, 24),
                BorderBrush = accentBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(16),
                Child = new TextBlock
                {
                    Text = text,
                    FontSize = 12,
                    Foreground = GetThemeBrush("SecondaryTextBrush"),
                    TextWrapping = TextWrapping.Wrap,
                    LineHeight = 20
                }
            };
        }

        private Brush CreateSoftAccentBrush(Brush accentBrush, byte alpha = 28)
        {
            if (accentBrush is SolidColorBrush solidBrush)
            {
                return new SolidColorBrush(Color.FromArgb(alpha, solidBrush.Color.R, solidBrush.Color.G, solidBrush.Color.B));
            }

            return GetThemeBrush("AccentMutedBrush");
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
            if (!CanCurrentUserAddMembers(team))
            {
                ShowStyledAlertDialog("EQUIPE", "Permissão insuficiente", "Seu papel atual não permite adicionar participantes nesta equipe.", "Fechar", GetThemeBrush("AccentBrush"));
                return;
            }

            var dialog = CreateStyledDialogWindow("Adicionar membro", 620, 580);

            var root = new Grid { Margin = new Thickness(20) };
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
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
                Text = "Busque por nome, matrícula ou email. Toda entrada nova chega como aluno; a liderança discente é definida depois pela docência focal.",
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
            ApplyDialogInputStyle(queryBox);
            root.Children.Add(queryBox);
            Grid.SetRow(queryBox, 2);

            var roleBox = new ComboBox
            {
                Height = 42,
                Margin = new Thickness(0, 0, 0, 12),
                SelectedIndex = 0,
                IsEnabled = false
            };
            ApplyDialogInputStyle(roleBox);
            roleBox.Items.Add(new ComboBoxItem { Content = "Aluno", Tag = "student" });
            root.Children.Add(roleBox);
            Grid.SetRow(roleBox, 3);

            var resultsList = new ListBox
            {
                Height = 300
            };
            ApplyDialogInputStyle(resultsList);
            resultsList.ItemTemplate = new DataTemplate(typeof(UserInfo))
            {
                VisualTree = CreateTeamMemberSearchItemTemplate()
            };
            root.Children.Add(resultsList);
            Grid.SetRow(resultsList, 4);

            var statusText = new TextBlock
            {
                Margin = new Thickness(0, 12, 0, 10),
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                Text = "Digite para buscar no Firebase."
            };
            root.Children.Add(statusText);
            Grid.SetRow(statusText, 5);

            var footer = new DockPanel { LastChildFill = false, Margin = new Thickness(0, 8, 0, 0) };
            var cancelButton = CreateDialogActionButton("Cancelar", Brushes.Transparent, GetThemeBrush("PrimaryTextBrush"), GetThemeBrush("CardBorderBrush"), 110);
            var addButton = CreateDialogActionButton("Adicionar", GetThemeBrush("AccentBrush"), Brushes.White, Brushes.Transparent, 120);
            DockPanel.SetDock(cancelButton, Dock.Right);
            DockPanel.SetDock(addButton, Dock.Right);
            footer.Children.Add(addButton);
            footer.Children.Add(cancelButton);
            dialog.Content = CreateStyledDialogShell(root);

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
                statusText.Text = "Buscando participantes...";
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
                    ? "Nenhum participante disponível para adicionar com esse critério."
                    : $"{filteredResults.Count} participante(s) disponível(is). Selecione um para confirmar.";
            }

            queryBox.TextChanged += async (s, e) => await RefreshSearchResultsAsync();
            cancelButton.Click += (s, e) => dialog.Close();

            addButton.Click += (s, e) =>
            {
                if (resultsList.SelectedItem is not UserInfo selected)
                {
                    statusText.Text = "Selecione um participante da lista.";
                    return;
                }

                if (team.Members.Any(member => string.Equals(member.UserId, selected.UserId, StringComparison.OrdinalIgnoreCase)))
                {
                    statusText.Text = "Esse participante já faz parte da equipe.";
                    return;
                }

                var selectedRole = roleBox.SelectedItem is ComboBoxItem roleItem && roleItem.Tag is string roleTag
                    ? roleTag
                    : "student";
                var draftMember = CloneUserInfo(selected, selectedRole);
                team.Members.Add(draftMember);
                AddTeamNotification(team, $"{draftMember.Name} entrou na equipe como {TeamPermissionService.GetRoleLabel(draftMember.Role).ToLowerInvariant()}.");
                SaveTeamWorkspace(team);
                dialog.DialogResult = true;
                dialog.Close();
            };

            root.Children.Add(footer);
            Grid.SetRow(footer, 6);

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
            if (!CanCurrentUserManageMembers(team))
            {
                ShowStyledAlertDialog("EQUIPE", "Permissão insuficiente", "Seu papel atual não permite remover participantes desta equipe.", "Fechar", GetThemeBrush("AccentBrush"));
                return;
            }

            var removableMembers = team.Members.Where(member => !string.Equals(member.UserId, _currentProfile?.UserId, StringComparison.OrdinalIgnoreCase)).ToList();
            if (removableMembers.Count == 0)
            {
                ShowStyledAlertDialog("EQUIPE", "Nada para remover", "Não há membros disponíveis para remoção nesta equipe.", "Fechar", GetThemeBrush("AccentBrush"));
                return;
            }

            var dialog = CreateStyledDialogWindow("Remover membro", 560, 500);

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
            ApplyDialogInputStyle(filterBox);
            root.Children.Add(filterBox);
            Grid.SetRow(filterBox, 1);

            var list = new ListBox
            {
                Height = 280,
                ItemsSource = removableMembers,
                DisplayMemberPath = "DisplayLabel"
            };
            ApplyDialogInputStyle(list);
            root.Children.Add(list);
            Grid.SetRow(list, 2);

            var footer = new DockPanel { LastChildFill = false, Margin = new Thickness(0, 14, 0, 0) };
            var cancelButton = CreateDialogActionButton("Cancelar", Brushes.Transparent, GetThemeBrush("PrimaryTextBrush"), GetThemeBrush("CardBorderBrush"), 110);
            var removeButton = CreateDialogActionButton("Remover selecionado", new SolidColorBrush(Color.FromRgb(220, 38, 38)), Brushes.White, Brushes.Transparent, 170);
            DockPanel.SetDock(cancelButton, Dock.Right);
            DockPanel.SetDock(removeButton, Dock.Right);
            footer.Children.Add(removeButton);
            footer.Children.Add(cancelButton);
            dialog.Content = CreateStyledDialogShell(root);

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

        private void OpenAssignTeamLeaderDialog(TeamWorkspaceInfo team)
        {
            if (!CanCurrentUserAssignLeadership(team))
            {
                ShowStyledAlertDialog("EQUIPE", "Permissão insuficiente", "Somente a docência focal pode definir a liderança discente desta equipe.", "Fechar", GetThemeBrush("AccentBrush"));
                return;
            }

            var candidates = GetStudentTeamMembers(team);
            if (candidates.Count == 0)
            {
                ShowStyledAlertDialog("EQUIPE", "Sem candidatos", "Adicione pelo menos um aluno à equipe antes de nomear uma liderança discente.", "Fechar", GetThemeBrush("AccentBrush"));
                return;
            }

            var dialog = CreateStyledDialogWindow("Definir liderança discente", 560, 520);
            var currentLeader = GetStudentLeaders(team).FirstOrDefault();

            var root = new Grid { Margin = new Thickness(20) };
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            root.Children.Add(new TextBlock
            {
                Text = "Nomear liderança estudantil",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            });

            var helperText = new TextBlock
            {
                Text = currentLeader == null
                    ? "Escolha o aluno que vai coordenar o ritmo do projeto. A docência continua separada da execução e das entregas do grupo."
                    : $"Liderança atual: {currentLeader.Name}. Selecione outro aluno para transferir a coordenação operacional da equipe.",
                Margin = new Thickness(0, 8, 0, 12),
                TextWrapping = TextWrapping.Wrap,
                FontSize = 12,
                Foreground = GetThemeBrush("SecondaryTextBrush")
            };
            root.Children.Add(helperText);
            Grid.SetRow(helperText, 1);

            var list = new ListBox
            {
                ItemsSource = candidates,
                SelectedItem = currentLeader,
                ItemTemplate = new DataTemplate(typeof(UserInfo))
                {
                    VisualTree = CreateTeamMemberSearchItemTemplate()
                }
            };
            ApplyDialogInputStyle(list);
            root.Children.Add(list);
            Grid.SetRow(list, 2);

            var footer = new DockPanel { LastChildFill = false, Margin = new Thickness(0, 14, 0, 0) };
            var cancelButton = CreateDialogActionButton("Cancelar", Brushes.Transparent, GetThemeBrush("PrimaryTextBrush"), GetThemeBrush("CardBorderBrush"), 110);
            var saveButton = CreateDialogActionButton("Aplicar liderança", GetThemeBrush("AccentBrush"), Brushes.White, Brushes.Transparent, 160);
            DockPanel.SetDock(cancelButton, Dock.Right);
            DockPanel.SetDock(saveButton, Dock.Right);
            footer.Children.Add(saveButton);
            footer.Children.Add(cancelButton);
            root.Children.Add(footer);
            Grid.SetRow(footer, 3);

            dialog.Content = CreateStyledDialogShell(root);

            cancelButton.Click += (s, e) => dialog.Close();
            saveButton.Click += (s, e) =>
            {
                if (list.SelectedItem is not UserInfo selectedLeader)
                {
                    helperText.Text = "Selecione um aluno antes de aplicar a liderança discente.";
                    helperText.Foreground = new SolidColorBrush(Color.FromRgb(220, 38, 38));
                    return;
                }

                var changed = false;
                foreach (var member in team.Members)
                {
                    if (TeamPermissionService.IsFacultyRole(member.Role))
                    {
                        continue;
                    }

                    var targetRole = string.Equals(member.UserId, selectedLeader.UserId, StringComparison.OrdinalIgnoreCase)
                        ? "leader"
                        : "student";

                    if (!string.Equals(TeamPermissionService.NormalizeRole(member.Role), targetRole, StringComparison.OrdinalIgnoreCase))
                    {
                        member.Role = targetRole;
                        changed = true;
                    }
                }

                if (!changed)
                {
                    helperText.Text = $"{selectedLeader.Name} já está como liderança discente desta equipe.";
                    helperText.Foreground = GetThemeBrush("SecondaryTextBrush");
                    return;
                }

                AddTeamNotification(team, $"{selectedLeader.Name} foi definido(a) como liderança discente da equipe.");
                SaveTeamWorkspace(team);
                dialog.DialogResult = true;
                dialog.Close();
            };

            if (dialog.ShowDialog() == true)
            {
                RenderTeamWorkspace();
            }
        }

        private void RemoveMemberFromActiveTeam(UserInfo member)
        {
            if (_activeTeamWorkspace == null)
            {
                return;
            }

            if (!CanCurrentUserManageMembers(_activeTeamWorkspace))
            {
                ShowStyledAlertDialog("EQUIPE", "Permissão insuficiente", "Seu papel atual não permite remover participantes desta equipe.", "Fechar", GetThemeBrush("AccentBrush"));
                return;
            }

            if (!ShowStyledConfirmationDialog("EQUIPE", "Remover integrante", $"{member.Name} será removido da equipe e também das atribuições das tarefas atuais.", "Remover", new SolidColorBrush(Color.FromRgb(220, 38, 38))))
            {
                return;
            }

            _activeTeamWorkspace.Members.RemoveAll(item => string.Equals(item.UserId, member.UserId, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(_activeTeamWorkspace.FocalProfessorUserId) &&
                string.Equals(_activeTeamWorkspace.FocalProfessorUserId, member.UserId, StringComparison.OrdinalIgnoreCase))
            {
                _activeTeamWorkspace.FocalProfessorUserId = string.Empty;
                _activeTeamWorkspace.FocalProfessorName = string.Empty;
            }

            _activeTeamWorkspace.ProfessorSupervisorUserIds?.RemoveAll(id => string.Equals(id, member.UserId, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(member.Name))
            {
                _activeTeamWorkspace.ProfessorSupervisorNames?.RemoveAll(name => string.Equals(name, member.Name, StringComparison.OrdinalIgnoreCase));
            }

            foreach (var column in _activeTeamWorkspace.TaskColumns)
            {
                foreach (var card in column.Cards)
                {
                    var removedAssignments = card.AssignedUserIds.RemoveAll(id => string.Equals(id, member.UserId, StringComparison.OrdinalIgnoreCase));
                    if (removedAssignments > 0)
                    {
                        card.UpdatedAt = DateTime.Now;
                        card.UpdatedByUserId = GetCurrentUserId();
                    }
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

            if (!CanCurrentUserDeleteTeam(_activeTeamWorkspace))
            {
                ShowStyledAlertDialog("EQUIPE", "Permissão insuficiente", "Seu papel atual não permite apagar esta equipe.", "Entendi", GetThemeBrush("AccentBrush"));
                return;
            }

            if (!ShowStyledConfirmationDialog("EQUIPE", "Apagar equipe", $"A equipe {_activeTeamWorkspace.TeamName} será removida do seu painel e do Firebase.", "Apagar equipe", new SolidColorBrush(Color.FromRgb(220, 38, 38))))
            {
                return;
            }

            if (_teamService != null)
            {
                var deleteResult = await _teamService.DeleteTeamAsync(_activeTeamWorkspace);
                if (!deleteResult.Success)
                {
                    ShowStyledAlertDialog("EQUIPE", "Falha ao apagar equipe", $"Não foi possível apagar a equipe.\n\n{deleteResult.ErrorMessage}", "Fechar", new SolidColorBrush(Color.FromRgb(220, 38, 38)));
                    return;
                }
            }

            _teamWorkspaces.RemoveAll(team => string.Equals(team.TeamId, _activeTeamWorkspace.TeamId, StringComparison.OrdinalIgnoreCase));
            _activeTeamWorkspace = null;
            TeamWorkspaceHost.Content = null;
            RenderTeamsList();
            UpdateTeamsViewState();
        }

        private async void AddTeamAsset(string category)
        {
            if (_activeTeamWorkspace == null)
            {
                return;
            }

            if (string.Equals(category, "logo", StringComparison.OrdinalIgnoreCase))
            {
                var logoAsset = CreateDraftTeamLogoAsset();
                if (logoAsset == null)
                {
                    return;
                }

                var hadLogo = GetTeamLogoAsset(_activeTeamWorkspace) != null;
                ReplaceTeamLogoAsset(_activeTeamWorkspace, logoAsset);
                AddTeamNotification(_activeTeamWorkspace, hadLogo ? "Logo do projeto atualizado." : "Logo do projeto adicionado.");
                SaveTeamWorkspace(_activeTeamWorkspace);
                RenderTeamWorkspace();
                return;
            }

            if (!CanCurrentUserUploadFiles(_activeTeamWorkspace))
            {
                ShowStyledAlertDialog("ARQUIVOS", "Permissão insuficiente", "Seu papel atual não permite publicar materiais nesta equipe.", "Fechar", GetThemeBrush("AccentBrush"));
                return;
            }

            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                Title = $"Selecionar {category}",
                Filter = string.Equals(category, "imagens", StringComparison.OrdinalIgnoreCase)
                    ? "Imagens|*.png;*.jpg;*.jpeg;*.bmp;*.webp|Todos os arquivos|*.*"
                    : "Todos os arquivos|*.*"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            var options = ShowTeamAssetSyncOptionsDialog(
                _activeTeamWorkspace,
                $"Publicar {category}",
                "ARQUIVOS",
                "Defina o escopo do conteúdo remoto e registre um resumo para a primeira versão destes materiais.",
                _activeTeamWorkspace.DefaultFilePermissionScope,
                $"Versão inicial publicada em {category}.");
            if (!options.Confirmed)
            {
                return;
            }

            var importedCount = 0;
            var failures = new List<string>();
            foreach (var file in dialog.FileNames)
            {
                var assetResult = await CreateOrUpdateTeamAssetAsync(
                    _activeTeamWorkspace,
                    file,
                    category,
                    options.PermissionScope,
                    options.ChangeSummary,
                    null,
                    $"Material publicado manualmente em {category}.");
                if (!assetResult.Success || assetResult.Asset == null)
                {
                    failures.Add($"{IOPath.GetFileName(file)}: {assetResult.ErrorMessage}");
                    continue;
                }

                _activeTeamWorkspace.Assets.Add(assetResult.Asset);
                importedCount++;
            }

            if (importedCount > 0)
            {
                AddTeamNotification(_activeTeamWorkspace, $"{importedCount} arquivo(s) adicionado(s) em {category}.");
                SaveTeamWorkspace(_activeTeamWorkspace);
                RenderTeamWorkspace();
            }

            if (failures.Count > 0)
            {
                ShowStyledAlertDialog(
                    "ARQUIVOS",
                    "Alguns arquivos falharam",
                    string.Join("\n", failures.Take(3)) + (failures.Count > 3 ? "\n..." : string.Empty),
                    "Fechar",
                    new SolidColorBrush(Color.FromRgb(234, 88, 12)));
            }
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
            ShowStyledAlertDialog("EQUIPE", "Código copiado", $"Código da equipe copiado com sucesso: {team.TeamId}", "Fechar", GetThemeBrush("AccentBrush"));
        }

        private void OpenAddMilestoneDialog(TeamWorkspaceInfo team)
        {
            if (!CanCurrentUserEditProjectSettings(team))
            {
                ShowStyledAlertDialog("EQUIPE", "Permissão insuficiente", "Seu papel atual pode colaborar nas entregas, mas não criar novos marcos nesta equipe.", "Fechar", GetThemeBrush("AccentBrush"));
                return;
            }

            var dialog = CreateStyledDialogWindow("Nova entrega", 500, 420);

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
            ApplyDialogInputStyle(titleBox);
            ApplyDialogInputStyle(notesBox);
            ApplyDialogInputStyle(statusBox);
            ApplyDialogInputStyle(dueDatePicker);

            root.Children.Add(CreateDialogFieldLabel("Título da entrega"));
            root.Children.Add(titleBox);
            root.Children.Add(CreateDialogFieldLabel("Notas"));
            root.Children.Add(notesBox);
            root.Children.Add(CreateDialogFieldLabel("Status"));
            root.Children.Add(statusBox);
            root.Children.Add(CreateDialogFieldLabel("Prazo"));
            root.Children.Add(dueDatePicker);

            var footer = new WrapPanel { HorizontalAlignment = HorizontalAlignment.Right };
            var cancelButton = CreateDialogActionButton("Cancelar", Brushes.Transparent, GetThemeBrush("PrimaryTextBrush"), GetThemeBrush("CardBorderBrush"), 110);
            var saveButton = CreateDialogActionButton("Salvar entrega", GetThemeBrush("AccentBrush"), Brushes.White, Brushes.Transparent, 140);
            footer.Children.Add(cancelButton);
            footer.Children.Add(saveButton);
            root.Children.Add(footer);
            dialog.Content = CreateStyledDialogShell(root);

            cancelButton.Click += (s, e) => dialog.Close();
            saveButton.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(titleBox.Text))
                {
                    ShowStyledAlertDialog("EQUIPE", "Título obrigatório", "Informe um título para a entrega antes de continuar.", "Continuar", GetThemeBrush("AccentBrush"));
                    return;
                }

                team.Milestones.Add(new TeamMilestoneInfo
                {
                    Title = titleBox.Text.Trim(),
                    Notes = notesBox.Text.Trim(),
                    Status = statusBox.SelectedItem?.ToString() ?? "Planejada",
                    DueDate = dueDatePicker.SelectedDate,
                    CreatedByUserId = GetCurrentUserId(),
                    OwnerUserId = GetCurrentUserId(),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    UpdatedByUserId = GetCurrentUserId()
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

            if (!CanCurrentUserReviewDeliverables(team))
            {
                ShowStyledAlertDialog("ENTREGA", "Permissão insuficiente", "Seu papel atual não permite concluir ou reabrir entregas desta equipe.", "Fechar", GetThemeBrush("AccentBrush"));
                return;
            }

            milestone.Status = string.Equals(milestone.Status, "Concluida", StringComparison.OrdinalIgnoreCase)
                ? "Em andamento"
                : "Concluida";
            milestone.UpdatedAt = DateTime.Now;
            milestone.UpdatedByUserId = GetCurrentUserId();
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
            if (!CanCurrentUserEditProjectSettings(team))
            {
                ShowStyledAlertDialog("ENTREGA", "Permissão insuficiente", "Seu papel atual não permite remover entregas desta equipe.", "Fechar", GetThemeBrush("AccentBrush"));
                return;
            }

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
            var dialog = CreateStyledDialogWindow("Gestão do projeto", 560, 540, 500);
            var accentBrush = GetThemeBrush("AccentBrush");

            string GetProgressNarrative(int progress)
            {
                return progress switch
                {
                    <= 15 => "Exploração e estruturação inicial do projeto.",
                    <= 40 => "Execução base em andamento, com entregas começando a ganhar ritmo.",
                    <= 70 => "Projeto em fase forte de produção e consolidação das entregas.",
                    <= 90 => "Acabamento, revisão e ajustes finais dominando o fluxo.",
                    _ => "Entrega praticamente concluída, com foco em fechamento e validação final."
                };
            }

            var root = new Grid { Margin = new Thickness(6) };
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            root.Children.Add(CreateDialogHeader(
                "GESTAO",
                "Atualizar progresso, status e prazo",
                "Use um layout mais orientado a leitura para calibrar o andamento geral do projeto sem sair da linguagem visual do workspace.",
                accentBrush));

            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Margin = new Thickness(0, 4, 0, 0)
            };
            Grid.SetRow(scrollViewer, 1);
            root.Children.Add(scrollViewer);

            var form = new StackPanel();
            scrollViewer.Content = form;

            var progressValueText = new TextBlock
            {
                FontSize = 34,
                FontWeight = FontWeights.ExtraBold,
                Foreground = accentBrush
            };
            var progressNarrativeText = new TextBlock
            {
                Margin = new Thickness(0, 6, 0, 0),
                FontSize = 12,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20
            };
            var progressSlider = new Slider
            {
                Minimum = 0,
                Maximum = 100,
                TickFrequency = 5,
                IsSnapToTickEnabled = true,
                IsMoveToPointEnabled = true,
                Value = team.ProjectProgress,
                Margin = new Thickness(0, 16, 0, 8)
            };
            var progressMarkers = new UniformGrid
            {
                Columns = 5,
                Margin = new Thickness(0, 2, 0, 0)
            };
            foreach (var marker in new[] { "0", "25", "50", "75", "100" })
            {
                progressMarkers.Children.Add(new TextBlock
                {
                    Text = marker,
                    FontSize = 10,
                    Foreground = GetThemeBrush("TertiaryTextBrush"),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextAlignment = TextAlignment.Center
                });
            }

            void UpdateProgressState()
            {
                var value = (int)progressSlider.Value;
                progressValueText.Text = $"{value}%";
                progressNarrativeText.Text = GetProgressNarrative(value);
            }

            progressSlider.ValueChanged += (s, e) => UpdateProgressState();
            UpdateProgressState();

            var progressContent = new StackPanel();
            progressContent.Children.Add(new Border
            {
                Background = CreateSoftAccentBrush(accentBrush, 24),
                BorderBrush = accentBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(18),
                Padding = new Thickness(16),
                Child = new StackPanel
                {
                    Children =
                    {
                        new TextBlock
                        {
                            Text = "Leitura atual do projeto",
                            FontSize = 11,
                            FontWeight = FontWeights.Bold,
                            Foreground = GetThemeBrush("SecondaryTextBrush")
                        },
                        progressValueText,
                        progressNarrativeText
                    }
                }
            });
            progressContent.Children.Add(progressSlider);
            progressContent.Children.Add(progressMarkers);
            form.Children.Add(CreateDialogSectionCard(
                "Ritmo de entrega",
                "O slider agora fica ancorado em uma leitura visual maior do andamento do projeto.",
                accentBrush,
                progressContent));

            var statusBox = new ComboBox
            {
                ItemsSource = new[] { "Planejamento", "Em andamento", "Em revisao", "Concluido" },
                SelectedItem = team.ProjectStatus,
                Height = 48,
                Margin = new Thickness(0, 8, 0, 0)
            };
            var deadlinePicker = new DatePicker
            {
                SelectedDate = team.ProjectDeadline,
                Height = 48,
                Margin = new Thickness(0, 8, 0, 0)
            };
            ApplyDialogInputStyle(statusBox);
            ApplyDialogInputStyle(deadlinePicker);

            var planningContent = new StackPanel();
            var planningGrid = new Grid();
            planningGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            planningGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(16) });
            planningGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var statusStack = new StackPanel();
            statusStack.Children.Add(CreateDialogFieldLabel("Status do projeto"));
            statusStack.Children.Add(statusBox);
            planningGrid.Children.Add(statusStack);

            var deadlineStack = new StackPanel();
            deadlineStack.Children.Add(CreateDialogFieldLabel("Prazo principal"));
            deadlineStack.Children.Add(deadlinePicker);
            Grid.SetColumn(deadlineStack, 2);
            planningGrid.Children.Add(deadlineStack);

            planningContent.Children.Add(planningGrid);
            planningContent.Children.Add(CreateDialogHintCard(
                "Use o status para orientar a comunicação do time e o prazo principal para ancorar a leitura executiva do workspace.",
                accentBrush));
            form.Children.Add(CreateDialogSectionCard(
                "Sinalização do projeto",
                "Esses campos ajudam a deixar o quadro mais legível para toda a equipe.",
                accentBrush,
                planningContent,
                new Thickness(0, 0, 0, 8)));

            var footer = new Border
            {
                Padding = new Thickness(0, 16, 0, 0),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(0, 1, 0, 0)
            };
            Grid.SetRow(footer, 2);
            var footerButtons = new WrapPanel { HorizontalAlignment = HorizontalAlignment.Right };
            var cancelButton = CreateDialogActionButton("Cancelar", Brushes.Transparent, GetThemeBrush("PrimaryTextBrush"), GetThemeBrush("CardBorderBrush"), 110);
            var saveButton = CreateDialogActionButton("Salvar gestão", GetThemeBrush("AccentBrush"), Brushes.White, Brushes.Transparent, 150);
            footerButtons.Children.Add(cancelButton);
            footerButtons.Children.Add(saveButton);
            footer.Child = footerButtons;
            root.Children.Add(footer);
            dialog.Content = CreateStyledDialogShell(root);

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

                await EnrichUsersWithAcademicPortfolioAsync(results);
                if (requestVersion != _searchSlideRequestVersion)
                {
                    return;
                }

                _searchSlideTeamResults = await LoadSearchSlideTeamMatchesAsync(query);
                if (requestVersion != _searchSlideRequestVersion)
                {
                    return;
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
            _searchSlideTeamResults.Clear();
            SearchSlideTitleText.Text = $"Resultados para \"{query}\"";
            SearchSlideStatusText.Text = triggeredByTyping
                ? "Atualizando resultados enquanto você digita..."
                : "Buscando em perfis, equipes, chats e arquivos do ambiente acadêmico...";
            SearchSlideStatusText.Foreground = GetThemeBrush("SecondaryTextBrush");
            SearchSlideResultsHost.Children.Clear();
            SearchSlideEmptyStateText.Visibility = Visibility.Collapsed;
            SearchSlideResultsHost.Children.Add(CreateSearchSlideInfoCard("Buscando agora", "Assim que os perfis, equipes, chats e materiais forem encontrados, eles aparecem aqui sem abrir uma nova janela."));
            ShowSearchSlidePanel();
        }

        private void RenderSearchSlideResults(List<UserInfo> results)
        {
            _searchSlideResults = results;
            SearchSlideResultsHost.Children.Clear();

            var teamMatches = FindSearchSlideTeams(_searchSlideQuery);
            var conversationMatches = FindSearchSlideConversations(_searchSlideQuery);
            var fileMatches = FindSearchSlideFiles(_searchSlideQuery);
            var totalMatches = results.Count + teamMatches.Count + conversationMatches.Count + fileMatches.Count;

            if (totalMatches == 0)
            {
                SearchSlideStatusText.Text = string.IsNullOrWhiteSpace(_searchSlideQuery)
                    ? "Nenhuma pesquisa ativa."
                    : $"Nenhum resultado encontrado para \"{_searchSlideQuery}\".";
                SearchSlideStatusText.Foreground = GetThemeBrush("SecondaryTextBrush");
                SearchSlideEmptyStateText.Text = "Tente outro nome, matrícula, email, equipe, mensagem ou arquivo para localizar o contexto certo.";
                SearchSlideEmptyStateText.Visibility = Visibility.Visible;
                return;
            }

            SearchSlideEmptyStateText.Visibility = Visibility.Collapsed;
            SearchSlideStatusText.Text = totalMatches == 1
                ? "1 resultado encontrado. Você já pode abrir o contexto certo daqui."
                : $"{totalMatches} resultados encontrados entre perfis, equipes, chats e arquivos.";
            SearchSlideStatusText.Foreground = GetThemeBrush("SecondaryTextBrush");

            if (teamMatches.Count > 0)
            {
                SearchSlideResultsHost.Children.Add(CreateSearchSlideInfoCard("Equipes", "Abra o workspace certo, acompanhe o semestre e pule para o board ou agenda."));
                foreach (var team in teamMatches)
                {
                    SearchSlideResultsHost.Children.Add(CreateSearchSlideTeamResultCard(team));
                }
            }

            if (conversationMatches.Count > 0)
            {
                SearchSlideResultsHost.Children.Add(CreateSearchSlideInfoCard("Chats", "Retome conversas e contexto recente sem sair da busca global."));
                foreach (var conversation in conversationMatches)
                {
                    SearchSlideResultsHost.Children.Add(CreateSearchSlideConversationResultCard(conversation));
                }
            }

            if (fileMatches.Count > 0)
            {
                SearchSlideResultsHost.Children.Add(CreateSearchSlideInfoCard("Arquivos", "A busca também passa pelo hub local de materiais e documentos recentes."));
                foreach (var file in fileMatches)
                {
                    SearchSlideResultsHost.Children.Add(CreateSearchSlideFileResultCard(file));
                }
            }

            if (results.Count > 0)
            {
                SearchSlideResultsHost.Children.Add(CreateSearchSlideInfoCard(
                    "Perfis",
                    CurrentViewerCanUseProfessorDiscovery()
                        ? "Abra o perfil acadêmico, veja equipes do aluno e assuma rapidamente a supervisão focal quando necessário."
                        : "Converse, conecte ou abra os detalhes do participante encontrado."));
            }

            foreach (var user in results)
            {
                SearchSlideResultsHost.Children.Add(CreateSearchSlideResultCard(user));
            }
        }

        private List<TeamWorkspaceInfo> FindSearchSlideTeams(string query)
        {
            var normalizedQuery = NormalizeTeamValue(query);
            if (string.IsNullOrWhiteSpace(normalizedQuery))
            {
                return new List<TeamWorkspaceInfo>();
            }

            return (_searchSlideTeamResults.Count > 0 ? _searchSlideTeamResults : GetLocalTeamSearchMatches(query))
                .GroupBy(team => team.TeamId, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .OrderByDescending(team => team.UpdatedAt)
                .ThenBy(team => team.TeamName)
                .Take(8)
                .ToList();
        }

        private List<Conversation> FindSearchSlideConversations(string query)
        {
            var normalizedQuery = NormalizeTeamValue(query);
            if (string.IsNullOrWhiteSpace(normalizedQuery))
            {
                return new List<Conversation>();
            }

            return _conversations
                .Where(conversation =>
                    conversation.ContactName.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)
                    || (!string.IsNullOrWhiteSpace(conversation.LastMessage) && conversation.LastMessage.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(conversation => conversation.LastMessageTime)
                .Take(4)
                .ToList();
        }

        private List<FilesHubItem> FindSearchSlideFiles(string query)
        {
            var normalizedQuery = NormalizeTeamValue(query);
            if (string.IsNullOrWhiteSpace(normalizedQuery))
            {
                return new List<FilesHubItem>();
            }

            EnsureFilesHubStateLoaded();
            return (_filesHubState.Items ?? new List<FilesHubItem>())
                .Where(item =>
                    item.FileName.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)
                    || item.AssociationType.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)
                    || item.AssociationLabel.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(item => item.AddedAt)
                .Take(4)
                .ToList();
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
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var avatar = new Border
            {
                Width = 52,
                Height = 52,
                CornerRadius = new CornerRadius(26),
                ClipToBounds = true,
                Margin = new Thickness(0, 0, 14, 0),
                Child = CreateUserAvatarVisual(user, 52, true)
            };
            Grid.SetRowSpan(avatar, 2);
            layout.Children.Add(avatar);

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

            var roleLine = BuildSearchSlideUserRoleLine(user);
            if (!string.IsNullOrWhiteSpace(roleLine))
            {
                var roleBlock = CreateHighlightedSearchTextBlock(roleLine, GetThemeBrush("SecondaryTextBrush"), 11, FontWeights.SemiBold);
                roleBlock.Margin = new Thickness(0, 4, 0, 0);
                info.Children.Add(roleBlock);
            }

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

            Grid.SetRowSpan(actions, 2);
            Grid.SetColumn(actions, 2);
            layout.Children.Add(actions);

            if (CurrentViewerCanUseProfessorDiscovery())
            {
                var portfolioPanel = CreateSearchSlideUserPortfolioPanel(user);
                Grid.SetColumn(portfolioPanel, 1);
                Grid.SetRow(portfolioPanel, 1);
                layout.Children.Add(portfolioPanel);
            }

            card.Child = layout;
            return card;
        }

        private string BuildSearchSlideUserRoleLine(UserInfo user)
        {
            var parts = new List<string>();
            var roleLabel = TeamPermissionService.GetRoleLabel(user.Role);
            if (!string.IsNullOrWhiteSpace(roleLabel))
            {
                parts.Add(roleLabel);
            }

            if (!string.IsNullOrWhiteSpace(user.ProfessionalTitle) &&
                !string.Equals(user.ProfessionalTitle, roleLabel, StringComparison.OrdinalIgnoreCase))
            {
                parts.Add(user.ProfessionalTitle);
            }

            if (!string.IsNullOrWhiteSpace(user.AcademicDepartment))
            {
                parts.Add(user.AcademicDepartment);
            }

            return string.Join(" • ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
        }

        private Border CreateSearchSlideUserPortfolioPanel(UserInfo user)
        {
            var panel = new Border
            {
                Margin = new Thickness(0, 12, 0, 0),
                Padding = new Thickness(14, 12, 14, 12),
                Background = GetThemeBrush("CardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(16)
            };

            var stack = new StackPanel();
            var teams = (user.AcademicProjects ?? new List<TeamWorkspaceInfo>())
                .Where(team => team != null)
                .GroupBy(team => team.TeamId, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .OrderByDescending(team => team.UpdatedAt)
                .ThenBy(team => team.TeamName)
                .ToList();

            stack.Children.Add(new TextBlock
            {
                Text = teams.Count == 0
                    ? "Portfólio acadêmico sem equipes visíveis ainda"
                    : teams.Count == 1
                        ? "1 equipe/projeto visível para supervisão"
                        : $"{teams.Count} equipes/projetos visíveis para supervisão",
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = GetThemeBrush("PrimaryTextBrush")
            });

            if (teams.Count == 0)
            {
                stack.Children.Add(new TextBlock
                {
                    Text = "Ao abrir o perfil, você continua com os dados acadêmicos do aluno e pode voltar depois que houver equipe vinculada.",
                    Margin = new Thickness(0, 6, 0, 0),
                    FontSize = 11,
                    Foreground = GetThemeBrush("SecondaryTextBrush"),
                    TextWrapping = TextWrapping.Wrap,
                    LineHeight = 18
                });
            }
            else
            {
                foreach (var team in teams.Take(3))
                {
                    stack.Children.Add(CreateSearchSlideUserPortfolioRow(team));
                }

                if (teams.Count > 3)
                {
                    stack.Children.Add(new TextBlock
                    {
                        Text = $"+{teams.Count - 3} equipe(s) adicional(is) no perfil completo.",
                        Margin = new Thickness(0, 8, 0, 0),
                        FontSize = 10,
                        Foreground = GetThemeBrush("TertiaryTextBrush")
                    });
                }
            }

            panel.Child = stack;
            return panel;
        }

        private Border CreateSearchSlideUserPortfolioRow(TeamWorkspaceInfo team)
        {
            var row = new Border
            {
                Margin = new Thickness(0, 10, 0, 0),
                Padding = new Thickness(12, 10, 12, 10),
                Background = GetThemeBrush("MutedCardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(14)
            };

            var layout = new Grid();
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var info = new StackPanel();
            info.Children.Add(new TextBlock
            {
                Text = team.TeamName,
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = GetThemeBrush("PrimaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            });
            info.Children.Add(new TextBlock
            {
                Text = string.IsNullOrWhiteSpace(team.ClassName)
                    ? $"{team.Course} • {Math.Clamp(team.ProjectProgress, 0, 100)}% • {team.ProjectStatus}"
                    : $"{team.Course} • {team.ClassName} • {Math.Clamp(team.ProjectProgress, 0, 100)}%",
                Margin = new Thickness(0, 4, 0, 0),
                FontSize = 10,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            });
            info.Children.Add(new TextBlock
            {
                Text = BuildTeamProfessorFocusLabel(team),
                Margin = new Thickness(0, 4, 0, 0),
                FontSize = 10,
                Foreground = GetThemeBrush("TertiaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            });
            layout.Children.Add(info);

            var actions = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(12, 0, 0, 0)
            };

            var openButton = new Button
            {
                Content = "Abrir",
                Background = GetThemeBrush("CardBackgroundBrush"),
                Foreground = GetThemeBrush("PrimaryTextBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(12, 7, 12, 7),
                Cursor = Cursors.Hand,
                FontWeight = FontWeights.SemiBold,
                Tag = team
            };
            openButton.Click += SearchSlideOpenDiscoveredTeam_Click;
            actions.Children.Add(openButton);

            if (CurrentViewerCanClaimFocalProfessor())
            {
                var alreadyFocal = string.Equals(team.FocalProfessorUserId, GetCurrentUserId(), StringComparison.OrdinalIgnoreCase);
                var claimButton = new Button
                {
                    Content = alreadyFocal ? "Você é focal" : "Assumir foco",
                    Background = alreadyFocal ? GetThemeBrush("AccentMutedBrush") : GetThemeBrush("AccentBrush"),
                    Foreground = alreadyFocal ? GetThemeBrush("AccentBrush") : Brushes.White,
                    BorderBrush = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(12, 7, 12, 7),
                    Margin = new Thickness(8, 0, 0, 0),
                    Cursor = alreadyFocal ? Cursors.Arrow : Cursors.Hand,
                    FontWeight = FontWeights.SemiBold,
                    IsEnabled = !alreadyFocal,
                    Tag = team
                };
                claimButton.Click += SearchSlideClaimProfessorFocus_Click;
                actions.Children.Add(claimButton);
            }

            Grid.SetColumn(actions, 1);
            layout.Children.Add(actions);

            row.Child = layout;
            return row;
        }

        private string BuildTeamProfessorFocusLabel(TeamWorkspaceInfo team)
        {
            if (!string.IsNullOrWhiteSpace(team.FocalProfessorName))
            {
                return $"Professor focal: {team.FocalProfessorName}";
            }

            var supervisors = (team.ProfessorSupervisorNames ?? new List<string>())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (supervisors.Count == 0)
            {
                return "Sem professor focal definido";
            }

            return supervisors.Count == 1
                ? $"Docente vinculado: {supervisors[0]}"
                : $"Docentes vinculados: {string.Join(", ", supervisors.Take(2))}";
        }

        private string BuildTeamLeadershipLabel(TeamWorkspaceInfo team)
        {
            var leaders = GetStudentLeaders(team);
            if (leaders.Count == 0)
            {
                return "Sem líder discente definido";
            }

            return leaders.Count == 1
                ? $"Líder discente: {leaders[0].Name}"
                : $"Liderança discente: {string.Join(", ", leaders.Take(2).Select(member => member.Name))}";
        }

        private string BuildTeamBalanceLabel(TeamWorkspaceInfo team)
        {
            var studentCount = GetStudentTeamMembers(team).Count;
            var facultyCount = GetFacultyMembers(team).Count;
            return $"{studentCount} aluno(s) em execução • {facultyCount} docente(s) em orientação";
        }

        private Border CreateSearchSlideTeamResultCard(TeamWorkspaceInfo team)
        {
            var card = CreateSearchSlideInfoCard(team.TeamName, $"{team.Course} • {team.ClassName} • {team.AcademicTerm}");
            if (card.Child is not StackPanel stack)
            {
                return card;
            }

            var chips = new WrapPanel { Margin = new Thickness(0, 10, 0, 0) };
            chips.Children.Add(CreateStaticTeamChip($"Código {team.TeamId}", GetThemeBrush("AccentMutedBrush"), GetThemeBrush("AccentBrush")));
            if (!string.IsNullOrWhiteSpace(team.TemplateName))
            {
                chips.Children.Add(CreateStaticTeamChip(team.TemplateName, GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            }
            chips.Children.Add(CreateStaticTeamChip(BuildTeamBalanceLabel(team), GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            chips.Children.Add(CreateStaticTeamChip(BuildTeamProfessorFocusLabel(team), GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            chips.Children.Add(CreateStaticTeamChip(BuildTeamLeadershipLabel(team), GetThemeBrush("CardBackgroundBrush"), GetThemeBrush("PrimaryTextBrush")));
            stack.Children.Add(chips);

            var actions = new WrapPanel { Margin = new Thickness(0, 10, 0, 0) };

            var openButton = new Button
            {
                Content = "Abrir equipe",
                Background = GetThemeBrush("AccentBrush"),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(14, 8, 14, 8),
                Cursor = Cursors.Hand,
                FontWeight = FontWeights.SemiBold,
                Tag = team
            };
            openButton.Click += SearchSlideOpenDiscoveredTeam_Click;
            actions.Children.Add(openButton);

            if (CurrentViewerCanClaimFocalProfessor())
            {
                var alreadyFocal = string.Equals(team.FocalProfessorUserId, GetCurrentUserId(), StringComparison.OrdinalIgnoreCase);
                var assignButton = new Button
                {
                    Content = alreadyFocal ? "Você é focal" : "Assumir foco docente",
                    Background = alreadyFocal ? GetThemeBrush("AccentMutedBrush") : GetThemeBrush("CardBackgroundBrush"),
                    Foreground = alreadyFocal ? GetThemeBrush("AccentBrush") : GetThemeBrush("PrimaryTextBrush"),
                    BorderBrush = alreadyFocal ? Brushes.Transparent : GetThemeBrush("CardBorderBrush"),
                    BorderThickness = alreadyFocal ? new Thickness(0) : new Thickness(1),
                    Padding = new Thickness(14, 8, 14, 8),
                    Margin = new Thickness(8, 0, 0, 0),
                    Cursor = alreadyFocal ? Cursors.Arrow : Cursors.Hand,
                    FontWeight = FontWeights.SemiBold,
                    IsEnabled = !alreadyFocal,
                    Tag = team
                };
                assignButton.Click += SearchSlideClaimProfessorFocus_Click;
                actions.Children.Add(assignButton);
            }

            stack.Children.Add(actions);
            return card;
        }

        private Border CreateSearchSlideConversationResultCard(Conversation conversation)
        {
            var card = CreateSearchSlideInfoCard(conversation.ContactName, string.IsNullOrWhiteSpace(conversation.LastMessage) ? "Conversa pronta para retomar." : conversation.LastMessage);
            if (card.Child is not StackPanel stack)
            {
                return card;
            }

            stack.Children.Add(new TextBlock
            {
                Text = $"Última atividade em {conversation.LastMessageTime:dd/MM HH:mm}",
                Margin = new Thickness(0, 8, 0, 0),
                FontSize = 10,
                Foreground = GetThemeBrush("TertiaryTextBrush")
            });

            var openButton = new Button
            {
                Content = "Abrir conversa",
                Background = GetThemeBrush("AccentBrush"),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(14, 8, 14, 8),
                Margin = new Thickness(0, 10, 0, 0),
                Cursor = Cursors.Hand,
                FontWeight = FontWeights.SemiBold,
                Tag = conversation
            };
            openButton.Click += (s, e) =>
            {
                _selectedConversation = conversation;
                ResetNavigation();
                ChatsContent.Visibility = Visibility.Visible;
                RefreshChatsUI();
                HideSearchSlidePanel();
            };
            stack.Children.Add(openButton);
            return card;
        }

        private Border CreateSearchSlideFileResultCard(FilesHubItem item)
        {
            var card = CreateSearchSlideInfoCard(item.FileName, $"{item.AssociationType} • {item.AssociationLabel}");
            if (card.Child is not StackPanel stack)
            {
                return card;
            }

            stack.Children.Add(new TextBlock
            {
                Text = $"Adicionado em {item.AddedAt:dd/MM HH:mm} • {FormatFileSize(item.FileSizeBytes)}",
                Margin = new Thickness(0, 8, 0, 0),
                FontSize = 10,
                Foreground = GetThemeBrush("TertiaryTextBrush")
            });

            var openButton = new Button
            {
                Content = "Abrir hub de arquivos",
                Background = GetThemeBrush("CardBackgroundBrush"),
                Foreground = GetThemeBrush("PrimaryTextBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(14, 8, 14, 8),
                Margin = new Thickness(0, 10, 0, 0),
                Cursor = Cursors.Hand,
                FontWeight = FontWeights.SemiBold
            };
            openButton.Click += (s, e) =>
            {
                ResetNavigation();
                FilesContent.Visibility = Visibility.Visible;
                RenderFilesHub();
                HideSearchSlidePanel();
            };
            stack.Children.Add(openButton);
            return card;
        }

        private string FormatFileSize(long bytes)
        {
            if (bytes <= 0)
            {
                return "0 B";
            }

            string[] units = { "B", "KB", "MB", "GB" };
            double size = bytes;
            var unitIndex = 0;
            while (size >= 1024 && unitIndex < units.Length - 1)
            {
                size /= 1024;
                unitIndex++;
            }

            return $"{size:0.#} {units[unitIndex]}";
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

        private async void SearchSlideOpenDiscoveredTeam_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: TeamWorkspaceInfo team })
            {
                return;
            }

            var resolvedTeam = team;
            if (_teamService != null && !string.IsNullOrWhiteSpace(team.TeamId))
            {
                var loadedTeam = await _teamService.GetTeamByIdAsync(team.TeamId);
                if (loadedTeam != null)
                {
                    await EnrichTeamMembersAvatarsAsync(new List<TeamWorkspaceInfo> { loadedTeam });
                    resolvedTeam = loadedTeam;
                }
            }

            TrackTeamWorkspaceLocally(resolvedTeam);
            OpenTeamWorkspace(resolvedTeam, navigateToTeams: true);
            HideSearchSlidePanel();
        }

        private async void SearchSlideClaimProfessorFocus_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: TeamWorkspaceInfo team } button)
            {
                return;
            }

            button.IsEnabled = false;

            try
            {
                var updatedTeam = await AssignCurrentProfessorAsFocalAsync(team);
                if (updatedTeam == null)
                {
                    return;
                }

                SearchSlideStatusText.Text = $"{updatedTeam.TeamName} agora está sob sua supervisão focal.";
                SearchSlideStatusText.Foreground = new SolidColorBrush(Color.FromRgb(21, 128, 61));
                RenderSearchSlideResults(_searchSlideResults);
            }
            finally
            {
                button.IsEnabled = true;
            }
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
                        ShowTeamsSection();
                        break;
                    case "Professor":
                        ShowProfessorDashboardSection();
                        break;
                    case "Calendario":
                        CalendarContent.Visibility = Visibility.Visible;
                        RenderCalendarAgenda();
                        break;
                    case "Arquivos":
                        FilesContent.Visibility = Visibility.Visible;
                        RenderFilesHub();
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
            ProfessorDashboardContent.Visibility = Visibility.Collapsed;
            CalendarContent.Visibility = Visibility.Collapsed;
            FilesContent.Visibility = Visibility.Collapsed;
            SettingsContent.Visibility = Visibility.Collapsed;
            _showChoasIntroBubble = false;
            StopChoasAnimation();
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

            if (FilesContent.Visibility == Visibility.Visible)
            {
                RenderFilesHub();
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
            _currentProfile.AcademicDepartment = AcademicDepartmentTextBox.Text?.Trim() ?? string.Empty;
            _currentProfile.AcademicFocus = AcademicFocusTextBox.Text?.Trim() ?? string.Empty;
            _currentProfile.OfficeHours = OfficeHoursTextBox.Text?.Trim() ?? string.Empty;
            _currentProfile.Bio = BioTextBox.Text?.Trim() ?? string.Empty;
            _currentProfile.Skills = SkillsTextBox.Text?.Trim() ?? string.Empty;
            _currentProfile.ProgrammingLanguages = string.Join(", ", _selectedProgrammingLanguages.OrderBy(language => language));
            _currentProfile.PortfolioLink = PortfolioLinkTextBox.Text?.Trim() ?? string.Empty;
            _currentProfile.LinkedInLink = LinkedInLinkTextBox.Text?.Trim() ?? string.Empty;
            NormalizeProfileAvatarSelection(_currentProfile);
            NormalizeProfileCollections(_currentProfile);
            UpdateRoleAwareShellState(_currentProfile);

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
            profile.Role = TeamPermissionService.NormalizeRole(profile.Role);

            var endpoint = AppConfig.BuildFirestoreDocumentUrl($"users/{profile.UserId}");
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
                    academicDepartment = new { stringValue = profile.AcademicDepartment },
                    academicFocus = new { stringValue = profile.AcademicFocus },
                    officeHours = new { stringValue = profile.OfficeHours },
                    professorAccessLevel = new { stringValue = profile.ProfessorAccessLevel },
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
                    galleryImages = new { arrayValue = new { values = ConvertProfileGalleryImagesToFirestoreArray(profile.GalleryImages) } },
                    featuredProjectIds = new { arrayValue = new { values = ConvertStringValuesToFirestoreArray(profile.FeaturedProjectIds) } },
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

        private object[] ConvertProfileGalleryImagesToFirestoreArray(IEnumerable<ProfileGalleryImage> galleryImages)
        {
            return (galleryImages ?? Enumerable.Empty<ProfileGalleryImage>())
                .Where(item => item != null && !string.IsNullOrWhiteSpace(item.ImageDataUri))
                .Select(item => new
                {
                    mapValue = new
                    {
                        fields = new
                        {
                            imageId = new { stringValue = string.IsNullOrWhiteSpace(item.ImageId) ? Guid.NewGuid().ToString("N") : item.ImageId },
                            title = new { stringValue = item.Title ?? string.Empty },
                            description = new { stringValue = item.Description ?? string.Empty },
                            galleryAlbumId = new { stringValue = item.GalleryAlbumId ?? string.Empty },
                            galleryAlbumTitle = new { stringValue = item.GalleryAlbumTitle ?? string.Empty },
                            galleryAlbumDescription = new { stringValue = item.GalleryAlbumDescription ?? string.Empty },
                            imageDataUri = new { stringValue = item.ImageDataUri ?? string.Empty },
                            addedAt = new { timestampValue = (item.AddedAt == default ? DateTime.UtcNow : item.AddedAt.ToUniversalTime()).ToString("o") }
                        }
                    }
                })
                .Cast<object>()
                .ToArray();
        }

        private object[] ConvertStringValuesToFirestoreArray(IEnumerable<string> values)
        {
            return (values ?? Enumerable.Empty<string>())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Select(item => new { stringValue = item.Trim() })
                .Cast<object>()
                .ToArray();
        }

        private string? TryCreateCompressedImageDataUri(string filePath, int maxSide, int quality)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return null;
            }

            try
            {
                using var stream = File.OpenRead(filePath);
                var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                var source = decoder.Frames.FirstOrDefault();
                if (source == null)
                {
                    return null;
                }

                BitmapSource normalizedSource = source;
                var largestSide = Math.Max(source.PixelWidth, source.PixelHeight);
                if (largestSide > maxSide)
                {
                    var scale = (double)maxSide / largestSide;
                    var transformed = new TransformedBitmap(source, new ScaleTransform(scale, scale));
                    transformed.Freeze();
                    normalizedSource = transformed;
                }

                var encoder = new JpegBitmapEncoder
                {
                    QualityLevel = Math.Clamp(quality, 40, 90)
                };
                encoder.Frames.Add(BitmapFrame.Create(normalizedSource));

                using var memoryStream = new MemoryStream();
                encoder.Save(memoryStream);
                return $"data:image/jpeg;base64,{Convert.ToBase64String(memoryStream.ToArray())}";
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[ProfileImage] Falha ao converter imagem em data URI: {ex.Message}");
                return null;
            }
        }

        private ImageSource? TryCreateImageSourceFromDataUri(string? dataUri)
        {
            if (string.IsNullOrWhiteSpace(dataUri))
            {
                return null;
            }

            try
            {
                var commaIndex = dataUri.IndexOf(',');
                if (commaIndex < 0 || commaIndex >= dataUri.Length - 1)
                {
                    return null;
                }

                var bytes = Convert.FromBase64String(dataUri[(commaIndex + 1)..]);
                using var memoryStream = new MemoryStream(bytes);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = memoryStream;
                bitmap.EndInit();

                if (bitmap.CanFreeze)
                {
                    bitmap.Freeze();
                }

                return bitmap;
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[ProfileImage] Falha ao abrir data URI do perfil: {ex.Message}");
                return null;
            }
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
                        QueueChatsUiRefresh();
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

        private async void QueueChatsUiRefresh()
        {
            var renderSequence = ++_chatRenderSequence;
            await Dispatcher.Yield(DispatcherPriority.Background);

            if (renderSequence != _chatRenderSequence)
            {
                return;
            }

            RefreshChatsUI();
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
            actionPanel.Children.Add(CreateHeaderIconButton(PackIconMaterialKind.PhoneOutline, "Iniciar audio", new SolidColorBrush(Color.FromRgb(14, 165, 233))));
            actionPanel.Children.Add(CreateHeaderIconButton(PackIconMaterialKind.VideoOutline, "Iniciar video", new SolidColorBrush(Color.FromRgb(236, 72, 153))));
            actionPanel.Children.Add(CreateHeaderIconButton(PackIconMaterialKind.Magnify, "Buscar na conversa", new SolidColorBrush(Color.FromRgb(99, 102, 241))));

            var menuButton = CreateHeaderIconButton(PackIconMaterialKind.DotsHorizontal, "Mais acoes");
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
            var filesButton = CreateComposerChipButton("Arquivos", PackIconMaterialKind.FolderOutline, new SolidColorBrush(Color.FromRgb(59, 130, 246)));
            filesButton.Click += (s, e) => MessageBox.Show(
                $"A central de arquivos da conversa com {conv.ContactName} sera conectada aqui.",
                "Arquivos da conversa",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            quickTools.Children.Add(filesButton);

            var mediaButton = CreateComposerChipButton("Midia", PackIconMaterialKind.ImageOutline, new SolidColorBrush(Color.FromRgb(14, 165, 233)));
            mediaButton.Click += (s, e) => MessageBox.Show(
                $"O historico de midia compartilhada com {conv.ContactName} aparecera aqui.",
                "Midia compartilhada",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            quickTools.Children.Add(mediaButton);

            var mentionButton = CreateComposerChipButton("@Professor", PackIconMaterialKind.SchoolOutline, new SolidColorBrush(Color.FromRgb(124, 58, 237)));
            mentionButton.Click += (s, e) => MessageBox.Show(
                $"Voce podera chamar o professor orientador direto desta conversa com {conv.ContactName}.",
                "Contato academico",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            quickTools.Children.Add(mentionButton);

            var exportButton = CreateComposerChipButton("Exportar", PackIconMaterialKind.ExportVariant, new SolidColorBrush(Color.FromRgb(16, 185, 129)));
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
                    messagesList.Children.Add(CreateMessageBubble(conv, msg));
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
            quickActionPanel.Children.Add(CreateComposerChipButton("Ações rápidas", PackIconMaterialKind.AutoFix, new SolidColorBrush(Color.FromRgb(245, 158, 11))));
            quickActionPanel.Children.Add(CreateComposerChipButton("Midia", PackIconMaterialKind.ImageOutline, new SolidColorBrush(Color.FromRgb(14, 165, 233))));
            quickActionPanel.Children.Add(CreateComposerChipButton("Arquivo", PackIconMaterialKind.Paperclip, new SolidColorBrush(Color.FromRgb(59, 130, 246))));
            quickActionPanel.Children.Add(CreateComposerChipButton("Professor", PackIconMaterialKind.SchoolOutline, new SolidColorBrush(Color.FromRgb(124, 58, 237))));
            quickActionPanel.Children.Add(CreateComposerChipButton("Exportar", PackIconMaterialKind.ExportVariant, new SolidColorBrush(Color.FromRgb(16, 185, 129))));
            composerStack.Children.Add(quickActionPanel);

            var inputGrid = new Grid();
            inputGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            inputGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            inputGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            inputGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            inputGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var emojiButton = CreateHeaderIconButton(PackIconMaterialKind.EmoticonHappyOutline, "Emoji", new SolidColorBrush(Color.FromRgb(245, 158, 11)));
            var attachmentButton = CreateHeaderIconButton(PackIconMaterialKind.StickerOutline, "Enviar figurinha", new SolidColorBrush(Color.FromRgb(236, 72, 153)));
            attachmentButton.Background = new SolidColorBrush(Color.FromRgb(0, 168, 132));
            attachmentButton.Foreground = Brushes.White;
            attachmentButton.MinWidth = 40;
            attachmentButton.Height = 40;
            attachmentButton.FontSize = 16;

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

            var emojiPopup = CreateEmojiPickerPopup(emojiButton, inputBox);
            emojiButton.Click += (s, e) => emojiPopup.IsOpen = !emojiPopup.IsOpen;
            Grid.SetColumn(emojiButton, 0);
            inputGrid.Children.Add(emojiButton);

            var stickerPopup = CreateStickerPickerPopup(attachmentButton, conv);
            attachmentButton.Click += (s, e) => stickerPopup.IsOpen = !stickerPopup.IsOpen;
            Grid.SetColumn(attachmentButton, 1);
            inputGrid.Children.Add(attachmentButton);

            Grid.SetColumn(inputBox, 2);
            inputGrid.Children.Add(inputBox);

            var micButton = CreateHeaderIconButton(PackIconMaterialKind.MicrophoneOutline, "Gravar audio", new SolidColorBrush(Color.FromRgb(20, 184, 166)));
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

        private Border CreateMessageBubble(Conversation conv, ChatMessage msg)
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
                MaxWidth = 560
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
                Background = msg.IsDeleted ? Brushes.Transparent : (msg.IsOwn ? ownBubbleBackground : otherBubbleBackground),
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

            if (msg.IsDeleted)
            {
                stack.Children.Add(CreateDeletedBubbleContent(msg));
            }
            else if (msg.IsSticker)
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
                Text = msg.IsEdited && !msg.IsDeleted ? $"{msg.Timestamp:HH:mm} • editada" : msg.Timestamp.ToString("HH:mm"),
                FontSize = 10,
                Foreground = msg.IsOwn ? ownTimeBrush : otherTimeBrush,
                Margin = new Thickness(0, 6, 0, 0),
                TextAlignment = TextAlignment.Right
            });

            bubble.Child = stack;

            if (msg.IsOwn)
            {
                var actionsMenu = CreateMessageActionsContextMenu(conv, msg);
                bubble.ContextMenu = actionsMenu;

                var actionButton = new Button
                {
                    Content = "⋯",
                    Width = 28,
                    Height = 28,
                    Margin = new Thickness(8, 4, 0, 0),
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Foreground = _appDarkModeEnabled
                        ? new SolidColorBrush(Color.FromRgb(134, 150, 160))
                        : new SolidColorBrush(Color.FromRgb(71, 85, 105)),
                    Cursor = Cursors.Hand,
                    ToolTip = "Ações da mensagem"
                };
                actionButton.Click += (_, __) =>
                {
                    actionsMenu.PlacementTarget = actionButton;
                    actionsMenu.IsOpen = true;
                };

                row.Children.Add(bubble);
                row.Children.Add(actionButton);
            }
            else
            {
                row.Children.Add(bubble);
            }

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

            if (_stickerImageCache.TryGetValue(assetFileName, out var cachedSource))
            {
                return cachedSource;
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
                var source = CreateFrozenBitmapImage(new Uri($"pack://application:,,,/img/emojiobsseract/{assetFileName}", UriKind.Absolute));
                _stickerImageCache[assetFileName] = source;
                return source;
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

                    var source = CreateFrozenBitmapImage(new Uri(stickerPath, UriKind.Absolute));
                    _stickerImageCache[assetFileName] = source;
                    return source;
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
                QueueChatsUiRefresh();
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
                _userAcademicPortfolioCache.Clear();
                foreach (var team in teams)
                {
                    _teamWorkspaces.Add(team);
                }

                UpdateTeamsViewState();
                RenderTeamsList();
                if (_activeTeamWorkspace != null)
                {
                    _activeTeamWorkspace = _teamWorkspaces.FirstOrDefault(team => string.Equals(team.TeamId, _activeTeamWorkspace.TeamId, StringComparison.OrdinalIgnoreCase));
                    RenderTeamWorkspace();
                }

                RenderProfessorDashboard();
                RenderCalendarAgenda();

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

        private FontFamily GetAppTextFontFamily()
        {
            return TryFindResource("AppTextFontFamily") as FontFamily ?? new FontFamily("Segoe UI");
        }

        private FontFamily GetAppDisplayFontFamily()
        {
            return TryFindResource("AppDisplayFontFamily") as FontFamily ?? new FontFamily("Segoe UI Semibold");
        }

        private FontFamily GetAppEmojiFontFamily()
        {
            return TryFindResource("AppEmojiFontFamily") as FontFamily ?? new FontFamily("Segoe UI Emoji");
        }

        private static bool ContainsEmojiLikeGlyph(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && value.EnumerateRunes().Any(rune =>
            {
                var category = Rune.GetUnicodeCategory(rune);
                return category == UnicodeCategory.OtherSymbol || category == UnicodeCategory.ModifierSymbol;
            });
        }

        private static bool IsGlyphOnlyContent(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var visibleRunes = value.Trim().EnumerateRunes().Where(rune => !Rune.IsWhiteSpace(rune)).ToList();
            if (visibleRunes.Count == 0)
            {
                return false;
            }

            return visibleRunes.All(rune =>
            {
                var category = Rune.GetUnicodeCategory(rune);
                return category == UnicodeCategory.OtherSymbol
                    || category == UnicodeCategory.ModifierSymbol
                    || category == UnicodeCategory.MathSymbol
                    || category == UnicodeCategory.OtherPunctuation
                    || category == UnicodeCategory.DashPunctuation
                    || category == UnicodeCategory.OpenPunctuation
                    || category == UnicodeCategory.ClosePunctuation
                    || category == UnicodeCategory.ConnectorPunctuation
                    || category == UnicodeCategory.InitialQuotePunctuation
                    || category == UnicodeCategory.FinalQuotePunctuation
                    || category == UnicodeCategory.NonSpacingMark;
            });
        }

        private object CreateButtonLabelContent(string content, double fontSize, Brush foreground, FontWeight fontWeight, bool preferDisplayForGlyphOnly = false)
        {
            var trimmedContent = content?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(trimmedContent))
            {
                return string.Empty;
            }

            var label = new TextBlock
            {
                FontSize = fontSize,
                FontWeight = fontWeight,
                Foreground = foreground,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.NoWrap,
                TextTrimming = TextTrimming.CharacterEllipsis
            };

            var parts = trimmedContent.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length == 2 && (ContainsEmojiLikeGlyph(parts[0]) || IsGlyphOnlyContent(parts[0])))
            {
                label.Inlines.Add(new Run(parts[0])
                {
                    FontFamily = ContainsEmojiLikeGlyph(parts[0]) ? GetAppEmojiFontFamily() : GetAppDisplayFontFamily()
                });
                label.Inlines.Add(new Run(" " + parts[1])
                {
                    FontFamily = GetAppTextFontFamily()
                });
                return label;
            }

            label.Text = trimmedContent;
            label.FontFamily = ContainsEmojiLikeGlyph(trimmedContent)
                ? GetAppEmojiFontFamily()
                : preferDisplayForGlyphOnly && IsGlyphOnlyContent(trimmedContent)
                    ? GetAppDisplayFontFamily()
                    : GetAppTextFontFamily();

            return label;
        }

        private PackIconMaterial CreateMaterialIcon(PackIconMaterialKind iconKind, Brush foreground, double size)
        {
            return new PackIconMaterial
            {
                Kind = iconKind,
                Width = size,
                Height = size,
                Foreground = foreground,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        private object CreateIconLabelContent(string text, PackIconMaterialKind iconKind, Brush iconBrush, Brush textBrush, double iconSize, double fontSize, FontWeight fontWeight)
        {
            var stack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };

            var icon = CreateMaterialIcon(iconKind, iconBrush, iconSize);
            icon.Margin = new Thickness(0, 0, 8, 0);
            stack.Children.Add(icon);
            stack.Children.Add(new TextBlock
            {
                Text = text,
                FontFamily = GetAppTextFontFamily(),
                FontSize = fontSize,
                FontWeight = fontWeight,
                Foreground = textBrush,
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis
            });

            return stack;
        }

        private Button CreateHeaderIconButton(PackIconMaterialKind iconKind, string tooltip, Brush? iconBrush = null)
        {
            var foreground = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(134, 150, 160))
                : new SolidColorBrush(Color.FromRgb(71, 85, 105));
            var effectiveBrush = iconBrush ?? foreground;

            return new Button
            {
                Content = CreateMaterialIcon(iconKind, effectiveBrush, 18),
                ToolTip = tooltip,
                Background = new SolidColorBrush(Colors.Transparent),
                Foreground = foreground,
                BorderThickness = new Thickness(0),
                MinWidth = 34,
                Height = 34,
                Padding = new Thickness(10, 0, 10, 0),
                Margin = new Thickness(4, 0, 0, 0),
                Cursor = Cursors.Hand,
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center
            };
        }

        private Button CreateComposerChipButton(string content, PackIconMaterialKind? iconKind = null, Brush? iconBrush = null)
        {
            var foreground = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(233, 237, 239))
                : new SolidColorBrush(Color.FromRgb(30, 41, 59));
            var contentElement = iconKind.HasValue
                ? CreateIconLabelContent(content, iconKind.Value, iconBrush ?? foreground, foreground, 14, 11, FontWeights.SemiBold)
                : CreateButtonLabelContent(content, 11, foreground, FontWeights.SemiBold);

            return new Button
            {
                Content = contentElement,
                Background = _appDarkModeEnabled
                    ? new SolidColorBrush(Color.FromRgb(32, 44, 51))
                    : new SolidColorBrush(Color.FromRgb(241, 245, 249)),
                Foreground = foreground,
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

        private Popup CreateEmojiPickerPopup(Button anchorButton, TextBox inputBox)
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
                HorizontalOffset = -8,
                VerticalOffset = -10,
                AllowsTransparency = true,
                StaysOpen = false
            };

            var emojisWrap = new WrapPanel
            {
                ItemWidth = 54,
                Margin = new Thickness(0, 0, -8, -8)
            };

            foreach (var emoji in ChatComposerEmojis)
            {
                var emojiButton = new Button
                {
                    Content = CreateButtonLabelContent(emoji, 22, titleBrush, FontWeights.Normal),
                    Width = 46,
                    Height = 46,
                    FontSize = 22,
                    Margin = new Thickness(0, 0, 8, 8),
                    Background = _appDarkModeEnabled
                        ? new SolidColorBrush(Color.FromRgb(30, 41, 59))
                        : new SolidColorBrush(Color.FromRgb(248, 250, 252)),
                    Foreground = titleBrush,
                    BorderBrush = popupBorderBrush,
                    BorderThickness = new Thickness(1),
                    Cursor = Cursors.Hand
                };
                emojiButton.Click += (_, __) =>
                {
                    popup.IsOpen = false;
                    InsertEmojiIntoChatInput(inputBox, emoji);
                };
                emojisWrap.Children.Add(emojiButton);
            }

            var content = new StackPanel();
            content.Children.Add(new TextBlock
            {
                Text = "Emojis rápidos",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = titleBrush
            });
            content.Children.Add(new TextBlock
            {
                Text = "Selecione um emoji para inserir na mensagem atual.",
                FontSize = 11,
                Margin = new Thickness(0, 4, 0, 14),
                TextWrapping = TextWrapping.Wrap,
                Foreground = subtitleBrush
            });
            content.Children.Add(emojisWrap);

            popup.Child = new Border
            {
                Width = 340,
                Background = popupBackground,
                BorderBrush = popupBorderBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(20),
                Padding = new Thickness(16),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    BlurRadius = 24,
                    ShadowDepth = 8,
                    Opacity = 0.22,
                    Color = Colors.Black
                },
                Child = content
            };

            return popup;
        }

        private static void InsertEmojiIntoChatInput(TextBox inputBox, string emoji)
        {
            var currentText = inputBox.Text ?? string.Empty;
            var insertIndex = Math.Max(0, Math.Min(inputBox.CaretIndex, currentText.Length));
            var suffix = insertIndex < currentText.Length && !char.IsWhiteSpace(currentText[insertIndex]) ? " " : string.Empty;
            var prefix = insertIndex > 0 && !char.IsWhiteSpace(currentText[insertIndex - 1]) ? " " : string.Empty;
            var insertion = $"{prefix}{emoji}{suffix}";
            inputBox.Text = currentText.Insert(insertIndex, insertion);
            inputBox.CaretIndex = insertIndex + insertion.Length;
            inputBox.Focus();
        }

        private UIElement CreateDeletedBubbleContent(ChatMessage msg)
        {
            var deletedBorderBrush = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(100, 116, 139))
                : new SolidColorBrush(Color.FromRgb(148, 163, 184));
            var deletedTextBrush = _appDarkModeEnabled
                ? new SolidColorBrush(Color.FromRgb(203, 213, 225))
                : new SolidColorBrush(Color.FromRgb(71, 85, 105));

            var grid = new Grid
            {
                Width = 280,
                Height = 56
            };

            grid.Children.Add(new Rectangle
            {
                RadiusX = 16,
                RadiusY = 16,
                Stroke = deletedBorderBrush,
                StrokeThickness = 1.4,
                StrokeDashArray = new DoubleCollection { 4, 3 },
                Fill = Brushes.Transparent
            });

            grid.Children.Add(new TextBlock
            {
                Text = msg.DeletedDisplayText,
                Margin = new Thickness(14, 10, 14, 10),
                FontSize = 12,
                TextWrapping = TextWrapping.Wrap,
                Foreground = deletedTextBrush,
                TextDecorations = TextDecorations.Strikethrough,
                VerticalAlignment = VerticalAlignment.Center
            });

            return grid;
        }

        private ContextMenu CreateMessageActionsContextMenu(Conversation conv, ChatMessage msg)
        {
            var menu = new ContextMenu
            {
                Background = GetThemeBrush("CardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(8)
            };

            var itemStyle = new Style(typeof(MenuItem));
            itemStyle.Setters.Add(new Setter(Control.BackgroundProperty, Brushes.Transparent));
            itemStyle.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(0)));
            itemStyle.Setters.Add(new Setter(Control.ForegroundProperty, GetThemeBrush("PrimaryTextBrush")));
            itemStyle.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(12, 10, 12, 10)));
            itemStyle.Setters.Add(new Setter(Control.FontWeightProperty, FontWeights.SemiBold));
            itemStyle.Setters.Add(new Setter(Control.CursorProperty, Cursors.Hand));
            menu.Resources[typeof(MenuItem)] = itemStyle;

            if (msg.IsText && !msg.IsDeleted)
            {
                var editItem = new MenuItem { Header = "Editar mensagem" };
                editItem.Click += async (_, __) => await EditConversationMessageAsync(conv, msg);
                menu.Items.Add(editItem);
            }

            if (!msg.IsDeleted)
            {
                var deleteItem = new MenuItem { Header = "Apagar mensagem" };
                deleteItem.Click += async (_, __) => await DeleteConversationMessageAsync(conv, msg);
                menu.Items.Add(deleteItem);
            }

            return menu;
        }

        private async Task EditConversationMessageAsync(Conversation conv, ChatMessage msg)
        {
            var updatedText = ShowEditMessageDialog(msg.Content);
            if (string.IsNullOrWhiteSpace(updatedText) || string.Equals(updatedText, msg.Content, StringComparison.Ordinal))
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(_idToken))
            {
                var chatService = new ChatService(_idToken, _currentProfile?.UserId ?? string.Empty);
                var result = await chatService.UpdateMessageAsync(conv.ContactId, msg, updatedText.Trim());
                if (!result.Success)
                {
                    ShowStyledAlertDialog(
                        "CHAT",
                        "Não foi possível editar",
                        $"O Firestore recusou a atualização desta mensagem.\n\n{result.ErrorMessage}",
                        "Fechar",
                        new SolidColorBrush(Color.FromRgb(234, 88, 12)));
                    return;
                }
            }

            msg.Content = updatedText.Trim();
            msg.IsEdited = true;
            msg.EditedAt = DateTime.Now;
            RefreshConversationSummary(conv);
            RefreshChatsUI();
        }

        private async Task DeleteConversationMessageAsync(Conversation conv, ChatMessage msg)
        {
            if (!ShowStyledConfirmationDialog(
                "CHAT",
                "Apagar mensagem",
                "A mensagem será substituída por um aviso de exclusão no histórico da conversa.",
                "Apagar agora",
                new SolidColorBrush(Color.FromRgb(220, 38, 38))))
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(_idToken))
            {
                var chatService = new ChatService(_idToken, _currentProfile?.UserId ?? string.Empty);
                var result = await chatService.DeleteMessageAsync(conv.ContactId, msg);
                if (!result.Success)
                {
                    ShowStyledAlertDialog(
                        "CHAT",
                        "Não foi possível apagar",
                        $"A exclusão lógica da mensagem foi bloqueada.\n\n{result.ErrorMessage}",
                        "Entendi",
                        new SolidColorBrush(Color.FromRgb(220, 38, 38)));
                    return;
                }
            }

            msg.Content = msg.DeletedDisplayText;
            msg.MessageType = "deleted";
            msg.StickerAsset = string.Empty;
            msg.IsDeleted = true;
            msg.DeletedAt = DateTime.Now;
            RefreshConversationSummary(conv);
            RefreshChatsUI();
        }

        private void RefreshConversationSummary(Conversation conv)
        {
            var lastMessage = conv.Messages.OrderBy(message => message.Timestamp).LastOrDefault();
            if (lastMessage == null)
            {
                conv.LastMessage = "Nenhuma mensagem";
                conv.LastMessageTime = DateTime.Now;
                conv.LastSenderId = string.Empty;
                return;
            }

            conv.LastMessage = lastMessage.ConversationPreview;
            conv.LastMessageTime = lastMessage.Timestamp;
            conv.LastSenderId = lastMessage.SenderId;
        }

        private Window CreateStyledDialogWindow(string title, double width, double height, double? minHeight = null, bool canResize = false)
        {
            return new Window
            {
                Title = title,
                Width = width,
                Height = height,
                MinHeight = minHeight ?? height,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = canResize ? ResizeMode.CanResize : ResizeMode.NoResize,
                ShowInTaskbar = false,
                AllowsTransparency = true,
                WindowStyle = WindowStyle.None,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                BorderBrush = Brushes.Transparent
            };
        }

        private Border CreateStyledDialogShell(UIElement content, Thickness? padding = null)
        {
            var shell = new Border
            {
                Margin = new Thickness(14),
                CornerRadius = new CornerRadius(28),
                Background = GetThemeBrush("SurfaceBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                Padding = padding ?? new Thickness(24),
                SnapsToDevicePixels = true,
                Child = content,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    BlurRadius = 34,
                    ShadowDepth = 12,
                    Opacity = 0.18,
                    Color = Color.FromRgb(15, 23, 42)
                }
            };

            shell.Loaded += (_, __) =>
            {
                shell.Opacity = 0;
                var transform = new TranslateTransform(0, 20);
                shell.RenderTransform = transform;

                var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
                shell.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(180)) { EasingFunction = ease });
                transform.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(20, 0, TimeSpan.FromMilliseconds(220)) { EasingFunction = ease });
            };

            return shell;
        }

        private StackPanel CreateDialogHeader(string eyebrow, string title, string description, Brush accentBrush)
        {
            var header = new StackPanel { Margin = new Thickness(0, 0, 0, 18) };
            header.Children.Add(new Border
            {
                Background = accentBrush,
                CornerRadius = new CornerRadius(999),
                Padding = new Thickness(12, 5, 12, 5),
                HorizontalAlignment = HorizontalAlignment.Left,
                Child = new TextBlock
                {
                    Text = eyebrow,
                    FontSize = 11,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White,
                    TextAlignment = TextAlignment.Center
                }
            });
            header.Children.Add(new TextBlock
            {
                Text = title,
                Margin = new Thickness(0, 14, 0, 0),
                FontSize = 24,
                FontWeight = FontWeights.ExtraBold,
                Foreground = GetThemeBrush("PrimaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            });
            header.Children.Add(new TextBlock
            {
                Text = description,
                Margin = new Thickness(0, 10, 0, 0),
                FontSize = 13,
                Foreground = GetThemeBrush("SecondaryTextBrush"),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 21
            });

            return header;
        }

        private Button CreateDialogActionButton(string label, Brush background, Brush foreground, Brush borderBrush, double minWidth = 126)
        {
            return new Button
            {
                Content = label,
                MinWidth = minWidth,
                Height = 42,
                Margin = new Thickness(12, 0, 0, 0),
                Padding = new Thickness(18, 0, 18, 0),
                Background = background,
                Foreground = foreground,
                BorderBrush = borderBrush,
                BorderThickness = borderBrush == Brushes.Transparent ? new Thickness(0) : new Thickness(1),
                FontWeight = FontWeights.SemiBold,
                Cursor = Cursors.Hand
            };
        }

        private void ApplyDialogInputStyle(Control control)
        {
            control.Background = GetThemeBrush("SearchBackgroundBrush");
            control.BorderBrush = GetThemeBrush("SearchBorderBrush");
            control.Foreground = GetThemeBrush("PrimaryTextBrush");
            control.BorderThickness = new Thickness(1);

            if (control.Padding == default)
            {
                control.Padding = new Thickness(12, 10, 12, 10);
            }
        }

        private void ShowStyledAlertDialog(string eyebrow, string title, string message, string buttonLabel, Brush accentBrush)
        {
            var dialog = CreateStyledDialogWindow(title, 520, 320);
            var actionButton = CreateDialogActionButton(buttonLabel, accentBrush, Brushes.White, Brushes.Transparent);
            actionButton.Click += (_, __) => dialog.Close();

            var layout = new Grid();
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            layout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            layout.Children.Add(CreateDialogHeader(eyebrow, title, message, accentBrush));

            var helperCard = new Border
            {
                Background = GetThemeBrush("MutedCardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(18),
                Padding = new Thickness(16),
                Child = new TextBlock
                {
                    Text = "Esse aviso segue o novo padrão visual do projeto para reduzir caixas nativas sem identidade visual.",
                    FontSize = 12,
                    Foreground = GetThemeBrush("SecondaryTextBrush"),
                    TextWrapping = TextWrapping.Wrap,
                    LineHeight = 20
                }
            };
            Grid.SetRow(helperCard, 1);
            layout.Children.Add(helperCard);

            var actions = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 22, 0, 0)
            };
            Grid.SetRow(actions, 2);
            actions.Children.Add(actionButton);
            layout.Children.Add(actions);

            dialog.Content = CreateStyledDialogShell(layout);
            dialog.ShowDialog();
        }

        private bool ShowStyledConfirmationDialog(string eyebrow, string title, string message, string confirmLabel, Brush confirmBrush, string cancelLabel = "Cancelar")
        {
            var dialog = CreateStyledDialogWindow(title, 560, 340);
            var result = false;

            var cancelButton = CreateDialogActionButton(cancelLabel, Brushes.Transparent, GetThemeBrush("PrimaryTextBrush"), GetThemeBrush("CardBorderBrush"));
            cancelButton.Click += (_, __) => dialog.Close();

            var confirmButton = CreateDialogActionButton(confirmLabel, confirmBrush, Brushes.White, Brushes.Transparent);
            confirmButton.Click += (_, __) =>
            {
                result = true;
                dialog.Close();
            };

            var layout = new Grid();
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            layout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            layout.Children.Add(CreateDialogHeader(eyebrow, title, message, confirmBrush));

            var cautionCard = new Border
            {
                Background = GetThemeBrush("MutedCardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(18),
                Padding = new Thickness(16),
                Child = new TextBlock
                {
                    Text = "A ação será aplicada imediatamente após a confirmação.",
                    FontSize = 12,
                    Foreground = GetThemeBrush("SecondaryTextBrush"),
                    TextWrapping = TextWrapping.Wrap
                }
            };
            Grid.SetRow(cautionCard, 1);
            layout.Children.Add(cautionCard);

            var actions = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 22, 0, 0)
            };
            Grid.SetRow(actions, 2);
            actions.Children.Add(cancelButton);
            actions.Children.Add(confirmButton);
            layout.Children.Add(actions);

            dialog.Content = CreateStyledDialogShell(layout);
            dialog.ShowDialog();
            return result;
        }

        private string? ShowEditMessageDialog(string initialText)
        {
            var dialog = CreateStyledDialogWindow("Editar mensagem", 580, 420);

            var contentBox = new TextBox
            {
                Text = initialText,
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Height = 170,
                FontSize = 14,
                Padding = new Thickness(12),
                Margin = new Thickness(0, 10, 0, 0)
            };
            ApplyDialogInputStyle(contentBox);

            var counterText = new TextBlock
            {
                Margin = new Thickness(0, 10, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Right,
                FontSize = 11,
                Foreground = GetThemeBrush("TertiaryTextBrush")
            };
            void RefreshCounter()
            {
                counterText.Text = $"{(contentBox.Text ?? string.Empty).Length} caractere(s)";
            }
            contentBox.TextChanged += (_, __) => RefreshCounter();
            RefreshCounter();

            string? result = null;
            var saveButton = CreateDialogActionButton("Salvar ajuste", new SolidColorBrush(Color.FromRgb(14, 165, 233)), Brushes.White, Brushes.Transparent, 148);
            saveButton.Click += (_, __) =>
            {
                result = contentBox.Text;
                dialog.DialogResult = true;
                dialog.Close();
            };

            var cancelButton = CreateDialogActionButton("Cancelar", Brushes.Transparent, GetThemeBrush("PrimaryTextBrush"), GetThemeBrush("CardBorderBrush"));
            cancelButton.Click += (_, __) => dialog.Close();

            var layout = new Grid();
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            layout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            layout.Children.Add(CreateDialogHeader("CHAT", "Editar mensagem", "Refine o texto sem perder o histórico visual da conversa. Apenas mensagens de texto podem ser alteradas.", GetThemeBrush("AccentBrush")));

            var editorCard = new Border
            {
                Background = GetThemeBrush("MutedCardBackgroundBrush"),
                BorderBrush = GetThemeBrush("CardBorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(20),
                Padding = new Thickness(16),
                Child = new StackPanel
                {
                    Children =
                    {
                        new TextBlock
                        {
                            Text = "Prévia do conteúdo",
                            FontSize = 12,
                            FontWeight = FontWeights.SemiBold,
                            Foreground = GetThemeBrush("PrimaryTextBrush")
                        },
                        contentBox,
                        counterText
                    }
                }
            };
            Grid.SetRow(editorCard, 1);
            layout.Children.Add(editorCard);

            var actions = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 22, 0, 0)
            };
            Grid.SetRow(actions, 2);
            actions.Children.Add(cancelButton);
            actions.Children.Add(saveButton);
            layout.Children.Add(actions);

            dialog.Content = CreateStyledDialogShell(layout);

            contentBox.Loaded += (_, __) =>
            {
                contentBox.Focus();
                contentBox.CaretIndex = contentBox.Text.Length;
            };

            return dialog.ShowDialog() == true ? result : null;
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
