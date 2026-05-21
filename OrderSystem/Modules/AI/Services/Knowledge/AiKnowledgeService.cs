using Microsoft.Extensions.Options;
using OrderSystem.Infrastructure.Options;
using OrderSystem.Modules.AI.DTOs;

namespace OrderSystem.Modules.AI.Services.Knowledge;

public class AiKnowledgeService : IAiKnowledgeService
{
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorSearchService _vectorSearchService;
    private readonly IChatCompletionService _chatCompletionService;
    private readonly OpenAiOptions _openAiOptions;

    public AiKnowledgeService(
        IEmbeddingService embeddingService,
        IVectorSearchService vectorSearchService,
        IChatCompletionService chatCompletionService,
        IOptions<OpenAiOptions> openAiOptions
    )
    {
        _embeddingService = embeddingService;
        _vectorSearchService = vectorSearchService;
        _chatCompletionService = chatCompletionService;
        _openAiOptions = openAiOptions.Value;
    }

    public async Task<AskKnowledgeResponse> AskAsync(
        AskKnowledgeRequest request,
        CancellationToken cancellationToken
    )
    {
        var embedding = await _embeddingService.CreateEmbeddingAsync(
            request.Question,
            cancellationToken
        );

        var matchCount = request.MatchCount ?? _openAiOptions.DefaultMatchCount;

        var documents = await _vectorSearchService.SearchAsync(
            embedding,
            matchCount,
            cancellationToken
        );

        var answer = await _chatCompletionService.GenerateAnswerAsync(
            request.Question,
            documents,
            cancellationToken
        );

        return new AskKnowledgeResponse
        {
            Answer = answer,
            Sources = documents
                .Select(document => new KnowledgeSourceResponse
                {
                    Title = document.Title,
                    Url = document.Url,
                    Similarity = document.Similarity,
                })
                .ToList(),
        };
    }
}
