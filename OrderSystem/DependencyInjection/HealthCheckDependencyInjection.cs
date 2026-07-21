using Microsoft.Extensions.Diagnostics.HealthChecks;
using OrderSystem.Api.HealthCheck;

namespace OrderSystem.DependencyInjection;

public static class HealthCheckDependencyInjection
{
    public static IServiceCollection AddProjectHealthChecks(this IServiceCollection services)
    {
        services
            .AddHealthChecks()
            .AddCheck<InfrastructureHealthCheck>(
                name: "infrastructure",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready"],
                timeout: TimeSpan.FromSeconds(15)
            );

        services.AddSingleton<IHealthCheckPublisher, HealthCheckReportPublisher>();

        services.Configure<HealthCheckPublisherOptions>(options =>
        {
            // After the first check triggered
            options.Delay = TimeSpan.FromSeconds(10);

            // It runs every 60 minutes
            options.Period = TimeSpan.FromMinutes(60);

            options.Predicate = registration => registration.Tags.Contains("ready");
        });

        return services;
    }
}
