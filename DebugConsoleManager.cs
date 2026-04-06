using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace MeuApp
{
    public static class DebugConsoleManager
    {
        private static DebugConsoleWindow? _window;
        private static Window? _hostWindow;
        private static bool _allowWindowClose;
        private static bool _runtimeEnabled;

        public static bool IsEnabled => _runtimeEnabled;

        public static void Configure(string[]? args)
        {
            var normalizedArgs = args ?? Array.Empty<string>();
            var argEnabled = normalizedArgs.Any(arg => string.Equals(arg, "--debug-console", StringComparison.OrdinalIgnoreCase));
            var argDisabled = normalizedArgs.Any(arg => string.Equals(arg, "--no-debug-console", StringComparison.OrdinalIgnoreCase));
            var envEnabled = IsTruthy(Environment.GetEnvironmentVariable("MEUAPP_DEBUG_CONSOLE"));

            _runtimeEnabled = argDisabled ? false : argEnabled || envEnabled;
            DebugHelper.WriteLine(_runtimeEnabled
                ? "[DebugConsole] Mini console em tempo real habilitado."
                : "[DebugConsole] Mini console em tempo real desabilitado.");
        }

        public static void HandleApplicationActivated()
        {
            if (!_runtimeEnabled)
            {
                return;
            }

            EnsureVisible(GetPreferredHostWindow());
        }

        public static void Toggle(Window? preferredHost = null)
        {
            SetEnabled(!_runtimeEnabled, preferredHost);
        }

        public static void SetEnabled(bool enabled, Window? preferredHost = null)
        {
            _runtimeEnabled = enabled;

            if (!enabled)
            {
                if (_window is { HasBeenClosed: false, IsVisible: true })
                {
                    _window.Hide();
                }

                DebugHelper.WriteLine("[DebugConsole] Mini console desativado pelo usuário.");
                return;
            }

            DebugHelper.WriteLine("[DebugConsole] Mini console ativado pelo usuário.");
            EnsureVisible(preferredHost ?? GetPreferredHostWindow());
        }

        public static void EnsureVisible(Window? preferredHost = null)
        {
            if (!_runtimeEnabled)
            {
                return;
            }

            if (preferredHost is DebugConsoleWindow)
            {
                preferredHost = null;
            }

            preferredHost ??= GetPreferredHostWindow();

            EnsureWindowCreated();

            if (_window == null)
            {
                return;
            }

            if (preferredHost != null)
            {
                AttachToHost(preferredHost);
            }

            _window.SetHostWindow(_hostWindow);

            if (!TryShowWindow())
            {
                EnsureWindowCreated(forceRecreate: true);
                if (_window == null)
                {
                    return;
                }

                if (preferredHost != null)
                {
                    AttachToHost(preferredHost);
                }

                _window.SetHostWindow(_hostWindow);
                if (!TryShowWindow())
                {
                    return;
                }
            }

            UpdatePlacement();
        }

        public static void Shutdown()
        {
            DetachFromHost();

            if (_window == null)
            {
                return;
            }

            try
            {
                _allowWindowClose = true;
                _window.Close();
            }
            catch
            {
            }
            finally
            {
                _window = null;
                _allowWindowClose = false;
            }
        }

        private static void EnsureWindowCreated(bool forceRecreate = false)
        {
            if (forceRecreate || _window?.HasBeenClosed == true)
            {
                _window = null;
            }

            if (_window != null)
            {
                return;
            }

            _allowWindowClose = false;
            _window = new DebugConsoleWindow();
            _window.Closing += DebugConsoleWindow_Closing;
            _window.Closed += DebugConsoleWindow_Closed;
        }

        private static void DebugConsoleWindow_Closing(object? sender, CancelEventArgs e)
        {
            if (_allowWindowClose || sender is not DebugConsoleWindow window)
            {
                return;
            }

            e.Cancel = true;
            _runtimeEnabled = false;
            DetachFromHost();
            DebugHelper.WriteLine("[DebugConsole] Mini console fechada pelo usuário.");
            _ = window.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!window.HasBeenClosed && window.IsVisible)
                {
                    window.Hide();
                }
            }));
        }

        private static void DebugConsoleWindow_Closed(object? sender, EventArgs e)
        {
            if (ReferenceEquals(_window, sender))
            {
                _window = null;
            }

            _allowWindowClose = false;
            _runtimeEnabled = false;
            DetachFromHost();
        }

        private static Window? GetPreferredHostWindow()
        {
            if (Application.Current == null)
            {
                return null;
            }

            return Application.Current.Windows
                .OfType<Window>()
                .Where(window => window is not DebugConsoleWindow && window.IsVisible)
                .OrderByDescending(window => window.IsActive)
                .ThenByDescending(window => window.Topmost)
                .FirstOrDefault();
        }

        private static void AttachToHost(Window host)
        {
            if (host is DebugConsoleWindow)
            {
                return;
            }

            if (ReferenceEquals(_hostWindow, host))
            {
                UpdatePlacement();
                return;
            }

            DetachFromHost();

            _hostWindow = host;
            _hostWindow.LocationChanged += HostWindowLayoutChanged;
            _hostWindow.SizeChanged += HostWindowSizeChanged;
            _hostWindow.StateChanged += HostWindowStateChanged;
            _hostWindow.Closed += HostWindowClosed;
        }

        private static void DetachFromHost()
        {
            if (_hostWindow == null)
            {
                return;
            }

            _hostWindow.LocationChanged -= HostWindowLayoutChanged;
            _hostWindow.SizeChanged -= HostWindowSizeChanged;
            _hostWindow.StateChanged -= HostWindowStateChanged;
            _hostWindow.Closed -= HostWindowClosed;
            _hostWindow = null;
        }

        private static void HostWindowLayoutChanged(object? sender, EventArgs e)
        {
            UpdatePlacement();
        }

        private static void HostWindowSizeChanged(object? sender, SizeChangedEventArgs e)
        {
            UpdatePlacement();
        }

        private static void HostWindowStateChanged(object? sender, EventArgs e)
        {
            if (_window?.HasBeenClosed == true)
            {
                _window = null;
            }

            if (_window == null || _hostWindow == null)
            {
                return;
            }

            if (_hostWindow.WindowState == WindowState.Minimized)
            {
                _window.Hide();
                return;
            }

            if (_runtimeEnabled && !TryShowWindow())
            {
                return;
            }

            UpdatePlacement();
        }

        private static void HostWindowClosed(object? sender, EventArgs e)
        {
            DetachFromHost();

            if (_runtimeEnabled)
            {
                EnsureVisible(GetPreferredHostWindow());
            }
        }

        private static void UpdatePlacement()
        {
            if (_window?.HasBeenClosed == true)
            {
                _window = null;
            }

            if (_window == null || _hostWindow == null || !_window.IsLoaded)
            {
                return;
            }

            if (_hostWindow.WindowState == WindowState.Minimized)
            {
                return;
            }

            var workArea = SystemParameters.WorkArea;
            var hostWidth = ResolveWindowWidth(_hostWindow);
            var hostHeight = ResolveWindowHeight(_hostWindow);
            var targetWidth = Math.Max(430, Math.Min(680, workArea.Width * 0.36));
            var targetHeight = Math.Max(420, Math.Min(workArea.Height - 16, hostHeight));
            const double spacing = 12;

            var left = _hostWindow.Left + hostWidth + spacing;
            if (left + targetWidth > workArea.Right)
            {
                left = Math.Max(workArea.Left + 8, _hostWindow.Left - targetWidth - spacing);
            }

            var top = Math.Max(workArea.Top + 8, Math.Min(_hostWindow.Top, workArea.Bottom - targetHeight - 8));

            _window.Width = targetWidth;
            _window.Height = targetHeight;
            _window.Left = left;
            _window.Top = top;
            _window.SetHostWindow(_hostWindow);
        }

        private static bool TryShowWindow()
        {
            if (_window == null)
            {
                return false;
            }

            if (_window.HasBeenClosed)
            {
                _window = null;
                return false;
            }

            try
            {
                if (!_window.IsVisible)
                {
                    _window.Show();
                }

                return true;
            }
            catch (InvalidOperationException ex)
            {
                DebugHelper.WriteLine($"[DebugConsole] Instância encerrada detectada; recriando mini console. {ex.Message}");
                _window = null;
                return false;
            }
        }

        private static double ResolveWindowWidth(Window window)
        {
            if (window.ActualWidth > 160)
            {
                return window.ActualWidth;
            }

            if (window.Width > 160 && !double.IsNaN(window.Width))
            {
                return window.Width;
            }

            return window.RestoreBounds.Width > 160 ? window.RestoreBounds.Width : 980;
        }

        private static double ResolveWindowHeight(Window window)
        {
            if (window.ActualHeight > 220)
            {
                return window.ActualHeight;
            }

            if (window.Height > 220 && !double.IsNaN(window.Height))
            {
                return window.Height;
            }

            return window.RestoreBounds.Height > 220 ? window.RestoreBounds.Height : 760;
        }

        private static bool IsTruthy(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return value.Trim().Equals("1", StringComparison.OrdinalIgnoreCase)
                || value.Trim().Equals("true", StringComparison.OrdinalIgnoreCase)
                || value.Trim().Equals("yes", StringComparison.OrdinalIgnoreCase)
                || value.Trim().Equals("on", StringComparison.OrdinalIgnoreCase);
        }
    }
}