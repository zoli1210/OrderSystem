using OrderSystem.Modules.AI.DTOs;

namespace OrderSystem.Modules.AI.Services.Knowledge;

public interface IAiKnowledgeService
{
    Task<AskKnowledgeResponse> AskAsync(
        AskKnowledgeRequest request,
        CancellationToken cancellationToken
    );
}
