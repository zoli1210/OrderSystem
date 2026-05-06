using Azure.Messaging.ServiceBus;
using OrderSystem.Infrastructure.Messaging.DTOs;

namespace OrderSystem.Infrastructure.Messaging;

public class AzureServiceBusDeadLetterService : IDeadLetterService
{
    private static readonly HashSet<string> AllowedQueueNames = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        "order-created",
        "email-notification",
    };

    private readonly ServiceBusClient _serviceBusClient;

    public AzureServiceBusDeadLetterService(ServiceBusClient serviceBusClient)
    {
        _serviceBusClient = serviceBusClient;
    }

    public async Task<IReadOnlyList<DeadLetterMessageResponse>> GetDeadLettersAsync(
        string queueName,
        CancellationToken cancellationToken
    )
    {
        EnsureQueueNameIsAllowed(queueName);

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
        string queueName,
        long sequenceNumber,
        CancellationToken cancellationToken
    )
    {
        EnsureQueueNameIsAllowed(queueName);

        await using var deadLetterReceiver = _serviceBusClient.CreateReceiver(
            queueName,
            new ServiceBusReceiverOptions
            {
                SubQueue = SubQueue.DeadLetter,
                ReceiveMode = ServiceBusReceiveMode.PeekLock,
            }
        );

        var messages = await deadLetterReceiver.ReceiveMessagesAsync(
            maxMessages: 50,
            maxWaitTime: TimeSpan.FromSeconds(5),
            cancellationToken: cancellationToken
        );

        var lockedMessage = messages.FirstOrDefault(x => x.SequenceNumber == sequenceNumber);

        if (lockedMessage is null)
        {
            foreach (var message in messages)
            {
                await deadLetterReceiver.AbandonMessageAsync(
                    message,
                    cancellationToken: cancellationToken
                );
            }

            return false;
        }

        await using var sender = _serviceBusClient.CreateSender(queueName);

        var retryMessage = new ServiceBusMessage(lockedMessage.Body)
        {
            ContentType = lockedMessage.ContentType,
            Subject = lockedMessage.Subject,
            CorrelationId = lockedMessage.CorrelationId,
        };

        foreach (var property in lockedMessage.ApplicationProperties)
        {
            retryMessage.ApplicationProperties[property.Key] = property.Value;
        }

        await sender.SendMessageAsync(retryMessage, cancellationToken);

        await deadLetterReceiver.CompleteMessageAsync(lockedMessage, cancellationToken);

        foreach (var message in messages.Where(x => x.SequenceNumber != sequenceNumber))
        {
            await deadLetterReceiver.AbandonMessageAsync(
                message,
                cancellationToken: cancellationToken
            );
        }

        return true;
    }

    private static void EnsureQueueNameIsAllowed(string queueName)
    {
        if (!AllowedQueueNames.Contains(queueName))
        {
            throw new InvalidOperationException($"Queue '{queueName}' is not allowed.");
        }
    }
}
