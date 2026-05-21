namespace OrderSystem.Modules.AI.Services.Shared;

public interface IHtmlContentExtractor
{
    Task<string> ExtractTextFromUrlAsync(string url, CancellationToken cancellationToken);
}
