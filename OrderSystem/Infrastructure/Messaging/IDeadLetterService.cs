using OrderSystem.Infrastructure.Messaging.DTOs;

namespace OrderSystem.Infrastructure.Messaging;

public interface IDeadLetterService
{
    Task<IReadOnlyList<DeadLetterMessageResponse>> GetDeadLettersAsync(
        CancellationToken cancellationToken
    );
}
