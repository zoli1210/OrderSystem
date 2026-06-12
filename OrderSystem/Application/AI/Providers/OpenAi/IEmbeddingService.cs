namespace OrderSystem.Application.AI.Providers.OpenAi;

public interface IEmbeddingService
{
    Task<IReadOnlyList<float>> CreateEmbeddingAsync(
        string input,
        CancellationToken cancellationToken
    );
}
