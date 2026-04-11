using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MeuApp
{
    public sealed class DebugConsoleEntryViewModel
    {
        public DebugConsoleEntryViewModel(DebugLogEntry entry)
        {
            TimestampLabel = entry.Timestamp.ToString("HH:mm:ss.fff");
            Message = entry.Message;
            RawText = entry.FormattedText;
            Foreground = ResolveForeground(entry.Message);
        }

        public string TimestampLabel { get; }
        public string Message { get; }
        public string RawText { get; }
        public Brush Foreground { get; }

        private static Brush ResolveForeground(string message)
        {
            var value = message ?? string.Empty;

            if (value.Contains("ERROR", StringComparison.OrdinalIgnoreCase)
                || value.Contains("EXCEÇÃO", StringComparison.OrdinalIgnoreCase)
                || value.Contains("EXCECAO", StringComparison.OrdinalIgnoreCase)
                || value.Contains("FALHA", StringComparison.OrdinalIgnoreCase)
                || value.Contains("❌", StringComparison.OrdinalIgnoreCase)
                || value.Contains("HTTP 4", StringComparison.OrdinalIgnoreCase)
                || value.Contains("HTTP 5", StringComparison.OrdinalIgnoreCase))
            {
                return new SolidColorBrush(Color.FromRgb(252, 165, 165));
            }

            if (value.Contains("Firebase", StringComparison.OrdinalIgnoreCase)
                || value.Contains("HTTP", StringComparison.OrdinalIgnoreCase)
                || value.Contains("firestore", StringComparison.OrdinalIgnoreCase))
            {
                return new SolidColorBrush(Color.FromRgb(125, 211, 252));
            }

            if (value.Contains("Sync", StringComparison.OrdinalIgnoreCase)
                || value.Contains("Save", StringComparison.OrdinalIgnoreCase)
                || value.Contains("Load", StringComparison.OrdinalIgnoreCase)
                || value.Contains("Queue", StringComparison.OrdinalIgnoreCase))
            {
                return new SolidColorBrush(Color.FromRgb(134, 239, 172));
            }

            if (value.Contains("Warning", StringComparison.OrdinalIgnoreCase)
                || value.Contains("Aviso", StringComparison.OrdinalIgnoreCase))
            {
                return new SolidColorBrush(Color.FromRgb(253, 224, 71));
            }

            return new SolidColorBrush(Color.FromRgb(226, 232, 240));
        }
    }

    public partial class DebugConsoleWindow : ChromeWindow
    {
        private const int MaxStoredEntries = 6000;
        private const int MaxVisibleEntries = 2000;

        private readonly ObservableCollection<DebugConsoleEntryViewModel> _visibleEntries = new ObservableCollection<DebugConsoleEntryViewModel>();
        private readonly List<DebugLogEntry> _allEntries = new List<DebugLogEntry>();
        private readonly List<DebugLogEntry> _pausedEntries = new List<DebugLogEntry>();
        private bool _controlsReady;
        private bool _snapshotLoaded;
        private bool _isPaused;
        private bool _autoScroll = true;

        public bool HasBeenClosed { get; private set; }

        public DebugConsoleWindow()
        {
            InitializeComponent();
            _controlsReady = true;
            LogListBox.ItemsSource = _visibleEntries;
            LogPathTextBlock.Text = DebugHelper.GetLogFilePath();
            Loaded += DebugConsoleWindow_Loaded;
            Closed += DebugConsoleWindow_Closed;
            UpdateStatus();
        }

        public void SetHostWindow(Window? hostWindow)
        {
            HostWindowTextBlock.Text = hostWindow == null
                ? "Sem janela associada"
                : $"Host: {hostWindow.Title}";
        }

        private void DebugConsoleWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_snapshotLoaded)
            {
                AppendEntries(DebugHelper.GetRecentEntries(500), forceScroll: false);
                _snapshotLoaded = true;
            }

            DebugHelper.LogReceived -= DebugHelper_LogReceived;
            DebugHelper.LogReceived += DebugHelper_LogReceived;
            UpdateStatus();
        }

        private void DebugConsoleWindow_Closed(object? sender, EventArgs e)
        {
            HasBeenClosed = true;
            DebugHelper.LogReceived -= DebugHelper_LogReceived;
        }

        private void DebugHelper_LogReceived(DebugLogEntry entry)
        {
            if (!Dispatcher.CheckAccess())
            {
                _ = Dispatcher.BeginInvoke(new Action(() => DebugHelper_LogReceived(entry)));
                return;
            }

            if (_isPaused)
            {
                _pausedEntries.Add(entry);
                TrimList(_pausedEntries, 800);
                UpdateStatus();
                return;
            }

            AppendEntries(new[] { entry }, forceScroll: true);
        }

        private void AppendEntries(IEnumerable<DebugLogEntry> entries, bool forceScroll)
        {
            var batch = (entries ?? Array.Empty<DebugLogEntry>()).ToList();
            if (batch.Count == 0)
            {
                UpdateStatus();
                return;
            }

            _allEntries.AddRange(batch);
            TrimList(_allEntries, MaxStoredEntries);

            if (HasActiveFilter())
            {
                RebuildVisibleEntries();
            }
            else
            {
                foreach (var entry in batch)
                {
                    _visibleEntries.Add(new DebugConsoleEntryViewModel(entry));
                }

                while (_visibleEntries.Count > MaxVisibleEntries)
                {
                    _visibleEntries.RemoveAt(0);
                }
            }

            UpdateStatus();

            if (_autoScroll && forceScroll)
            {
                ScrollToBottom();
            }
        }

        private void RebuildVisibleEntries()
        {
            var filter = FilterTextBox.Text?.Trim() ?? string.Empty;
            var filtered = string.IsNullOrWhiteSpace(filter)
                ? _allEntries
                : _allEntries.Where(entry =>
                        entry.Message.Contains(filter, StringComparison.OrdinalIgnoreCase)
                        || entry.FormattedText.Contains(filter, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            var recent = filtered.Count <= MaxVisibleEntries
                ? filtered
                : filtered.Skip(filtered.Count - MaxVisibleEntries).ToList();

            _visibleEntries.Clear();
            foreach (var entry in recent)
            {
                _visibleEntries.Add(new DebugConsoleEntryViewModel(entry));
            }

            UpdateStatus();
            if (_autoScroll)
            {
                ScrollToBottom();
            }
        }

        private void PauseToggleButton_Changed(object sender, RoutedEventArgs e)
        {
            if (!AreControlsReady())
            {
                return;
            }

            _isPaused = PauseToggleButton.IsChecked == true;
            ModeBadgeText.Text = _isPaused ? "PAUSADO" : "AO VIVO";

            if (!_isPaused && _pausedEntries.Count > 0)
            {
                var buffered = _pausedEntries.ToList();
                _pausedEntries.Clear();
                AppendEntries(buffered, forceScroll: true);
                return;
            }

            UpdateStatus();
        }

        private void AutoScrollToggleButton_Changed(object sender, RoutedEventArgs e)
        {
            if (!AreControlsReady())
            {
                return;
            }

            _autoScroll = AutoScrollToggleButton.IsChecked != false;
            UpdateStatus();

            if (_autoScroll)
            {
                ScrollToBottom();
            }
        }

        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!AreControlsReady())
            {
                return;
            }

            RebuildVisibleEntries();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _allEntries.Clear();
            _pausedEntries.Clear();
            _visibleEntries.Clear();
            UpdateStatus();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            var content = string.Join(Environment.NewLine, _visibleEntries.Select(entry => entry.RawText));
            if (string.IsNullOrWhiteSpace(content))
            {
                return;
            }

            Clipboard.SetText(content);
            StatusTextBlock.Text = "Console copiado para a área de transferência.";
        }

        private void OpenLogButton_Click(object sender, RoutedEventArgs e)
        {
            DebugHelper.OpenLogFile();
        }

        private void UpdateStatus()
        {
            if (!AreControlsReady())
            {
                return;
            }

            var filteredText = HasActiveFilter() ? " filtradas" : string.Empty;
            CountTextBlock.Text = $"{_visibleEntries.Count} linha(s){filteredText}";

            if (_isPaused)
            {
                StatusTextBlock.Text = _pausedEntries.Count == 0
                    ? "Pausado. Novos eventos ficam em buffer até retomar."
                    : $"Pausado com {_pausedEntries.Count} evento(s) em buffer.";
                return;
            }

            StatusTextBlock.Text = _autoScroll
                ? "Recebendo eventos em tempo real com auto-scroll ligado."
                : "Recebendo eventos em tempo real com auto-scroll desligado.";
        }

        private bool HasActiveFilter()
        {
            return !string.IsNullOrWhiteSpace(FilterTextBox?.Text);
        }

        private void ScrollToBottom()
        {
            if (!AreControlsReady() || _visibleEntries.Count == 0)
            {
                return;
            }

            LogListBox.ScrollIntoView(_visibleEntries[_visibleEntries.Count - 1]);
        }

        private bool AreControlsReady()
        {
            return _controlsReady
                && FilterTextBox != null
                && CountTextBlock != null
                && StatusTextBlock != null
                && ModeBadgeText != null
                && LogListBox != null
                && AutoScrollToggleButton != null
                && PauseToggleButton != null
                && HostWindowTextBlock != null
                && LogPathTextBlock != null;
        }

        private static void TrimList<T>(List<T> values, int maxCount)
        {
            if (values.Count <= maxCount)
            {
                return;
            }

            values.RemoveRange(0, values.Count - maxCount);
        }
    }
}