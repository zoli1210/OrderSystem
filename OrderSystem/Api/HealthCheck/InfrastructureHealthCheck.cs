using Azure.Messaging.ServiceBus.Administration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OrderSystem.Repository.Persistence;

namespace OrderSystem.Api.HealthCheck;

public sealed class InfrastructureHealthCheck : IHealthCheck
{
    private readonly AppDbContext _dbContext;
    private readonly ServiceBusAdministrationClient _serviceBusClient;
    private readonly IConfiguration _configuration;

    public InfrastructureHealthCheck(
        AppDbContext dbContext,
        ServiceBusAdministrationClient serviceBusClient,
        IConfiguration configuration
    )
    {
        _dbContext = dbContext;
        _serviceBusClient = serviceBusClient;
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        var results = new Dictionary<string, object>();
        var failures = new List<string>();

        await CheckDatabaseAsync(results, failures, cancellationToken);
        await CheckServiceBusAsync(results, failures, cancellationToken);

        if (failures.Count > 0)
        {
            return HealthCheckResult.Unhealthy(string.Join(" ", failures), data: results);
        }

        return HealthCheckResult.Healthy(
            "SQL Server and Azure Service Bus are reachable.",
            results
        );
    }

    private async Task CheckDatabaseAsync(
        IDictionary<string, object> results,
        ICollection<string> failures,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);

            results["sqlDatabase"] = canConnect ? "Healthy" : "Unhealthy";

            if (!canConnect)
            {
                failures.Add("SQL Server is unreachable.");
            }
        }
        catch (Exception exception)
        {
            results["sqlDatabase"] = $"Unhealthy: {exception.Message}";

            failures.Add("SQL Server connection failed.");
        }
    }

    private async Task CheckServiceBusAsync(
        IDictionary<string, object> results,
        ICollection<string> failures,
        CancellationToken cancellationToken
    )
    {
        string[] queueNames;

        try
        {
            queueNames =
            [
                GetRequiredQueueName("AzureServiceBus:OrderCreatedQueueName"),
                GetRequiredQueueName("AzureServiceBus:OrderStatusChangedQueueName"),
                GetRequiredQueueName("AzureServiceBus:EmailNotificationQueueName"),
            ];
        }
        catch (Exception exception)
        {
            results["azureServiceBus"] = $"Unhealthy: {exception.Message}";

            failures.Add("Azure Service Bus configuration is incomplete.");

            return;
        }

        try
        {
            foreach (var queueName in queueNames)
            {
                var exists = await _serviceBusClient.QueueExistsAsync(queueName, cancellationToken);

                results[$"serviceBus:{queueName}"] = exists.Value ? "Healthy" : "Unhealthy";

                if (!exists.Value)
                {
                    failures.Add($"Azure Service Bus queue '{queueName}' does not exist.");
                }
            }
        }
        catch (Exception exception)
        {
            results["azureServiceBus"] = $"Unhealthy: {exception.Message}";

            failures.Add("Azure Service Bus connection failed.");
        }
    }

    private string GetRequiredQueueName(string configurationKey)
    {
        var queueName = _configuration[configurationKey];

        if (string.IsNullOrWhiteSpace(queueName))
        {
            throw new InvalidOperationException($"{configurationKey} is missing.");
        }

        return queueName;
    }
}

public sealed class HealthCheckReportPublisher : IHealthCheckPublisher
{
    private readonly ILogger<HealthCheckReportPublisher> _logger;

    public HealthCheckReportPublisher(ILogger<HealthCheckReportPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
    {
        if (report.Status == HealthStatus.Healthy)
        {
            _logger.LogInformation(
                "Periodic infrastructure health check completed. "
                    + "Status: {Status}, DurationMs: {DurationMs}",
                report.Status,
                report.TotalDuration.TotalMilliseconds
            );
        }
        else
        {
            _logger.LogError(
                "Periodic infrastructure health check failed. "
                    + "Status: {Status}, DurationMs: {DurationMs}, Checks: {@Checks}",
                report.Status,
                report.TotalDuration.TotalMilliseconds,
                report.Entries.ToDictionary(
                    entry => entry.Key,
                    entry => new
                    {
                        Status = entry.Value.Status.ToString(),
                        entry.Value.Description,
                        Error = entry.Value.Exception?.Message,
                        entry.Value.Data,
                    }
                )
            );
        }

        return Task.CompletedTask;
    }
}
