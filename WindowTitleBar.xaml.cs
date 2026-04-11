using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Media3D;
using MahApps.Metro.IconPacks;

namespace MeuApp
{
    public partial class WindowTitleBar : UserControl
    {
        private static readonly Brush DefaultMutedForegroundBrush = CreateFrozenBrush(71, 85, 105);
        private static readonly Brush DefaultCommandHoverBrush = CreateFrozenBrush(248, 250, 252);
        private static readonly Brush DefaultCloseHoverBrush = CreateFrozenBrush(254, 226, 226);
        private static readonly Brush DefaultCloseForegroundBrush = CreateFrozenBrush(220, 38, 38);
        private static readonly Brush DefaultWindowCommandsBackground = CreateFrozenBrush(248, 250, 252);
        private static readonly Brush DefaultWindowCommandsBorderBrush = CreateFrozenBrush(219, 229, 240);
        private static readonly Brush DefaultAccentBrush = CreateFrozenBrush(37, 99, 235);
        private static readonly Brush DefaultAccentMutedBrush = CreateFrozenBrush(232, 238, 255);
        private static readonly Brush DefaultQuickActionsBackgroundBrush = Brushes.White;
        private static readonly Brush DefaultQuickActionsMutedSurfaceBrush = CreateFrozenBrush(248, 250, 252);
        private static readonly Brush DefaultQuickActionsBorderBrush = CreateFrozenBrush(219, 229, 240);
        private static readonly Brush DefaultPrimaryForegroundBrush = CreateFrozenBrush(15, 23, 42);
        private static readonly Brush HighContrastSurfaceBrush = CreateFrozenBrush(4, 9, 18);
        private static readonly Brush HighContrastMutedSurfaceBrush = CreateFrozenBrush(11, 18, 32);
        private static readonly Brush HighContrastBorderBrush = CreateFrozenBrush(56, 189, 248);
        private static readonly Brush HighContrastPrimaryBrush = CreateFrozenBrush(248, 250, 252);
        private static readonly Brush HighContrastSecondaryBrush = CreateFrozenBrush(203, 213, 225);
        private static readonly Brush HighContrastAccentBrush = CreateFrozenBrush(125, 211, 252);
        private static readonly Brush HighContrastAccentMutedBrush = CreateFrozenBrush(13, 36, 63);
        private static readonly Brush HighContrastHoverBrush = CreateFrozenBrush(15, 23, 42);
        private readonly ScaleTransform _titleBarScaleTransform = new ScaleTransform(1, 1);

        private Window? _hostWindow;
        private bool _quickHelpExpanded;

        public WindowTitleBar()
        {
            InitializeComponent();
            TitleBarScaleHost.LayoutTransform = _titleBarScaleTransform;
            Loaded += WindowTitleBar_Loaded;
            Unloaded += WindowTitleBar_Unloaded;
        }

        public static readonly DependencyProperty HeaderContentProperty = DependencyProperty.Register(
            nameof(HeaderContent),
            typeof(object),
            typeof(WindowTitleBar),
            new PropertyMetadata(null));

        public static readonly DependencyProperty CenterContentProperty = DependencyProperty.Register(
            nameof(CenterContent),
            typeof(object),
            typeof(WindowTitleBar),
            new PropertyMetadata(null));

        public static readonly DependencyProperty RightContentProperty = DependencyProperty.Register(
            nameof(RightContent),
            typeof(object),
            typeof(WindowTitleBar),
            new PropertyMetadata(null));

        public static readonly DependencyProperty MutedForegroundBrushProperty = DependencyProperty.Register(
            nameof(MutedForegroundBrush),
            typeof(Brush),
            typeof(WindowTitleBar),
            new PropertyMetadata(DefaultMutedForegroundBrush));

        public static readonly DependencyProperty CommandHoverBrushProperty = DependencyProperty.Register(
            nameof(CommandHoverBrush),
            typeof(Brush),
            typeof(WindowTitleBar),
            new PropertyMetadata(DefaultCommandHoverBrush));

        public static readonly DependencyProperty CloseHoverBrushProperty = DependencyProperty.Register(
            nameof(CloseHoverBrush),
            typeof(Brush),
            typeof(WindowTitleBar),
            new PropertyMetadata(DefaultCloseHoverBrush));

        public static readonly DependencyProperty CloseForegroundBrushProperty = DependencyProperty.Register(
            nameof(CloseForegroundBrush),
            typeof(Brush),
            typeof(WindowTitleBar),
            new PropertyMetadata(DefaultCloseForegroundBrush));

        public static readonly DependencyProperty InnerPaddingProperty = DependencyProperty.Register(
            nameof(InnerPadding),
            typeof(Thickness),
            typeof(WindowTitleBar),
            new PropertyMetadata(new Thickness(20, 12, 20, 12)));

        public static readonly DependencyProperty TitleBarMinHeightProperty = DependencyProperty.Register(
            nameof(TitleBarMinHeight),
            typeof(double),
            typeof(WindowTitleBar),
            new PropertyMetadata(72d));

        public static readonly DependencyProperty WindowCommandsBackgroundProperty = DependencyProperty.Register(
            nameof(WindowCommandsBackground),
            typeof(Brush),
            typeof(WindowTitleBar),
            new PropertyMetadata(DefaultWindowCommandsBackground));

        public static readonly DependencyProperty WindowCommandsBorderBrushProperty = DependencyProperty.Register(
            nameof(WindowCommandsBorderBrush),
            typeof(Brush),
            typeof(WindowTitleBar),
            new PropertyMetadata(DefaultWindowCommandsBorderBrush));

        public static readonly DependencyProperty WindowCommandsPaddingProperty = DependencyProperty.Register(
            nameof(WindowCommandsPadding),
            typeof(Thickness),
            typeof(WindowTitleBar),
            new PropertyMetadata(new Thickness(4)));

        public static readonly DependencyProperty WindowCommandsCornerRadiusProperty = DependencyProperty.Register(
            nameof(WindowCommandsCornerRadius),
            typeof(CornerRadius),
            typeof(WindowTitleBar),
            new PropertyMetadata(new CornerRadius(16)));

        public static readonly DependencyProperty ShowWindowCommandsProperty = DependencyProperty.Register(
            nameof(ShowWindowCommands),
            typeof(bool),
            typeof(WindowTitleBar),
            new PropertyMetadata(true));

        public static readonly DependencyProperty AccentBrushProperty = DependencyProperty.Register(
            nameof(AccentBrush),
            typeof(Brush),
            typeof(WindowTitleBar),
            new PropertyMetadata(DefaultAccentBrush));

        public static readonly DependencyProperty AccentMutedBrushProperty = DependencyProperty.Register(
            nameof(AccentMutedBrush),
            typeof(Brush),
            typeof(WindowTitleBar),
            new PropertyMetadata(DefaultAccentMutedBrush));

        public static readonly DependencyProperty QuickActionsBackgroundBrushProperty = DependencyProperty.Register(
            nameof(QuickActionsBackgroundBrush),
            typeof(Brush),
            typeof(WindowTitleBar),
            new PropertyMetadata(DefaultQuickActionsBackgroundBrush));

        public static readonly DependencyProperty QuickActionsMutedSurfaceBrushProperty = DependencyProperty.Register(
            nameof(QuickActionsMutedSurfaceBrush),
            typeof(Brush),
            typeof(WindowTitleBar),
            new PropertyMetadata(DefaultQuickActionsMutedSurfaceBrush));

        public static readonly DependencyProperty QuickActionsBorderBrushProperty = DependencyProperty.Register(
            nameof(QuickActionsBorderBrush),
            typeof(Brush),
            typeof(WindowTitleBar),
            new PropertyMetadata(DefaultQuickActionsBorderBrush));

        public static readonly DependencyProperty ShowQuickActionsProperty = DependencyProperty.Register(
            nameof(ShowQuickActions),
            typeof(bool),
            typeof(WindowTitleBar),
            new PropertyMetadata(true));

        public static readonly DependencyProperty ShowProfileActionProperty = DependencyProperty.Register(
            nameof(ShowProfileAction),
            typeof(bool),
            typeof(WindowTitleBar),
            new PropertyMetadata(false));

        public static readonly DependencyProperty EffectiveQuickActionsBackgroundBrushProperty = DependencyProperty.Register(
            nameof(EffectiveQuickActionsBackgroundBrush),
            typeof(Brush),
            typeof(WindowTitleBar),
            new PropertyMetadata(DefaultQuickActionsBackgroundBrush));

        public static readonly DependencyProperty EffectiveQuickActionsMutedSurfaceBrushProperty = DependencyProperty.Register(
            nameof(EffectiveQuickActionsMutedSurfaceBrush),
            typeof(Brush),
            typeof(WindowTitleBar),
            new PropertyMetadata(DefaultQuickActionsMutedSurfaceBrush));

        public static readonly DependencyProperty EffectiveQuickActionsBorderBrushProperty = DependencyProperty.Register(
            nameof(EffectiveQuickActionsBorderBrush),
            typeof(Brush),
            typeof(WindowTitleBar),
            new PropertyMetadata(DefaultQuickActionsBorderBrush));

        public static readonly DependencyProperty EffectiveQuickActionsPrimaryBrushProperty = DependencyProperty.Register(
            nameof(EffectiveQuickActionsPrimaryBrush),
            typeof(Brush),
            typeof(WindowTitleBar),
            new PropertyMetadata(DefaultPrimaryForegroundBrush));

        public static readonly DependencyProperty EffectiveQuickActionsSecondaryBrushProperty = DependencyProperty.Register(
            nameof(EffectiveQuickActionsSecondaryBrush),
            typeof(Brush),
            typeof(WindowTitleBar),
            new PropertyMetadata(DefaultMutedForegroundBrush));

        public static readonly DependencyProperty EffectiveQuickActionsAccentBrushProperty = DependencyProperty.Register(
            nameof(EffectiveQuickActionsAccentBrush),
            typeof(Brush),
            typeof(WindowTitleBar),
            new PropertyMetadata(DefaultAccentBrush));

        public static readonly DependencyProperty EffectiveQuickActionsAccentMutedBrushProperty = DependencyProperty.Register(
            nameof(EffectiveQuickActionsAccentMutedBrush),
            typeof(Brush),
            typeof(WindowTitleBar),
            new PropertyMetadata(DefaultAccentMutedBrush));

        public static readonly DependencyProperty EffectiveQuickActionsHoverBrushProperty = DependencyProperty.Register(
            nameof(EffectiveQuickActionsHoverBrush),
            typeof(Brush),
            typeof(WindowTitleBar),
            new PropertyMetadata(DefaultCommandHoverBrush));

        public event RoutedEventHandler? ProfileRequested;

        public object? HeaderContent
        {
            get => GetValue(HeaderContentProperty);
            set => SetValue(HeaderContentProperty, value);
        }

        public object? CenterContent
        {
            get => GetValue(CenterContentProperty);
            set => SetValue(CenterContentProperty, value);
        }

        public object? RightContent
        {
            get => GetValue(RightContentProperty);
            set => SetValue(RightContentProperty, value);
        }

        public Brush MutedForegroundBrush
        {
            get => (Brush)GetValue(MutedForegroundBrushProperty);
            set => SetValue(MutedForegroundBrushProperty, value);
        }

        public Brush CommandHoverBrush
        {
            get => (Brush)GetValue(CommandHoverBrushProperty);
            set => SetValue(CommandHoverBrushProperty, value);
        }

        public Brush CloseHoverBrush
        {
            get => (Brush)GetValue(CloseHoverBrushProperty);
            set => SetValue(CloseHoverBrushProperty, value);
        }

        public Brush CloseForegroundBrush
        {
            get => (Brush)GetValue(CloseForegroundBrushProperty);
            set => SetValue(CloseForegroundBrushProperty, value);
        }

        public Thickness InnerPadding
        {
            get => (Thickness)GetValue(InnerPaddingProperty);
            set => SetValue(InnerPaddingProperty, value);
        }

        public double TitleBarMinHeight
        {
            get => (double)GetValue(TitleBarMinHeightProperty);
            set => SetValue(TitleBarMinHeightProperty, value);
        }

        public Brush WindowCommandsBackground
        {
            get => (Brush)GetValue(WindowCommandsBackgroundProperty);
            set => SetValue(WindowCommandsBackgroundProperty, value);
        }

        public Brush WindowCommandsBorderBrush
        {
            get => (Brush)GetValue(WindowCommandsBorderBrushProperty);
            set => SetValue(WindowCommandsBorderBrushProperty, value);
        }

        public Thickness WindowCommandsPadding
        {
            get => (Thickness)GetValue(WindowCommandsPaddingProperty);
            set => SetValue(WindowCommandsPaddingProperty, value);
        }

        public CornerRadius WindowCommandsCornerRadius
        {
            get => (CornerRadius)GetValue(WindowCommandsCornerRadiusProperty);
            set => SetValue(WindowCommandsCornerRadiusProperty, value);
        }

        public bool ShowWindowCommands
        {
            get => (bool)GetValue(ShowWindowCommandsProperty);
            set => SetValue(ShowWindowCommandsProperty, value);
        }

        public Brush AccentBrush
        {
            get => (Brush)GetValue(AccentBrushProperty);
            set => SetValue(AccentBrushProperty, value);
        }

        public Brush AccentMutedBrush
        {
            get => (Brush)GetValue(AccentMutedBrushProperty);
            set => SetValue(AccentMutedBrushProperty, value);
        }

        public Brush QuickActionsBackgroundBrush
        {
            get => (Brush)GetValue(QuickActionsBackgroundBrushProperty);
            set => SetValue(QuickActionsBackgroundBrushProperty, value);
        }

        public Brush QuickActionsMutedSurfaceBrush
        {
            get => (Brush)GetValue(QuickActionsMutedSurfaceBrushProperty);
            set => SetValue(QuickActionsMutedSurfaceBrushProperty, value);
        }

        public Brush QuickActionsBorderBrush
        {
            get => (Brush)GetValue(QuickActionsBorderBrushProperty);
            set => SetValue(QuickActionsBorderBrushProperty, value);
        }

        public bool ShowQuickActions
        {
            get => (bool)GetValue(ShowQuickActionsProperty);
            set => SetValue(ShowQuickActionsProperty, value);
        }

        public bool ShowProfileAction
        {
            get => (bool)GetValue(ShowProfileActionProperty);
            set => SetValue(ShowProfileActionProperty, value);
        }

        public Brush EffectiveQuickActionsBackgroundBrush
        {
            get => (Brush)GetValue(EffectiveQuickActionsBackgroundBrushProperty);
            private set => SetValue(EffectiveQuickActionsBackgroundBrushProperty, value);
        }

        public Brush EffectiveQuickActionsMutedSurfaceBrush
        {
            get => (Brush)GetValue(EffectiveQuickActionsMutedSurfaceBrushProperty);
            private set => SetValue(EffectiveQuickActionsMutedSurfaceBrushProperty, value);
        }

        public Brush EffectiveQuickActionsBorderBrush
        {
            get => (Brush)GetValue(EffectiveQuickActionsBorderBrushProperty);
            private set => SetValue(EffectiveQuickActionsBorderBrushProperty, value);
        }

        public Brush EffectiveQuickActionsPrimaryBrush
        {
            get => (Brush)GetValue(EffectiveQuickActionsPrimaryBrushProperty);
            private set => SetValue(EffectiveQuickActionsPrimaryBrushProperty, value);
        }

        public Brush EffectiveQuickActionsSecondaryBrush
        {
            get => (Brush)GetValue(EffectiveQuickActionsSecondaryBrushProperty);
            private set => SetValue(EffectiveQuickActionsSecondaryBrushProperty, value);
        }

        public Brush EffectiveQuickActionsAccentBrush
        {
            get => (Brush)GetValue(EffectiveQuickActionsAccentBrushProperty);
            private set => SetValue(EffectiveQuickActionsAccentBrushProperty, value);
        }

        public Brush EffectiveQuickActionsAccentMutedBrush
        {
            get => (Brush)GetValue(EffectiveQuickActionsAccentMutedBrushProperty);
            private set => SetValue(EffectiveQuickActionsAccentMutedBrushProperty, value);
        }

        public Brush EffectiveQuickActionsHoverBrush
        {
            get => (Brush)GetValue(EffectiveQuickActionsHoverBrushProperty);
            private set => SetValue(EffectiveQuickActionsHoverBrushProperty, value);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == ForegroundProperty
                || e.Property == MutedForegroundBrushProperty
                || e.Property == CommandHoverBrushProperty
                || e.Property == AccentBrushProperty
                || e.Property == AccentMutedBrushProperty
                || e.Property == QuickActionsBackgroundBrushProperty
                || e.Property == QuickActionsMutedSurfaceBrushProperty
                || e.Property == QuickActionsBorderBrushProperty
                || e.Property == ShowWindowCommandsProperty
                || e.Property == ShowQuickActionsProperty
                || e.Property == ShowProfileActionProperty)
            {
                UpdateQuickActionsAppearance();
                UpdateWindowCommandButtons();
                UpdateQuickActionsState();
            }
        }

        public T? FindTaggedContentElement<T>(string tag) where T : FrameworkElement
        {
            return FindTaggedContentElementCore<T>(HeaderContent, tag)
                ?? FindTaggedContentElementCore<T>(CenterContent, tag)
                ?? FindTaggedContentElementCore<T>(RightContent, tag);
        }

        private void WindowTitleBar_Loaded(object sender, RoutedEventArgs e)
        {
            AttachToHostWindow();
            AccessibilityPreferences.SettingsChanged += AccessibilityPreferences_SettingsChanged;
            ApplyAccessibilitySettings(AccessibilityPreferences.Current);
            UpdateWindowCommandButtons();
        }

        private void WindowTitleBar_Unloaded(object sender, RoutedEventArgs e)
        {
            AccessibilityPreferences.SettingsChanged -= AccessibilityPreferences_SettingsChanged;
            DetachFromHostWindow();
        }

        private void AttachToHostWindow()
        {
            var window = Window.GetWindow(this);
            if (ReferenceEquals(_hostWindow, window))
            {
                return;
            }

            DetachFromHostWindow();
            _hostWindow = window;

            if (_hostWindow != null)
            {
                _hostWindow.StateChanged += HostWindow_StateChanged;
                _hostWindow.PreviewKeyDown += HostWindow_PreviewKeyDown;
            }
        }

        private void DetachFromHostWindow()
        {
            if (_hostWindow != null)
            {
                _hostWindow.StateChanged -= HostWindow_StateChanged;
                _hostWindow.PreviewKeyDown -= HostWindow_PreviewKeyDown;
                _hostWindow = null;
            }
        }

        private void HostWindow_StateChanged(object? sender, EventArgs e)
        {
            UpdateWindowCommandButtons();
            UpdateQuickActionsState();
        }

        private void HostWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.T && Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                ToggleKeepOnTop();
                e.Handled = true;
                return;
            }

            if ((e.Key == Key.OemPeriod || e.Key == Key.Decimal) && Keyboard.Modifiers == ModifierKeys.Control)
            {
                ShowKeyboardShortcutsDialog();
                e.Handled = true;
            }
        }

        private void TitleBarRoot_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AttachToHostWindow();
            if (_hostWindow == null || IsInteractiveSource(e.OriginalSource))
            {
                return;
            }

            if (_hostWindow is ChromeWindow chromeWindow)
            {
                chromeWindow.BeginTitleBarDrag(e);
            }
            else
            {
                BeginFallbackDrag(_hostWindow, e);
            }

            e.Handled = true;
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            AttachToHostWindow();
            if (_hostWindow is ChromeWindow chromeWindow)
            {
                chromeWindow.MinimizeWindow();
            }
            else if (_hostWindow != null && _hostWindow.ResizeMode != ResizeMode.NoResize)
            {
                SystemCommands.MinimizeWindow(_hostWindow);
            }
        }

        private void WindowStateButton_Click(object sender, RoutedEventArgs e)
        {
            AttachToHostWindow();
            if (_hostWindow is ChromeWindow chromeWindow)
            {
                chromeWindow.ToggleWindowState();
            }
            else if (_hostWindow != null && CanToggleWindowState(_hostWindow))
            {
                if (_hostWindow.WindowState == WindowState.Maximized)
                {
                    SystemCommands.RestoreWindow(_hostWindow);
                }
                else
                {
                    SystemCommands.MaximizeWindow(_hostWindow);
                }
            }

            UpdateWindowCommandButtons();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            AttachToHostWindow();
            if (_hostWindow is ChromeWindow chromeWindow)
            {
                chromeWindow.CloseWindowCommand();
            }
            else if (_hostWindow != null)
            {
                SystemCommands.CloseWindow(_hostWindow);
            }
        }

        private void QuickActionsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ShowQuickActions)
            {
                return;
            }

            if (QuickActionsPopup.IsOpen)
            {
                QuickActionsPopup.IsOpen = false;
                return;
            }

            _quickHelpExpanded = false;
            UpdateQuickActionsState();
            QuickActionsPopup.IsOpen = true;
        }

        private void QuickActionsPopup_Closed(object sender, EventArgs e)
        {
            _quickHelpExpanded = false;
            UpdateQuickActionsState();
        }

        private void ToggleKeepOnTopQuickAction_Click(object sender, RoutedEventArgs e)
        {
            ToggleKeepOnTop();
        }

        private void OpenProfileQuickAction_Click(object sender, RoutedEventArgs e)
        {
            QuickActionsPopup.IsOpen = false;
            ProfileRequested?.Invoke(this, new RoutedEventArgs());
        }

        private void DecreaseTextScaleButton_Click(object sender, RoutedEventArgs e)
        {
            AccessibilityPreferences.AdjustTextScale(-AccessibilityPreferences.TextScaleStepPercent);
        }

        private void IncreaseTextScaleButton_Click(object sender, RoutedEventArgs e)
        {
            AccessibilityPreferences.AdjustTextScale(AccessibilityPreferences.TextScaleStepPercent);
        }

        private void ToggleHelpQuickAction_Click(object sender, RoutedEventArgs e)
        {
            _quickHelpExpanded = !_quickHelpExpanded;
            UpdateQuickActionsState();
        }

        private void OpenQuickGuideQuickAction_Click(object sender, RoutedEventArgs e)
        {
            QuickActionsPopup.IsOpen = false;
            ShowQuickHelpDialog();
        }

        private void OpenApplicationLogQuickAction_Click(object sender, RoutedEventArgs e)
        {
            QuickActionsPopup.IsOpen = false;
            OpenApplicationLogFile();
        }

        private void ShowKeyboardShortcutsQuickAction_Click(object sender, RoutedEventArgs e)
        {
            QuickActionsPopup.IsOpen = false;
            ShowKeyboardShortcutsDialog();
        }

        private void OpenAccessibilityQuickAction_Click(object sender, RoutedEventArgs e)
        {
            QuickActionsPopup.IsOpen = false;
            ShowAccessibilityDialog();
        }

        private void UpdateWindowCommandButtons()
        {
            if (WindowCommandsHost == null
                || QuickActionsButton == null
                || MinimizeButton == null
                || WindowStateButton == null
                || CloseButton == null
                || WindowStateIcon == null)
            {
                return;
            }

            if (!ShowWindowCommands && !ShowQuickActions)
            {
                WindowCommandsHost.Visibility = Visibility.Collapsed;
                return;
            }

            WindowCommandsHost.Visibility = Visibility.Visible;
            QuickActionsButton.Visibility = ShowQuickActions ? Visibility.Visible : Visibility.Collapsed;

            if (_hostWindow == null)
            {
                MinimizeButton.Visibility = ShowWindowCommands ? Visibility.Visible : Visibility.Collapsed;
                WindowStateButton.Visibility = ShowWindowCommands ? Visibility.Visible : Visibility.Collapsed;
                CloseButton.Visibility = ShowWindowCommands ? Visibility.Visible : Visibility.Collapsed;
                return;
            }

            if (!ShowWindowCommands)
            {
                MinimizeButton.Visibility = Visibility.Collapsed;
                WindowStateButton.Visibility = Visibility.Collapsed;
                CloseButton.Visibility = Visibility.Collapsed;
                return;
            }

            var canMinimize = _hostWindow is ChromeWindow chromeWindow
                ? chromeWindow.CanMinimizeWindow
                : _hostWindow.ResizeMode != ResizeMode.NoResize;
            var canToggleWindowState = _hostWindow is ChromeWindow typedChromeWindow
                ? typedChromeWindow.CanToggleWindowState
                : CanToggleWindowState(_hostWindow);

            MinimizeButton.Visibility = canMinimize ? Visibility.Visible : Visibility.Collapsed;
            WindowStateButton.Visibility = canToggleWindowState ? Visibility.Visible : Visibility.Collapsed;

            var isMaximized = _hostWindow.WindowState == WindowState.Maximized;
            WindowStateIcon.Kind = isMaximized
                ? PackIconMaterialKind.WindowRestore
                : PackIconMaterialKind.WindowMaximize;
            WindowStateButton.ToolTip = isMaximized ? "Restaurar" : "Maximizar";
        }

        private void AccessibilityPreferences_SettingsChanged(object? sender, AccessibilitySettingsChangedEventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => ApplyAccessibilitySettings(e.Settings));
                return;
            }

            ApplyAccessibilitySettings(e.Settings);
        }

        private void ApplyAccessibilitySettings(AccessibilitySettings settings)
        {
            var scale = settings.TextScalePercent / 100d;
            _titleBarScaleTransform.ScaleX = scale;
            _titleBarScaleTransform.ScaleY = scale;
            QuickActionsPopup.PopupAnimation = settings.ReduceAnimations ? PopupAnimation.None : PopupAnimation.Fade;
            UpdateQuickActionsAppearance();
            UpdateQuickActionsState();
        }

        private void UpdateQuickActionsAppearance()
        {
            if (AccessibilityPreferences.HighContrastEnabled)
            {
                EffectiveQuickActionsBackgroundBrush = HighContrastSurfaceBrush;
                EffectiveQuickActionsMutedSurfaceBrush = HighContrastMutedSurfaceBrush;
                EffectiveQuickActionsBorderBrush = HighContrastBorderBrush;
                EffectiveQuickActionsPrimaryBrush = HighContrastPrimaryBrush;
                EffectiveQuickActionsSecondaryBrush = HighContrastSecondaryBrush;
                EffectiveQuickActionsAccentBrush = HighContrastAccentBrush;
                EffectiveQuickActionsAccentMutedBrush = HighContrastAccentMutedBrush;
                EffectiveQuickActionsHoverBrush = HighContrastHoverBrush;
                return;
            }

            EffectiveQuickActionsBackgroundBrush = QuickActionsBackgroundBrush ?? DefaultQuickActionsBackgroundBrush;
            EffectiveQuickActionsMutedSurfaceBrush = QuickActionsMutedSurfaceBrush ?? DefaultQuickActionsMutedSurfaceBrush;
            EffectiveQuickActionsBorderBrush = QuickActionsBorderBrush ?? DefaultQuickActionsBorderBrush;
            EffectiveQuickActionsPrimaryBrush = Foreground as Brush ?? DefaultPrimaryForegroundBrush;
            EffectiveQuickActionsSecondaryBrush = MutedForegroundBrush ?? DefaultMutedForegroundBrush;
            EffectiveQuickActionsAccentBrush = AccentBrush ?? DefaultAccentBrush;
            EffectiveQuickActionsAccentMutedBrush = AccentMutedBrush ?? DefaultAccentMutedBrush;
            EffectiveQuickActionsHoverBrush = CommandHoverBrush ?? DefaultCommandHoverBrush;
        }

        private void UpdateQuickActionsState()
        {
            if (QuickActionsButton == null
                || ProfileQuickActionButton == null
                || KeepOnTopQuickActionStatusText == null
                || TextScalePercentText == null
                || DecreaseTextScaleButton == null
                || IncreaseTextScaleButton == null
                || HelpMenuPanel == null
                || HelpChevronIcon == null)
            {
                return;
            }

            QuickActionsButton.Visibility = ShowQuickActions ? Visibility.Visible : Visibility.Collapsed;
            ProfileQuickActionButton.Visibility = ShowProfileAction ? Visibility.Visible : Visibility.Collapsed;

            if (!ShowQuickActions)
            {
                QuickActionsPopup.IsOpen = false;
            }

            var topMostEnabled = _hostWindow?.Topmost == true;
            KeepOnTopQuickActionStatusText.Text = topMostEnabled ? "Ativado" : "Desativado";
            KeepOnTopQuickActionStatusText.Foreground = topMostEnabled
                ? EffectiveQuickActionsAccentBrush
                : EffectiveQuickActionsSecondaryBrush;

            TextScalePercentText.Text = $"({AccessibilityPreferences.TextScalePercent})%";
            DecreaseTextScaleButton.IsEnabled = AccessibilityPreferences.TextScalePercent > AccessibilityPreferences.MinTextScalePercent;
            IncreaseTextScaleButton.IsEnabled = AccessibilityPreferences.TextScalePercent < AccessibilityPreferences.MaxTextScalePercent;

            HelpMenuPanel.Visibility = _quickHelpExpanded ? Visibility.Visible : Visibility.Collapsed;
            HelpChevronIcon.Kind = _quickHelpExpanded
                ? PackIconMaterialKind.ChevronDown
                : PackIconMaterialKind.ChevronRight;
        }

        private void ToggleKeepOnTop()
        {
            AttachToHostWindow();
            if (_hostWindow == null)
            {
                return;
            }

            _hostWindow.Topmost = !_hostWindow.Topmost;
            UpdateQuickActionsState();
        }

        private void OpenApplicationLogFile()
        {
            if (!DebugHelper.IsInitialized)
            {
                DebugHelper.Initialize();
            }

            DebugHelper.OpenLogFile();
        }

        private void ShowQuickHelpDialog()
        {
            var dialog = CreateDialogWindow("Ajuda rápida", 680, 560, 540);
            var accentBrush = EffectiveQuickActionsAccentBrush;

            var layout = new Grid();
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            layout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            layout.Children.Add(CreateDialogHeader(
                "GUIA",
                "Ajuda rápida da janela",
                "Use o menu compartilhado da title bar para manter a janela no topo, ajustar leitura, abrir suporte e revisar atalhos.",
                accentBrush));

            var helpCards = new StackPanel();
            helpCards.Children.Add(CreateUtilityCard(
                "Ações rápidas compartilhadas",
                "O botão de três pontos agora está na title bar compartilhada e aparece em todas as janelas principais do shell."));
            helpCards.Children.Add(CreateUtilityCard(
                "Leitura adaptável",
                "Tamanho do texto, alto contraste e redução de animações ficam centralizados e se propagam para as janelas abertas."));
            helpCards.Children.Add(CreateUtilityCard(
                "Suporte e diagnóstico",
                "Abra o log atual diretamente daqui quando precisar validar fluxo, integração ou erro de runtime."));
            helpCards.Children.Add(CreateUtilityCard(
                "Janela fixa",
                "Ctrl+Shift+T mantém a janela atual sempre visível durante aula, revisão ou acompanhamento em paralelo."));

            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Content = helpCards
            };
            Grid.SetRow(scrollViewer, 1);
            layout.Children.Add(scrollViewer);

            var footer = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 22, 0, 0)
            };
            Grid.SetRow(footer, 2);

            var openLogButton = CreateDialogActionButton("Abrir log", Brushes.Transparent, EffectiveQuickActionsPrimaryBrush, EffectiveQuickActionsBorderBrush, 118);
            openLogButton.Click += (_, __) =>
            {
                OpenApplicationLogFile();
                dialog.Close();
            };

            var closeButton = CreateDialogActionButton("Fechar", accentBrush, Brushes.White, Brushes.Transparent, 112);
            closeButton.Click += (_, __) => dialog.Close();

            footer.Children.Add(openLogButton);
            footer.Children.Add(closeButton);
            layout.Children.Add(footer);

            dialog.Content = CreateStyledDialogShell(layout);
            dialog.ShowDialog();
        }

        private void ShowKeyboardShortcutsDialog()
        {
            var dialog = CreateDialogWindow("Atalhos do teclado", 700, 580, 540);
            var accentBrush = EffectiveQuickActionsAccentBrush;

            var layout = new Grid();
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            layout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            layout.Children.Add(CreateDialogHeader(
                "ATALHOS",
                "Atalhos da title bar",
                "Os atalhos abaixo funcionam em qualquer janela que usa o shell compartilhado.",
                accentBrush));

            var shortcutsStack = new StackPanel();
            shortcutsStack.Children.Add(CreateShortcutCard("Ctrl+Shift+T", "Manter janela no topo", "Alterna a visibilidade prioritária da janela atual."));
            shortcutsStack.Children.Add(CreateShortcutCard("Ctrl+.", "Atalhos do teclado", "Abre este catálogo de atalhos compartilhados."));
            shortcutsStack.Children.Add(CreateShortcutCard("Botão ⋯", "Ações rápidas", "Abre o menu compartilhado com ajuda, log e acessibilidade."));
            shortcutsStack.Children.Add(CreateShortcutCard("Esc", "Fechar diálogo", "Fecha os diálogos rápidos quando o foco estiver na janela ativa."));

            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Content = shortcutsStack
            };
            Grid.SetRow(scrollViewer, 1);
            layout.Children.Add(scrollViewer);

            var footer = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 22, 0, 0)
            };
            Grid.SetRow(footer, 2);

            var closeButton = CreateDialogActionButton("Fechar", accentBrush, Brushes.White, Brushes.Transparent, 112);
            closeButton.Click += (_, __) => dialog.Close();
            footer.Children.Add(closeButton);
            layout.Children.Add(footer);

            dialog.Content = CreateStyledDialogShell(layout);
            dialog.ShowDialog();
        }

        private void ShowAccessibilityDialog()
        {
            var dialog = CreateDialogWindow("Acessibilidade", 720, 620, 580);
            var accentBrush = EffectiveQuickActionsAccentBrush;

            var layout = new Grid();
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            layout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            layout.Children.Add(CreateDialogHeader(
                "A11Y",
                "Acessibilidade",
                "Ajuste contraste, leitura e movimento do shell compartilhado sem depender só das configurações do MainWindow.",
                accentBrush));

            var contentStack = new StackPanel();

            var highContrastToggle = CreateAccessibilityCheckBox(AccessibilityPreferences.HighContrastEnabled);
            highContrastToggle.Checked += (_, __) => AccessibilityPreferences.SetHighContrastEnabled(true);
            highContrastToggle.Unchecked += (_, __) => AccessibilityPreferences.SetHighContrastEnabled(false);
            contentStack.Children.Add(CreateUtilityCard(
                "Alto contraste",
                "Reforça a legibilidade do menu rápido compartilhado e da paleta principal do workspace para cenários de baixa visibilidade.",
                highContrastToggle));

            var textScaleValue = new TextBlock
            {
                Width = 68,
                Margin = new Thickness(10, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = EffectiveQuickActionsPrimaryBrush
            };

            void RefreshTextScaleLabel()
            {
                textScaleValue.Text = $"({AccessibilityPreferences.TextScalePercent})%";
            }

            var decreaseButton = CreateMiniActionButton("-", 18);
            decreaseButton.Click += (_, __) =>
            {
                AccessibilityPreferences.AdjustTextScale(-AccessibilityPreferences.TextScaleStepPercent);
                RefreshTextScaleLabel();
            };

            var increaseButton = CreateMiniActionButton("+", 18);
            increaseButton.Click += (_, __) =>
            {
                AccessibilityPreferences.AdjustTextScale(AccessibilityPreferences.TextScaleStepPercent);
                RefreshTextScaleLabel();
            };

            RefreshTextScaleLabel();

            var textScaleControls = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Children = { decreaseButton, textScaleValue, increaseButton }
            };
            contentStack.Children.Add(CreateUtilityCard(
                "Tamanho do texto",
                "Aplica uma escala global de leitura nas janelas que usam o shell compartilhado.",
                textScaleControls));

            var reduceAnimationsToggle = CreateAccessibilityCheckBox(AccessibilityPreferences.ReduceAnimations);
            reduceAnimationsToggle.Checked += (_, __) => AccessibilityPreferences.SetReduceAnimations(true);
            reduceAnimationsToggle.Unchecked += (_, __) => AccessibilityPreferences.SetReduceAnimations(false);
            contentStack.Children.Add(CreateUtilityCard(
                "Reduzir animações",
                "Remove fades e transições rápidas de painéis e diálogos para diminuir distração e desconforto visual.",
                reduceAnimationsToggle));

            var shortcutsButton = CreateDialogActionButton("Ver atalhos", Brushes.Transparent, EffectiveQuickActionsPrimaryBrush, EffectiveQuickActionsBorderBrush, 126);
            shortcutsButton.Click += (_, __) =>
            {
                dialog.Close();
                ShowKeyboardShortcutsDialog();
            };
            contentStack.Children.Add(CreateUtilityCard(
                "Atalhos e apoio",
                "Ctrl+. abre o catálogo de atalhos. O log e o guia rápido continuam disponíveis pelo mesmo menu da title bar.",
                shortcutsButton));

            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Content = contentStack
            };
            Grid.SetRow(scrollViewer, 1);
            layout.Children.Add(scrollViewer);

            var footer = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 22, 0, 0)
            };
            Grid.SetRow(footer, 2);

            var closeButton = CreateDialogActionButton("Fechar", accentBrush, Brushes.White, Brushes.Transparent, 112);
            closeButton.Click += (_, __) => dialog.Close();
            footer.Children.Add(closeButton);
            layout.Children.Add(footer);

            dialog.Content = CreateStyledDialogShell(layout);
            dialog.ShowDialog();
        }

        private Window CreateDialogWindow(string title, double width, double height, double minHeight)
        {
            return new Window
            {
                Title = title,
                Owner = _hostWindow,
                Width = width,
                Height = height,
                MinHeight = minHeight,
                WindowStartupLocation = _hostWindow != null ? WindowStartupLocation.CenterOwner : WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false,
                AllowsTransparency = true,
                WindowStyle = WindowStyle.None,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                BorderBrush = Brushes.Transparent
            };
        }

        private Border CreateStyledDialogShell(UIElement content)
        {
            var shell = new Border
            {
                Margin = new Thickness(14),
                CornerRadius = new CornerRadius(28),
                Background = EffectiveQuickActionsBackgroundBrush,
                BorderBrush = EffectiveQuickActionsBorderBrush,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(24),
                SnapsToDevicePixels = true,
                Child = content,
                Effect = new DropShadowEffect
                {
                    BlurRadius = 34,
                    ShadowDepth = 12,
                    Opacity = 0.18,
                    Color = Color.FromRgb(15, 23, 42)
                }
            };

            if (!AccessibilityPreferences.ReduceAnimations)
            {
                shell.Loaded += (_, __) =>
                {
                    shell.Opacity = 0;
                    var transform = new TranslateTransform(0, 20);
                    shell.RenderTransform = transform;

                    var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
                    shell.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(180)) { EasingFunction = ease });
                    transform.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(20, 0, TimeSpan.FromMilliseconds(220)) { EasingFunction = ease });
                };
            }

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
                Foreground = EffectiveQuickActionsPrimaryBrush,
                TextWrapping = TextWrapping.Wrap,
                FontFamily = TryGetResource("AppDisplayFontFamily") as FontFamily
            });
            header.Children.Add(new TextBlock
            {
                Text = description,
                Margin = new Thickness(0, 10, 0, 0),
                FontSize = 12,
                Foreground = EffectiveQuickActionsSecondaryBrush,
                TextWrapping = TextWrapping.Wrap
            });
            return header;
        }

        private Border CreateUtilityCard(string title, string description, UIElement? accessory = null)
        {
            var card = new Border
            {
                Background = EffectiveQuickActionsMutedSurfaceBrush,
                BorderBrush = EffectiveQuickActionsBorderBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(18),
                Padding = new Thickness(16),
                Margin = new Thickness(0, 0, 0, 12)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var textStack = new StackPanel();
            textStack.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = EffectiveQuickActionsPrimaryBrush
            });
            textStack.Children.Add(new TextBlock
            {
                Text = description,
                Margin = new Thickness(0, 6, 0, 0),
                FontSize = 11,
                Foreground = EffectiveQuickActionsSecondaryBrush,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 420
            });
            grid.Children.Add(textStack);

            if (accessory != null)
            {
                if (accessory is FrameworkElement accessoryElement)
                {
                    accessoryElement.Margin = new Thickness(16, 0, 0, 0);
                }

                Grid.SetColumn(accessory, 1);
                grid.Children.Add(accessory);
            }

            card.Child = grid;
            return card;
        }

        private Border CreateShortcutCard(string shortcut, string title, string description)
        {
            var card = new Border
            {
                Background = EffectiveQuickActionsMutedSurfaceBrush,
                BorderBrush = EffectiveQuickActionsBorderBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(18),
                Padding = new Thickness(16),
                Margin = new Thickness(0, 0, 0, 12)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var chip = new Border
            {
                Background = EffectiveQuickActionsAccentMutedBrush,
                CornerRadius = new CornerRadius(999),
                Padding = new Thickness(12, 5, 12, 5),
                VerticalAlignment = VerticalAlignment.Top,
                Child = new TextBlock
                {
                    Text = shortcut,
                    Foreground = EffectiveQuickActionsAccentBrush,
                    FontSize = 11,
                    FontWeight = FontWeights.Bold
                }
            };
            grid.Children.Add(chip);

            var textStack = new StackPanel { Margin = new Thickness(14, 0, 0, 0) };
            textStack.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = EffectiveQuickActionsPrimaryBrush
            });
            textStack.Children.Add(new TextBlock
            {
                Text = description,
                Margin = new Thickness(0, 6, 0, 0),
                FontSize = 11,
                Foreground = EffectiveQuickActionsSecondaryBrush,
                TextWrapping = TextWrapping.Wrap
            });
            Grid.SetColumn(textStack, 1);
            grid.Children.Add(textStack);

            card.Child = grid;
            return card;
        }

        private Button CreateDialogActionButton(string label, Brush background, Brush foreground, Brush borderBrush, double width)
        {
            return new Button
            {
                Content = label,
                Width = width,
                Height = 42,
                Margin = new Thickness(10, 0, 0, 0),
                Cursor = Cursors.Hand,
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Background = background,
                Foreground = foreground,
                BorderBrush = borderBrush,
                BorderThickness = new Thickness(1)
            };
        }

        private Button CreateMiniActionButton(string content, double fontSize)
        {
            return new Button
            {
                Content = content,
                Width = 34,
                Height = 34,
                Cursor = Cursors.Hand,
                FontSize = fontSize,
                FontWeight = FontWeights.SemiBold,
                Foreground = EffectiveQuickActionsPrimaryBrush,
                Background = EffectiveQuickActionsMutedSurfaceBrush,
                BorderBrush = EffectiveQuickActionsBorderBrush,
                BorderThickness = new Thickness(1)
            };
        }

        private CheckBox CreateAccessibilityCheckBox(bool isChecked)
        {
            return new CheckBox
            {
                IsChecked = isChecked,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = EffectiveQuickActionsPrimaryBrush
            };
        }

        private object? TryGetResource(string key)
        {
            return TryFindResource(key);
        }

        private static bool CanToggleWindowState(Window window)
        {
            return window.ResizeMode == ResizeMode.CanResize || window.ResizeMode == ResizeMode.CanResizeWithGrip;
        }

        private static T? FindTaggedContentElementCore<T>(object? source, string tag) where T : FrameworkElement
        {
            if (source is null)
            {
                return null;
            }

            if (source is T typedElement && Equals(typedElement.Tag, tag))
            {
                return typedElement;
            }

            if (source is not DependencyObject dependencyObject)
            {
                return null;
            }

            foreach (var child in EnumerateChildren(dependencyObject))
            {
                var result = FindTaggedContentElementCore<T>(child, tag);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private static IEnumerable<DependencyObject> EnumerateChildren(DependencyObject parent)
        {
            if (parent is Panel panel)
            {
                foreach (UIElement child in panel.Children)
                {
                    yield return child;
                }
            }

            if (parent is Decorator decorator && decorator.Child != null)
            {
                yield return decorator.Child;
            }

            if (parent is ContentControl contentControl && contentControl.Content is DependencyObject contentChild)
            {
                yield return contentChild;
            }

            if (parent is ContentPresenter contentPresenter && contentPresenter.Content is DependencyObject presenterChild)
            {
                yield return presenterChild;
            }

            if (parent is ItemsControl itemsControl)
            {
                foreach (var item in itemsControl.Items)
                {
                    if (item is DependencyObject itemDependency)
                    {
                        yield return itemDependency;
                    }
                }
            }

            foreach (var logicalChild in LogicalTreeHelper.GetChildren(parent))
            {
                if (logicalChild is DependencyObject logicalDependency)
                {
                    yield return logicalDependency;
                }
            }
        }

        private static bool IsInteractiveSource(object? source)
        {
            var current = source as DependencyObject;
            while (current != null)
            {
                if (current is ButtonBase
                    || current is TextBoxBase
                    || current is PasswordBox
                    || current is ComboBox
                    || current is Selector
                    || current is Slider
                    || current is ScrollBar
                    || current is Thumb)
                {
                    return true;
                }

                current = GetParent(current);
            }

            return false;
        }

        private static DependencyObject? GetParent(DependencyObject current)
        {
            if (current is Visual || current is Visual3D)
            {
                var parent = VisualTreeHelper.GetParent(current);
                if (parent != null)
                {
                    return parent;
                }
            }

            return LogicalTreeHelper.GetParent(current);
        }

        private static void BeginFallbackDrag(Window window, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            if (e.ClickCount == 2)
            {
                if (CanToggleWindowState(window))
                {
                    if (window.WindowState == WindowState.Maximized)
                    {
                        SystemCommands.RestoreWindow(window);
                    }
                    else
                    {
                        SystemCommands.MaximizeWindow(window);
                    }
                }

                return;
            }

            try
            {
                window.DragMove();
            }
            catch (InvalidOperationException)
            {
            }
        }

        private static Brush CreateFrozenBrush(byte red, byte green, byte blue)
        {
            var brush = new SolidColorBrush(Color.FromRgb(red, green, blue));
            brush.Freeze();
            return brush;
        }
    }
}