using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;

namespace MeuApp
{
    public partial class UserProfileViewWindow : MetroWindow
    {
        private readonly UserProfileViewModel _viewModel;

        public UserProfileViewWindow(UserProfile profile, UIElement avatarVisual)
        {
            InitializeComponent();
            _viewModel = new UserProfileViewModel(profile);
            DataContext = _viewModel;
            AvatarHost.Content = avatarVisual;
            OpenPortfolioButton.IsEnabled = _viewModel.HasPortfolio;
            OpenLinkedInButton.IsEnabled = _viewModel.HasLinkedIn;
        }

        private void OpenPortfolio_Click(object sender, RoutedEventArgs e)
        {
            OpenExternalLink(_viewModel.PortfolioUrl);
        }

        private void OpenLinkedIn_Click(object sender, RoutedEventArgs e)
        {
            OpenExternalLink(_viewModel.LinkedInUrl);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OpenExternalLink(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Não foi possível abrir o link informado.\n\n{ex.Message}",
                    "Link indisponível",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
    }

    public sealed class UserProfileViewModel
    {
        public UserProfileViewModel(UserProfile profile)
        {
            DisplayName = string.IsNullOrWhiteSpace(profile.Name) ? "Aluno sem identificação" : profile.Name;
            Headline = BuildHeadline(profile);
            ProfessionalSummary = string.IsNullOrWhiteSpace(profile.Bio)
                ? "Este aluno ainda não adicionou uma descrição profissional."
                : profile.Bio;
            EmailValue = Normalize(profile.Email, "Email não informado");
            PhoneValue = Normalize(profile.Phone, "Telefone não informado");
            CourseValue = Normalize(profile.Course, "Curso não informado");
            RegistrationValue = Normalize(profile.Registration, "Matrícula não informada");
            SkillsValue = Normalize(Join(profile.Skills, profile.ProgrammingLanguages), "Sem competências cadastradas até o momento.");
            PortfolioValue = Normalize(profile.PortfolioLink, "Portfólio não informado");
            LinkedInValue = Normalize(profile.LinkedInLink, "LinkedIn não informado");
            PortfolioUrl = NormalizeUrl(profile.PortfolioLink);
            LinkedInUrl = NormalizeUrl(profile.LinkedInLink);
            CourseBadge = string.IsNullOrWhiteSpace(profile.Course) ? "Curso não informado" : profile.Course;
            RegistrationBadge = string.IsNullOrWhiteSpace(profile.Registration) ? "Matrícula indisponível" : $"Matrícula: {profile.Registration}";
            LanguagesBadge = string.IsNullOrWhiteSpace(profile.ProgrammingLanguages) ? "Sem stack cadastrada" : profile.ProgrammingLanguages;
        }

        public string DisplayName { get; }
        public string Headline { get; }
        public string ProfessionalSummary { get; }
        public string EmailValue { get; }
        public string PhoneValue { get; }
        public string CourseValue { get; }
        public string RegistrationValue { get; }
        public string SkillsValue { get; }
        public string PortfolioValue { get; }
        public string LinkedInValue { get; }
        public string CourseBadge { get; }
        public string RegistrationBadge { get; }
        public string LanguagesBadge { get; }
        public string? PortfolioUrl { get; }
        public string? LinkedInUrl { get; }
        public bool HasPortfolio => !string.IsNullOrWhiteSpace(PortfolioUrl);
        public bool HasLinkedIn => !string.IsNullOrWhiteSpace(LinkedInUrl);

        private static string BuildHeadline(UserProfile profile)
        {
            var nickname = string.IsNullOrWhiteSpace(profile.Nickname) ? null : profile.Nickname;
            var title = string.IsNullOrWhiteSpace(profile.ProfessionalTitle) ? "Aluno" : profile.ProfessionalTitle;

            return nickname == null
                ? title
                : $"{title} • {nickname}";
        }

        private static string Join(string left, string right)
        {
            if (string.IsNullOrWhiteSpace(left))
            {
                return right ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(right))
            {
                return left;
            }

            return $"{left}\n\nStack: {right}";
        }

        private static string Normalize(string? value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }

        private static string? NormalizeUrl(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var trimmed = value.Trim();
            if (trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return trimmed;
            }

            return $"https://{trimmed}";
        }
    }
}
