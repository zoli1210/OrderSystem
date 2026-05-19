using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace OrderSystem.Infrastructure.HealthChecks;

public class ApplicationInsightsConfigurationHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;

    public ApplicationInsightsConfigurationHealthCheck(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        var connectionString = _configuration["ApplicationInsights:ConnectionString"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return Task.FromResult(
                HealthCheckResult.Degraded("Application Insights connection string is missing.")
            );
        }

        return Task.FromResult(
            HealthCheckResult.Healthy("Application Insights configuration is valid.")
        );
    }
}
