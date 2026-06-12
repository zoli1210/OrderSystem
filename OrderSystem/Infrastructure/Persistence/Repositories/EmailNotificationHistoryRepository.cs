using Microsoft.EntityFrameworkCore;
using OrderSystem.Domain.Entities;
using OrderSystem.Domain.Enums;

namespace OrderSystem.Infrastructure.Persistence.Repositories;

public class EmailNotificationHistoryRepository : IEmailNotificationHistoryRepository
{
    private readonly AppDbContext _dbContext;

    public EmailNotificationHistoryRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        EmailNotificationHistory history,
        CancellationToken cancellationToken
    )
    {
        await _dbContext.EmailNotificationHistories.AddAsync(history, cancellationToken);
    }

    public async Task<bool> ExistsSentAsync(
        Guid orderId,
        string emailType,
        CancellationToken cancellationToken
    )
    {
        return await _dbContext.EmailNotificationHistories.AnyAsync(
            history =>
                history.OrderId == orderId
                && history.EmailType == emailType
                && history.Status == EmailNotificationStatus.Sent,
            cancellationToken
        );
    }

    public async Task<IReadOnlyList<EmailNotificationHistory>> GetByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken
    )
    {
        return await _dbContext
            .EmailNotificationHistories.Where(history => history.OrderId == orderId)
            .OrderByDescending(history => history.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EmailNotificationHistory>> GetByOrderIdsAsync(
        IReadOnlyCollection<Guid> orderIds,
        CancellationToken cancellationToken
    )
    {
        return await _dbContext
            .EmailNotificationHistories.Where(history => orderIds.Contains(history.OrderId))
            .OrderByDescending(history => history.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
