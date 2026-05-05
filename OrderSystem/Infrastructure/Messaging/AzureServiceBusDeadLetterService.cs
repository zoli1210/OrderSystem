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
        var queueName = GetQueueName();

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

    public async Task<bool> RetryDeadLetterAsync(
        long sequenceNumber,
        CancellationToken cancellationToken
    )
    {
        var queueName = GetQueueName();

        await using var deadLetterReceiver = _serviceBusClient.CreateReceiver(
            queueName,
            new ServiceBusReceiverOptions
            {
                SubQueue = SubQueue.DeadLetter,
                ReceiveMode = ServiceBusReceiveMode.PeekLock,
            }
        );

        var deadLetterMessage = await deadLetterReceiver.PeekMessageAsync(
            fromSequenceNumber: sequenceNumber,
            cancellationToken: cancellationToken
        );

        if (deadLetterMessage is null || deadLetterMessage.SequenceNumber != sequenceNumber)
        {
            return false;
        }

        var lockedMessage = await ReceiveDeadLetterBySequenceNumberAsync(
            deadLetterReceiver,
            sequenceNumber,
            cancellationToken
        );

        if (lockedMessage is null)
        {
            return false;
        }

        await using var sender = _serviceBusClient.CreateSender(queueName);

        var retryMessage = new ServiceBusMessage(lockedMessage.Body)
        {
            ContentType = lockedMessage.ContentType,
            Subject = lockedMessage.Subject,
        };

        foreach (var property in lockedMessage.ApplicationProperties)
        {
            retryMessage.ApplicationProperties[property.Key] = property.Value;
        }

        await sender.SendMessageAsync(retryMessage, cancellationToken);

        await deadLetterReceiver.CompleteMessageAsync(lockedMessage, cancellationToken);

        return true;
    }

    private async Task<ServiceBusReceivedMessage?> ReceiveDeadLetterBySequenceNumberAsync(
        ServiceBusReceiver receiver,
        long sequenceNumber,
        CancellationToken cancellationToken
    )
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var message = await receiver.ReceiveMessageAsync(
                maxWaitTime: TimeSpan.FromSeconds(3),
                cancellationToken: cancellationToken
            );

            if (message is null)
            {
                return null;
            }

            if (message.SequenceNumber == sequenceNumber)
            {
                return message;
            }

            await receiver.AbandonMessageAsync(message, cancellationToken: cancellationToken);
        }

        return null;
    }

    private string GetQueueName()
    {
        var queueName = _configuration["AzureServiceBus:OrderCreatedQueueName"];

        if (string.IsNullOrWhiteSpace(queueName))
        {
            throw new InvalidOperationException(
                "AzureServiceBus:OrderCreatedQueueName is missing."
            );
        }

        return queueName;
    }
}
