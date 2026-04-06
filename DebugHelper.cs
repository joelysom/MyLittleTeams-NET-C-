using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace MeuApp
{
    public sealed class DebugLogEntry
    {
        public DebugLogEntry(DateTime timestamp, string message)
        {
            Timestamp = timestamp;
            Message = message ?? string.Empty;
        }

        public DateTime Timestamp { get; }
        public string Message { get; }
        public string FormattedText => $"[{Timestamp:HH:mm:ss.fff}] {Message}";
    }

    /// <summary>
    /// Classe auxiliar para capturar logs de debug em um arquivo
    /// </summary>
    public static class DebugHelper
    {
        private const long MaxLogFileSizeBytes = 2 * 1024 * 1024;
        private const int MaxRecentEntries = 1200;
        private static readonly string LogDirectoryPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "logs"
        );
        private static readonly string LogFilePath = Path.Combine(LogDirectoryPath, "AppDebug.log");
        private static readonly object SyncRoot = new object();
        private static readonly Queue<DebugLogEntry> _recentEntries = new Queue<DebugLogEntry>();

        private static StreamWriter? _logWriter;
        private static TextWriterTraceListener? _traceListener;
        private static bool _initialized = false;

        public static event Action<DebugLogEntry>? LogReceived;

        /// <summary>
        /// Inicializa o logging em arquivo
        /// </summary>
        public static void Initialize()
        {
            InitializeCore(showMessage: true);
        }

        /// <summary>
        /// Inicializa o logging em arquivo sem exibir mensagem na UI.
        /// </summary>
        public static void InitializeSilent()
        {
            InitializeCore(showMessage: false);
        }

        /// <summary>
        /// Escreve uma linha no arquivo de log
        /// </summary>
        public static void WriteLine(string message)
        {
            var entry = new DebugLogEntry(DateTime.Now, message);

            try
            {
                if (_logWriter != null)
                {
                    _logWriter.WriteLine(entry.FormattedText);
                }
            }
            catch (IOException ex)
            {
                Debug.WriteLine($"[DebugHelper] Falha ao gravar log em arquivo: {ex.Message}");
                CleanupWriter();
            }
            catch (ObjectDisposedException)
            {
                CleanupWriter();
            }

            CacheEntry(entry);
            NotifyListeners(entry);
            Debug.WriteLine(entry.Message);
        }

        public static IReadOnlyList<DebugLogEntry> GetRecentEntries(int maxEntries = 400)
        {
            lock (SyncRoot)
            {
                if (_recentEntries.Count == 0)
                {
                    return Array.Empty<DebugLogEntry>();
                }

                var entries = _recentEntries.ToArray();
                if (entries.Length <= maxEntries)
                {
                    return entries;
                }

                return entries.Skip(entries.Length - maxEntries).ToArray();
            }
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
                CleanupWriter();
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

        private static void InitializeCore(bool showMessage)
        {
            if (_initialized) return;

            try
            {
                Directory.CreateDirectory(LogDirectoryPath);
                ResetLogFileIfTooLarge();

                _logWriter = new StreamWriter(LogFilePath, false)
                {
                    AutoFlush = true
                };

                _traceListener = new TextWriterTraceListener(_logWriter);
                Trace.Listeners.Add(_traceListener);

                _initialized = true;

                WriteLine($"========== LOG INICIADO EM {DateTime.Now:yyyy-MM-dd HH:mm:ss} ==========");
                WriteLine($"Caminho do arquivo: {LogFilePath}");
                WriteLine("");

                if (showMessage)
                {
                    MessageBox.Show(
                        $"Modo de diagnóstico ativado!\n\nOs logs serão salvos em:\n{LogFilePath}",
                        "Debug Ativado",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DebugHelper] Falha ao inicializar logging: {ex.Message}");

                if (showMessage)
                {
                    MessageBox.Show($"Erro ao inicializar logging: {ex.Message}");
                }
            }
        }

        private static void ResetLogFileIfTooLarge()
        {
            if (!File.Exists(LogFilePath))
            {
                return;
            }

            var fileInfo = new FileInfo(LogFilePath);
            if (fileInfo.Length <= MaxLogFileSizeBytes)
            {
                return;
            }

            File.WriteAllText(LogFilePath, string.Empty);
        }

        private static void CacheEntry(DebugLogEntry entry)
        {
            lock (SyncRoot)
            {
                _recentEntries.Enqueue(entry);
                while (_recentEntries.Count > MaxRecentEntries)
                {
                    _recentEntries.Dequeue();
                }
            }
        }

        private static void NotifyListeners(DebugLogEntry entry)
        {
            try
            {
                LogReceived?.Invoke(entry);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DebugHelper] Falha ao notificar listeners de log: {ex.Message}");
            }
        }

        private static void CleanupWriter()
        {
            if (_traceListener != null)
            {
                Trace.Listeners.Remove(_traceListener);
                _traceListener.Dispose();
                _traceListener = null;
            }

            try
            {
                _logWriter?.Close();
                _logWriter?.Dispose();
            }
            catch
            {
            }

            _logWriter = null;
            _initialized = false;
        }
    }
}
