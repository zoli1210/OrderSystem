using Azure.Core;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
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
        var fullyQualifiedNamespace = configuration["AzureServiceBus:FullyQualifiedNamespace"];

        var managedIdentityClientId =
            configuration["AzureServiceBus:ManagedIdentityClientId"]
            ?? configuration["AZURE_CLIENT_ID"];

        TokenCredential CreateCredential()
        {
            var options = new DefaultAzureCredentialOptions();

            if (!string.IsNullOrWhiteSpace(managedIdentityClientId))
            {
                options.ManagedIdentityClientId = managedIdentityClientId;
            }

            return new DefaultAzureCredential(options);
        }

        services.AddSingleton(_ =>
        {
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                return new ServiceBusClient(
                    connectionString,
                    new ServiceBusClientOptions
                    {
                        TransportType = ServiceBusTransportType.AmqpWebSockets,
                    }
                );
            }

            if (string.IsNullOrWhiteSpace(fullyQualifiedNamespace))
            {
                throw new InvalidOperationException(
                    "Either AzureServiceBus:ConnectionString or AzureServiceBus:FullyQualifiedNamespace must be configured."
                );
            }

            return new ServiceBusClient(
                fullyQualifiedNamespace,
                CreateCredential(),
                new ServiceBusClientOptions
                {
                    TransportType = ServiceBusTransportType.AmqpWebSockets,
                }
            );
        });

        services.AddSingleton(_ =>
        {
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                return new ServiceBusAdministrationClient(connectionString);
            }

            if (string.IsNullOrWhiteSpace(fullyQualifiedNamespace))
            {
                throw new InvalidOperationException(
                    "Either AzureServiceBus:ConnectionString or AzureServiceBus:FullyQualifiedNamespace must be configured."
                );
            }

            return new ServiceBusAdministrationClient(fullyQualifiedNamespace, CreateCredential());
        });

        services.AddScoped<IOrderMessageSender, AzureServiceBusOrderMessageSender>();

        services.AddScoped<IEmailMessageSender, AzureServiceBusEmailMessageSender>();

        services.AddScoped<IDeadLetterService, AzureServiceBusDeadLetterService>();

        return services;
    }
}
