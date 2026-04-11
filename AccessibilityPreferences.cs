using System;
using System.IO;
using System.Text.Json;

namespace MeuApp
{
    public sealed class AccessibilitySettings
    {
        public bool HighContrastEnabled { get; init; }

        public bool DarkModeEnabled { get; init; }

        public int TextScalePercent { get; init; } = 100;

        public bool ReduceAnimations { get; init; }
    }

    public sealed class AccessibilitySettingsChangedEventArgs : EventArgs
    {
        public AccessibilitySettingsChangedEventArgs(AccessibilitySettings settings)
        {
            Settings = settings;
        }

        public AccessibilitySettings Settings { get; }
    }

    internal sealed class AccessibilitySettingsDocument
    {
        public bool HighContrastEnabled { get; set; }

        public bool DarkModeEnabled { get; set; }

        public int TextScalePercent { get; set; } = 100;

        public bool ReduceAnimations { get; set; }
    }

    public static class AccessibilityPreferences
    {
        public const int MinTextScalePercent = 90;
        public const int MaxTextScalePercent = 140;
        public const int TextScaleStepPercent = 10;
        private static readonly string SettingsDirectoryPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MeuApp");
        private static readonly string SettingsFilePath = Path.Combine(SettingsDirectoryPath, "accessibility.settings.json");
        private static readonly object SyncRoot = new object();

        private static bool _highContrastEnabled;
        private static bool _darkModeEnabled;
        private static int _textScalePercent = 100;
        private static bool _reduceAnimations;

        static AccessibilityPreferences()
        {
            Load();
        }

        public static event EventHandler<AccessibilitySettingsChangedEventArgs>? SettingsChanged;

        public static bool HighContrastEnabled => _highContrastEnabled;

        public static bool DarkModeEnabled => _darkModeEnabled;

        public static int TextScalePercent => _textScalePercent;

        public static bool ReduceAnimations => _reduceAnimations;

        public static AccessibilitySettings Current => new AccessibilitySettings
        {
            HighContrastEnabled = _highContrastEnabled,
            DarkModeEnabled = _darkModeEnabled,
            TextScalePercent = _textScalePercent,
            ReduceAnimations = _reduceAnimations
        };

        public static void SetHighContrastEnabled(bool enabled)
        {
            UpdateSettings(enabled, _darkModeEnabled, _textScalePercent, _reduceAnimations);
        }

        public static void SetDarkModeEnabled(bool enabled)
        {
            UpdateSettings(_highContrastEnabled, enabled, _textScalePercent, _reduceAnimations);
        }

        public static void SetTextScalePercent(int textScalePercent)
        {
            UpdateSettings(_highContrastEnabled, _darkModeEnabled, NormalizeTextScale(textScalePercent), _reduceAnimations);
        }

        public static void AdjustTextScale(int deltaPercent)
        {
            SetTextScalePercent(_textScalePercent + deltaPercent);
        }

        public static void SetReduceAnimations(bool enabled)
        {
            UpdateSettings(_highContrastEnabled, _darkModeEnabled, _textScalePercent, enabled);
        }

        private static void UpdateSettings(bool highContrastEnabled, bool darkModeEnabled, int textScalePercent, bool reduceAnimations)
        {
            AccessibilitySettings snapshot;
            lock (SyncRoot)
            {
                var normalizedTextScale = NormalizeTextScale(textScalePercent);
                if (_highContrastEnabled == highContrastEnabled
                    && _darkModeEnabled == darkModeEnabled
                    && _textScalePercent == normalizedTextScale
                    && _reduceAnimations == reduceAnimations)
                {
                    return;
                }

                _highContrastEnabled = highContrastEnabled;
                _darkModeEnabled = darkModeEnabled;
                _textScalePercent = normalizedTextScale;
                _reduceAnimations = reduceAnimations;
                Save();
                snapshot = Current;
            }

            SettingsChanged?.Invoke(null, new AccessibilitySettingsChangedEventArgs(snapshot));
        }

        private static void Load()
        {
            lock (SyncRoot)
            {
                if (!File.Exists(SettingsFilePath))
                {
                    _highContrastEnabled = false;
                    _darkModeEnabled = false;
                    _textScalePercent = 100;
                    _reduceAnimations = false;
                    return;
                }

                try
                {
                    var json = File.ReadAllText(SettingsFilePath);
                    var document = JsonSerializer.Deserialize<AccessibilitySettingsDocument>(json);
                    _highContrastEnabled = document?.HighContrastEnabled == true;
                    _darkModeEnabled = document?.DarkModeEnabled == true;
                    _textScalePercent = NormalizeTextScale(document?.TextScalePercent ?? 100);
                    _reduceAnimations = document?.ReduceAnimations == true;
                }
                catch
                {
                    _highContrastEnabled = false;
                    _darkModeEnabled = false;
                    _textScalePercent = 100;
                    _reduceAnimations = false;
                }
            }
        }

        private static void Save()
        {
            try
            {
                Directory.CreateDirectory(SettingsDirectoryPath);
                var document = new AccessibilitySettingsDocument
                {
                    HighContrastEnabled = _highContrastEnabled,
                    DarkModeEnabled = _darkModeEnabled,
                    TextScalePercent = _textScalePercent,
                    ReduceAnimations = _reduceAnimations
                };

                var json = JsonSerializer.Serialize(document, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(SettingsFilePath, json);
            }
            catch
            {
            }
        }

        private static int NormalizeTextScale(int value)
        {
            return Math.Clamp(value, MinTextScalePercent, MaxTextScalePercent);
        }
    }
}
