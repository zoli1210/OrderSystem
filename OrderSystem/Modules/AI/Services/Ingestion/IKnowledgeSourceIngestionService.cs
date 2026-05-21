using OrderSystem.Modules.AI.DTOs;

namespace OrderSystem.Modules.AI.Services.Ingestion;

public interface IKnowledgeSourceIngestionService
{
    Task<IngestKnowledgeSourceResponse> IngestAsync(
        Guid sourceId,
        CancellationToken cancellationToken
    );
}
