using OrderSystem.Modules.AI.Models;

namespace OrderSystem.Modules.AI.Services;

public interface IKnowledgeSourceQueryService
{
    Task<IReadOnlyList<KnowledgeSource>> GetSourcesDueForIngestionAsync(
        CancellationToken cancellationToken
    );
}
