using OrderSystem.Modules.AI.Models;

namespace OrderSystem.Application.AI.Sources;

public interface IKnowledgeSourceRepository
{
    Task<KnowledgeSource?> GetByIdAsync(Guid sourceId, CancellationToken cancellationToken);

    Task MarkAsSucceededAsync(Guid sourceId, CancellationToken cancellationToken);

    Task MarkAsFailedAsync(Guid sourceId, string errorMessage, CancellationToken cancellationToken);
}
