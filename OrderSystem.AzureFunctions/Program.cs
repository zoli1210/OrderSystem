using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderSystem.AzureFunctions.DependencyInjection;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(
        (context, services) =>
        {
            services
                .AddApplicationInsightsTelemetryWorkerService()
                .ConfigureFunctionsApplicationInsights();

            services.AddAzureFunctionServices(context.Configuration);
        }
    )
    .Build();

host.Run();
