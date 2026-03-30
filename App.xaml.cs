using System.Windows;
using System.IO;

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
                string errorMsg = $"[UNHANDLED] {ex.ExceptionObject}\n{ex.ToString()}";
                LogToFile(errorMsg);
                MessageBox.Show($"Erro não capturado:\n\n{ex.ExceptionObject}", "Erro Crítico");
                Environment.Exit(1);
            };

            // Handler para exceções da UI
            this.DispatcherUnhandledException += (s, ex) =>
            {
                string errorMsg = $"[UI_ERROR] {ex.Exception.Message}\n{ex.Exception.StackTrace}";
                LogToFile(errorMsg);
                MessageBox.Show($"Erro na interface:\n\n{ex.Exception.Message}\n\nVerifique {_logFile}", "Erro na Tela");
                ex.Handled = true;
            };

            LogToFile("Startup completed successfully");
        }
        catch (Exception ex)
        {
            string errorMsg = $"[STARTUP_ERROR] {ex.Message}\n{ex.StackTrace}";
            LogToFile(errorMsg);
            MessageBox.Show($"Erro ao iniciar:\n\n{ex.Message}\n\nLog salvo em: {_logFile}", "Erro na Inicialização");
            Environment.Exit(1);
        }
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

