using Microsoft.EntityFrameworkCore;
using OrderSystem.Domain.Entities;

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
