using OrderSystem.Modules.AI.Services;
using OrderSystem.Modules.AI.Services.Ingestion;
using OrderSystem.Modules.AI.Services.Sources;

namespace OrderSystem.Modules.AI.HostedServices;

public class AiKnowledgeIngestionHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AiKnowledgeIngestionHostedService> _logger;

    public AiKnowledgeIngestionHostedService(
        IServiceProvider serviceProvider,
        ILogger<AiKnowledgeIngestionHostedService> logger
    )
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            using var scope = _serviceProvider.CreateScope();

            var sourceQueryService =
                scope.ServiceProvider.GetRequiredService<IKnowledgeSourceQueryService>();

            var ingestionService =
                scope.ServiceProvider.GetRequiredService<IKnowledgeSourceIngestionService>();

            var sources = await sourceQueryService.GetSourcesDueForIngestionAsync(stoppingToken);

            if (!sources.Any())
            {
                _logger.LogInformation("No AI knowledge sources are due for ingestion.");

                return;
            }

            _logger.LogInformation(
                "AI knowledge source ingestion started. SourceCount: {SourceCount}",
                sources.Count
            );

            foreach (var source in sources)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    var result = await ingestionService.IngestAsync(source.Id, stoppingToken);

                    _logger.LogInformation(
                        "AI knowledge source ingested. SourceId: {SourceId}, SourceName: {SourceName}, ChunkCount: {ChunkCount}",
                        result.SourceId,
                        result.SourceName,
                        result.ChunkCount
                    );
                }
                catch (Exception exception)
                {
                    _logger.LogError(
                        exception,
                        "AI knowledge source ingestion failed. SourceId: {SourceId}, SourceName: {SourceName}",
                        source.Id,
                        source.Name
                    );
                }
            }

            _logger.LogInformation("AI knowledge source ingestion finished.");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("AI knowledge source ingestion was cancelled.");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "AI knowledge ingestion hosted service failed.");
        }
    }
}
