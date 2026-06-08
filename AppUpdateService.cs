using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MeuApp;

public enum UpdatePackageKind
{
    Installer,
    ZipArchive,
    PortableExecutable
}

public sealed record UpdateReleaseAsset(string Name, string DownloadUrl, long Size);

public sealed record AppUpdateInfo(
    Version Version,
    string TagName,
    string ReleaseName,
    string ReleasePageUrl,
    string? ReleaseNotes,
    UpdateReleaseAsset Asset,
    UpdatePackageKind PackageKind)
{
    public string AssetName => Asset.Name;
    public string AssetUrl => Asset.DownloadUrl;
}

public static class AppUpdateService
{
    private static readonly HttpClient HttpClient = CreateHttpClient();
    private static readonly Regex VersionRegex = new Regex(
        @"(?<!\d)v?(?<version>\d+(?:\.\d+){0,3})(?:[-+][0-9A-Za-z.-]+)?",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static int _startupCheckStarted;

    public static async Task CheckForUpdatesOnStartupAsync()
    {
        if (Interlocked.Exchange(ref _startupCheckStarted, 1) == 1)
        {
            return;
        }

        try
        {
            await Task.Delay(TimeSpan.FromSeconds(3));

            var update = await CheckForUpdateAsync();
            if (update is null)
            {
                return;
            }

            await PromptAndApplyUpdateAsync(update);
        }
        catch (Exception ex)
        {
            LogUpdaterFailure(ex);
        }
    }

    public static async Task<AppUpdateInfo?> CheckForUpdateAsync(CancellationToken cancellationToken = default)
    {
        var settings = AppConfig.Updater;
        if (!settings.Enabled ||
            string.IsNullOrWhiteSpace(settings.GitHubOwner) ||
            string.IsNullOrWhiteSpace(settings.GitHubRepository))
        {
            return null;
        }

        var currentVersion = GetCurrentVersion();
        var endpoint = settings.IncludePrereleases
            ? BuildGitHubApiUrl(settings.GitHubOwner, settings.GitHubRepository, "releases?per_page=20")
            : BuildGitHubApiUrl(settings.GitHubOwner, settings.GitHubRepository, "releases/latest");

        using var response = await HttpClient.GetAsync(endpoint, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        if (document.RootElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var releaseElement in document.RootElement.EnumerateArray())
            {
                var update = TryBuildUpdateInfo(releaseElement, currentVersion, settings.IncludePrereleases);
                if (update is not null)
                {
                    return update;
                }
            }

            return null;
        }

        return TryBuildUpdateInfo(document.RootElement, currentVersion, settings.IncludePrereleases);
    }

    public static Version GetCurrentVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var informationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        return TryParseReleaseVersion(informationalVersion) ??
            NormalizeVersion(assembly.GetName().Version) ??
            new Version(0, 0, 0, 0);
    }

    public static Version? TryParseReleaseVersion(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var match = VersionRegex.Match(value.Trim());
        if (!match.Success)
        {
            return null;
        }

        var versionParts = match.Groups["version"].Value
            .Split('.', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => int.TryParse(part, NumberStyles.None, CultureInfo.InvariantCulture, out var number) ? number : -1)
            .ToList();

        if (versionParts.Count == 0 || versionParts.Any(part => part < 0))
        {
            return null;
        }

        while (versionParts.Count < 4)
        {
            versionParts.Add(0);
        }

        return new Version(versionParts[0], versionParts[1], versionParts[2], versionParts[3]);
    }

    public static UpdateReleaseAsset? SelectPreferredAsset(IEnumerable<UpdateReleaseAsset> assets)
    {
        return assets
            .Where(asset => !string.IsNullOrWhiteSpace(asset.Name) && !string.IsNullOrWhiteSpace(asset.DownloadUrl))
            .Select(asset => new
            {
                Asset = asset,
                Kind = GetPackageKind(asset.Name),
                Rank = GetAssetRank(asset.Name)
            })
            .Where(candidate => candidate.Kind.HasValue)
            .OrderBy(candidate => candidate.Rank)
            .ThenByDescending(candidate => candidate.Asset.Name.Contains("Choas", StringComparison.OrdinalIgnoreCase))
            .ThenBy(candidate => candidate.Asset.Name, StringComparer.OrdinalIgnoreCase)
            .Select(candidate => candidate.Asset)
            .FirstOrDefault();
    }

    public static UpdatePackageKind? GetPackageKind(string assetName)
    {
        var normalized = (assetName ?? string.Empty).Trim().ToLowerInvariant();

        if (normalized.EndsWith(".msi", StringComparison.OrdinalIgnoreCase))
        {
            return UpdatePackageKind.Installer;
        }

        if (normalized.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            return UpdatePackageKind.ZipArchive;
        }

        if (normalized.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
            return normalized.Contains("setup", StringComparison.OrdinalIgnoreCase) ||
                normalized.Contains("installer", StringComparison.OrdinalIgnoreCase) ||
                normalized.Contains("install", StringComparison.OrdinalIgnoreCase)
                    ? UpdatePackageKind.Installer
                    : UpdatePackageKind.PortableExecutable;
        }

        return null;
    }

    private static async Task PromptAndApplyUpdateAsync(AppUpdateInfo update)
    {
        var accepted = await AskUserToInstallUpdateAsync(update);
        if (!accepted)
        {
            return;
        }

        try
        {
            var packagePath = await DownloadUpdatePackageAsync(update);
            StartDownloadedUpdate(update, packagePath);
        }
        catch (Exception ex)
        {
            LogUpdaterFailure(ex);
            await ShowUpdateErrorAsync(ex);
        }
    }

    private static async Task<bool> AskUserToInstallUpdateAsync(AppUpdateInfo update)
    {
        var app = Application.Current;
        if (app is null)
        {
            return false;
        }

        return await app.Dispatcher.InvokeAsync(() =>
        {
            var currentVersion = GetCurrentVersion();
            var message = new StringBuilder()
                .AppendLine($"Uma nova versão do Choas está disponível.")
                .AppendLine()
                .AppendLine($"Versão instalada: {FormatVersion(currentVersion)}")
                .AppendLine($"Nova versão: {FormatVersion(update.Version)}")
                .AppendLine($"Pacote: {update.AssetName}")
                .AppendLine()
                .AppendLine("Deseja baixar e instalar agora? O aplicativo será fechado durante a atualização.")
                .ToString();

            var owner = CurrentWindowOrNull();
            var result = owner is null
                ? MessageBox.Show(message, "Atualização disponível", MessageBoxButton.YesNo, MessageBoxImage.Information)
                : MessageBox.Show(owner, message, "Atualização disponível", MessageBoxButton.YesNo, MessageBoxImage.Information);

            return result == MessageBoxResult.Yes;
        });
    }

    private static async Task<string> DownloadUpdatePackageAsync(AppUpdateInfo update, CancellationToken cancellationToken = default)
    {
        var updateDirectory = Path.Combine(Path.GetTempPath(), "ChoasUpdates", update.Version.ToString());
        Directory.CreateDirectory(updateDirectory);

        var packagePath = Path.Combine(updateDirectory, MakeSafeFileName(update.AssetName));

        using var response = await HttpClient.GetAsync(update.AssetUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var remoteStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var localStream = File.Create(packagePath);
        await remoteStream.CopyToAsync(localStream, cancellationToken);

        return packagePath;
    }

    private static void StartDownloadedUpdate(AppUpdateInfo update, string packagePath)
    {
        if (update.PackageKind == UpdatePackageKind.Installer)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = packagePath,
                WorkingDirectory = Path.GetDirectoryName(packagePath) ?? AppContext.BaseDirectory,
                UseShellExecute = true
            });

            ShutdownApplication();
            return;
        }

        var scriptPath = CreatePowerShellUpdateScript(packagePath, update.PackageKind);
        Process.Start(new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -File {QuoteProcessArgument(scriptPath)}",
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        });

        ShutdownApplication();
    }

    private static string CreatePowerShellUpdateScript(string packagePath, UpdatePackageKind packageKind)
    {
        var appDirectory = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var targetExecutable = Environment.ProcessPath ??
            Process.GetCurrentProcess().MainModule?.FileName ??
            Path.Combine(appDirectory, "Choas.exe");

        if (!targetExecutable.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Atualização portátil só é suportada quando o aplicativo está rodando por um executável .exe.");
        }

        var updateDirectory = Path.Combine(Path.GetTempPath(), "ChoasUpdates");
        Directory.CreateDirectory(updateDirectory);

        var scriptPath = Path.Combine(updateDirectory, $"ApplyChoasUpdate_{Guid.NewGuid():N}.ps1");
        var logPath = Path.Combine(updateDirectory, "Choas_Update.log");
        var processId = Environment.ProcessId.ToString(CultureInfo.InvariantCulture);

        var scriptLines = new[]
        {
            "$ErrorActionPreference = 'Stop'",
            "$ProgressPreference = 'SilentlyContinue'",
            $"$processIdToWait = {processId}",
            $"$packagePath = '{EscapePowerShellSingleQuotedString(packagePath)}'",
            $"$appDir = '{EscapePowerShellSingleQuotedString(appDirectory)}'",
            $"$targetExe = '{EscapePowerShellSingleQuotedString(targetExecutable)}'",
            $"$kind = '{packageKind}'",
            $"$logPath = '{EscapePowerShellSingleQuotedString(logPath)}'",
            "function Write-UpdateLog($message) {",
            "    Add-Content -LiteralPath $logPath -Value \"[$(Get-Date -Format s)] $message\"",
            "}",
            "try {",
            "    Write-UpdateLog 'Aguardando o aplicativo fechar.'",
            "    Wait-Process -Id $processIdToWait -ErrorAction SilentlyContinue",
            "    Start-Sleep -Milliseconds 800",
            "    if ($kind -eq 'ZipArchive') {",
            "        $staging = Join-Path $env:TEMP ('Choas_Update_' + [guid]::NewGuid().ToString('N'))",
            "        New-Item -ItemType Directory -Path $staging -Force | Out-Null",
            "        Expand-Archive -LiteralPath $packagePath -DestinationPath $staging -Force",
            "        $source = $staging",
            "        $exeName = Split-Path -Leaf $targetExe",
            "        if (-not (Test-Path -LiteralPath (Join-Path $source $exeName))) {",
            "            foreach ($child in Get-ChildItem -LiteralPath $staging -Directory -Force) {",
            "                if (Test-Path -LiteralPath (Join-Path $child.FullName $exeName)) {",
            "                    $source = $child.FullName",
            "                    break",
            "                }",
            "            }",
            "        }",
            "        Get-ChildItem -LiteralPath $source -Force | Copy-Item -Destination $appDir -Recurse -Force",
            "        Remove-Item -LiteralPath $staging -Recurse -Force",
            "    } elseif ($kind -eq 'PortableExecutable') {",
            "        Copy-Item -LiteralPath $packagePath -Destination $targetExe -Force",
            "    }",
            "    Write-UpdateLog 'Atualização aplicada. Reiniciando.'",
            "    Start-Process -FilePath $targetExe -WorkingDirectory $appDir",
            "} catch {",
            "    Write-UpdateLog ('Falha na atualização: ' + $_.Exception.ToString())",
            "}"
        };

        File.WriteAllText(scriptPath, string.Join(Environment.NewLine, scriptLines), Encoding.UTF8);
        return scriptPath;
    }

    private static AppUpdateInfo? TryBuildUpdateInfo(JsonElement releaseElement, Version currentVersion, bool includePrereleases)
    {
        if (GetBooleanProperty(releaseElement, "draft") == true)
        {
            return null;
        }

        if (!includePrereleases && GetBooleanProperty(releaseElement, "prerelease") == true)
        {
            return null;
        }

        var tagName = GetStringProperty(releaseElement, "tag_name") ?? string.Empty;
        var releaseName = GetStringProperty(releaseElement, "name") ?? tagName;
        var releaseVersion = TryParseReleaseVersion(tagName) ?? TryParseReleaseVersion(releaseName);
        if (releaseVersion is null || releaseVersion.CompareTo(currentVersion) <= 0)
        {
            return null;
        }

        if (!releaseElement.TryGetProperty("assets", out var assetsElement) ||
            assetsElement.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var asset = SelectPreferredAsset(ParseAssets(assetsElement));
        if (asset is null)
        {
            return null;
        }

        var packageKind = GetPackageKind(asset.Name);
        if (packageKind is null)
        {
            return null;
        }

        return new AppUpdateInfo(
            releaseVersion,
            tagName,
            releaseName,
            GetStringProperty(releaseElement, "html_url") ?? string.Empty,
            GetStringProperty(releaseElement, "body"),
            asset,
            packageKind.Value);
    }

    private static IEnumerable<UpdateReleaseAsset> ParseAssets(JsonElement assetsElement)
    {
        foreach (var assetElement in assetsElement.EnumerateArray())
        {
            var name = GetStringProperty(assetElement, "name");
            var downloadUrl = GetStringProperty(assetElement, "browser_download_url");
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(downloadUrl))
            {
                continue;
            }

            var size = assetElement.TryGetProperty("size", out var sizeElement) && sizeElement.TryGetInt64(out var parsedSize)
                ? parsedSize
                : 0;

            yield return new UpdateReleaseAsset(name, downloadUrl, size);
        }
    }

    private static int GetAssetRank(string assetName)
    {
        var normalized = (assetName ?? string.Empty).Trim().ToLowerInvariant();
        var kind = GetPackageKind(normalized);

        return kind switch
        {
            UpdatePackageKind.Installer => normalized.EndsWith(".msi", StringComparison.OrdinalIgnoreCase) ? 0 : 1,
            UpdatePackageKind.ZipArchive => 2,
            UpdatePackageKind.PortableExecutable => 3,
            _ => 100
        };
    }

    private static HttpClient CreateHttpClient()
    {
        var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Choas-Updater/1.0");
        httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");

        return httpClient;
    }

    private static string BuildGitHubApiUrl(string owner, string repository, string path)
    {
        return $"https://api.github.com/repos/{Uri.EscapeDataString(owner.Trim())}/{Uri.EscapeDataString(repository.Trim())}/{path}";
    }

    private static Version? NormalizeVersion(Version? version)
    {
        if (version is null)
        {
            return null;
        }

        return new Version(
            Math.Max(version.Major, 0),
            Math.Max(version.Minor, 0),
            Math.Max(version.Build, 0),
            Math.Max(version.Revision, 0));
    }

    private static string FormatVersion(Version version)
    {
        return version.Revision > 0
            ? $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}"
            : $"{version.Major}.{version.Minor}.{version.Build}";
    }

    private static string MakeSafeFileName(string fileName)
    {
        var safeName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
        return string.IsNullOrWhiteSpace(safeName) ? $"ChoasUpdate_{Guid.NewGuid():N}" : safeName;
    }

    private static Window? CurrentWindowOrNull()
    {
        return Application.Current?.Windows
            .OfType<Window>()
            .FirstOrDefault(window => window.IsActive) ??
            Application.Current?.MainWindow;
    }

    private static string EscapePowerShellSingleQuotedString(string value)
    {
        return (value ?? string.Empty).Replace("'", "''");
    }

    private static string QuoteProcessArgument(string value)
    {
        return "\"" + (value ?? string.Empty).Replace("\"", "\\\"") + "\"";
    }

    private static string? GetStringProperty(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static bool? GetBooleanProperty(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? property.GetBoolean()
            : null;
    }

    private static void ShutdownApplication()
    {
        var app = Application.Current;
        if (app is null)
        {
            return;
        }

        if (app.Dispatcher.CheckAccess())
        {
            app.Shutdown();
            return;
        }

        app.Dispatcher.Invoke(app.Shutdown);
    }

    private static async Task ShowUpdateErrorAsync(Exception exception)
    {
        var app = Application.Current;
        if (app is null)
        {
            return;
        }

        await app.Dispatcher.InvokeAsync(() =>
        {
            var message = $"Não foi possível instalar a atualização agora.\n\n{exception.Message}";
            var owner = CurrentWindowOrNull();

            if (owner is null)
            {
                MessageBox.Show(message, "Falha na atualização", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                MessageBox.Show(owner, message, "Falha na atualização", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        });
    }

    private static void LogUpdaterFailure(Exception exception)
    {
        try
        {
            DebugHelper.WriteLine($"[AppUpdateService] {exception}");
        }
        catch
        {
            // Evita que erro no log derrube a verificação de atualização.
        }
    }
}
