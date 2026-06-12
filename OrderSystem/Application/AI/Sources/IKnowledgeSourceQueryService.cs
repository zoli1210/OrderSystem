using OrderSystem.Modules.AI.Models;

namespace OrderSystem.Application.AI.Sources;

public interface IKnowledgeSourceQueryService
{
    Task<IReadOnlyList<KnowledgeSource>> GetSourcesDueForIngestionAsync(
        CancellationToken cancellationToken
    );
}
