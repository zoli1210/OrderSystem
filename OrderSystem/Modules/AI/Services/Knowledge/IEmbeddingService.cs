namespace OrderSystem.Modules.AI.Services.Knowledge;

public interface IEmbeddingService
{
    Task<IReadOnlyList<float>> CreateEmbeddingAsync(
        string input,
        CancellationToken cancellationToken
    );
}
