using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MeuApp
{
    public sealed class ChatLinkPreviewMetadata
    {
        public string Url { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string ImageUrl { get; init; } = string.Empty;
        public string SiteName { get; init; } = string.Empty;
    }

    public static class ChatLinkPreviewService
    {
        private static readonly HttpClient HttpClient = CreateClient();
        private static readonly Regex UrlRegex = new Regex("https?://[^\\s<>()\\[\\]\"']+", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string? ExtractFirstUrl(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            var match = UrlRegex.Match(text);
            if (!match.Success)
            {
                return null;
            }

            return NormalizeUrl(match.Value);
        }

        public static async Task<ChatLinkPreviewMetadata?> TryBuildPreviewAsync(string? text)
        {
            var url = ExtractFirstUrl(text);
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            var siteName = GetSiteName(url);

            try
            {
                using var response = await HttpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                if (!response.IsSuccessStatusCode)
                {
                    return BuildFallback(url, siteName);
                }

                var html = await response.Content.ReadAsStringAsync();
                var title = FirstNonEmpty(
                    ExtractMetaContent(html, "property", "og:title"),
                    ExtractMetaContent(html, "name", "twitter:title"),
                    ExtractTitle(html));
                var description = FirstNonEmpty(
                    ExtractMetaContent(html, "property", "og:description"),
                    ExtractMetaContent(html, "name", "description"),
                    ExtractMetaContent(html, "name", "twitter:description"));
                var imageUrl = FirstNonEmpty(
                    ExtractMetaContent(html, "property", "og:image"),
                    ExtractMetaContent(html, "name", "twitter:image"));
                var resolvedSiteName = FirstNonEmpty(
                    ExtractMetaContent(html, "property", "og:site_name"),
                    siteName);

                return new ChatLinkPreviewMetadata
                {
                    Url = url,
                    Title = HtmlDecode(title),
                    Description = HtmlDecode(description),
                    ImageUrl = MakeAbsoluteUrl(url, imageUrl),
                    SiteName = HtmlDecode(resolvedSiteName)
                };
            }
            catch
            {
                return BuildFallback(url, siteName);
            }
        }

        private static ChatLinkPreviewMetadata BuildFallback(string url, string siteName)
        {
            return new ChatLinkPreviewMetadata
            {
                Url = url,
                Title = siteName,
                Description = url,
                SiteName = siteName
            };
        }

        private static HttpClient CreateClient()
        {
            var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(8)
            };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) ChoasChatPreview/1.0");
            client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            return client;
        }

        private static string? ExtractMetaContent(string html, string attributeName, string attributeValue)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return null;
            }

            var pattern = $"<meta[^>]+{attributeName}=[\"']{Regex.Escape(attributeValue)}[\"'][^>]+content=[\"'](?<content>[^\"']+)[\"'][^>]*>|<meta[^>]+content=[\"'](?<content>[^\"']+)[\"'][^>]+{attributeName}=[\"']{Regex.Escape(attributeValue)}[\"'][^>]*>";
            var match = Regex.Match(html, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            return match.Success ? match.Groups["content"].Value.Trim() : null;
        }

        private static string? ExtractTitle(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return null;
            }

            var match = Regex.Match(html, @"<title[^>]*>(?<title>.*?)</title>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            return match.Success ? match.Groups["title"].Value.Trim() : null;
        }

        private static string HtmlDecode(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : WebUtility.HtmlDecode(value).Trim();
        }

        private static string FirstNonEmpty(params string?[] values)
        {
            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }

            return string.Empty;
        }

        private static string NormalizeUrl(string url)
        {
            return (url ?? string.Empty).Trim().TrimEnd('.', ',', ';', ':', ')', ']', '}');
        }

        private static string GetSiteName(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uri)
                ? uri.Host
                : string.Empty;
        }

        private static string MakeAbsoluteUrl(string baseUrl, string? rawUrl)
        {
            if (string.IsNullOrWhiteSpace(rawUrl))
            {
                return string.Empty;
            }

            if (Uri.TryCreate(rawUrl, UriKind.Absolute, out var absoluteUri))
            {
                return absoluteUri.ToString();
            }

            if (Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri)
                && Uri.TryCreate(baseUri, rawUrl, out var resolvedUri))
            {
                return resolvedUri.ToString();
            }

            return rawUrl.Trim();
        }
    }
}