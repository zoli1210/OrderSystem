using OrderSystem.Modules.AI.DTOs;

namespace OrderSystem.Application.AI.Sources;

public interface IKnowledgeSourceSeedService
{
    Task<KnowledgeSourceSeedResponse> SeedAsync(CancellationToken cancellationToken);
}
