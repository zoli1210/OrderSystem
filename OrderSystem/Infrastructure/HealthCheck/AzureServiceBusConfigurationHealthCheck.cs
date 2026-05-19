using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace OrderSystem.Infrastructure.HealthChecks;

public class AzureServiceBusConfigurationHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;

    public AzureServiceBusConfigurationHealthCheck(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        var connectionString = _configuration["AzureServiceBus:ConnectionString"];
        var orderQueueName = _configuration["AzureServiceBus:OrderCreatedQueueName"];
        var emailQueueName = _configuration["AzureServiceBus:EmailNotificationQueueName"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return Task.FromResult(
                HealthCheckResult.Unhealthy("AzureServiceBus:ConnectionString is missing.")
            );
        }

        if (string.IsNullOrWhiteSpace(orderQueueName))
        {
            return Task.FromResult(
                HealthCheckResult.Unhealthy("AzureServiceBus:OrderCreatedQueueName is missing.")
            );
        }

        if (string.IsNullOrWhiteSpace(emailQueueName))
        {
            return Task.FromResult(
                HealthCheckResult.Unhealthy(
                    "AzureServiceBus:EmailNotificationQueueName is missing."
                )
            );
        }

        return Task.FromResult(
            HealthCheckResult.Healthy("Azure Service Bus configuration is valid.")
        );
    }
}
