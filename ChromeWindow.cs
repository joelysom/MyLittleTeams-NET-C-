using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;

namespace MeuApp
{
    public class ChromeWindow : Window
    {
        private const int WmGetMinMaxInfo = 0x0024;
        private const uint MonitorDefaultToNearest = 0x00000002;
        private static readonly Thickness ResizeBorder = new Thickness(8);
        private readonly ScaleTransform _contentScaleTransform = new ScaleTransform(1, 1);
        private HwndSource? _hwndSource;
        private FrameworkElement? _contentScaleTarget;

        public ChromeWindow()
        {
            WindowStyle = WindowStyle.None;
            BorderThickness = new Thickness(0);
            Loaded += ChromeWindow_Loaded;
        }

        public bool CanMinimizeWindow => ResizeMode != ResizeMode.NoResize;

        public bool CanToggleWindowState => ResizeMode == ResizeMode.CanResize || ResizeMode == ResizeMode.CanResizeWithGrip;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            ApplyChrome();
            _hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            _hwndSource?.AddHook(WndProc);
            AccessibilityPreferences.SettingsChanged += AccessibilityPreferences_SettingsChanged;
            ApplyAccessibilitySettings(AccessibilityPreferences.Current);
        }

        protected override void OnClosed(EventArgs e)
        {
            AccessibilityPreferences.SettingsChanged -= AccessibilityPreferences_SettingsChanged;
            _hwndSource?.RemoveHook(WndProc);
            _hwndSource = null;
            base.OnClosed(e);
        }

        public void ApplyChrome()
        {
            WindowStyle = WindowStyle.None;
            BorderThickness = new Thickness(0);

            WindowChrome.SetWindowChrome(this, new WindowChrome
            {
                CaptionHeight = 0,
                CornerRadius = new CornerRadius(0),
                GlassFrameThickness = new Thickness(0),
                ResizeBorderThickness = CanToggleWindowState ? ResizeBorder : new Thickness(0),
                UseAeroCaptionButtons = false
            });
        }

        public void BeginTitleBarDrag(MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            if (e.ClickCount == 2)
            {
                if (CanToggleWindowState)
                {
                    ToggleWindowState();
                }

                return;
            }

            try
            {
                if (WindowState == WindowState.Maximized && CanToggleWindowState)
                {
                    var mousePosition = e.GetPosition(this);
                    var screenPosition = PointToScreen(mousePosition);
                    var restoreWidth = RestoreBounds.Width > 0 ? RestoreBounds.Width : ActualWidth;
                    var horizontalRatio = ActualWidth > 0 ? mousePosition.X / ActualWidth : 0.5;

                    SystemCommands.RestoreWindow(this);
                    Left = screenPosition.X - (restoreWidth * horizontalRatio);
                    Top = Math.Max(screenPosition.Y - Math.Min(mousePosition.Y, 40), 0);
                }

                DragMove();
            }
            catch (InvalidOperationException)
            {
            }
        }

        public void MinimizeWindow()
        {
            if (CanMinimizeWindow)
            {
                SystemCommands.MinimizeWindow(this);
            }
        }

        public void ToggleWindowState()
        {
            if (!CanToggleWindowState)
            {
                return;
            }

            if (WindowState == WindowState.Maximized)
            {
                SystemCommands.RestoreWindow(this);
            }
            else
            {
                SystemCommands.MaximizeWindow(this);
            }
        }

        public void CloseWindowCommand()
        {
            SystemCommands.CloseWindow(this);
        }

        protected virtual void OnAccessibilitySettingsChanged(AccessibilitySettings settings)
        {
        }

        private void ChromeWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyAccessibilitySettings(AccessibilityPreferences.Current);
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
            ApplyTextScale(settings.TextScalePercent);
            OnAccessibilitySettingsChanged(settings);
        }

        private void ApplyTextScale(int textScalePercent)
        {
            var target = ResolveContentScaleTarget();
            if (target == null)
            {
                return;
            }

            if (!ReferenceEquals(_contentScaleTarget, target))
            {
                _contentScaleTarget = target;
                _contentScaleTarget.LayoutTransform = _contentScaleTransform;
                _contentScaleTarget.RenderTransformOrigin = new Point(0, 0);
            }

            var normalizedScale = Math.Clamp(textScalePercent, AccessibilityPreferences.MinTextScalePercent, AccessibilityPreferences.MaxTextScalePercent) / 100d;
            _contentScaleTransform.ScaleX = normalizedScale;
            _contentScaleTransform.ScaleY = normalizedScale;
        }

        private FrameworkElement? ResolveContentScaleTarget()
        {
            if (Content is not FrameworkElement rootElement)
            {
                return null;
            }

            if (rootElement is Panel panel)
            {
                WindowTitleBar? titleBar = null;
                FrameworkElement? fallback = null;

                foreach (UIElement child in panel.Children)
                {
                    if (child is WindowTitleBar windowTitleBar)
                    {
                        titleBar = windowTitleBar;
                        continue;
                    }

                    if (child is not FrameworkElement childElement)
                    {
                        continue;
                    }

                    if (titleBar != null && panel is Grid && Grid.GetRow(childElement) > Grid.GetRow(titleBar))
                    {
                        return childElement;
                    }

                    fallback ??= childElement;
                }

                return fallback;
            }

            if (rootElement is ContentControl contentControl
                && contentControl.Content is FrameworkElement contentChild
                && contentChild is not WindowTitleBar)
            {
                return contentChild;
            }

            return rootElement is WindowTitleBar ? null : rootElement;
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WmGetMinMaxInfo)
            {
                UpdateMinMaxInfo(hwnd, lParam);
                handled = true;
            }

            return IntPtr.Zero;
        }

        private static void UpdateMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            var minMaxInfo = Marshal.PtrToStructure<MinMaxInfo>(lParam);
            var monitor = MonitorFromWindow(hwnd, MonitorDefaultToNearest);
            if (monitor == IntPtr.Zero)
            {
                Marshal.StructureToPtr(minMaxInfo, lParam, true);
                return;
            }

            var monitorInfo = new MonitorInfo();
            monitorInfo.cbSize = Marshal.SizeOf<MonitorInfo>();
            if (!GetMonitorInfo(monitor, ref monitorInfo))
            {
                Marshal.StructureToPtr(minMaxInfo, lParam, true);
                return;
            }

            var workArea = monitorInfo.rcWork;
            var monitorArea = monitorInfo.rcMonitor;

            minMaxInfo.ptMaxPosition.x = workArea.left - monitorArea.left;
            minMaxInfo.ptMaxPosition.y = workArea.top - monitorArea.top;
            minMaxInfo.ptMaxSize.x = workArea.right - workArea.left;
            minMaxInfo.ptMaxSize.y = workArea.bottom - workArea.top;
            minMaxInfo.ptMaxTrackSize.x = minMaxInfo.ptMaxSize.x;
            minMaxInfo.ptMaxTrackSize.y = minMaxInfo.ptMaxSize.y;

            Marshal.StructureToPtr(minMaxInfo, lParam, true);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfo lpmi);

        [StructLayout(LayoutKind.Sequential)]
        private struct PointStruct
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MinMaxInfo
        {
            public PointStruct ptReserved;
            public PointStruct ptMaxSize;
            public PointStruct ptMaxPosition;
            public PointStruct ptMinTrackSize;
            public PointStruct ptMaxTrackSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RectStruct
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MonitorInfo
        {
            public int cbSize;
            public RectStruct rcMonitor;
            public RectStruct rcWork;
            public int dwFlags;
        }
    }
}