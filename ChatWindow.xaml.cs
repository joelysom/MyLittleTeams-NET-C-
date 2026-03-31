using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using MahApps.Metro.Controls;

namespace MeuApp
{
    public partial class ChatWindow : MetroWindow
    {
        private UserProfile? _currentUser;
        private string _contactId = string.Empty;
        private string _contactName = string.Empty;
        private List<ChatMessage> _messages = new();
        private ChatService? _chatService;
        private string _idToken = "";

        public ChatWindow()
        {
            InitializeComponent();
            this.GlowBrush = new SolidColorBrush(Color.FromRgb(0, 120, 212));
            this.KeyDown += ChatWindow_KeyDown;
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
                
                ContactNameText.Text = _contactName;
                DebugHelper.WriteLine($"[LoadChatData] ContactNameText definido");
                
                ContactStatusText.Text = "Online";
                DebugHelper.WriteLine($"[LoadChatData] ContactStatusText definido");

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

        private void LoadConversations()
        {
            try
            {
                DebugHelper.WriteLine($"[LoadConversations] Limpando lista...");
                ConversationsList.Children.Clear();
                DebugHelper.WriteLine($"[LoadConversations] Lista limpa");

                var conversations = new List<(string id, string name, string lastMessage, string time, bool unread)>
                {
                    (_contactId, _contactName, "Ótimo! Vamos começar?", "Agora", false),
                    ("user2", "João Silva", "Combinado, até logoo!", "há 2 min", true),
                    ("user3", "Maria Santos", "Pode ser amanhã?", "há 15 min", false),
                    ("user4", "Pedro Costa", "Enviou um arquivo", "há 1 hora", false),
                    ("user5", "Ana Lima", "Perfeito!", "há 3 horas", false),
                };

                DebugHelper.WriteLine($"[LoadConversations] Criando {conversations.Count} botões de conversa...");
                foreach (var (id, name, lastMsg, time, unread) in conversations)
                {
                    var button = CreateConversationButton(id, name, lastMsg, time, unread, id == _contactId);
                    ConversationsList.Children.Add(button);
                }
                DebugHelper.WriteLine($"[LoadConversations] Sucesso!");
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[LoadConversations ERROR] {ex.GetType().Name}: {ex.Message}");
                DebugHelper.WriteLine($"[LoadConversations ERROR StackTrace] {ex.StackTrace}");
                throw;
            }
        }

        private Button CreateConversationButton(string id, string name, string lastMessage, string time, bool unread, bool isSelected)
        {
            var button = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Background = isSelected ? new SolidColorBrush(Color.FromRgb(245, 245, 245)) : new SolidColorBrush(Colors.White),
                Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
                BorderThickness = new Thickness(0),
                Height = 76,
                Padding = new Thickness(12),
                Cursor = Cursors.Hand,
                Tag = id
            };

            var grid = new Grid { HorizontalAlignment = HorizontalAlignment.Stretch };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(48) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });

            var avatar = new Ellipse
            {
                Width = 48,
                Height = 48,
                Fill = new SolidColorBrush(Color.FromRgb(0, 120, 212)),
                Margin = new Thickness(0, 0, 12, 0)
            };
            Grid.SetColumn(avatar, 0);
            grid.Children.Add(avatar);

            var content = new StackPanel
            {
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(content, 1);

            var nameBlock = new TextBlock
            {
                Text = name,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            content.Children.Add(nameBlock);

            var msgBlock = new TextBlock
            {
                Text = unread ? $"• {lastMessage}" : lastMessage,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(119, 119, 119)),
                Margin = new Thickness(0, 4, 0, 0),
                TextTrimming = TextTrimming.CharacterEllipsis,
                FontWeight = unread ? FontWeights.SemiBold : FontWeights.Normal
            };
            content.Children.Add(msgBlock);
            grid.Children.Add(content);

            var timeBlock = new TextBlock
            {
                Text = time,
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153)),
                VerticalAlignment = VerticalAlignment.Top
            };
            Grid.SetColumn(timeBlock, 2);
            grid.Children.Add(timeBlock);

            button.Content = grid;
            button.Click += (s, e) =>
            {
                ContactNameText.Text = name;
                LoadMessages();
            };

            return button;
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
                        _messages = firestoreMessages;
                    }
                    else
                    {
                        DebugHelper.WriteLine($"[LoadMessages] Nenhuma mensagem encontrada no Firebase, usando dados simulados");
                        _messages = GetSimulatedMessages();
                    }
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
                Background = msg.IsOwn 
                    ? new SolidColorBrush(Color.FromRgb(0, 120, 212))
                    : new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                Padding = new Thickness(12, 8, 12, 8)
            };

            var msgText = new TextBlock
            {
                Text = msg.Content,
                FontSize = 13,
                Foreground = msg.IsOwn 
                    ? new SolidColorBrush(Colors.White)
                    : new SolidColorBrush(Color.FromRgb(51, 51, 51)),
                TextWrapping = TextWrapping.WrapWithOverflow
            };

            msgBorder.Child = msgText;
            content.Children.Add(msgBorder);

            var timeBlock = new TextBlock
            {
                Text = msg.Timestamp.ToString("HH:mm"),
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153)),
                Margin = new Thickness(8, 4, 0, 0)
            };
            content.Children.Add(timeBlock);

            container.Children.Add(content);
            return container;
        }

        private async void SendButton_Click(object? sender, RoutedEventArgs? e)
        {
            var messageText = MessageInput.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(messageText))
            {
                MessageBox.Show("Digite uma mensagem!", "Chat", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Criar nova mensagem
            var newMessage = new ChatMessage
            {
                SenderId = _currentUser?.UserId ?? "",
                SenderName = _currentUser?.Name ?? "Você",
                Content = messageText,
                Timestamp = DateTime.Now,
                IsOwn = true
            };

            // Tentar salvar no Firebase
            if (_chatService != null)
            {
                DebugHelper.WriteLine($"[SendButton_Click] Salvando mensagem no Firebase...");
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
                
                DebugHelper.WriteLine($"[SendButton_Click] Mensagem salva no Firebase com sucesso!");
            }

            // Adicionar à lista local e exibir
            _messages.Add(newMessage);
            var messageControl = CreateMessageControl(newMessage);
            MessagesList.Children.Add(messageControl);

            // Limpar input
            MessageInput.Clear();
            MessageInput.Focus();

            // Scroll para a última mensagem
            MessagesScrollViewer.ScrollToEnd();

            DebugHelper.WriteLine($"[SendButton_Click] Mensagem exibida localmente");
        }

    }

    public class ChatMessage
    {
        public string SenderId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool IsOwn { get; set; }
    }
}
