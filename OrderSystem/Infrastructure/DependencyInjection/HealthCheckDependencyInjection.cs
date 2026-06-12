using OrderSystem.Api.HealthCheck;
using OrderSystem.Infrastructure.Persistence;

namespace OrderSystem.Infrastructure.DependencyInjection;

public static class HealthCheckDependencyInjection
{
    public static IServiceCollection AddProjectHealthChecks(this IServiceCollection services)
    {
        services
            .AddHealthChecks()
            .AddDbContextCheck<AppDbContext>(name: "sql-database")
            .AddCheck<AzureServiceBusConfigurationHealthCheck>(
                name: "azure-service-bus-configuration"
            )
            .AddCheck<ApplicationInsightsConfigurationHealthCheck>(
                name: "application-insights-configuration"
            );

        return services;
    }
}
