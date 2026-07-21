using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;

namespace OrderSystem.Infrastructure.Messaging;

public static class ServiceBusClientFactory
{
    public static ServiceBusClient Create(IConfiguration configuration)
    {
        var connectionString = configuration["AzureServiceBus:ConnectionString"];
        var fullyQualifiedNamespace = configuration["AzureServiceBus:FullyQualifiedNamespace"];
        var managedIdentityClientId =
            configuration["AzureServiceBus:ManagedIdentityClientId"]
            ?? configuration["AZURE_CLIENT_ID"];

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            return new ServiceBusClient(connectionString);
        }

        if (string.IsNullOrWhiteSpace(fullyQualifiedNamespace))
        {
            throw new InvalidOperationException(
                "Either AzureServiceBus:ConnectionString or AzureServiceBus:FullyQualifiedNamespace must be configured."
            );
        }

        var credentialOptions = new DefaultAzureCredentialOptions();

        if (!string.IsNullOrWhiteSpace(managedIdentityClientId))
        {
            credentialOptions.ManagedIdentityClientId = managedIdentityClientId;
        }

        return new ServiceBusClient(
            fullyQualifiedNamespace,
            new DefaultAzureCredential(credentialOptions)
        );
    }
}
