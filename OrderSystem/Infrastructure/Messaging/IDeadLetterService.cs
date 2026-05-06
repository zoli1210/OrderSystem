using OrderSystem.Infrastructure.Messaging.DTOs;

namespace OrderSystem.Infrastructure.Messaging;

public interface IDeadLetterService
{
    Task<IReadOnlyList<DeadLetterMessageResponse>> GetDeadLettersAsync(
        string queueName,
        CancellationToken cancellationToken
    );

    Task<bool> RetryDeadLetterAsync(
        string queueName,
        long sequenceNumber,
        CancellationToken cancellationToken
    );
}
