namespace OrderSystem.Application.AI.Providers.WebContent;

public interface IHtmlContentExtractor
{
    Task<string> ExtractTextFromUrlAsync(string url, CancellationToken cancellationToken);
}
