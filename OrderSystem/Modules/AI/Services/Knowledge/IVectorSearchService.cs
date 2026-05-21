using OrderSystem.Modules.AI.Models;

namespace OrderSystem.Modules.AI.Services.Knowledge;

public interface IVectorSearchService
{
    Task<IReadOnlyList<KnowledgeDocumentMatch>> SearchAsync(
        IReadOnlyList<float> embedding,
        int matchCount,
        CancellationToken cancellationToken
    );
}
