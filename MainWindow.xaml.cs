using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;

namespace MeuApp
{
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            this.GlowBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 120, 212));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Funcionou!");
        }

        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string tag = button.Tag?.ToString() ?? "";
                ResetNavigation();
                
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

        private void ResetNavigation()
        {
            ChatsContent.Visibility = Visibility.Collapsed;
            TeamsContent.Visibility = Visibility.Collapsed;
            CalendarContent.Visibility = Visibility.Collapsed;
            FilesContent.Visibility = Visibility.Collapsed;
            SettingsContent.Visibility = Visibility.Collapsed;
        }
    }
}