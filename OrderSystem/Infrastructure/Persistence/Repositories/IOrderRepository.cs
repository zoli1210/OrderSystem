using OrderSystem.Domain.Entities;
using OrderSystem.Domain.Entities.OrderSummary;
using OrderSystem.Domain.Enums;

namespace OrderSystem.Infrastructure.Persistence.Repositories;

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken cancellationToken);

    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<(IReadOnlyList<Order> Items, int TotalCount)> GetAllAsync(
        OrderStatus? status,
        string? createdByUserId,
        int page,
        int pageSize,
        string sortBy,
        string sortOrder,
        CancellationToken cancellationToken
    );

    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task UpdateAsync(Order order, CancellationToken cancellationToken);

    Task<IReadOnlyList<Order>> GetByUserIdAsync(string userId, CancellationToken cancellationToken);

    Task<OrderSummary> GetSummaryAsync(
        DateTime todayStartUtc,
        DateTime last7DaysStartUtc,
        CancellationToken cancellationToken
    );
}
