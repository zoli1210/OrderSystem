using OrderSystem.Domain.Entities;

namespace OrderSystem.Infrastructure.Persistence.Repositories;

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken cancellationToken);

    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task UpdateAsync(Order order, CancellationToken cancellationToken);
}
