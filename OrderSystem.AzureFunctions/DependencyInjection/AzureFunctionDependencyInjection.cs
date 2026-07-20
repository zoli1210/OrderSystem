using Azure.Core;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderSystem.AzureFunctions.Services;
using OrderSystem.Infrastructure.Messaging;
using OrderSystem.Modules.Email.Services;
using OrderSystem.Modules.Payments.Services;
using OrderSystem.Repository.DependencyInjection;
using OrderSystem.Repository.Repositories;

namespace OrderSystem.AzureFunctions.DependencyInjection;

public static class AzureFunctionsDependencyInjection
{
    public static IServiceCollection AddAzureFunctionServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var sqlConnectionString =
            configuration.GetConnectionString("SQLConnection")
            ?? configuration["SqlConnectionString"];

        if (string.IsNullOrWhiteSpace(sqlConnectionString))
        {
            throw new InvalidOperationException("SqlConnectionString is missing.");
        }

        var serviceBusConnectionString =
            configuration["AzureServiceBus:ConnectionString"]
            ?? configuration["AzureServiceBusConnection"];

        var serviceBusFullyQualifiedNamespace =
            configuration["AzureServiceBus:FullyQualifiedNamespace"]
            ?? configuration["AzureServiceBusConnection:fullyQualifiedNamespace"];

        var managedIdentityClientId =
            configuration["AzureServiceBus:ManagedIdentityClientId"]
            ?? configuration["AzureServiceBusConnection:clientId"]
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

        services.AddRepository(configuration);

        services.AddSingleton(_ =>
        {
            if (!string.IsNullOrWhiteSpace(serviceBusConnectionString))
            {
                return new ServiceBusClient(
                    serviceBusConnectionString,
                    new ServiceBusClientOptions
                    {
                        TransportType = ServiceBusTransportType.AmqpWebSockets,
                    }
                );
            }

            if (string.IsNullOrWhiteSpace(serviceBusFullyQualifiedNamespace))
            {
                throw new InvalidOperationException(
                    "Either AzureServiceBus:ConnectionString, AzureServiceBusConnection or AzureServiceBus:FullyQualifiedNamespace must be configured."
                );
            }

            return new ServiceBusClient(
                serviceBusFullyQualifiedNamespace,
                CreateCredential(),
                new ServiceBusClientOptions
                {
                    TransportType = ServiceBusTransportType.AmqpWebSockets,
                }
            );
        });

        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IPaymentProcessor, PaymentProcessor>();

        services.AddScoped<IEmailMessageSender, AzureServiceBusEmailMessageSender>();

        services.AddScoped<IEmailService, AzureCommunicationEmailService>();
        services.AddScoped<IEmailProcessor, EmailProcessor>();
        services.AddScoped<IOrderStatusEmailProcessor, OrderStatusEmailProcessorService>();
        services.AddScoped<IOrderMessageSender, AzureServiceBusOrderMessageSender>();

        return services;
    }
}
