using OrderSystem.Modules.AI.Models;

namespace OrderSystem.Modules.AI.Services.Documents;

public interface IKnowledgeDocumentRepository
{
    Task DeleteBySourceIdAsync(Guid sourceId, CancellationToken cancellationToken);

    Task InsertAsync(
        KnowledgeSource source,
        string content,
        IReadOnlyList<float> embedding,
        CancellationToken cancellationToken
    );
}
