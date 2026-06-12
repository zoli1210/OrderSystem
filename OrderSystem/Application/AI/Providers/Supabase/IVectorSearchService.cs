using OrderSystem.Modules.AI.Models;

namespace OrderSystem.Application.AI.Providers.Supabase;

public interface IVectorSearchService
{
    Task<IReadOnlyList<KnowledgeDocumentMatch>> SearchAsync(
        IReadOnlyList<float> embedding,
        int matchCount,
        CancellationToken cancellationToken
    );
}
