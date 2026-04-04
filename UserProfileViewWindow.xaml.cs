using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls;

namespace MeuApp
{
    public partial class UserProfileViewWindow : MetroWindow
    {
        private readonly UserProfileViewModel _viewModel;

        public UserProfileViewWindow(UserProfile profile, UIElement avatarVisual, IReadOnlyList<TeamWorkspaceInfo>? featuredProjects = null)
        {
            InitializeComponent();
            _viewModel = new UserProfileViewModel(profile, featuredProjects ?? Array.Empty<TeamWorkspaceInfo>());
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

        private void PreviewImage_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.Tag is not ImagePreviewCardViewModel image || image.PreviewImage == null)
            {
                return;
            }

            var items = BuildViewerItems(image);
            if (items.Count == 0)
            {
                return;
            }

            var initialIndex = image is ProfileGalleryAlbumViewModel
                ? 0
                : items.FindIndex(item => string.Equals(item.ItemId, image.ItemId, StringComparison.OrdinalIgnoreCase));
            if (initialIndex < 0)
            {
                initialIndex = 0;
            }

            var previewWindow = new GalleryImageViewerWindow(
                items,
                initialIndex,
                this,
                Color.FromRgb(56, 189, 248),
                allowAdjustment: false,
                contextLabel: image is ProjectImagePreviewViewModel
                    ? "Projeto em destaque • somente leitura"
                    : "Galeria do aluno • somente leitura");
            previewWindow.ShowDialog();
        }

        private List<GalleryViewerItem> BuildViewerItems(ImagePreviewCardViewModel selectedImage)
        {
            if (selectedImage is ProfileGalleryAlbumViewModel album)
            {
                return album.BuildViewerItems();
            }

            if (selectedImage is ProfileGalleryImageViewModel)
            {
                return _viewModel.GalleryImages
                    .OfType<ProfileGalleryImageViewModel>()
                    .Select(item => item.ToViewerItem())
                    .Where(item => item != null)
                    .Cast<GalleryViewerItem>()
                    .ToList();
            }

            var ownerProject = _viewModel.FeaturedProjects
                .FirstOrDefault(project => project.PreviewImages.Any(image => string.Equals(image.ItemId, selectedImage.ItemId, StringComparison.OrdinalIgnoreCase)));

            var sourceItems = ownerProject?.PreviewImages.Cast<ImagePreviewCardViewModel>() ?? new[] { selectedImage };
            return sourceItems
                .Select(item => item.ToViewerItem())
                .Where(item => item != null)
                .Cast<GalleryViewerItem>()
                .ToList();
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
        public UserProfileViewModel(UserProfile profile, IReadOnlyCollection<TeamWorkspaceInfo> featuredProjects)
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
            GalleryImages = BuildGalleryImages(profile.GalleryImages);
            FeaturedProjects = BuildFeaturedProjects(featuredProjects);

            var hiddenProjectsCount = Math.Max(0, (profile.FeaturedProjectIds?.Count ?? 0) - FeaturedProjects.Count);
            GalleryVisibility = GalleryImages.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            EmptyGalleryVisibility = GalleryImages.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            FeaturedProjectsVisibility = FeaturedProjects.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            EmptyFeaturedProjectsVisibility = FeaturedProjects.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            RestrictedProjectsNoteVisibility = hiddenProjectsCount > 0 ? Visibility.Visible : Visibility.Collapsed;
            EmptyGalleryMessage = "Este aluno ainda não publicou imagens ou blocos de galeria no currículo.";
            EmptyFeaturedProjectsMessage = hiddenProjectsCount > 0
                ? "Este aluno destacou projetos, mas eles não estão disponíveis para o seu acesso."
                : "Este aluno ainda não destacou projetos no perfil.";
            RestrictedProjectsNote = hiddenProjectsCount == 1
                ? "1 projeto destacado foi ocultado por restrição de acesso."
                : $"{hiddenProjectsCount} projetos destacados foram ocultados por restrição de acesso.";
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
        public string EmptyGalleryMessage { get; }
        public string EmptyFeaturedProjectsMessage { get; }
        public string RestrictedProjectsNote { get; }
        public string? PortfolioUrl { get; }
        public string? LinkedInUrl { get; }
        public IReadOnlyList<ImagePreviewCardViewModel> GalleryImages { get; }
        public IReadOnlyList<FeaturedProjectViewModel> FeaturedProjects { get; }
        public Visibility GalleryVisibility { get; }
        public Visibility EmptyGalleryVisibility { get; }
        public Visibility FeaturedProjectsVisibility { get; }
        public Visibility EmptyFeaturedProjectsVisibility { get; }
        public Visibility RestrictedProjectsNoteVisibility { get; }
        public bool HasPortfolio => !string.IsNullOrWhiteSpace(PortfolioUrl);
        public bool HasLinkedIn => !string.IsNullOrWhiteSpace(LinkedInUrl);

        public static ImageSource? CreateImageSourceFromDataUri(string? dataUri)
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
            catch
            {
                return null;
            }
        }

        private static IReadOnlyList<ImagePreviewCardViewModel> BuildGalleryImages(IEnumerable<ProfileGalleryImage>? images)
        {
            var orderedImages = (images ?? Enumerable.Empty<ProfileGalleryImage>())
                .OrderByDescending(image => image.AddedAt == default ? DateTime.MinValue : image.AddedAt)
                .ToList();

            var entries = new List<ImagePreviewCardViewModel>();
            var processedAlbums = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var image in orderedImages)
            {
                var albumId = image.GalleryAlbumId?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(albumId))
                {
                    var singleImage = new ProfileGalleryImageViewModel(image);
                    if (singleImage.PreviewImage != null)
                    {
                        entries.Add(singleImage);
                    }

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
                var album = new ProfileGalleryAlbumViewModel(albumImages);
                if (album.PreviewImage != null)
                {
                    entries.Add(album);
                }
            }

            return entries;
        }

        private static IReadOnlyList<FeaturedProjectViewModel> BuildFeaturedProjects(IEnumerable<TeamWorkspaceInfo>? teams)
        {
            return (teams ?? Enumerable.Empty<TeamWorkspaceInfo>())
                .Select(team => new FeaturedProjectViewModel(team))
                .ToList();
        }

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

    public abstract class ImagePreviewCardViewModel
    {
        protected ImagePreviewCardViewModel(
            string itemId,
            string title,
            string subtitle,
            string? description,
            string? imageDataUri,
            double cardWidth,
            double cardHeight,
            string? countLabel = null)
        {
            ItemId = string.IsNullOrWhiteSpace(itemId) ? Guid.NewGuid().ToString("N") : itemId;
            Title = string.IsNullOrWhiteSpace(title) ? "Imagem" : title;
            Subtitle = subtitle;
            Description = description ?? string.Empty;
            CardText = !string.IsNullOrWhiteSpace(Description) ? Description : Subtitle;
            PreviewImage = UserProfileViewModel.CreateImageSourceFromDataUri(imageDataUri);
            CardWidth = cardWidth;
            CardHeight = cardHeight;
            CountLabel = countLabel ?? string.Empty;
            CountLabelVisibility = string.IsNullOrWhiteSpace(CountLabel) ? Visibility.Collapsed : Visibility.Visible;
        }

        public string ItemId { get; }
        public string Title { get; }
        public string Subtitle { get; }
        public string Description { get; }
        public string CardText { get; }
        public ImageSource? PreviewImage { get; }
        public double CardWidth { get; }
        public double CardHeight { get; }
        public string CountLabel { get; }
        public Visibility CountLabelVisibility { get; }

        public virtual GalleryViewerItem? ToViewerItem()
        {
            return PreviewImage == null
                ? null
                : new GalleryViewerItem(ItemId, PreviewImage, Title, Subtitle, Description);
        }
    }

    public sealed class ProfileGalleryImageViewModel : ImagePreviewCardViewModel
    {
        public ProfileGalleryImageViewModel(ProfileGalleryImage image)
            : base(
                image.ImageId,
                string.IsNullOrWhiteSpace(image.Title) ? "Imagem do currículo" : image.Title,
                image.AddedAt == default ? "Galeria do currículo" : $"Adicionada em {image.AddedAt:dd/MM/yyyy}",
                image.Description,
                image.ImageDataUri,
                160,
                160)
        {
        }
    }

    public sealed class ProfileGalleryAlbumViewModel : ImagePreviewCardViewModel
    {
        public ProfileGalleryAlbumViewModel(IReadOnlyList<ProfileGalleryImage> images)
            : base(
                GetAlbumItemId(images),
                GetAlbumTitle(images),
                $"{GetAlbumCountLabel(images)} • bloco do currículo",
                GetAlbumDescription(images),
                GetAlbumCoverDataUri(images),
                248,
                176,
                GetAlbumCountLabel(images))
        {
            Images = images ?? Array.Empty<ProfileGalleryImage>();
        }

        public IReadOnlyList<ProfileGalleryImage> Images { get; }

        public List<GalleryViewerItem> BuildViewerItems()
        {
            var totalCount = Math.Max(1, Images.Count);
            return Images
                .OrderBy(image => image.AddedAt == default ? DateTime.Now : image.AddedAt)
                .Select((image, index) => CreateViewerItem(image, index, totalCount))
                .Where(item => item != null)
                .Cast<GalleryViewerItem>()
                .ToList();
        }

        private GalleryViewerItem? CreateViewerItem(ProfileGalleryImage image, int index, int totalCount)
        {
            var source = UserProfileViewModel.CreateImageSourceFromDataUri(image.ImageDataUri);
            if (source == null)
            {
                return null;
            }

            var imageLabel = string.IsNullOrWhiteSpace(image.Title)
                ? $"Foto {index + 1} de {totalCount}"
                : $"{image.Title} • {index + 1} de {totalCount}";
            var description = !string.IsNullOrWhiteSpace(image.Description)
                ? image.Description
                : GetAlbumDescription(Images);

            return new GalleryViewerItem(
                image.ImageId,
                source,
                Title,
                $"{GetAlbumCountLabel(Images)} • {imageLabel}",
                description);
        }

        private static string GetAlbumItemId(IReadOnlyList<ProfileGalleryImage>? images)
        {
            var albumId = (images ?? Array.Empty<ProfileGalleryImage>())
                .Select(image => image.GalleryAlbumId)
                .FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));
            return string.IsNullOrWhiteSpace(albumId) ? Guid.NewGuid().ToString("N") : albumId;
        }

        private static string GetAlbumTitle(IReadOnlyList<ProfileGalleryImage>? images)
        {
            var title = (images ?? Array.Empty<ProfileGalleryImage>())
                .Select(image => image.GalleryAlbumTitle)
                .FirstOrDefault(text => !string.IsNullOrWhiteSpace(text));
            if (string.IsNullOrWhiteSpace(title))
            {
                title = (images ?? Array.Empty<ProfileGalleryImage>())
                    .Select(image => image.Title)
                    .FirstOrDefault(text => !string.IsNullOrWhiteSpace(text));
            }

            return string.IsNullOrWhiteSpace(title) ? "Galeria do evento" : title.Trim();
        }

        private static string GetAlbumDescription(IReadOnlyList<ProfileGalleryImage>? images)
        {
            var description = (images ?? Array.Empty<ProfileGalleryImage>())
                .Select(image => image.GalleryAlbumDescription)
                .FirstOrDefault(text => !string.IsNullOrWhiteSpace(text));
            if (string.IsNullOrWhiteSpace(description))
            {
                description = (images ?? Array.Empty<ProfileGalleryImage>())
                    .Select(image => image.Description)
                    .FirstOrDefault(text => !string.IsNullOrWhiteSpace(text));
            }

            return description?.Trim() ?? string.Empty;
        }

        private static string GetAlbumCoverDataUri(IReadOnlyList<ProfileGalleryImage>? images)
        {
            return (images ?? Array.Empty<ProfileGalleryImage>())
                .OrderByDescending(image => image.AddedAt == default ? DateTime.MinValue : image.AddedAt)
                .Select(image => image.ImageDataUri)
                .FirstOrDefault(dataUri => !string.IsNullOrWhiteSpace(dataUri)) ?? string.Empty;
        }

        private static string GetAlbumCountLabel(IReadOnlyList<ProfileGalleryImage>? images)
        {
            var count = (images ?? Array.Empty<ProfileGalleryImage>()).Count;
            return count == 1 ? "1 foto" : $"{Math.Max(0, count)} fotos";
        }
    }

    public sealed class ProjectImagePreviewViewModel : ImagePreviewCardViewModel
    {
        public ProjectImagePreviewViewModel(TeamAssetInfo asset)
            : base(
                asset.AssetId,
                string.IsNullOrWhiteSpace(asset.FileName) ? "Imagem do projeto" : asset.FileName,
                asset.AddedAt == default ? "Projeto publicado" : $"Publicada em {asset.AddedAt:dd/MM/yyyy}",
                string.Empty,
                asset.PreviewImageDataUri,
                132,
                104)
        {
        }
    }

    public sealed class FeaturedProjectViewModel
    {
        public FeaturedProjectViewModel(TeamWorkspaceInfo team)
        {
            DisplayName = string.IsNullOrWhiteSpace(team.TeamName) ? "Projeto sem nome" : team.TeamName;
            Subtitle = BuildSubtitle(team);
            StatusLabel = string.IsNullOrWhiteSpace(team.ProjectStatus) ? "Projeto em andamento" : team.ProjectStatus;
            ProgressLabel = $"{Math.Clamp(team.ProjectProgress, 0, 100)}%";
            var memberCount = team.Members?.Count ?? 0;
            MembersLabel = memberCount == 1 ? "1 integrante" : $"{memberCount} integrantes";
            DeadlineLabel = team.ProjectDeadline.HasValue
                ? $"Prazo {team.ProjectDeadline.Value:dd/MM/yyyy}"
                : "Sem prazo definido";
            UpdatedAtLabel = team.UpdatedAt == default
                ? "Atualização não informada"
                : $"Atualizado em {team.UpdatedAt:dd/MM/yyyy}";
            Summary = BuildSummary(team);
            PreviewImages = (team.Assets ?? new List<TeamAssetInfo>())
                .Where(asset => !string.IsNullOrWhiteSpace(asset.PreviewImageDataUri))
                .OrderByDescending(asset => asset.AddedAt)
                .Select(asset => new ProjectImagePreviewViewModel(asset))
                .Where(asset => asset.PreviewImage != null)
                .Take(6)
                .ToList();
            PreviewImagesVisibility = PreviewImages.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            EmptyPreviewVisibility = PreviewImages.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            EmptyPreviewMessage = "Nenhuma imagem do projeto foi publicada até o momento.";
        }

        public string DisplayName { get; }
        public string Subtitle { get; }
        public string StatusLabel { get; }
        public string ProgressLabel { get; }
        public string MembersLabel { get; }
        public string DeadlineLabel { get; }
        public string UpdatedAtLabel { get; }
        public string Summary { get; }
        public string EmptyPreviewMessage { get; }
        public IReadOnlyList<ProjectImagePreviewViewModel> PreviewImages { get; }
        public Visibility PreviewImagesVisibility { get; }
        public Visibility EmptyPreviewVisibility { get; }

        private static string BuildSubtitle(TeamWorkspaceInfo team)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(team.Course))
            {
                parts.Add(team.Course);
            }

            if (!string.IsNullOrWhiteSpace(team.ClassName))
            {
                parts.Add(team.ClassName);
            }

            if (!string.IsNullOrWhiteSpace(team.TeamId))
            {
                parts.Add($"Código {team.TeamId}");
            }

            return parts.Count == 0 ? "Projeto acadêmico destacado pelo aluno." : string.Join(" • ", parts);
        }

        private static string BuildSummary(TeamWorkspaceInfo team)
        {
            var parts = new List<string>();

            if (team.Ucs != null && team.Ucs.Count > 0)
            {
                parts.Add(team.Ucs.Count == 1 ? $"UC: {team.Ucs[0]}" : $"{team.Ucs.Count} UCs vinculadas");
            }

            if (team.Milestones != null && team.Milestones.Count > 0)
            {
                parts.Add(team.Milestones.Count == 1 ? "1 marco registrado" : $"{team.Milestones.Count} marcos registrados");
            }

            if (parts.Count == 0)
            {
                parts.Add("Projeto disponível para consulta conforme suas permissões.");
            }

            return string.Join(" • ", parts);
        }
    }
}
