using System.Text.RegularExpressions;

namespace OrderSystem.Application.AI.Providers.WebContent;

public class HtmlContentExtractor : IHtmlContentExtractor
{
    private readonly HttpClient _httpClient;

    public HtmlContentExtractor(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> ExtractTextFromUrlAsync(
        string url,
        CancellationToken cancellationToken
    )
    {
        var html = await _httpClient.GetStringAsync(url, cancellationToken);

        return ExtractTextFromHtml(html);
    }

    private static string ExtractTextFromHtml(string html)
    {
        var withoutScripts = Regex.Replace(
            html,
            "<script[\\s\\S]*?</script>",
            " ",
            RegexOptions.IgnoreCase
        );

        var withoutStyles = Regex.Replace(
            withoutScripts,
            "<style[\\s\\S]*?</style>",
            " ",
            RegexOptions.IgnoreCase
        );

        var withoutTags = Regex.Replace(withoutStyles, "<[^>]+>", " ");

        var decoded = System.Net.WebUtility.HtmlDecode(withoutTags);

        var normalized = Regex.Replace(decoded, "\\s+", " ");

        return normalized.Trim();
    }
}
