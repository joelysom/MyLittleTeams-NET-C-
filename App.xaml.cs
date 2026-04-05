using System.Windows;
using System.IO;
using System.Text;

namespace MeuApp;

public partial class App : Application
{
    private static string _logFile = "MeuApp_Errors.log";

    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            // Log de inicialização
            LogToFile($"=== App started at {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} ===");
            
            base.OnStartup(e);
            
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
        }
        catch (Exception ex)
        {
            string errorMsg = $"[STARTUP_ERROR]\n{FormatExceptionDetails(ex)}";
            LogToFile(errorMsg);
            MessageBox.Show($"Erro ao iniciar:\n\n{BuildUserFacingErrorSummary(ex)}\n\nLog salvo em: {_logFile}", "Erro na Inicialização");
            Environment.Exit(1);
        }
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

