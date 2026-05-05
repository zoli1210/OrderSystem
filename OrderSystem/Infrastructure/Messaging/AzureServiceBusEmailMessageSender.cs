using System.Text.Json;
using Azure.Messaging.ServiceBus;
using OrderSystem.Infrastructure.Messaging.Messages;

namespace OrderSystem.Infrastructure.Messaging;

public class AzureServiceBusEmailMessageSender : IEmailMessageSender
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly IConfiguration _configuration;

    public AzureServiceBusEmailMessageSender(
        ServiceBusClient serviceBusClient,
        IConfiguration configuration
    )
    {
        _serviceBusClient = serviceBusClient;
        _configuration = configuration;
    }

    public async Task SendEmailNotificationAsync(
        EmailNotificationMessage message,
        CancellationToken cancellationToken
    )
    {
        var queueName = _configuration["AzureServiceBus:EmailNotificationQueueName"];

        if (string.IsNullOrWhiteSpace(queueName))
        {
            throw new InvalidOperationException(
                "AzureServiceBus:EmailNotificationQueueName is missing."
            );
        }

        await using var sender = _serviceBusClient.CreateSender(queueName);

        var body = JsonSerializer.Serialize(message);

        var serviceBusMessage = new ServiceBusMessage(body)
        {
            ContentType = "application/json",
            Subject = "EmailNotification",
        };

        await sender.SendMessageAsync(serviceBusMessage, cancellationToken);
    }
}
