using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using System.Threading.Tasks;

namespace MeuApp
{
    public partial class SearchResultsWindow : MetroWindow
    {
        // Evento disparado quando usuário iniciar conversa
        public delegate void ConversationStartedEventHandler(UserInfo selectedUser);
        public event ConversationStartedEventHandler? OnConversationStarted;
        public delegate void ConnectionCreatedEventHandler(UserInfo selectedUser);
        public event ConnectionCreatedEventHandler? OnConnectionCreated;

        public ObservableCollection<UserInfo> Results { get; set; }
        private string _searchQuery;
        private string _currentUserId;
        private UserProfile? _currentProfile;
        private readonly ConnectionService? _connectionService;

        public SearchResultsWindow(string query, string currentUserId, string idToken, UserProfile? currentProfile = null)
        {
            InitializeComponent();
            _searchQuery = query;
            _currentUserId = currentUserId;
            _currentProfile = currentProfile;
            _connectionService = string.IsNullOrWhiteSpace(idToken) || currentProfile == null ? null : new ConnectionService(idToken, currentProfile);
            Results = new ObservableCollection<UserInfo>();
            DataContext = this;
            SearchQueryText.Text = $"Pesquisando conexões por: \"{query}\"";
        }

        public void SetResults(System.Collections.Generic.List<UserInfo> users)
        {
            Results.Clear();

            if (users == null || users.Count == 0)
            {
                NoResultsText.Visibility = Visibility.Visible;
                LoadingText.Visibility = Visibility.Collapsed;
                ResultsList.Visibility = Visibility.Collapsed;
                return;
            }

            foreach (var user in users)
            {
                Results.Add(user);
            }

            NoResultsText.Visibility = Visibility.Collapsed;
            LoadingText.Visibility = Visibility.Collapsed;
            ResultsList.Visibility = Visibility.Visible;
        }

        public void ShowLoading()
        {
            ResultsList.Visibility = Visibility.Collapsed;
            NoResultsText.Visibility = Visibility.Collapsed;
            LoadingText.Visibility = Visibility.Visible;
        }

        private async void AddFriend_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.DataContext is not UserInfo user)
            {
                return;
            }

            if (_connectionService == null)
            {
                MessageBox.Show(
                    "Sua sessão atual não possui credenciais para criar conexões. Faça login novamente e tente outra vez.",
                    "Conexão indisponível",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            user.IsConnecting = true;
            try
            {
                var result = await _connectionService.CreateConnectionRequestAsync(user);
                if (!result.Success)
                {
                    MessageBox.Show(
                        $"Não foi possível enviar a solicitação de conexão agora.\n\n{result.ErrorMessage}",
                        "Falha ao conectar",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                user.ConnectionState = "pendingOutgoing";
                OnConnectionCreated?.Invoke(user);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erro inesperado ao criar conexão:\n\n{ex.Message}",
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                user.IsConnecting = false;
            }
        }

        private void StartConversation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DebugHelper.WriteLine($"[StartConversation_Click] EVENTO DISPARADO");
                
                if (sender is Button button && button.DataContext is UserInfo user)
                {
                    DebugHelper.WriteLine($"[StartConversation] UserInfo encontrado: {user.Name}");
                    
                    // Disparar evento para o MainWindow tratar
                    OnConversationStarted?.Invoke(user);
                    
                    // Fechar janela de resultados
                    this.Close();
                    return;
                }

                MessageBox.Show("Erro: Usuário não selecionado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"[StartConversation ERROR] {ex.GetType().Name}: {ex.Message}");
                DebugHelper.WriteLine($"[StartConversation ERROR StackTrace] {ex.StackTrace}");
                
                MessageBox.Show(
                    $"Erro ao iniciar conversa:\n\n{ex.Message}",
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }
}
