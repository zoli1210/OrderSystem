using OrderSystem.Application.AI.Documents;
using OrderSystem.Application.AI.Processing;
using OrderSystem.Application.AI.Providers.OpenAi;
using OrderSystem.Application.AI.Providers.WebContent;
using OrderSystem.Modules.AI.DTOs;

namespace OrderSystem.Application.AI.Sources;

public class KnowledgeSourceIngestionService : IKnowledgeSourceIngestionService
{
    private readonly IKnowledgeSourceRepository _sourceRepository;
    private readonly IKnowledgeDocumentRepository _documentRepository;
    private readonly IHtmlContentExtractor _htmlContentExtractor;
    private readonly ITextChunkingService _textChunkingService;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILogger<KnowledgeSourceIngestionService> _logger;

    public KnowledgeSourceIngestionService(
        IKnowledgeSourceRepository sourceRepository,
        IKnowledgeDocumentRepository documentRepository,
        IHtmlContentExtractor htmlContentExtractor,
        ITextChunkingService textChunkingService,
        IEmbeddingService embeddingService,
        ILogger<KnowledgeSourceIngestionService> logger
    )
    {
        _sourceRepository = sourceRepository;
        _documentRepository = documentRepository;
        _htmlContentExtractor = htmlContentExtractor;
        _textChunkingService = textChunkingService;
        _embeddingService = embeddingService;
        _logger = logger;
    }

    public async Task<IngestKnowledgeSourceResponse> IngestAsync(
        Guid sourceId,
        CancellationToken cancellationToken
    )
    {
        var source = await _sourceRepository.GetByIdAsync(sourceId, cancellationToken);

        if (source is null)
        {
            throw new InvalidOperationException("Knowledge source not found.");
        }

        if (!source.IsActive)
        {
            throw new InvalidOperationException("Knowledge source is not active.");
        }

        if (string.IsNullOrWhiteSpace(source.Url))
        {
            throw new InvalidOperationException("Knowledge source URL is missing.");
        }

        try
        {
            var text = await _htmlContentExtractor.ExtractTextFromUrlAsync(
                source.Url,
                cancellationToken
            );

            if (string.IsNullOrWhiteSpace(text))
            {
                throw new InvalidOperationException(
                    "No readable content was extracted from the source URL."
                );
            }

            var chunks = _textChunkingService.SplitIntoChunks(text);

            if (chunks.Count == 0)
            {
                throw new InvalidOperationException(
                    "No text chunks were created from the source URL."
                );
            }

            await _documentRepository.DeleteBySourceIdAsync(source.Id, cancellationToken);

            for (var index = 0; index < chunks.Count; index++)
            {
                var chunk = chunks[index];

                _logger.LogInformation(
                    "Creating embedding for knowledge chunk. SourceId: {SourceId}, SourceName: {SourceName}, Chunk: {CurrentChunk}/{TotalChunks}",
                    source.Id,
                    source.Name,
                    index + 1,
                    chunks.Count
                );

                var embedding = await _embeddingService.CreateEmbeddingAsync(
                    chunk,
                    cancellationToken
                );

                await _documentRepository.InsertAsync(source, chunk, embedding, cancellationToken);
            }

            await _sourceRepository.MarkAsSucceededAsync(source.Id, cancellationToken);

            _logger.LogInformation(
                "Knowledge source ingested. SourceId: {SourceId}, SourceName: {SourceName}, ChunkCount: {ChunkCount}",
                source.Id,
                source.Name,
                chunks.Count
            );

            return new IngestKnowledgeSourceResponse
            {
                SourceId = source.Id,
                SourceName = source.Name,
                ChunkCount = chunks.Count,
                Status = "Succeeded",
            };
        }
        catch (Exception exception)
        {
            await _sourceRepository.MarkAsFailedAsync(
                source.Id,
                exception.Message,
                cancellationToken
            );

            _logger.LogError(
                exception,
                "Knowledge source ingestion failed. SourceId: {SourceId}, SourceName: {SourceName}",
                source.Id,
                source.Name
            );

            throw;
        }
    }
}
