using System.Windows;
using System.Windows.Controls;

namespace MeuAppWPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Funcionou 🔥");
        }

        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string tag = button.Tag?.ToString() ?? "";
                
                // Oculta todo conteúdo
                ChatsContent.Visibility = Visibility.Collapsed;
                TeamsContent.Visibility = Visibility.Collapsed;
                CalendarContent.Visibility = Visibility.Collapsed;
                FilesContent.Visibility = Visibility.Collapsed;
                SettingsContent.Visibility = Visibility.Collapsed;

                // Mostra o conteúdo correspondente
                switch (tag)
                {
                    case "Chats":
                        ChatsContent.Visibility = Visibility.Visible;
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
                        break;
                }
            }
        }
    }
}