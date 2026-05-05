using Azure.Messaging.ServiceBus;
using OrderSystem.Infrastructure.Messaging.DTOs;

namespace OrderSystem.Infrastructure.Messaging;

public class AzureServiceBusDeadLetterService : IDeadLetterService
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly IConfiguration _configuration;

    public AzureServiceBusDeadLetterService(
        ServiceBusClient serviceBusClient,
        IConfiguration configuration
    )
    {
        _serviceBusClient = serviceBusClient;
        _configuration = configuration;
    }

    public async Task<IReadOnlyList<DeadLetterMessageResponse>> GetDeadLettersAsync(
        CancellationToken cancellationToken
    )
    {
        var queueName = _configuration["AzureServiceBus:OrderCreatedQueueName"];

        if (string.IsNullOrWhiteSpace(queueName))
        {
            throw new InvalidOperationException(
                "AzureServiceBus:OrderCreatedQueueName is missing."
            );
        }

        await using var receiver = _serviceBusClient.CreateReceiver(
            queueName,
            new ServiceBusReceiverOptions { SubQueue = SubQueue.DeadLetter }
        );

        var messages = await receiver.PeekMessagesAsync(
            maxMessages: 50,
            cancellationToken: cancellationToken
        );

        return messages
            .Select(message => new DeadLetterMessageResponse
            {
                MessageId = message.MessageId,
                SequenceNumber = message.SequenceNumber,
                DeliveryCount = message.DeliveryCount,
                EnqueuedTimeUtc = message.EnqueuedTime,
                Body = message.Body.ToString(),
                DeadLetterReason = message.DeadLetterReason,
                DeadLetterErrorDescription = message.DeadLetterErrorDescription,
            })
            .ToList();
    }
}
