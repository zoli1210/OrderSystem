using OrderSystem.Modules.AI.Models;

namespace OrderSystem.Modules.AI.Services.Sources;

public interface IKnowledgeSourceRepository
{
    Task<KnowledgeSource?> GetByIdAsync(Guid sourceId, CancellationToken cancellationToken);

    Task MarkAsSucceededAsync(Guid sourceId, CancellationToken cancellationToken);

    Task MarkAsFailedAsync(Guid sourceId, string errorMessage, CancellationToken cancellationToken);
}
