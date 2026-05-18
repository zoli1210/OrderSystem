using OrderSystem.Domain.Enums;
using OrderSystem.Modules.Orders.DTOs;
using OrderSystem.Shared.Pagination;

namespace OrderSystem.Modules.Orders.Services;

public interface IOrderService
{
    Task<OrderResponse> CreateAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken
    );

    Task<OrderResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedResponse<OrderResponse>> GetAllAsync(
        GetOrdersQuery query,
        CancellationToken cancellationToken
    );

    Task<OrderResponse> CancelAsync(
        Guid id,
        CancelOrderRequest request,
        CancellationToken cancellationToken
    );
}
