using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls;

namespace MeuApp
{
    public sealed class GalleryViewerItem
    {
        public GalleryViewerItem(string itemId, ImageSource imageSource, string title, string? subtitle = null, string? description = null)
        {
            ItemId = itemId;
            ImageSource = imageSource;
            Title = string.IsNullOrWhiteSpace(title) ? "Imagem" : title;
            Subtitle = subtitle ?? string.Empty;
            Description = description ?? string.Empty;
        }

        public string ItemId { get; }
        public ImageSource ImageSource { get; }
        public string Title { get; }
        public string Subtitle { get; }
        public string Description { get; }
    }

    public sealed class GalleryImageViewerWindow : Window
    {
        private readonly IReadOnlyList<GalleryViewerItem> _items;
        private readonly ScrollViewer _scrollViewer;
        private readonly Grid _imageStage;
        private readonly Image _image;
        private readonly Slider _zoomSlider;
        private readonly TextBlock _zoomValueText;
        private readonly TextBlock _resolutionText;
        private readonly TextBlock _titleText;
        private readonly TextBlock _subtitleText;
        private readonly TextBlock _descriptionText;
        private readonly TextBlock _counterText;
        private readonly TextBlock _dimensionChipText;
        private readonly TextBlock _contextText;
        private readonly TextBlock _helperText;
        private readonly StackPanel _thumbnailPanel;
        private readonly Border _thumbnailSection;
        private readonly Button _previousButton;
        private readonly Button _nextButton;
        private readonly Button _fitButton;
        private readonly bool _allowAdjustment;
        private readonly string _contextLabel;
        private readonly Color _accent;
        private int _currentIndex;
        private double _sourceWidth;
        private double _sourceHeight;

        public GalleryImageViewerWindow(
            ImageSource imageSource,
            string title,
            string? subtitle = null,
            Window? owner = null,
            Color? accentColor = null,
            bool allowAdjustment = true,
            string? contextLabel = null)
            : this(
                new[]
                {
                    new GalleryViewerItem(Guid.NewGuid().ToString("N"), imageSource, title, subtitle)
                },
                0,
                owner,
                accentColor,
                allowAdjustment,
                contextLabel)
        {
        }

        public GalleryImageViewerWindow(
            IReadOnlyList<GalleryViewerItem> items,
            int initialIndex = 0,
            Window? owner = null,
            Color? accentColor = null,
            bool allowAdjustment = true,
            string? contextLabel = null)
        {
            _items = (items ?? Array.Empty<GalleryViewerItem>())
                .Where(item => item?.ImageSource != null)
                .ToList();
            if (_items.Count == 0)
            {
                throw new ArgumentException("GalleryImageViewerWindow requires at least one image item.", nameof(items));
            }

            _allowAdjustment = allowAdjustment;
            _contextLabel = string.IsNullOrWhiteSpace(contextLabel)
                ? (allowAdjustment ? "Sua galeria profissional" : "Modo somente leitura")
                : contextLabel;
            _accent = accentColor ?? Color.FromRgb(56, 189, 248);
            _currentIndex = Math.Max(0, Math.Min(initialIndex, _items.Count - 1));

            Title = _items[_currentIndex].Title;
            Owner = owner;
            Width = 1320;
            Height = 900;
            MinWidth = 980;
            MinHeight = 700;
            WindowStartupLocation = owner == null ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.CanResize;
            Background = new SolidColorBrush(Color.FromRgb(4, 9, 18));
            KeyDown += OnViewerKeyDown;

            var root = new Grid { Margin = new Thickness(24) };
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var header = new Grid();
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var headingStack = new StackPanel();
            _contextText = new TextBlock
            {
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White
            };
            headingStack.Children.Add(CreateInfoChip(_contextText, _accent));

            _titleText = new TextBlock
            {
                Margin = new Thickness(0, 8, 0, 0),
                FontSize = 30,
                FontWeight = FontWeights.ExtraBold,
                Foreground = Brushes.White,
                TextWrapping = TextWrapping.Wrap
            };
            headingStack.Children.Add(_titleText);

            _subtitleText = new TextBlock
            {
                Margin = new Thickness(0, 8, 0, 0),
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
                TextWrapping = TextWrapping.Wrap
            };
            headingStack.Children.Add(_subtitleText);
            header.Children.Add(headingStack);

            var closeButton = new Button
            {
                Content = "Fechar",
                Width = 124,
                Height = 42,
                Margin = new Thickness(18, 0, 0, 0),
                Background = new SolidColorBrush(Color.FromRgb(15, 23, 42)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(51, 65, 85)),
                BorderThickness = new Thickness(1),
                FontWeight = FontWeights.SemiBold,
                Cursor = Cursors.Hand,
                VerticalAlignment = VerticalAlignment.Top
            };
            closeButton.Click += (_, __) => Close();
            Grid.SetColumn(closeButton, 1);
            header.Children.Add(closeButton);
            root.Children.Add(header);

            var mainGrid = new Grid { Margin = new Thickness(0, 22, 0, 0) };
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(332) });
            Grid.SetRow(mainGrid, 1);

            var viewportCard = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(10, 16, 28)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(30, 41, 59)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(30),
                Padding = new Thickness(18),
                Margin = new Thickness(0, 0, 18, 0)
            };

            var viewportGrid = new Grid();
            viewportGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            viewportGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            viewportGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            _image = new Image
            {
                Stretch = Stretch.Fill,
                SnapsToDevicePixels = true,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            _imageStage = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            _imageStage.Children.Add(_image);

            _previousButton = CreateNavigationButton("‹");
            _previousButton.Margin = new Thickness(0, 0, 14, 0);
            _previousButton.Click += (_, __) => ChangeSlide(-1);
            viewportGrid.Children.Add(_previousButton);

            _scrollViewer = new ScrollViewer
            {
                Background = Brushes.Transparent,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                PanningMode = PanningMode.Both,
                Content = _imageStage
            };
            _scrollViewer.PreviewMouseWheel += ScrollViewer_PreviewMouseWheel;
            _scrollViewer.SizeChanged += (_, __) => UpdateImageLayout();
            Grid.SetColumn(_scrollViewer, 1);
            viewportGrid.Children.Add(_scrollViewer);

            _nextButton = CreateNavigationButton("›");
            _nextButton.Margin = new Thickness(14, 0, 0, 0);
            _nextButton.Click += (_, __) => ChangeSlide(1);
            Grid.SetColumn(_nextButton, 2);
            viewportGrid.Children.Add(_nextButton);

            viewportCard.Child = viewportGrid;
            mainGrid.Children.Add(viewportCard);

            var sidePanel = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(9, 14, 24)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(30, 41, 59)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(28),
                Padding = new Thickness(18)
            };
            Grid.SetColumn(sidePanel, 1);

            var sideScroll = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Content = new StackPanel()
            };
            var sideStack = (StackPanel)sideScroll.Content;

            var infoWrap = new WrapPanel();
            _counterText = new TextBlock
            {
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White
            };
            _dimensionChipText = new TextBlock
            {
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White
            };
            infoWrap.Children.Add(CreateInfoChip(_counterText, Color.FromRgb(14, 165, 233)));
            infoWrap.Children.Add(CreateInfoChip(_dimensionChipText, _accent));
            sideStack.Children.Add(infoWrap);

            var descriptionCard = new Border
            {
                Margin = new Thickness(0, 12, 0, 0),
                Padding = new Thickness(14),
                Background = new SolidColorBrush(Color.FromRgb(15, 23, 42)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(51, 65, 85)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(18)
            };
            _descriptionText = new TextBlock
            {
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 21
            };
            descriptionCard.Child = _descriptionText;
            sideStack.Children.Add(descriptionCard);

            sideStack.Children.Add(new TextBlock
            {
                Text = "Zoom e enquadramento",
                Margin = new Thickness(0, 18, 0, 0),
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White
            });

            var controls = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 10, 0, 0)
            };

            var zoomOutButton = CreateControlButton("-", 42);
            zoomOutButton.Click += (_, __) => AdjustZoom(-0.12);
            controls.Children.Add(zoomOutButton);

            _zoomSlider = new Slider
            {
                Width = 154,
                Minimum = 0.7,
                Maximum = 3.2,
                Value = 1,
                Margin = new Thickness(12, 0, 12, 0),
                TickFrequency = 0.05,
                IsSnapToTickEnabled = false
            };
            _zoomSlider.ValueChanged += (_, __) => UpdateImageLayout();
            controls.Children.Add(_zoomSlider);

            var zoomInButton = CreateControlButton("+", 42);
            zoomInButton.Click += (_, __) => AdjustZoom(0.12);
            controls.Children.Add(zoomInButton);

            _zoomValueText = new TextBlock
            {
                Width = 54,
                Margin = new Thickness(12, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White
            };
            controls.Children.Add(_zoomValueText);

            _fitButton = CreateControlButton("Ajustar", 94);
            _fitButton.Margin = new Thickness(12, 0, 0, 0);
            _fitButton.Click += (_, __) => _zoomSlider.Value = 1;
            _fitButton.Visibility = _allowAdjustment ? Visibility.Visible : Visibility.Collapsed;
            controls.Children.Add(_fitButton);
            sideStack.Children.Add(controls);

            _helperText = new TextBlock
            {
                Margin = new Thickness(0, 12, 0, 0),
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 18
            };
            sideStack.Children.Add(_helperText);

            _resolutionText = new TextBlock
            {
                Margin = new Thickness(0, 8, 0, 0),
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                TextWrapping = TextWrapping.Wrap
            };
            sideStack.Children.Add(_resolutionText);

            _thumbnailSection = new Border
            {
                Margin = new Thickness(0, 18, 0, 0),
                Padding = new Thickness(14),
                Background = new SolidColorBrush(Color.FromRgb(15, 23, 42)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(51, 65, 85)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(18)
            };
            var thumbnailStack = new StackPanel();
            thumbnailStack.Children.Add(new TextBlock
            {
                Text = "Trilha da galeria",
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White
            });
            thumbnailStack.Children.Add(new TextBlock
            {
                Text = "Uma navegacao mais limpa para trocar de imagem sem repetir miniaturas grandes no painel lateral.",
                Margin = new Thickness(0, 6, 0, 0),
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 18
            });
            var slideRailScroll = new ScrollViewer
            {
                Margin = new Thickness(0, 12, 0, 0),
                Height = 228,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            };
            _thumbnailPanel = new StackPanel();
            slideRailScroll.Content = _thumbnailPanel;
            thumbnailStack.Children.Add(slideRailScroll);
            _thumbnailSection.Child = thumbnailStack;
            sideStack.Children.Add(_thumbnailSection);

            sidePanel.Child = sideScroll;
            mainGrid.Children.Add(sidePanel);
            root.Children.Add(mainGrid);

            Content = root;
            Loaded += (_, __) => UpdateDisplayedItem(resetZoom: true);
            SizeChanged += (_, __) => UpdateImageLayout();
        }

        private Border CreateInfoChip(string text, Color color)
        {
            return CreateInfoChip(new TextBlock
            {
                Text = text,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White
            }, color);
        }

        private Border CreateInfoChip(TextBlock content, Color color)
        {
            return new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(34, color.R, color.G, color.B)),
                BorderBrush = new SolidColorBrush(color),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(999),
                Padding = new Thickness(10, 5, 10, 5),
                Margin = new Thickness(0, 0, 8, 8),
                Child = content
            };
        }

        private Button CreateControlButton(string label, double width)
        {
            return new Button
            {
                Content = label,
                Width = width,
                Height = 40,
                Background = new SolidColorBrush(Color.FromRgb(15, 23, 42)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(51, 65, 85)),
                BorderThickness = new Thickness(1),
                FontWeight = FontWeights.SemiBold,
                Cursor = Cursors.Hand
            };
        }

        private Button CreateNavigationButton(string label)
        {
            return new Button
            {
                Content = label,
                Width = 56,
                Height = 56,
                Background = new SolidColorBrush(Color.FromArgb(188, 15, 23, 42)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(51, 65, 85)),
                BorderThickness = new Thickness(1),
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Cursor = Cursors.Hand,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        private void RebuildThumbnails()
        {
            _thumbnailPanel.Children.Clear();
            _thumbnailSection.Visibility = _items.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
            if (_items.Count <= 1)
            {
                return;
            }

            for (var index = 0; index < _items.Count; index++)
            {
                var snapshotIndex = index;
                var item = _items[index];
                var isActive = snapshotIndex == _currentIndex;
                var previewText = BuildSlidePreviewText(item);
                var button = new Button
                {
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(0),
                    Margin = new Thickness(0, 0, 0, 10),
                    Cursor = Cursors.Hand,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    Content = new Border
                    {
                        Background = isActive
                            ? new SolidColorBrush(Color.FromArgb(48, _accent.R, _accent.G, _accent.B))
                            : new SolidColorBrush(Color.FromRgb(12, 18, 30)),
                        BorderBrush = isActive
                            ? new SolidColorBrush(_accent)
                            : new SolidColorBrush(Color.FromRgb(51, 65, 85)),
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(16),
                        Padding = new Thickness(12),
                        Child = new Grid
                        {
                            ColumnDefinitions =
                            {
                                new ColumnDefinition { Width = GridLength.Auto },
                                new ColumnDefinition { Width = new GridLength(12) },
                                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                                new ColumnDefinition { Width = GridLength.Auto }
                            },
                            Children =
                            {
                                new Border
                                {
                                    Width = 28,
                                    Height = 28,
                                    Background = isActive
                                        ? new SolidColorBrush(_accent)
                                        : new SolidColorBrush(Color.FromRgb(30, 41, 59)),
                                    CornerRadius = new CornerRadius(999),
                                    VerticalAlignment = VerticalAlignment.Top,
                                    Child = new TextBlock
                                    {
                                        Text = (snapshotIndex + 1).ToString(),
                                        FontSize = 11,
                                        FontWeight = FontWeights.Bold,
                                        Foreground = Brushes.White,
                                        HorizontalAlignment = HorizontalAlignment.Center,
                                        VerticalAlignment = VerticalAlignment.Center,
                                        TextAlignment = TextAlignment.Center
                                    }
                                },
                                new StackPanel
                                {
                                    VerticalAlignment = VerticalAlignment.Center,
                                    Children =
                                    {
                                        new TextBlock
                                        {
                                            Text = item.Title,
                                            FontSize = 11,
                                            FontWeight = FontWeights.SemiBold,
                                            Foreground = Brushes.White,
                                            TextTrimming = TextTrimming.CharacterEllipsis
                                        },
                                        new TextBlock
                                        {
                                            Text = previewText,
                                            Margin = new Thickness(0, 4, 0, 0),
                                            FontSize = 10,
                                            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
                                            TextWrapping = TextWrapping.Wrap,
                                            LineHeight = 16,
                                            MaxWidth = 180
                                        }
                                    }
                                },
                                new Border
                                {
                                    Background = isActive
                                        ? new SolidColorBrush(Color.FromArgb(44, _accent.R, _accent.G, _accent.B))
                                        : new SolidColorBrush(Color.FromRgb(15, 23, 42)),
                                    BorderBrush = isActive
                                        ? new SolidColorBrush(_accent)
                                        : new SolidColorBrush(Color.FromRgb(51, 65, 85)),
                                    BorderThickness = new Thickness(1),
                                    CornerRadius = new CornerRadius(999),
                                    Padding = new Thickness(10, 5, 10, 5),
                                    VerticalAlignment = VerticalAlignment.Center,
                                    Child = new TextBlock
                                    {
                                        Text = isActive ? "Atual" : "Abrir",
                                        FontSize = 10,
                                        FontWeight = FontWeights.SemiBold,
                                        Foreground = isActive ? Brushes.White : new SolidColorBrush(Color.FromRgb(203, 213, 225))
                                    }
                                }
                            }
                        }
                    }
                };
                Grid.SetColumn((UIElement)((Grid)((Border)button.Content).Child).Children[1], 2);
                Grid.SetColumn((UIElement)((Grid)((Border)button.Content).Child).Children[2], 3);
                button.Click += (_, __) =>
                {
                    _currentIndex = snapshotIndex;
                    UpdateDisplayedItem(resetZoom: true);
                };
                _thumbnailPanel.Children.Add(button);
            }
        }

        private string BuildSlidePreviewText(GalleryViewerItem item)
        {
            var preview = !string.IsNullOrWhiteSpace(item.Subtitle)
                ? item.Subtitle
                : item.Description;

            if (string.IsNullOrWhiteSpace(preview))
            {
                return "Abrir esta imagem na sequencia.";
            }

            var normalized = preview.Trim();
            return normalized.Length <= 72
                ? normalized
                : normalized.Substring(0, 69) + "...";
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control)
            {
                return;
            }

            e.Handled = true;
            AdjustZoom(e.Delta > 0 ? 0.08 : -0.08);
        }

        private void OnViewerKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
                return;
            }

            if (e.Key == Key.Right)
            {
                ChangeSlide(1);
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Left)
            {
                ChangeSlide(-1);
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Add || e.Key == Key.OemPlus)
            {
                AdjustZoom(0.12);
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Subtract || e.Key == Key.OemMinus)
            {
                AdjustZoom(-0.12);
                e.Handled = true;
                return;
            }

            if (e.Key == Key.D0 || e.Key == Key.NumPad0)
            {
                _zoomSlider.Value = 1;
                e.Handled = true;
            }
        }

        private void AdjustZoom(double delta)
        {
            _zoomSlider.Value = Math.Max(_zoomSlider.Minimum, Math.Min(_zoomSlider.Maximum, _zoomSlider.Value + delta));
        }

        private void ChangeSlide(int delta)
        {
            if (_items.Count <= 1)
            {
                return;
            }

            _currentIndex = (_currentIndex + delta + _items.Count) % _items.Count;
            UpdateDisplayedItem(resetZoom: true);
        }

        private void UpdateDisplayedItem(bool resetZoom)
        {
            var item = _items[_currentIndex];
            Title = item.Title;
            _contextText.Text = _contextLabel;
            _titleText.Text = item.Title;
            _subtitleText.Text = item.Subtitle;
            _subtitleText.Visibility = string.IsNullOrWhiteSpace(item.Subtitle) ? Visibility.Collapsed : Visibility.Visible;
            _descriptionText.Text = string.IsNullOrWhiteSpace(item.Description)
                ? "Sem descrição complementar para esta imagem."
                : item.Description;
            _descriptionText.Opacity = string.IsNullOrWhiteSpace(item.Description) ? 0.72 : 1;
            _counterText.Text = _items.Count <= 1 ? "Imagem única" : $"{_currentIndex + 1} de {_items.Count}";

            _image.Source = item.ImageSource;
            _sourceWidth = item.ImageSource is BitmapSource bitmapSource ? bitmapSource.PixelWidth : Math.Max(1, item.ImageSource.Width);
            _sourceHeight = item.ImageSource is BitmapSource bitmapSourceHeight ? bitmapSourceHeight.PixelHeight : Math.Max(1, item.ImageSource.Height);
            _dimensionChipText.Text = $"{_sourceWidth:0} × {_sourceHeight:0}px";
            _helperText.Text = _allowAdjustment
                ? "A foto abre encaixada na área útil. Use o slider, Ctrl + roda do mouse ou o botão Ajustar para testar leitura rápida sem perder o enquadramento original."
                : "Modo somente leitura. Use o slider, Ctrl + roda do mouse, as setas laterais ou o teclado para navegar pela sequência sem alterar a foto do aluno.";

            _previousButton.Visibility = _items.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
            _nextButton.Visibility = _items.Count > 1 ? Visibility.Visible : Visibility.Collapsed;

            RebuildThumbnails();

            if (resetZoom)
            {
                _zoomSlider.Value = 1;
            }
            else
            {
                UpdateImageLayout();
            }
        }

        private void UpdateImageLayout()
        {
            if (_scrollViewer.ViewportWidth <= 0 || _scrollViewer.ViewportHeight <= 0 || _sourceWidth <= 0 || _sourceHeight <= 0)
            {
                return;
            }

            var availableWidth = Math.Max(320, _scrollViewer.ViewportWidth - 52);
            var availableHeight = Math.Max(260, _scrollViewer.ViewportHeight - 52);
            var fitScale = Math.Min(availableWidth / _sourceWidth, availableHeight / _sourceHeight);
            var scale = fitScale * _zoomSlider.Value;

            _image.Width = Math.Max(64, _sourceWidth * scale);
            _image.Height = Math.Max(64, _sourceHeight * scale);
            _imageStage.Width = Math.Max(_scrollViewer.ViewportWidth, _image.Width + 30);
            _imageStage.Height = Math.Max(_scrollViewer.ViewportHeight, _image.Height + 30);
            _zoomValueText.Text = $"{Math.Round(_zoomSlider.Value * 100)}%";
            _resolutionText.Text = $"A visualização está aberta em {_image.Width:0} × {_image.Height:0}px dentro do visor. O encaixe base continua respeitando a proporção original da imagem.";
        }
    }
}