using OrderSystem.Modules.AI.Models;

namespace OrderSystem.Modules.AI.Services.Knowledge;

public interface IChatCompletionService
{
    Task<string> GenerateAnswerAsync(
        string question,
        IReadOnlyList<KnowledgeDocumentMatch> contextDocuments,
        CancellationToken cancellationToken
    );
}
