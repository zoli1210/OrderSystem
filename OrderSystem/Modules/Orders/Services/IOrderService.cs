using OrderSystem.Common.Pagination;
using OrderSystem.Domain.Enums;
using OrderSystem.Modules.Orders.DTOs;

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

    Task<IReadOnlyList<OrderStatusHistoryResponse>> GetStatusHistoryAsync(
        Guid orderId,
        CancellationToken cancellationToken
    );

    Task<OrderResponse> RetryPaymentAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<UserOrderHistoryResponse>> GetUserHistoryAsync(
        CancellationToken cancellationToken
    );

    Task<IReadOnlyList<EmailNotificationHistoryResponse>> GetEmailHistoryAsync(
        Guid orderId,
        CancellationToken cancellationToken
    );
}
