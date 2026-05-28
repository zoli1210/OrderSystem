using Microsoft.EntityFrameworkCore;
using OrderSystem.Domain.Entities;

namespace OrderSystem.Infrastructure.Persistence.Repositories;

public class OrderStatusHistoryRepository : IOrderStatusHistoryRepository
{
    private readonly AppDbContext _dbContext;

    public OrderStatusHistoryRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(OrderStatusHistory history, CancellationToken cancellationToken)
    {
        _dbContext.OrderStatusHistories.Add(history);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OrderStatusHistory>> GetByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken
    )
    {
        return await _dbContext
            .OrderStatusHistories.Where(history => history.OrderId == orderId)
            .OrderBy(history => history.ChangedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OrderStatusHistory>> GetByOrderIdsAsync(
        IReadOnlyList<Guid> orderIds,
        CancellationToken cancellationToken
    )
    {
        return await _dbContext
            .OrderStatusHistories.Where(history => orderIds.Contains(history.OrderId))
            .OrderBy(history => history.ChangedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
