using OrderSystem.Modules.AI.Models;

namespace OrderSystem.Application.AI.Documents;

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
