using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace MeuApp
{
    /// <summary>
    /// Classe auxiliar para capturar logs de debug em um arquivo
    /// </summary>
    public static class DebugHelper
    {
        private static readonly string LogFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            "AppDebug.log"
        );

        private static StreamWriter? _logWriter;
        private static bool _initialized = false;

        /// <summary>
        /// Inicializa o logging em arquivo
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;

            try
            {
                _logWriter = new StreamWriter(LogFilePath, false)
                {
                    AutoFlush = true
                };

                WriteLine($"========== LOG INICIADO EM {DateTime.Now:yyyy-MM-dd HH:mm:ss} ==========");
                WriteLine($"Caminho do arquivo: {LogFilePath}");
                WriteLine("");

                // Adicionar trace listener
                Trace.Listeners.Add(new TextWriterTraceListener(_logWriter));
                
                _initialized = true;

                MessageBox.Show(
                    $"Modo de diagnóstico ativado!\n\nOs logs serão salvos em:\n{LogFilePath}",
                    "Debug Ativado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao inicializar logging: {ex.Message}");
            }
        }

        /// <summary>
        /// Inicializa o logging em arquivo sem exibir mensagem na UI.
        /// </summary>
        public static void InitializeSilent()
        {
            if (_initialized) return;

            try
            {
                _logWriter = new StreamWriter(LogFilePath, false)
                {
                    AutoFlush = true
                };

                WriteLine($"========== LOG INICIADO EM {DateTime.Now:yyyy-MM-dd HH:mm:ss} ==========");
                WriteLine($"Caminho do arquivo: {LogFilePath}");
                WriteLine("Modo silencioso ativado para diagnóstico de rede.");
                WriteLine("");

                Trace.Listeners.Add(new TextWriterTraceListener(_logWriter));
                _initialized = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Falha ao inicializar log silencioso: {ex.Message}");
            }
        }

        /// <summary>
        /// Escreve uma linha no arquivo de log
        /// </summary>
        public static void WriteLine(string message)
        {
            try
            {
                if (_logWriter != null)
                {
                    _logWriter.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
                }
            }
            catch (IOException ex)
            {
                Debug.WriteLine($"[DebugHelper] Falha ao gravar log em arquivo: {ex.Message}");
                try
                {
                    _logWriter?.Dispose();
                }
                catch
                {
                }

                _logWriter = null;
                _initialized = false;
            }
            catch (ObjectDisposedException)
            {
                _logWriter = null;
                _initialized = false;
            }

            Debug.WriteLine(message);
        }

        /// <summary>
        /// Fecha o arquivo de log
        /// </summary>
        public static void Shutdown()
        {
            if (_logWriter != null)
            {
                WriteLine($"========== LOG ENCERRADO EM {DateTime.Now:yyyy-MM-dd HH:mm:ss} ==========");
                _logWriter.Flush();
                _logWriter.Close();
                _logWriter.Dispose();
                _logWriter = null;
            }
        }

        /// <summary>
        /// Abre o arquivo de log no Bloco de Notas
        /// </summary>
        public static void OpenLogFile()
        {
            if (File.Exists(LogFilePath))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = LogFilePath,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao abrir arquivo: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Arquivo de log não encontrado!");
            }
        }

        /// <summary>
        /// Retorna o caminho do arquivo de log
        /// </summary>
        public static string GetLogFilePath()
        {
            return LogFilePath;
        }

        /// <summary>
        /// Verifica se o logging está inicializado
        /// </summary>
        public static bool IsInitialized => _initialized;
    }
}
