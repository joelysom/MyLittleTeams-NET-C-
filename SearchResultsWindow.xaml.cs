using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;

namespace MeuApp
{
    public partial class SearchResultsWindow : MetroWindow
    {
        // Evento disparado quando usuário iniciar conversa
        public delegate void ConversationStartedEventHandler(UserInfo selectedUser);
        public event ConversationStartedEventHandler? OnConversationStarted;

        public ObservableCollection<UserInfo> Results { get; set; }
        private string _searchQuery;
        private string _currentUserId;
        private UserProfile? _currentProfile;

        public SearchResultsWindow(string query, string currentUserId, UserProfile? currentProfile = null)
        {
            InitializeComponent();
            _searchQuery = query;
            _currentUserId = currentUserId;
            _currentProfile = currentProfile;
            Results = new ObservableCollection<UserInfo>();
            DataContext = this;
            SearchQueryText.Text = $"Buscando por: \"{query}\"";
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

        private void AddFriend_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string friendId)
            {
                MessageBox.Show(
                    $"Amigo adicionado com sucesso! 👋\n\nUserId: {friendId}",
                    "Sucesso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
                // TODO: Implementar lógica de adicionar amigo no banco de dados
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
