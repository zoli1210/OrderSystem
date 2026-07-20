using OrderSystem.Domain.Entities;

namespace OrderSystem.Repository.Repositories;

public interface IEmailNotificationHistoryRepository
{
    Task AddAsync(EmailNotificationHistory history, CancellationToken cancellationToken);

    Task<bool> ExistsSentEmailForOrderAsync(
        Guid orderId,
        string emailType,
        CancellationToken cancellationToken
    );

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
