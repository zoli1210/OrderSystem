using OrderSystem.Modules.Orders.DTOs;

namespace OrderSystem.Modules.Orders.Services;

public interface IOrderService
{
    Task<OrderResponse> CreateAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken);

    Task<OrderResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);
}