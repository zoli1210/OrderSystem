using OrderSystem.Modules.AI.Models;

namespace OrderSystem.Application.AI.Providers.OpenAi;

public interface IChatCompletionService
{
    Task<string> GenerateAnswerAsync(
        string question,
        IReadOnlyList<KnowledgeDocumentMatch> contextDocuments,
        CancellationToken cancellationToken
    );

    Task<string> GenerateOrderExplanationAsync(
        string question,
        string orderContext,
        IReadOnlyList<KnowledgeDocumentMatch> contextDocuments,
        CancellationToken cancellationToken
    );
}
