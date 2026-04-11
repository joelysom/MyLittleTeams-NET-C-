using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using Microsoft.Win32;

namespace MeuApp
{
    public partial class ChatWindow : ChromeWindow
    {
        private enum ConversationFilterMode
        {
            Favorites,
            AllChats
        }

        private enum ConversationSortMode
        {
            Custom,
            MostRecent,
            UnreadOnly
        }

        private const string SelfChatPlaceholderText = "Aqui é o Chat com você mesmo, pode deixar seus textos, anotações, fotos e aquivos.";
        private const string SelfChatPreviewText = "Seus lembretes, fotos e arquivos ficam aqui.";

        private UserProfile? _currentUser;
        private string _contactId = string.Empty;
        private string _contactName = string.Empty;
        private List<ChatMessage> _messages = new();
        private ChatService? _chatService;
        private string _idToken = "";
        private readonly List<Conversation> _conversations = new();
        private ConversationFilterMode _activeConversationFilter = ConversationFilterMode.AllChats;
        private ConversationSortMode _activeConversationSortMode = ConversationSortMode.Custom;
        private string _selectedConversationId = string.Empty;

        public ChatWindow()
        {
            InitializeComponent();
            this.KeyDown += ChatWindow_KeyDown;
            ApplyConversationFilterVisualState();
            UpdateConversationSortState();
        }

        public ChatWindow(UserProfile currentUser, string contactId, string contactName, string idToken = "") : this()
        {
            try
            {
                DebugHelper.WriteLine($"[ChatWindow Constructor] Inicializando com usuário: {currentUser?.Name}");
                
                _currentUser = currentUser;
                _contactId = contactId;
                _contactName = contactName;
                _idToken = idToken;
                _selectedConversationId = BuildConversationId(contactId);
                
                // Inicializar ChatService
                if (!string.IsNullOrEmpty(idToken))
                {
                    _chatService = new ChatService(idToken, currentUser?.UserId ?? "");
                    DebugHelper.WriteLine($"[ChatWindow Constructor] ChatService inicializado");
                }
                
                DebugHelper.WriteLine($"[ChatWindow Constructor] Variáveis configuradas");
                
                LoadChatData();
                
                DebugHelper.WriteLine($"[ChatWindow Constructor] ChatWindow pronta");
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[ChatWindow ERRO] {ex.GetType().Name}: {ex.Message}");
                DebugHelper.WriteLine($"[ChatWindow ERRO StackTrace] {ex.StackTrace}");
                throw;
            }
        }

        private Brush GetThemeBrush(string key, Color fallback)
        {
            return TryFindResource(key) as Brush ?? new SolidColorBrush(fallback);
        }

        private void ChatWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Enter para enviar mensagem, Shift+Enter para quebra de linha
            if (e.Key == System.Windows.Input.Key.Return && 
                System.Windows.Input.Keyboard.Modifiers != System.Windows.Input.ModifierKeys.Shift)
            {
                SendButton_Click(null, null);
                e.Handled = true;
            }
        }

        private void LoadChatData()
        {
            try
            {
                DebugHelper.WriteLine($"[LoadChatData] Iniciando...");

                UpdateConversationHeaderState(CreateConversationSnapshot(_contactId, _contactName));
                DebugHelper.WriteLine($"[LoadChatData] Cabeçalho da conversa definido");

                // Carregar conversas simuladas
                DebugHelper.WriteLine($"[LoadChatData] Chamando LoadConversations()...");
                LoadConversations();
                DebugHelper.WriteLine($"[LoadChatData] LoadConversations() concluído");
                
                // Carregar mensagens simuladas
                DebugHelper.WriteLine($"[LoadChatData] Chamando LoadMessages()...");
                LoadMessages();
                DebugHelper.WriteLine($"[LoadChatData] LoadMessages() concluído");
                
                DebugHelper.WriteLine($"[LoadChatData] Sucesso!");
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[LoadChatData ERROR] {ex.GetType().Name}: {ex.Message}");
                DebugHelper.WriteLine($"[LoadChatData ERROR StackTrace] {ex.StackTrace}");
                throw;
            }
        }

        private Conversation CreateConversationSnapshot(string contactId, string contactName)
        {
            var resolvedContactName = string.IsNullOrWhiteSpace(contactName)
                ? (_currentUser?.Name ?? "Você")
                : contactName;

            return new Conversation
            {
                ConversationId = BuildConversationId(contactId),
                ContactId = string.IsNullOrWhiteSpace(contactId) ? GetCurrentUserId() : contactId,
                ContactName = resolvedContactName,
                IsSelfConversation = IsSelfConversation(contactId),
                IsFavorite = IsSelfConversation(contactId),
                CanRemoveFromFavorites = !IsSelfConversation(contactId),
                LastMessage = IsSelfConversation(contactId) ? SelfChatPreviewText : "Abra a conversa para continuar.",
                LastMessageTime = DateTime.Now
            };
        }

        private void UpdateConversationHeaderState(Conversation conversation)
        {
            ContactNameText.Text = conversation.IsSelfConversation ? "Você" : conversation.ContactName;
            ContactStatusText.Text = conversation.IsSelfConversation
                ? "Seu espaço pessoal para notas, fotos e arquivos"
                : conversation.HasUnread
                    ? "Mensagens não lidas"
                    : "Online";
            ContactStatusText.Foreground = conversation.IsSelfConversation
                ? GetThemeBrush("AccentBrush", Color.FromRgb(0, 120, 212))
                : conversation.HasUnread
                    ? GetThemeBrush("WarningBrush", Color.FromRgb(245, 158, 11))
                    : new SolidColorBrush(Color.FromRgb(16, 124, 16));
        }

        private async void LoadConversations()
        {
            try
            {
                DebugHelper.WriteLine($"[LoadConversations] Limpando lista...");
                ConversationsList.Children.Clear();
                DebugHelper.WriteLine($"[LoadConversations] Lista limpa");

                var conversations = await BuildConversationEntriesAsync();
                _conversations.Clear();
                _conversations.AddRange(conversations);

                if (string.IsNullOrWhiteSpace(_selectedConversationId) && !string.IsNullOrWhiteSpace(_contactId))
                {
                    _selectedConversationId = BuildConversationId(_contactId);
                }

                if (_conversations.Count > 0 && !_conversations.Any(conversation => string.Equals(conversation.ConversationId, _selectedConversationId, StringComparison.OrdinalIgnoreCase)))
                {
                    var firstConversation = _conversations[0];
                    SelectConversation(firstConversation, reloadMessages: false);
                }

                RenderConversationList();
                DebugHelper.WriteLine($"[LoadConversations] Sucesso!");
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[LoadConversations ERROR] {ex.GetType().Name}: {ex.Message}");
                DebugHelper.WriteLine($"[LoadConversations ERROR StackTrace] {ex.StackTrace}");
                throw;
            }
        }

        private async Task<List<Conversation>> BuildConversationEntriesAsync()
        {
            var conversations = new List<Conversation>();

            if (_chatService != null)
            {
                try
                {
                    conversations = await _chatService.LoadConversationsAsync();
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine($"[BuildConversationEntriesAsync] Falha ao carregar conversas reais: {ex.Message}");
                }
            }

            if (conversations.Count == 0)
            {
                conversations = CreateFallbackConversations();
            }

            EnsureCurrentConversationEntry(conversations);
            EnsureSelfConversationEntry(conversations);

            for (var index = 0; index < conversations.Count; index++)
            {
                NormalizeConversation(conversations[index], index + 1);
            }

            return conversations;
        }

        private List<Conversation> CreateFallbackConversations()
        {
            var currentConversationName = string.IsNullOrWhiteSpace(_contactName) ? "Contato atual" : _contactName;

            return new List<Conversation>
            {
                new()
                {
                    ConversationId = BuildConversationId(_contactId),
                    ContactId = _contactId,
                    ContactName = currentConversationName,
                    LastMessage = "Ótimo! Vamos começar?",
                    LastMessageTime = DateTime.Now.AddMinutes(-1),
                    HasUnread = false,
                    CustomSortOrder = 1
                },
                new()
                {
                    ConversationId = BuildConversationId("user2"),
                    ContactId = "user2",
                    ContactName = "João Silva",
                    LastMessage = "Combinado, até logo!",
                    LastMessageTime = DateTime.Now.AddMinutes(-2),
                    HasUnread = true,
                    CustomSortOrder = 2
                },
                new()
                {
                    ConversationId = BuildConversationId("user3"),
                    ContactId = "user3",
                    ContactName = "Maria Santos",
                    LastMessage = "Pode ser amanhã?",
                    LastMessageTime = DateTime.Now.AddMinutes(-15),
                    HasUnread = false,
                    CustomSortOrder = 3
                },
                new()
                {
                    ConversationId = BuildConversationId("user4"),
                    ContactId = "user4",
                    ContactName = "Pedro Costa",
                    LastMessage = "Enviou um arquivo",
                    LastMessageTime = DateTime.Now.AddHours(-1),
                    HasUnread = false,
                    CustomSortOrder = 4
                },
                new()
                {
                    ConversationId = BuildConversationId("user5"),
                    ContactId = "user5",
                    ContactName = "Ana Lima",
                    LastMessage = "Perfeito!",
                    LastMessageTime = DateTime.Now.AddHours(-3),
                    HasUnread = false,
                    CustomSortOrder = 5
                }
            };
        }

        private void EnsureCurrentConversationEntry(List<Conversation> conversations)
        {
            if (string.IsNullOrWhiteSpace(_contactId))
            {
                return;
            }

            var existing = conversations.FirstOrDefault(conversation => string.Equals(conversation.ContactId, _contactId, StringComparison.OrdinalIgnoreCase));
            if (existing == null)
            {
                conversations.Insert(0, CreateConversationSnapshot(_contactId, _contactName));
                return;
            }

            if (string.IsNullOrWhiteSpace(existing.ContactName))
            {
                existing.ContactName = string.IsNullOrWhiteSpace(_contactName) ? existing.ContactName : _contactName;
            }
        }

        private void EnsureSelfConversationEntry(List<Conversation> conversations)
        {
            var currentUserId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return;
            }

            var existing = conversations.FirstOrDefault(conversation => IsSelfConversation(conversation.ContactId));
            if (existing == null)
            {
                conversations.Insert(0, new Conversation
                {
                    ConversationId = BuildConversationId(currentUserId),
                    ContactId = currentUserId,
                    ContactName = _currentUser?.Name ?? "Você",
                    LastMessage = SelfChatPreviewText,
                    LastMessageTime = DateTime.Now,
                    HasUnread = false,
                    IsFavorite = true,
                    CanRemoveFromFavorites = false,
                    IsSelfConversation = true,
                    CustomSortOrder = 0
                });
                return;
            }

            existing.ContactId = currentUserId;
            existing.ContactName = string.IsNullOrWhiteSpace(existing.ContactName) ? _currentUser?.Name ?? "Você" : existing.ContactName;
            existing.IsFavorite = true;
            existing.CanRemoveFromFavorites = false;
            existing.IsSelfConversation = true;
            if (string.IsNullOrWhiteSpace(existing.LastMessage))
            {
                existing.LastMessage = SelfChatPreviewText;
                existing.LastMessageTime = DateTime.Now;
            }
        }

        private void NormalizeConversation(Conversation conversation, int fallbackSortOrder)
        {
            conversation.ConversationId = string.IsNullOrWhiteSpace(conversation.ConversationId)
                ? BuildConversationId(conversation.ContactId)
                : conversation.ConversationId;

            conversation.ContactName = string.IsNullOrWhiteSpace(conversation.ContactName)
                ? (conversation.IsSelfConversation ? _currentUser?.Name ?? "Você" : "Contato")
                : conversation.ContactName;

            conversation.LastMessage = string.IsNullOrWhiteSpace(conversation.LastMessage)
                ? (conversation.IsSelfConversation ? SelfChatPreviewText : "Abra a conversa para continuar.")
                : conversation.LastMessage;

            if (conversation.LastMessageTime == default)
            {
                conversation.LastMessageTime = DateTime.Now.AddMinutes(-fallbackSortOrder);
            }

            if (conversation.CustomSortOrder <= 0 && !conversation.IsSelfConversation)
            {
                conversation.CustomSortOrder = fallbackSortOrder;
            }

            if (conversation.IsSelfConversation)
            {
                conversation.IsFavorite = true;
                conversation.CanRemoveFromFavorites = false;
                conversation.CustomSortOrder = 0;
            }
        }

        private void RenderConversationList()
        {
            ConversationsList.Children.Clear();

            var visibleConversations = GetVisibleConversations().ToList();
            foreach (var conversation in visibleConversations)
            {
                var button = CreateConversationButton(conversation, string.Equals(conversation.ConversationId, _selectedConversationId, StringComparison.OrdinalIgnoreCase));
                ConversationsList.Children.Add(button);
            }

            if (visibleConversations.Count == 0)
            {
                ConversationsList.Children.Add(CreateConversationEmptyState());
            }

            ApplyConversationFilterVisualState();
            UpdateConversationSortState();
        }

        private Border CreateConversationEmptyState()
        {
            var title = _activeConversationFilter == ConversationFilterMode.Favorites
                ? "Nenhum favorito além do seu chat pessoal"
                : "Nenhuma conversa disponível com este filtro";
            var description = _activeConversationFilter == ConversationFilterMode.Favorites
                ? "Seu chat com você mesmo fica fixo aqui e pode guardar notas, fotos e arquivos."
                : "Tente trocar a classificação para ver mais conversas.";

            return new Border
            {
                Margin = new Thickness(12, 10, 12, 0),
                Padding = new Thickness(18),
                CornerRadius = new CornerRadius(18),
                Background = GetThemeBrush("SearchBackgroundBrush", Color.FromRgb(248, 250, 252)),
                BorderBrush = GetThemeBrush("SearchBorderBrush", Color.FromRgb(226, 232, 240)),
                BorderThickness = new Thickness(1),
                Child = new StackPanel
                {
                    Children =
                    {
                        new TextBlock
                        {
                            Text = title,
                            FontSize = 12,
                            FontWeight = FontWeights.SemiBold,
                            Foreground = GetThemeBrush("PrimaryTextBrush", Color.FromRgb(15, 23, 42))
                        },
                        new TextBlock
                        {
                            Text = description,
                            Margin = new Thickness(0, 8, 0, 0),
                            FontSize = 11,
                            TextWrapping = TextWrapping.Wrap,
                            Foreground = GetThemeBrush("SecondaryTextBrush", Color.FromRgb(100, 116, 139))
                        }
                    }
                }
            };
        }

        private IEnumerable<Conversation> GetVisibleConversations()
        {
            IEnumerable<Conversation> query = _conversations;

            if (_activeConversationFilter == ConversationFilterMode.Favorites)
            {
                query = query.Where(conversation => conversation.IsFavorite);
            }

            query = _activeConversationSortMode switch
            {
                ConversationSortMode.MostRecent => query
                    .OrderByDescending(conversation => conversation.IsSelfConversation)
                    .ThenByDescending(conversation => conversation.LastMessageTime),
                ConversationSortMode.UnreadOnly => query
                    .Where(conversation => conversation.HasUnread || (_activeConversationFilter == ConversationFilterMode.Favorites && conversation.IsSelfConversation))
                    .OrderByDescending(conversation => conversation.IsSelfConversation)
                    .ThenByDescending(conversation => conversation.HasUnread)
                    .ThenByDescending(conversation => conversation.LastMessageTime),
                _ => query
                    .OrderBy(conversation => conversation.CustomSortOrder)
                    .ThenByDescending(conversation => conversation.IsSelfConversation)
            };

            return query;
        }

        private void ApplyConversationFilterVisualState()
        {
            ApplyConversationFilterButtonState(FavoritesFilterButton, _activeConversationFilter == ConversationFilterMode.Favorites);
            ApplyConversationFilterButtonState(AllChatsFilterButton, _activeConversationFilter == ConversationFilterMode.AllChats);
        }

        private void ApplyConversationFilterButtonState(Button button, bool isActive)
        {
            button.Background = isActive
                ? GetThemeBrush("AccentBrush", Color.FromRgb(0, 120, 212))
                : GetThemeBrush("SearchBackgroundBrush", Color.FromRgb(248, 250, 252));
            button.Foreground = isActive
                ? Brushes.White
                : GetThemeBrush("PrimaryTextBrush", Color.FromRgb(15, 23, 42));
            button.BorderBrush = isActive
                ? Brushes.Transparent
                : GetThemeBrush("SearchBorderBrush", Color.FromRgb(226, 232, 240));
        }

        private void UpdateConversationSortState()
        {
            ConversationSortStatusText.Text = _activeConversationSortMode switch
            {
                ConversationSortMode.MostRecent => "Mais recentes",
                ConversationSortMode.UnreadOnly => "Somente não lidos",
                _ => "Personalizada"
            };

            CustomSortMenuItem.IsChecked = _activeConversationSortMode == ConversationSortMode.Custom;
            MostRecentSortMenuItem.IsChecked = _activeConversationSortMode == ConversationSortMode.MostRecent;
            UnreadOnlySortMenuItem.IsChecked = _activeConversationSortMode == ConversationSortMode.UnreadOnly;
        }

        private Button CreateConversationButton(Conversation conversation, bool isSelected)
        {
            var button = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Background = isSelected ? GetThemeBrush("MutedCardBackgroundBrush", Color.FromRgb(245, 245, 245)) : GetThemeBrush("CardBackgroundBrush", Colors.White),
                Foreground = GetThemeBrush("PrimaryTextBrush", Color.FromRgb(51, 51, 51)),
                BorderThickness = new Thickness(0),
                Height = 84,
                Padding = new Thickness(12),
                Cursor = Cursors.Hand,
                Tag = conversation.ContactId
            };

            var grid = new Grid { HorizontalAlignment = HorizontalAlignment.Stretch };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(48) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });

            var avatar = new Ellipse
            {
                Width = 48,
                Height = 48,
                Fill = conversation.IsSelfConversation
                    ? GetThemeBrush("AccentMutedBrush", Color.FromRgb(232, 238, 255))
                    : new SolidColorBrush(Color.FromRgb(0, 120, 212)),
                Margin = new Thickness(0, 0, 12, 0)
            };
            Grid.SetColumn(avatar, 0);
            grid.Children.Add(avatar);

            var avatarGlyph = new TextBlock
            {
                Text = conversation.IsSelfConversation ? "Você" : string.Concat(conversation.ContactName.Take(1)).ToUpperInvariant(),
                FontSize = conversation.IsSelfConversation ? 10 : 16,
                FontWeight = FontWeights.Bold,
                Foreground = conversation.IsSelfConversation
                    ? GetThemeBrush("AccentBrush", Color.FromRgb(0, 120, 212))
                    : Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(avatarGlyph, 0);
            grid.Children.Add(avatarGlyph);

            var content = new StackPanel
            {
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(content, 1);

            var nameRow = new Grid();
            nameRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            nameRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var nameBlock = new TextBlock
            {
                Text = conversation.IsSelfConversation ? "Você" : conversation.ContactName,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = GetThemeBrush("PrimaryTextBrush", Color.FromRgb(51, 51, 51)),
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            nameRow.Children.Add(nameBlock);

            if (conversation.IsFavorite)
            {
                var favoriteBadge = new Border
                {
                    Margin = new Thickness(8, 0, 0, 0),
                    Padding = new Thickness(8, 2, 8, 2),
                    CornerRadius = new CornerRadius(999),
                    Background = GetThemeBrush("AccentMutedBrush", Color.FromRgb(232, 238, 255)),
                    Child = new TextBlock
                    {
                        Text = conversation.IsSelfConversation ? "Fixo" : "Favorito",
                        FontSize = 10,
                        FontWeight = FontWeights.Bold,
                        Foreground = GetThemeBrush("AccentBrush", Color.FromRgb(0, 120, 212))
                    }
                };
                Grid.SetColumn(favoriteBadge, 1);
                nameRow.Children.Add(favoriteBadge);
            }

            content.Children.Add(nameRow);

            var msgBlock = new TextBlock
            {
                Text = conversation.HasUnread ? $"• {conversation.LastMessage}" : conversation.LastMessage,
                FontSize = 12,
                Foreground = GetThemeBrush("SecondaryTextBrush", Color.FromRgb(119, 119, 119)),
                Margin = new Thickness(0, 4, 0, 0),
                TextTrimming = TextTrimming.CharacterEllipsis,
                FontWeight = conversation.HasUnread ? FontWeights.SemiBold : FontWeights.Normal
            };
            content.Children.Add(msgBlock);
            grid.Children.Add(content);

            var timeBlock = new TextBlock
            {
                Text = conversation.IsSelfConversation ? "Fixo" : conversation.FormattedTime,
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153)),
                VerticalAlignment = VerticalAlignment.Top
            };
            Grid.SetColumn(timeBlock, 2);
            grid.Children.Add(timeBlock);

            button.Content = grid;
            button.Click += (s, e) =>
            {
                SelectConversation(conversation);
            };

            return button;
        }

        private void SelectConversation(Conversation conversation, bool reloadMessages = true)
        {
            _contactId = conversation.ContactId;
            _contactName = conversation.ContactName;
            _selectedConversationId = conversation.ConversationId;
            conversation.HasUnread = false;

            UpdateConversationHeaderState(conversation);
            RenderConversationList();

            if (reloadMessages)
            {
                LoadMessages();
            }
        }

        private string GetCurrentUserId()
        {
            return _currentUser?.UserId ?? string.Empty;
        }

        private bool IsSelfConversation(string contactId)
        {
            return !string.IsNullOrWhiteSpace(contactId)
                && !string.IsNullOrWhiteSpace(GetCurrentUserId())
                && string.Equals(contactId, GetCurrentUserId(), StringComparison.OrdinalIgnoreCase);
        }

        private bool IsCurrentConversationSelfChat => IsSelfConversation(_contactId);

        private string BuildConversationId(string contactId)
        {
            if (_chatService != null && !string.IsNullOrWhiteSpace(contactId))
            {
                return _chatService.BuildConversationId(contactId);
            }

            return string.IsNullOrWhiteSpace(contactId) ? Guid.NewGuid().ToString("N") : contactId;
        }

        private void FavoritesFilterButton_Click(object sender, RoutedEventArgs e)
        {
            _activeConversationFilter = ConversationFilterMode.Favorites;
            RenderConversationList();
        }

        private void AllChatsFilterButton_Click(object sender, RoutedEventArgs e)
        {
            _activeConversationFilter = ConversationFilterMode.AllChats;
            RenderConversationList();
        }

        private void ConversationSortButton_Click(object sender, RoutedEventArgs e)
        {
            if (ConversationSortButton.ContextMenu == null)
            {
                return;
            }

            ConversationSortButton.ContextMenu.PlacementTarget = ConversationSortButton;
            ConversationSortButton.ContextMenu.IsOpen = true;
        }

        private void CustomSortMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _activeConversationSortMode = ConversationSortMode.Custom;
            RenderConversationList();
        }

        private void MostRecentSortMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _activeConversationSortMode = ConversationSortMode.MostRecent;
            RenderConversationList();
        }

        private void UnreadOnlySortMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _activeConversationSortMode = ConversationSortMode.UnreadOnly;
            RenderConversationList();
        }

        private async void LoadMessages()
        {
            try
            {
                DebugHelper.WriteLine($"[LoadMessages] Limpando mensagens...");
                MessagesList.Children.Clear();
                _messages.Clear();

                if (_chatService != null)
                {
                    DebugHelper.WriteLine($"[LoadMessages] Carregando mensagens do Firebase...");
                    var firestoreMessages = await _chatService.LoadMessagesAsync(_contactId);

                    if (firestoreMessages.Count > 0)
                    {
                        DebugHelper.WriteLine($"[LoadMessages] {firestoreMessages.Count} mensagens carregadas do Firebase");
                        _messages = firestoreMessages.Select(NormalizeChatMessageForUi).ToList();
                    }
                    else if (IsCurrentConversationSelfChat)
                    {
                        DebugHelper.WriteLine($"[LoadMessages] Self-chat sem histórico. Exibindo placeholder vazio.");
                        _messages = new List<ChatMessage>();
                    }
                    else
                    {
                        DebugHelper.WriteLine($"[LoadMessages] Nenhuma mensagem encontrada no Firebase, usando dados simulados");
                        _messages = GetSimulatedMessages();
                    }
                }
                else if (IsCurrentConversationSelfChat)
                {
                    DebugHelper.WriteLine($"[LoadMessages] Self-chat local sem histórico. Exibindo placeholder vazio.");
                    _messages = new List<ChatMessage>();
                }
                else
                {
                    DebugHelper.WriteLine($"[LoadMessages] ChatService não inicializado, usando dados simulados");
                    _messages = GetSimulatedMessages();
                }

                DebugHelper.WriteLine($"[LoadMessages] Criando {_messages.Count} controles de mensagem...");

                foreach (var msg in _messages)
                {
                    var messageControl = CreateMessageControl(msg);
                    MessagesList.Children.Add(messageControl);
                }

                UpdateSelfChatPlaceholderVisibility();
                UpdateConversationSummaryFromMessages();
                DebugHelper.WriteLine($"[LoadMessages] Scrollando para última mensagem...");

                // Scroll para a última mensagem
                MessagesScrollViewer.ScrollToEnd();
                DebugHelper.WriteLine($"[LoadMessages] Sucesso!");
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[LoadMessages ERROR] {ex.GetType().Name}: {ex.Message}");
                DebugHelper.WriteLine($"[LoadMessages ERROR StackTrace] {ex.StackTrace}");
                throw;
            }
        }

        private List<ChatMessage> GetSimulatedMessages()
        {
            return new List<ChatMessage>
            {
                new() { SenderId = "other", SenderName = _contactName, Content = "Olá! Tudo bem?", Timestamp = DateTime.Now.AddMinutes(-30), IsOwn = false },
                new() { SenderId = _currentUser?.UserId ?? "", SenderName = _currentUser?.Name ?? "Você", Content = "Oi! Tudo certo! 😊", Timestamp = DateTime.Now.AddMinutes(-29), IsOwn = true },
                new() { SenderId = "other", SenderName = _contactName, Content = "Que ótimo! Eu queria conversar sobre o projeto...", Timestamp = DateTime.Now.AddMinutes(-28), IsOwn = false },
                new() { SenderId = _currentUser?.UserId ?? "", SenderName = _currentUser?.Name ?? "Você", Content = "Claro! Estou disponível agora", Timestamp = DateTime.Now.AddMinutes(-25), IsOwn = true },
                new() { SenderId = "other", SenderName = _contactName, Content = "Perfeito! Vamos começar?", Timestamp = DateTime.Now.AddMinutes(-5), IsOwn = false },
            };
        }

        private void UpdateSelfChatPlaceholderVisibility()
        {
            SelfChatPlaceholderPanel.Visibility = IsCurrentConversationSelfChat && _messages.Count == 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void UpdateConversationSummaryFromMessages()
        {
            var conversation = _conversations.FirstOrDefault(item => string.Equals(item.ContactId, _contactId, StringComparison.OrdinalIgnoreCase));
            if (conversation == null)
            {
                return;
            }

            conversation.HasUnread = false;

            if (_messages.Count == 0)
            {
                if (conversation.IsSelfConversation)
                {
                    conversation.LastMessage = SelfChatPreviewText;
                    conversation.LastMessageTime = DateTime.Now;
                }

                RenderConversationList();
                return;
            }

            var latestMessage = _messages.Last();
            conversation.LastMessage = latestMessage.ConversationPreview;
            conversation.LastMessageTime = latestMessage.Timestamp == default ? DateTime.Now : latestMessage.Timestamp;
            conversation.LastSenderId = latestMessage.SenderId;

            RenderConversationList();
        }

        private StackPanel CreateMessageControl(ChatMessage msg)
        {
            var container = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = msg.IsOwn ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                Margin = new Thickness(0, 8, 0, 0)
            };

            if (!msg.IsOwn)
            {
                var avatar = new Ellipse
                {
                    Width = 36,
                    Height = 36,
                    Fill = new SolidColorBrush(Color.FromRgb(0, 120, 212)),
                    Margin = new Thickness(0, 0, 8, 0),
                    VerticalAlignment = VerticalAlignment.Top
                };
                container.Children.Add(avatar);
            }

            var content = new StackPanel
            {
                Orientation = Orientation.Vertical,
                MaxWidth = 500
            };

            if (!msg.IsOwn)
            {
                var nameBlock = new TextBlock
                {
                    Text = msg.SenderName,
                    FontSize = 11,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)),
                    Margin = new Thickness(0, 0, 0, 4)
                };
                content.Children.Add(nameBlock);
            }

            var msgBorder = new Border
            {
                CornerRadius = new CornerRadius(12),
                Background = msg.IsDeleted
                    ? Brushes.Transparent
                    : msg.IsOwn 
                    ? new SolidColorBrush(Color.FromRgb(0, 120, 212))
                    : GetThemeBrush("MutedCardBackgroundBrush", Color.FromRgb(240, 240, 240)),
                Padding = msg.IsSticker ? new Thickness(8) : new Thickness(12, 8, 12, 8)
            };

            var primaryTextBrush = msg.IsOwn
                ? new SolidColorBrush(Colors.White)
                : GetThemeBrush("PrimaryTextBrush", Color.FromRgb(51, 51, 51));
            var secondaryTextBrush = msg.IsOwn
                ? new SolidColorBrush(Color.FromArgb(214, 255, 255, 255))
                : GetThemeBrush("SecondaryTextBrush", Color.FromRgb(100, 116, 139));

            if (msg.IsDeleted)
            {
                msgBorder.Child = new TextBlock
                {
                    Text = msg.DeletedDisplayText,
                    FontSize = 12,
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = GetThemeBrush("SecondaryTextBrush", Color.FromRgb(100, 116, 139)),
                    TextDecorations = TextDecorations.Strikethrough
                };
            }
            else if (msg.IsSticker)
            {
                var stickerContent = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Width = 150
                };

                var stickerSource = TryCreateStickerImageSource(msg.StickerAsset);
                if (stickerSource != null)
                {
                    stickerContent.Children.Add(new Image
                    {
                        Source = stickerSource,
                        Width = 110,
                        Height = 110,
                        Stretch = Stretch.Uniform,
                        HorizontalAlignment = HorizontalAlignment.Center
                    });
                }

                stickerContent.Children.Add(new TextBlock
                {
                    Text = GetStickerDisplayName(msg.StickerAsset),
                    FontSize = 11,
                    Margin = new Thickness(0, 6, 0, 0),
                    TextAlignment = TextAlignment.Center,
                    Foreground = msg.IsOwn
                        ? new SolidColorBrush(Color.FromArgb(230, 255, 255, 255))
                        : GetThemeBrush("SecondaryTextBrush", Color.FromRgb(100, 116, 139))
                });

                msgBorder.Child = stickerContent;
            }
            else
            {
                var messageContent = new StackPanel
                {
                    Orientation = Orientation.Vertical
                };

                if (ShouldShowMessageText(msg))
                {
                    messageContent.Children.Add(new TextBlock
                    {
                        Text = msg.Content,
                        FontSize = 13,
                        Foreground = primaryTextBrush,
                        TextWrapping = TextWrapping.WrapWithOverflow
                    });
                }

                if (msg.HasAttachment)
                {
                    messageContent.Children.Add(CreateAttachmentContent(msg, primaryTextBrush, secondaryTextBrush));
                }

                if (msg.HasLinkPreview)
                {
                    messageContent.Children.Add(CreateLinkPreviewContent(msg, primaryTextBrush, secondaryTextBrush));
                }

                msgBorder.Child = messageContent;
            }
            content.Children.Add(msgBorder);

            var timeBlock = new TextBlock
            {
                Text = msg.IsEdited && !msg.IsDeleted ? $"{msg.Timestamp:HH:mm} • editada" : msg.Timestamp.ToString("HH:mm"),
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153)),
                Margin = new Thickness(8, 4, 0, 0)
            };
            content.Children.Add(timeBlock);

            container.Children.Add(content);
            return container;
        }

        private bool ShouldShowMessageText(ChatMessage msg)
        {
            if (msg == null || msg.IsDeleted || msg.IsSticker)
            {
                return false;
            }

            var normalizedContent = (msg.Content ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedContent))
            {
                return false;
            }

            if (!msg.HasAttachment)
            {
                return true;
            }

            return !string.Equals(
                normalizedContent,
                BuildAttachmentPreviewText(msg.MessageType, msg.AttachmentFileName),
                StringComparison.OrdinalIgnoreCase);
        }

        private UIElement CreateAttachmentContent(ChatMessage msg, Brush primaryTextBrush, Brush secondaryTextBrush)
        {
            var card = new Border
            {
                Margin = new Thickness(0, ShouldShowMessageText(msg) ? 8 : 0, 0, 0),
                Padding = new Thickness(12),
                CornerRadius = new CornerRadius(12),
                Background = msg.IsOwn
                    ? new SolidColorBrush(Color.FromArgb(36, 255, 255, 255))
                    : GetThemeBrush("SearchBackgroundBrush", Color.FromRgb(248, 250, 252)),
                BorderBrush = msg.IsOwn
                    ? new SolidColorBrush(Color.FromArgb(70, 255, 255, 255))
                    : GetThemeBrush("SearchBorderBrush", Color.FromRgb(226, 232, 240)),
                BorderThickness = new Thickness(1)
            };

            var stack = new StackPanel { Orientation = Orientation.Vertical };
            var previewSource = TryCreateAttachmentPreviewSource(msg);
            if (previewSource != null)
            {
                stack.Children.Add(new Border
                {
                    CornerRadius = new CornerRadius(10),
                    ClipToBounds = true,
                    Margin = new Thickness(0, 0, 0, 10),
                    Child = new Image
                    {
                        Source = previewSource,
                        Height = 170,
                        Stretch = Stretch.UniformToFill
                    }
                });
            }

            stack.Children.Add(new TextBlock
            {
                Text = msg.AttachmentDisplayLabel + " anexado",
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = secondaryTextBrush
            });
            stack.Children.Add(new TextBlock
            {
                Text = string.IsNullOrWhiteSpace(msg.AttachmentFileName) ? "anexo" : msg.AttachmentFileName,
                Margin = new Thickness(0, 4, 0, 0),
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = primaryTextBrush,
                TextWrapping = TextWrapping.Wrap
            });
            stack.Children.Add(new TextBlock
            {
                Text = FormatAttachmentSize(msg.AttachmentSizeBytes),
                Margin = new Thickness(0, 4, 0, 0),
                FontSize = 11,
                Foreground = secondaryTextBrush,
                TextWrapping = TextWrapping.Wrap
            });

            var openButton = new Button
            {
                Content = string.IsNullOrWhiteSpace(msg.AttachmentLocalPath) || !File.Exists(msg.AttachmentLocalPath)
                    ? "Baixar e abrir"
                    : "Abrir",
                Margin = new Thickness(0, 10, 0, 0),
                Padding = new Thickness(12, 6, 12, 6),
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Left,
                Background = msg.IsOwn
                    ? new SolidColorBrush(Color.FromArgb(56, 255, 255, 255))
                    : GetThemeBrush("AccentBrush", Color.FromRgb(0, 120, 212)),
                Foreground = Brushes.White
            };
            openButton.Click += async (_, __) => await OpenAttachmentAsync(msg);
            stack.Children.Add(openButton);

            card.Child = stack;
            return card;
        }

        private UIElement CreateLinkPreviewContent(ChatMessage msg, Brush primaryTextBrush, Brush secondaryTextBrush)
        {
            var border = new Border
            {
                Margin = new Thickness(0, ShouldShowMessageText(msg) ? 8 : 0, 0, 0),
                Padding = new Thickness(12),
                CornerRadius = new CornerRadius(12),
                Background = msg.IsOwn
                    ? new SolidColorBrush(Color.FromArgb(36, 255, 255, 255))
                    : GetThemeBrush("SearchBackgroundBrush", Color.FromRgb(248, 250, 252)),
                BorderBrush = msg.IsOwn
                    ? new SolidColorBrush(Color.FromArgb(70, 255, 255, 255))
                    : GetThemeBrush("SearchBorderBrush", Color.FromRgb(226, 232, 240)),
                BorderThickness = new Thickness(1),
                Cursor = Cursors.Hand,
                ToolTip = "Abrir link"
            };

            var stack = new StackPanel { Orientation = Orientation.Vertical };
            stack.Children.Add(new TextBlock
            {
                Text = string.IsNullOrWhiteSpace(msg.LinkDisplayHost) ? "Link" : msg.LinkDisplayHost,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = secondaryTextBrush
            });
            stack.Children.Add(new TextBlock
            {
                Text = string.IsNullOrWhiteSpace(msg.LinkTitle) ? msg.LinkUrl : msg.LinkTitle,
                Margin = new Thickness(0, 4, 0, 0),
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = primaryTextBrush,
                TextWrapping = TextWrapping.Wrap
            });

            var description = string.IsNullOrWhiteSpace(msg.LinkDescription)
                ? msg.LinkUrl
                : msg.LinkDescription;
            if (!string.IsNullOrWhiteSpace(description))
            {
                stack.Children.Add(new TextBlock
                {
                    Text = description,
                    Margin = new Thickness(0, 6, 0, 0),
                    FontSize = 11,
                    Foreground = secondaryTextBrush,
                    TextWrapping = TextWrapping.Wrap
                });
            }

            border.Child = stack;
            border.MouseLeftButtonUp += (_, __) => OpenExternalLink(msg.LinkUrl);
            return border;
        }

        private ImageSource? TryCreateAttachmentPreviewSource(ChatMessage msg)
        {
            if (msg == null || !msg.IsImageAttachment || string.IsNullOrWhiteSpace(msg.AttachmentLocalPath) || !File.Exists(msg.AttachmentLocalPath))
            {
                return null;
            }

            try
            {
                return new BitmapImage(new Uri(msg.AttachmentLocalPath, UriKind.Absolute));
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[ChatWindow.AttachmentPreview] Falha ao montar preview: {ex.Message}");
                return null;
            }
        }

        private static string BuildAttachmentPreviewText(string messageType, string fileName)
        {
            var normalizedName = string.IsNullOrWhiteSpace(fileName) ? "anexo" : fileName;
            return messageType switch
            {
                "image" => $"Imagem • {normalizedName}",
                "video" => $"Video • {normalizedName}",
                "audio" => $"Audio • {normalizedName}",
                _ => $"Arquivo • {normalizedName}"
            };
        }

        private static string ResolveAttachmentMessageType(string fileName)
        {
            var extension = System.IO.Path.GetExtension(fileName)?.Trim().ToLowerInvariant() ?? string.Empty;
            return extension switch
            {
                ".png" or ".jpg" or ".jpeg" or ".gif" or ".webp" or ".bmp" => "image",
                ".mp4" or ".mov" or ".avi" or ".mkv" or ".wmv" or ".webm" => "video",
                ".mp3" or ".wav" or ".m4a" or ".aac" or ".ogg" or ".flac" or ".wma" => "audio",
                _ => "file"
            };
        }

        private static string GetAttachmentContentType(string fileName)
        {
            var extension = System.IO.Path.GetExtension(fileName)?.Trim().ToLowerInvariant() ?? string.Empty;
            return extension switch
            {
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".bmp" => "image/bmp",
                ".mp4" => "video/mp4",
                ".mov" => "video/quicktime",
                ".avi" => "video/x-msvideo",
                ".mkv" => "video/x-matroska",
                ".wmv" => "video/x-ms-wmv",
                ".webm" => "video/webm",
                ".mp3" => "audio/mpeg",
                ".wav" => "audio/wav",
                ".m4a" => "audio/mp4",
                ".aac" => "audio/aac",
                ".ogg" => "audio/ogg",
                ".flac" => "audio/flac",
                ".wma" => "audio/x-ms-wma",
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".txt" => "text/plain",
                ".zip" => "application/zip",
                ".rar" => "application/vnd.rar",
                _ => "application/octet-stream"
            };
        }

        private static string FormatAttachmentSize(long sizeBytes)
        {
            if (sizeBytes <= 0)
            {
                return "Tamanho nao informado";
            }

            string[] units = { "B", "KB", "MB", "GB" };
            var value = (double)sizeBytes;
            var unitIndex = 0;
            while (value >= 1024 && unitIndex < units.Length - 1)
            {
                value /= 1024;
                unitIndex++;
            }

            return $"{value:0.#} {units[unitIndex]}";
        }

        private ChatMessage NormalizeChatMessageForUi(ChatMessage message)
        {
            if (message == null)
            {
                return new ChatMessage();
            }

            if (message.Timestamp.Kind == DateTimeKind.Utc)
            {
                message.Timestamp = message.Timestamp.ToLocalTime();
            }

            if (message.EditedAt.HasValue && message.EditedAt.Value.Kind == DateTimeKind.Utc)
            {
                message.EditedAt = message.EditedAt.Value.ToLocalTime();
            }

            if (message.DeletedAt.HasValue && message.DeletedAt.Value.Kind == DateTimeKind.Utc)
            {
                message.DeletedAt = message.DeletedAt.Value.ToLocalTime();
            }

            return message;
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

        private string GetStickerDisplayName(string? assetFileName)
        {
            if (string.IsNullOrWhiteSpace(assetFileName))
            {
                return "Figurinha";
            }

            var normalized = System.IO.Path.GetFileNameWithoutExtension(assetFileName)
                ?.Replace('_', ' ')
                ?.Trim();

            if (string.IsNullOrWhiteSpace(normalized))
            {
                return "Figurinha";
            }

            return normalized ?? "Figurinha";
        }

        private async void SendButton_Click(object? sender, RoutedEventArgs? e)
        {
            var messageText = MessageInput.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(messageText))
            {
                MessageBox.Show("Digite uma mensagem!", "Chat", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var newMessage = new ChatMessage
            {
                SenderId = _currentUser?.UserId ?? string.Empty,
                SenderName = _currentUser?.Name ?? "Você",
                Content = messageText,
                MessageType = "text",
                Timestamp = DateTime.Now,
                IsOwn = true
            };

            ChatMessage messageToAppend = newMessage;
            if (_chatService != null)
            {
                DebugHelper.WriteLine("[SendButton_Click] Salvando mensagem no Firebase...");
                var sendResult = await _chatService.SendMessageAsync(_contactId, _contactName, newMessage.SenderName, messageText);

                if (!sendResult.Success)
                {
                    MessageBox.Show(
                        $"Erro ao enviar mensagem.\n\n{sendResult.ErrorMessage}\n\nLog salvo em:\n{DebugHelper.GetLogFilePath()}",
                        "Erro",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    DebugHelper.WriteLine($"[SendButton_Click] Falha ao salvar no Firebase: {sendResult.ErrorMessage}");
                    return;
                }

                messageToAppend = NormalizeChatMessageForUi(sendResult.Message ?? newMessage);
                DebugHelper.WriteLine("[SendButton_Click] Mensagem salva no Firebase com sucesso!");
            }

            AppendMessages(messageToAppend);
            MessageInput.Clear();
            MessageInput.Focus();
            DebugHelper.WriteLine("[SendButton_Click] Mensagem exibida localmente");
        }

        private async void AttachFileButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                Title = "Selecionar anexo para o chat",
                Filter = "Midia|*.png;*.jpg;*.jpeg;*.gif;*.webp;*.bmp;*.mp4;*.mov;*.avi;*.wmv;*.webm|Audio|*.mp3;*.wav;*.m4a;*.aac;*.ogg;*.flac;*.wma|Todos os arquivos|*.*"
            };

            if (dialog.ShowDialog() != true || dialog.FileNames.Length == 0)
            {
                return;
            }

            await SendAttachmentsAsync(dialog.FileNames);
            MessageInput.Focus();
        }

        private async Task SendAttachmentsAsync(IEnumerable<string> filePaths)
        {
            var sentMessages = new List<ChatMessage>();
            var failedFiles = new List<string>();

            foreach (var filePath in filePaths)
            {
                try
                {
                    var localMessage = CreateLocalAttachmentMessage(filePath);
                    ChatMessage messageToAppend = localMessage;

                    if (_chatService != null)
                    {
                        var sendResult = await _chatService.SendAttachmentMessageAsync(_contactId, _contactName, localMessage.SenderName, filePath);
                        if (!sendResult.Success)
                        {
                            failedFiles.Add($"{System.IO.Path.GetFileName(filePath)} ({sendResult.ErrorMessage})");
                            continue;
                        }

                        messageToAppend = NormalizeChatMessageForUi(sendResult.Message ?? localMessage);
                        if (string.IsNullOrWhiteSpace(messageToAppend.AttachmentLocalPath))
                        {
                            messageToAppend.AttachmentLocalPath = localMessage.AttachmentLocalPath;
                        }
                    }

                    sentMessages.Add(messageToAppend);
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine($"[ChatWindow.Attachments] Falha ao enviar {filePath}: {ex.Message}");
                    failedFiles.Add($"{System.IO.Path.GetFileName(filePath)} ({ex.Message})");
                }
            }

            if (sentMessages.Count > 0)
            {
                AppendMessages(sentMessages);
            }

            if (failedFiles.Count == 0)
            {
                return;
            }

            var failureText = string.Join(Environment.NewLine, failedFiles.Take(4));
            if (failedFiles.Count > 4)
            {
                failureText += $"{Environment.NewLine}... e mais {failedFiles.Count - 4} arquivo(s).";
            }

            MessageBox.Show(
                sentMessages.Count > 0
                    ? $"Alguns anexos nao puderam ser enviados:\n\n{failureText}"
                    : $"Nenhum anexo foi enviado.\n\n{failureText}",
                sentMessages.Count > 0 ? "Envio parcial" : "Falha ao enviar anexo",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        private ChatMessage CreateLocalAttachmentMessage(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            var messageType = ResolveAttachmentMessageType(fileInfo.Name);
            return new ChatMessage
            {
                MessageId = Guid.NewGuid().ToString("N"),
                DocumentId = Guid.NewGuid().ToString("N"),
                SenderId = _currentUser?.UserId ?? string.Empty,
                SenderName = _currentUser?.Name ?? "Você",
                Content = BuildAttachmentPreviewText(messageType, fileInfo.Name),
                MessageType = messageType,
                AttachmentFileName = fileInfo.Name,
                AttachmentContentType = GetAttachmentContentType(fileInfo.Name),
                AttachmentLocalPath = fileInfo.FullName,
                AttachmentSizeBytes = fileInfo.Exists ? fileInfo.Length : 0,
                Timestamp = DateTime.Now,
                IsOwn = true
            };
        }

        private void AppendMessages(params ChatMessage[] messages)
        {
            AppendMessages((IEnumerable<ChatMessage>)messages);
        }

        private void AppendMessages(IEnumerable<ChatMessage> messages)
        {
            var appendedMessages = messages
                .Where(message => message != null)
                .Select(NormalizeChatMessageForUi)
                .ToList();
            if (appendedMessages.Count == 0)
            {
                return;
            }

            foreach (var message in appendedMessages)
            {
                _messages.Add(message);
                MessagesList.Children.Add(CreateMessageControl(message));
            }

            UpdateSelfChatPlaceholderVisibility();
            UpdateConversationSummaryFromMessages();
            MessagesScrollViewer.ScrollToEnd();
        }

        private async Task OpenAttachmentAsync(ChatMessage msg)
        {
            var localPath = msg.AttachmentLocalPath;
            if (string.IsNullOrWhiteSpace(localPath) || !File.Exists(localPath))
            {
                if (_chatService == null)
                {
                    MessageBox.Show("Este dispositivo nao possui uma copia local do anexo e a sessao atual nao pode baixa-lo do Firebase.", "Anexo indisponivel", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var downloadResult = await _chatService.EnsureAttachmentLocalCopyAsync(msg);
                if (!downloadResult.Success || string.IsNullOrWhiteSpace(downloadResult.LocalPath) || !File.Exists(downloadResult.LocalPath))
                {
                    MessageBox.Show($"Nao foi possivel baixar o anexo.\n\n{downloadResult.ErrorMessage}", "Falha ao baixar", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                localPath = downloadResult.LocalPath;
                msg.AttachmentLocalPath = localPath;
                MessagesList.Children.Clear();
                foreach (var existingMessage in _messages)
                {
                    MessagesList.Children.Add(CreateMessageControl(existingMessage));
                }
                MessagesScrollViewer.ScrollToEnd();
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = localPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[ChatWindow.OpenAttachment] Falha ao abrir {localPath}: {ex.Message}");
                MessageBox.Show("O sistema nao conseguiu abrir este anexo no aplicativo padrao.", "Nao foi possivel abrir", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OpenExternalLink(string? linkUrl)
        {
            var normalizedUrl = (linkUrl ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedUrl))
            {
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = normalizedUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[ChatWindow.Link] Falha ao abrir {normalizedUrl}: {ex.Message}");
                MessageBox.Show("Nao foi possivel abrir o link desta mensagem.", "Link indisponivel", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

    }
}
