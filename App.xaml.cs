using System.Windows;

namespace MeuApp;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            base.OnStartup(e);
            
            AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
            {
                MessageBox.Show($"Erro nao capturado: {ex.ExceptionObject}", "Erro");
            };
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao iniciar: {ex.Message}\n\n{ex.StackTrace}", "Erro na Aplicacao");
            Environment.Exit(1);
        }
    }
}

