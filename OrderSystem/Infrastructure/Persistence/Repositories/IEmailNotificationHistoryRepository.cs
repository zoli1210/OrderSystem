using OrderSystem.Domain.Entities;

namespace OrderSystem.Infrastructure.Persistence.Repositories;

public interface IEmailNotificationHistoryRepository
{
    Task AddAsync(EmailNotificationHistory history, CancellationToken cancellationToken);

    Task<bool> ExistsSentAsync(Guid orderId, string emailType, CancellationToken cancellationToken);

    Task<IReadOnlyList<EmailNotificationHistory>> GetByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken
    );

    Task<IReadOnlyList<EmailNotificationHistory>> GetByOrderIdsAsync(
        IReadOnlyCollection<Guid> orderIds,
        CancellationToken cancellationToken
    );

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
