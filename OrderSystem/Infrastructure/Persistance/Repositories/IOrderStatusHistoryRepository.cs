using OrderSystem.Domain.Entities;

namespace OrderSystem.Infrastructure.Persistence.Repositories;

public interface IOrderStatusHistoryRepository
{
    Task AddAsync(OrderStatusHistory history, CancellationToken cancellationToken);

    Task<IReadOnlyList<OrderStatusHistory>> GetByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken
    );

    Task SaveChangesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<OrderStatusHistory>> GetByOrderIdsAsync(
        IReadOnlyCollection<Guid> orderIds,
        CancellationToken cancellationToken
    );
}
