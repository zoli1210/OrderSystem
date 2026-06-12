using OrderSystem.Modules.AI.DTOs;

namespace OrderSystem.Application.AI.Knowledge;

public interface IAiKnowledgeService
{
    Task<AskKnowledgeResponse> AskAsync(
        AskKnowledgeRequest request,
        CancellationToken cancellationToken
    );
}
