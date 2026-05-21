using OrderSystem.Modules.AI.DTOs;

namespace OrderSystem.Modules.AI.Services.Sources;

public interface IKnowledgeSourceSeedService
{
    Task<KnowledgeSourceSeedResponse> SeedAsync(CancellationToken cancellationToken);
}
