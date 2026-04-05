using System;
using System.IO;
using System.Text.Json;

namespace MeuApp
{
    public sealed class FirebaseSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
    }

    internal sealed class AppConfigDocument
    {
        public FirebaseSettings Firebase { get; set; } = new FirebaseSettings();
    }

    public static class AppConfig
    {
        private const string DefaultFirebaseApiKey = "AIzaSyA2V4MEzgOoKEEZAAXH49DXbzxUo0_CuWU";
        private const string DefaultFirebaseProjectId = "obsseractpi";
        private static readonly Lazy<FirebaseSettings> FirebaseSettingsLazy = new Lazy<FirebaseSettings>(LoadFirebaseSettings);

        public static FirebaseSettings Firebase => FirebaseSettingsLazy.Value;

        public static string FirebaseApiKey => Firebase.ApiKey;

        public static string FirebaseProjectId => Firebase.ProjectId;

        public static string FirestoreBaseUrl => $"https://firestore.googleapis.com/v1/projects/{FirebaseProjectId}/databases/(default)";

        public static string BuildFirestoreDocumentUrl(string relativePath)
        {
            return $"{FirestoreBaseUrl}/documents/{relativePath}";
        }

        public static string BuildFirestoreRunQueryUrl()
        {
            return $"{FirestoreBaseUrl}/documents:runQuery";
        }

        private static FirebaseSettings LoadFirebaseSettings()
        {
            var settings = new FirebaseSettings
            {
                ApiKey = DefaultFirebaseApiKey,
                ProjectId = DefaultFirebaseProjectId
            };

            foreach (var configPath in new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.local.json"),
                Path.Combine(Environment.CurrentDirectory, "appsettings.local.json")
            })
            {
                if (!File.Exists(configPath))
                {
                    continue;
                }

                try
                {
                    var json = File.ReadAllText(configPath);
                    var document = JsonSerializer.Deserialize<AppConfigDocument>(json);
                    if (document?.Firebase == null)
                    {
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(document.Firebase.ApiKey))
                    {
                        settings.ApiKey = document.Firebase.ApiKey.Trim();
                    }

                    if (!string.IsNullOrWhiteSpace(document.Firebase.ProjectId))
                    {
                        settings.ProjectId = document.Firebase.ProjectId.Trim();
                    }
                }
                catch
                {
                    // Usa fallback interno quando a configuração local estiver inválida.
                }
            }

            var apiKeyOverride = Environment.GetEnvironmentVariable("OBSSERACT_FIREBASE_API_KEY");
            if (!string.IsNullOrWhiteSpace(apiKeyOverride))
            {
                settings.ApiKey = apiKeyOverride.Trim();
            }

            var projectIdOverride = Environment.GetEnvironmentVariable("OBSSERACT_FIREBASE_PROJECT_ID");
            if (!string.IsNullOrWhiteSpace(projectIdOverride))
            {
                settings.ProjectId = projectIdOverride.Trim();
            }

            return settings;
        }
    }
}