using Azure.Messaging.ServiceBus;
using OrderSystem.Infrastructure.Messaging;

namespace OrderSystem.Infrastructure.DependencyInjection;

public static class MessagingDependencyInjection
{
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionString = configuration["AzureServiceBus:ConnectionString"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("AzureServiceBus:ConnectionString is missing.");
        }

        services.AddSingleton(_ => new ServiceBusClient(
            connectionString,
            new ServiceBusClientOptions { TransportType = ServiceBusTransportType.AmqpWebSockets }
        ));

        services.AddScoped<IOrderMessageSender, AzureServiceBusOrderMessageSender>();
        services.AddScoped<IEmailMessageSender, AzureServiceBusEmailMessageSender>();
        services.AddScoped<IDeadLetterService, AzureServiceBusDeadLetterService>();

        return services;
    }
}
