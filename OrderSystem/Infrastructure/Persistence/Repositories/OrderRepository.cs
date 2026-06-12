using Microsoft.EntityFrameworkCore;
using OrderSystem.Domain.Entities;
using OrderSystem.Domain.Enums;

namespace OrderSystem.Infrastructure.Persistence.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _dbContext;

    public OrderRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken)
    {
        await _dbContext.Orders.AddAsync(order, cancellationToken);
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Orders.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<Order> Items, int TotalCount)> GetAllAsync(
        OrderStatus? status,
        string? createdByUserId,
        int page,
        int pageSize,
        string sortBy,
        string sortOrder,
        CancellationToken cancellationToken
    )
    {
        var query = _dbContext.Orders.AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(order => order.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(createdByUserId))
        {
            query = query.Where(order => order.CreatedByUserId == createdByUserId);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var isDescending = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);

        query = sortBy.ToLowerInvariant() switch
        {
            "totalamount" => isDescending
                ? query.OrderByDescending(order => order.TotalAmount)
                : query.OrderBy(order => order.TotalAmount),

            "status" => isDescending
                ? query.OrderByDescending(order => order.Status)
                : query.OrderBy(order => order.Status),

            "customername" => isDescending
                ? query.OrderByDescending(order => order.CustomerName)
                : query.OrderBy(order => order.CustomerName),

            "createdatutc" => isDescending
                ? query.OrderByDescending(order => order.CreatedAtUtc)
                : query.OrderBy(order => order.CreatedAtUtc),

            _ => query.OrderByDescending(order => order.CreatedAtUtc),
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<Order>> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken
    )
    {
        return await _dbContext
            .Orders.Where(order => order.CreatedByUserId == userId)
            .OrderByDescending(order => order.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task UpdateAsync(Order order, CancellationToken cancellationToken)
    {
        _dbContext.Orders.Update(order);
        return Task.CompletedTask;
    }
}
