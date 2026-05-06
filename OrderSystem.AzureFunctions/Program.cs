using Microsoft.Extensions.Hosting;
using OrderSystem.AzureFunctions.DependencyInjection;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(
        (context, services) =>
        {
            services.AddAzureFunctionServices(context.Configuration);
        }
    )
    .Build();

host.Run();
