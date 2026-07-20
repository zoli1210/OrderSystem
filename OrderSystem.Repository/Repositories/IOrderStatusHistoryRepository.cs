using OrderSystem.Domain.Entities;

namespace OrderSystem.Repository.Repositories;

public interface IOrderStatusHistoryRepository
{
    Task AddAsync(OrderStatusHistory history, CancellationToken cancellationToken);

    Task<IReadOnlyList<OrderStatusHistory>> GetByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken
    );

    Task<IReadOnlyList<OrderStatusHistory>> GetByOrderIdsAsync(
        IReadOnlyList<Guid> orderIds,
        CancellationToken cancellationToken
    );
}
