using System;
using System.IO;
using System.Text.Json;

namespace MeuApp
{
    public sealed class FirebaseSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public string StorageBucket { get; set; } = string.Empty;
    }

    internal sealed class AppConfigDocument
    {
        public FirebaseSettings Firebase { get; set; } = new FirebaseSettings();
    }

    public static class AppConfig
    {
        private const string DefaultFirebaseApiKey = "AIzaSyA2V4MEzgOoKEEZAAXH49DXbzxUo0_CuWU";
        private const string DefaultFirebaseProjectId = "obsseractpi";
        private const string DefaultFirebaseStorageBucket = "obsseractpi.appspot.com";
        private static readonly Lazy<FirebaseSettings> FirebaseSettingsLazy = new Lazy<FirebaseSettings>(LoadFirebaseSettings);

        public static FirebaseSettings Firebase => FirebaseSettingsLazy.Value;

        public static string FirebaseApiKey => Firebase.ApiKey;

        public static string FirebaseProjectId => Firebase.ProjectId;

        public static string FirebaseStorageBucket => Firebase.StorageBucket;

        public static string FirestoreBaseUrl => $"https://firestore.googleapis.com/v1/projects/{FirebaseProjectId}/databases/(default)";

        public static string FirebaseStorageBaseUrl => $"https://firebasestorage.googleapis.com/v0/b/{FirebaseStorageBucket}";

        public static string BuildFirestoreDocumentUrl(string relativePath)
        {
            return $"{FirestoreBaseUrl}/documents/{relativePath}";
        }

        public static string BuildFirestoreRunQueryUrl()
        {
            return $"{FirestoreBaseUrl}/documents:runQuery";
        }

        public static string BuildFirebaseStorageUploadUrl(string objectPath)
        {
            var escapedObjectPath = Uri.EscapeDataString((objectPath ?? string.Empty).Trim('/'));
            return $"{FirebaseStorageBaseUrl}/o?uploadType=media&name={escapedObjectPath}";
        }

        public static string BuildFirebaseStorageMetadataUrl(string objectPath)
        {
            var escapedObjectPath = Uri.EscapeDataString((objectPath ?? string.Empty).Trim('/'));
            return $"{FirebaseStorageBaseUrl}/o/{escapedObjectPath}";
        }

        public static string BuildFirebaseStorageDownloadUrl(string objectPath)
        {
            return $"{BuildFirebaseStorageMetadataUrl(objectPath)}?alt=media";
        }

        private static FirebaseSettings LoadFirebaseSettings()
        {
            var settings = new FirebaseSettings
            {
                ApiKey = DefaultFirebaseApiKey,
                ProjectId = DefaultFirebaseProjectId,
                StorageBucket = DefaultFirebaseStorageBucket
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

                    if (!string.IsNullOrWhiteSpace(document.Firebase.StorageBucket))
                    {
                        settings.StorageBucket = document.Firebase.StorageBucket.Trim();
                    }
                }
                catch
                {
                    // Usa fallback interno quando a configuração local estiver inválida.
                }
            }

            var apiKeyOverride = Environment.GetEnvironmentVariable("CHOAS_FIREBASE_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKeyOverride))
            {
                apiKeyOverride = Environment.GetEnvironmentVariable("OBSSERACT_FIREBASE_API_KEY");
            }
            if (!string.IsNullOrWhiteSpace(apiKeyOverride))
            {
                settings.ApiKey = apiKeyOverride.Trim();
            }

            var projectIdOverride = Environment.GetEnvironmentVariable("CHOAS_FIREBASE_PROJECT_ID");
            if (string.IsNullOrWhiteSpace(projectIdOverride))
            {
                projectIdOverride = Environment.GetEnvironmentVariable("OBSSERACT_FIREBASE_PROJECT_ID");
            }
            if (!string.IsNullOrWhiteSpace(projectIdOverride))
            {
                settings.ProjectId = projectIdOverride.Trim();
            }

            var storageBucketOverride = Environment.GetEnvironmentVariable("CHOAS_FIREBASE_STORAGE_BUCKET");
            if (string.IsNullOrWhiteSpace(storageBucketOverride))
            {
                storageBucketOverride = Environment.GetEnvironmentVariable("OBSSERACT_FIREBASE_STORAGE_BUCKET");
            }
            if (!string.IsNullOrWhiteSpace(storageBucketOverride))
            {
                settings.StorageBucket = storageBucketOverride.Trim();
            }

            if (string.IsNullOrWhiteSpace(settings.StorageBucket))
            {
                settings.StorageBucket = $"{settings.ProjectId}.appspot.com";
            }

            return settings;
        }
    }
}