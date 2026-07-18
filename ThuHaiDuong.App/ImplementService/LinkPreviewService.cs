using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Application.Payloads.ResultModels.Utils;

namespace ThuHaiDuong.Application.ImplementService;

public class LinkPreviewService : ILinkPreviewService
{
    private readonly HttpClient _http;

    public LinkPreviewService(HttpClient http)
    {
        _http = http;
    }

    public async Task<LinkPreviewResult> GetPreviewAsync(string url)
    {
        try
        {
            var html = await _http.GetStringAsync(url);
            return ParseOgTags(html, url);
        }
        catch
        {
            // Không throw — trả về result rỗng nếu fetch fail
            return new LinkPreviewResult { Url = url };
        }
    }

    private static LinkPreviewResult ParseOgTags(string html, string originalUrl)
    {
        var result = new LinkPreviewResult { Url = originalUrl };

        // Parse og: tags
        result.Image       = ExtractMeta(html, "og:image")
                          ?? ExtractMeta(html, "twitter:image")
                          ?? ExtractMeta(html, "twitter:image:src");

        result.Title       = ExtractMeta(html, "og:title")
                          ?? ExtractMeta(html, "twitter:title")
                          ?? ExtractTag(html, "title");

        result.Description = ExtractMeta(html, "og:description")
                          ?? ExtractMeta(html, "twitter:description")
                          ?? ExtractMeta(html, "description");

        result.SiteName    = ExtractMeta(html, "og:site_name");

        // Nếu image là relative URL → convert thành absolute
        if (!string.IsNullOrEmpty(result.Image)
            && !result.Image.StartsWith("http")
            && Uri.TryCreate(originalUrl, UriKind.Absolute, out var baseUri))
        {
            result.Image = new Uri(baseUri, result.Image).ToString();
        }

        return result;
    }

    // Parse <meta property="og:image" content="...">
    // và    <meta name="og:image" content="...">
    private static string? ExtractMeta(string html, string name)
    {
        var patterns = new[]
        {
            $@"<meta[^>]+property=[""']{System.Text.RegularExpressions.Regex.Escape(name)}[""'][^>]+content=[""']([^""']+)[""']",
            $@"<meta[^>]+content=[""']([^""']+)[""'][^>]+property=[""']{System.Text.RegularExpressions.Regex.Escape(name)}[""']",
            $@"<meta[^>]+name=[""']{System.Text.RegularExpressions.Regex.Escape(name)}[""'][^>]+content=[""']([^""']+)[""']",
            $@"<meta[^>]+content=[""']([^""']+)[""'][^>]+name=[""']{System.Text.RegularExpressions.Regex.Escape(name)}[""']",
        };

        foreach (var pattern in patterns)
        {
            var match = System.Text.RegularExpressions.Regex.Match(
                html, pattern,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (match.Success)
                return System.Net.WebUtility.HtmlDecode(match.Groups[1].Value.Trim());
        }

        return null;
    }

    private static string? ExtractTag(string html, string tag)
    {
        var match = System.Text.RegularExpressions.Regex.Match(
            html, $@"<{tag}[^>]*>([^<]+)</{tag}>",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        return match.Success
            ? System.Net.WebUtility.HtmlDecode(match.Groups[1].Value.Trim())
            : null;
    }
}