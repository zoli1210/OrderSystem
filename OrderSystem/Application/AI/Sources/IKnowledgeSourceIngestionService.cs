using OrderSystem.Modules.AI.DTOs;

namespace OrderSystem.Application.AI.Sources;

public interface IKnowledgeSourceIngestionService
{
    Task<IngestKnowledgeSourceResponse> IngestAsync(
        Guid sourceId,
        CancellationToken cancellationToken
    );
}
