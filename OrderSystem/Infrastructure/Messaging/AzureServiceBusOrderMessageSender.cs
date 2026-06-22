using System.Text.Json;
using Azure.Messaging.ServiceBus;
using OrderSystem.Infrastructure.Messaging.Messages;

namespace OrderSystem.Infrastructure.Messaging;

public class AzureServiceBusOrderMessageSender : IOrderMessageSender
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly IConfiguration _configuration;

    public AzureServiceBusOrderMessageSender(ServiceBusClient client, IConfiguration configuration)
    {
        _serviceBusClient = client;
        _configuration = configuration;
    }

    public async Task SendOrderCreatedAsync(
        OrderCreatedMessage message,
        CancellationToken cancellationToken
    )
    {
        var queueName = _configuration["AzureServiceBus:OrderCreatedQueueName"];

        var sender = _serviceBusClient.CreateSender(queueName);

        var body = JsonSerializer.Serialize(message);

        var serviceBusMessage = new ServiceBusMessage(body)
        {
            ContentType = "application/json",
            Subject = "OrderCreated",
        };

        await sender.SendMessageAsync(serviceBusMessage, cancellationToken);
    }

    public async Task SendOrderStatusChangedAsync(
        OrderStatusChangedMessage message,
        CancellationToken cancellationToken
    )
    {
        var queueName = _configuration["AzureServiceBus:OrderStatusChangedQueueName"];

        if (string.IsNullOrWhiteSpace(queueName))
        {
            throw new InvalidOperationException(
                "AzureServiceBus:OrderStatusChangedQueueName is missing."
            );
        }

        await using var sender = _serviceBusClient.CreateSender(queueName);

        var body = JsonSerializer.Serialize(message);

        var serviceBusMessage = new ServiceBusMessage(body) { ContentType = "application/json" };

        await sender.SendMessageAsync(serviceBusMessage, cancellationToken);
    }
}
