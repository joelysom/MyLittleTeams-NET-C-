using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace MeuApp;

public partial class App : Application
{
    private static string _logFile = "Choas_Errors.log";

    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            DebugHelper.InitializeSilent();
            DebugConsoleManager.Configure(e.Args);
            ApplyGlobalTheme(AccessibilityPreferences.Current);

            // Log de inicialização
            LogToFile($"=== App started at {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} ===");
            
            base.OnStartup(e);

            Activated += (_, __) => DebugConsoleManager.HandleApplicationActivated();
            
            // Handler para exceções não tratadas do AppDomain
            AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
            {
                string errorMsg = $"[UNHANDLED]\n{FormatExceptionDetails(ex.ExceptionObject as Exception) ?? ex.ExceptionObject?.ToString() ?? "Unknown unhandled exception"}";
                LogToFile(errorMsg);
                MessageBox.Show($"Erro não capturado:\n\n{ex.ExceptionObject}", "Erro Crítico");
                Environment.Exit(1);
            };

            // Handler para exceções da UI
            this.DispatcherUnhandledException += (s, ex) =>
            {
                string errorMsg = $"[UI_ERROR]\n{FormatExceptionDetails(ex.Exception)}";
                LogToFile(errorMsg);
                MessageBox.Show($"Erro na interface:\n\n{BuildUserFacingErrorSummary(ex.Exception)}\n\nVerifique {_logFile}", "Erro na Tela");
                ex.Handled = true;
            };

            LogToFile("Startup completed successfully");
            _ = AppUpdateService.CheckForUpdatesOnStartupAsync();
        }
        catch (Exception ex)
        {
            string errorMsg = $"[STARTUP_ERROR]\n{FormatExceptionDetails(ex)}";
            LogToFile(errorMsg);
            MessageBox.Show($"Erro ao iniciar:\n\n{BuildUserFacingErrorSummary(ex)}\n\nLog salvo em: {_logFile}", "Erro na Inicialização");
            Environment.Exit(1);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        DebugConsoleManager.Shutdown();
        DebugHelper.Shutdown();
        base.OnExit(e);
    }

    public static void ApplyGlobalTheme(AccessibilitySettings settings)
    {
        var highContrastEnabled = settings.HighContrastEnabled;
        var darkModeEnabled = settings.DarkModeEnabled;

        SetGlobalBrush("WindowBackgroundBrush", highContrastEnabled
            ? Color.FromRgb(3, 7, 18)
            : darkModeEnabled ? Color.FromRgb(7, 17, 31) : Color.FromRgb(243, 247, 252));
        SetGlobalBrush("SurfaceBrush", highContrastEnabled
            ? Color.FromRgb(6, 12, 24)
            : darkModeEnabled ? Color.FromRgb(11, 18, 32) : Colors.White);
        SetGlobalBrush("SidebarBackgroundBrush", highContrastEnabled
            ? Color.FromRgb(0, 0, 0)
            : darkModeEnabled ? Color.FromRgb(9, 17, 30) : Color.FromRgb(248, 250, 252));
        SetGlobalBrush("SidebarBorderBrush", highContrastEnabled
            ? Color.FromRgb(56, 189, 248)
            : darkModeEnabled ? Color.FromRgb(30, 41, 59) : Color.FromRgb(226, 232, 240));
        SetGlobalBrush("TopBarBackgroundBrush", highContrastEnabled
            ? Color.FromRgb(6, 12, 24)
            : darkModeEnabled ? Color.FromRgb(11, 18, 32) : Colors.White);
        SetGlobalBrush("MainContentBackgroundBrush", highContrastEnabled
            ? Color.FromRgb(4, 9, 18)
            : darkModeEnabled ? Color.FromRgb(8, 16, 27) : Color.FromRgb(248, 251, 255));
        SetGlobalBrush("SearchBackgroundBrush", highContrastEnabled
            ? Color.FromRgb(11, 18, 32)
            : darkModeEnabled ? Color.FromRgb(17, 28, 46) : Color.FromRgb(248, 250, 252));
        SetGlobalBrush("SearchBorderBrush", highContrastEnabled
            ? Color.FromRgb(56, 189, 248)
            : darkModeEnabled ? Color.FromRgb(36, 50, 71) : Color.FromRgb(216, 226, 238));
        SetGlobalBrush("PrimaryTextBrush", highContrastEnabled
            ? Color.FromRgb(248, 250, 252)
            : darkModeEnabled ? Color.FromRgb(226, 232, 240) : Color.FromRgb(15, 23, 42));
        SetGlobalBrush("SecondaryTextBrush", highContrastEnabled
            ? Color.FromRgb(203, 213, 225)
            : darkModeEnabled ? Color.FromRgb(148, 163, 184) : Color.FromRgb(71, 85, 105));
        SetGlobalBrush("TertiaryTextBrush", highContrastEnabled
            ? Color.FromRgb(148, 163, 184)
            : darkModeEnabled ? Color.FromRgb(125, 139, 162) : Color.FromRgb(100, 116, 139));
        SetGlobalBrush("CardBackgroundBrush", highContrastEnabled
            ? Color.FromRgb(6, 12, 24)
            : darkModeEnabled ? Color.FromRgb(15, 23, 42) : Colors.White);
        SetGlobalBrush("MutedCardBackgroundBrush", highContrastEnabled
            ? Color.FromRgb(11, 18, 32)
            : darkModeEnabled ? Color.FromRgb(17, 28, 46) : Color.FromRgb(248, 250, 252));
        SetGlobalBrush("CardBorderBrush", highContrastEnabled
            ? Color.FromRgb(56, 189, 248)
            : darkModeEnabled ? Color.FromRgb(34, 50, 71) : Color.FromRgb(219, 229, 240));
        SetGlobalBrush("AccentBrush", highContrastEnabled
            ? Color.FromRgb(125, 211, 252)
            : darkModeEnabled ? Color.FromRgb(96, 165, 250) : Color.FromRgb(37, 99, 235));
        SetGlobalBrush("AccentMutedBrush", highContrastEnabled
            ? Color.FromRgb(12, 36, 63)
            : darkModeEnabled ? Color.FromRgb(16, 38, 69) : Color.FromRgb(232, 238, 255));
        SetGlobalBrush("SuccessBrush", highContrastEnabled
            ? Color.FromRgb(74, 222, 128)
            : darkModeEnabled ? Color.FromRgb(34, 197, 94) : Color.FromRgb(22, 163, 74));
        SetGlobalBrush("WarningBrush", highContrastEnabled
            ? Color.FromRgb(250, 204, 21)
            : Color.FromRgb(245, 158, 11));
        SetGlobalBrush("DangerBrush", highContrastEnabled
            ? Color.FromRgb(248, 113, 113)
            : darkModeEnabled ? Color.FromRgb(248, 113, 113) : Color.FromRgb(220, 38, 38));
        SetGlobalBrush("ChatIconBrush", highContrastEnabled
            ? Color.FromRgb(94, 234, 212)
            : darkModeEnabled ? Color.FromRgb(45, 212, 191) : Color.FromRgb(15, 118, 110));
        SetGlobalBrush("ConnectionsIconBrush", highContrastEnabled
            ? Color.FromRgb(125, 211, 252)
            : darkModeEnabled ? Color.FromRgb(56, 189, 248) : Color.FromRgb(2, 132, 199));
        SetGlobalBrush("TeamsIconBrush", highContrastEnabled
            ? Color.FromRgb(147, 197, 253)
            : darkModeEnabled ? Color.FromRgb(96, 165, 250) : Color.FromRgb(37, 99, 235));
        SetGlobalBrush("CalendarIconBrush", highContrastEnabled
            ? Color.FromRgb(253, 186, 116)
            : darkModeEnabled ? Color.FromRgb(251, 146, 60) : Color.FromRgb(234, 88, 12));
        SetGlobalBrush("FilesIconBrush", highContrastEnabled
            ? Color.FromRgb(196, 181, 253)
            : darkModeEnabled ? Color.FromRgb(167, 139, 250) : Color.FromRgb(124, 58, 237));
        SetGlobalBrush("SettingsIconBrush", highContrastEnabled
            ? Color.FromRgb(203, 213, 225)
            : darkModeEnabled ? Color.FromRgb(148, 163, 184) : Color.FromRgb(100, 116, 139));
        SetGlobalBrush("ToggleTrackOffBrush", highContrastEnabled
            ? Color.FromRgb(71, 85, 105)
            : darkModeEnabled ? Color.FromRgb(51, 65, 85) : Color.FromRgb(203, 213, 225));
        SetGlobalBrush("ToggleTrackOnBrush", highContrastEnabled
            ? Color.FromRgb(14, 165, 233)
            : darkModeEnabled ? Color.FromRgb(56, 189, 248) : Color.FromRgb(37, 99, 235));
        SetGlobalBrush("ToggleThumbBrush", Colors.White);
        SetGlobalBrush("ScrollBarTrackBrush", highContrastEnabled
            ? Color.FromRgb(3, 7, 18)
            : darkModeEnabled ? Color.FromRgb(15, 23, 42) : Color.FromRgb(241, 243, 245));
        SetGlobalBrush("ScrollBarThumbBrush", highContrastEnabled
            ? Color.FromRgb(125, 211, 252)
            : darkModeEnabled ? Color.FromRgb(71, 85, 105) : Color.FromRgb(168, 176, 189));
        SetGlobalBrush("ScrollBarThumbHoverBrush", highContrastEnabled
            ? Color.FromRgb(186, 230, 253)
            : darkModeEnabled ? Color.FromRgb(100, 116, 139) : Color.FromRgb(127, 137, 150));
        SetGlobalBrush("ScrollBarThumbPressedBrush", highContrastEnabled
            ? Color.FromRgb(224, 242, 254)
            : darkModeEnabled ? Color.FromRgb(148, 163, 184) : Color.FromRgb(100, 116, 139));

        SetGlobalBrush("BrandSurfaceBrush", darkModeEnabled || highContrastEnabled ? Color.FromRgb(11, 18, 32) : Color.FromRgb(248, 250, 252));
        SetGlobalBrush("BrandTextBrush", darkModeEnabled || highContrastEnabled ? Color.FromRgb(226, 232, 240) : Color.FromRgb(15, 23, 42));
        SetGlobalBrush("BrandMutedTextBrush", darkModeEnabled || highContrastEnabled ? Color.FromRgb(148, 163, 184) : Color.FromRgb(71, 85, 105));
        SetGlobalBrush("BrandBorderBrush", darkModeEnabled || highContrastEnabled ? Color.FromRgb(34, 50, 71) : Color.FromRgb(226, 232, 240));
    }

    private static void SetGlobalBrush(string key, Color color)
    {
        var resources = Current?.Resources;
        if (resources == null)
        {
            return;
        }

        if (resources[key] is SolidColorBrush brush && !brush.IsFrozen)
        {
            brush.Color = color;
            return;
        }

        resources[key] = new SolidColorBrush(color);
    }

    private static string BuildUserFacingErrorSummary(Exception? exception)
    {
        if (exception is null)
        {
            return "Erro desconhecido.";
        }

        var root = exception;
        while (root.InnerException is not null)
        {
            root = root.InnerException;
        }

        return root == exception
            ? $"{exception.GetType().Name}: {exception.Message}"
            : $"{exception.GetType().Name}: {exception.Message}\n\nCausa raiz: {root.GetType().Name}: {root.Message}";
    }

    private static string FormatExceptionDetails(Exception? exception)
    {
        if (exception is null)
        {
            return "Unknown exception.";
        }

        var builder = new StringBuilder();
        var current = exception;
        var depth = 0;

        while (current is not null)
        {
            if (depth > 0)
            {
                builder.AppendLine();
            }

            builder.AppendLine($"[{depth}] {current.GetType().FullName}: {current.Message}");

            if (!string.IsNullOrWhiteSpace(current.StackTrace))
            {
                builder.AppendLine(current.StackTrace);
            }

            current = current.InnerException;
            depth++;
        }

        return builder.ToString().TrimEnd();
    }

    private static void LogToFile(string message)
    {
        try
        {
            if (DebugHelper.IsInitialized)
            {
                DebugHelper.WriteLine($"[App] {message}");
            }

            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _logFile);
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string logEntry = $"[{timestamp}] {message}";
            
            File.AppendAllText(logPath, logEntry + Environment.NewLine);
        }
        catch
        {
            // Falha silenciosa se não conseguir escrever o log
        }
    }
}

