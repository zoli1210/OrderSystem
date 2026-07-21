using OrderSystem.Application.Orders.Contracts.Queries;
using OrderSystem.Application.Orders.Contracts.Requests;
using OrderSystem.Application.Orders.Contracts.Responses;
using OrderSystem.Common.Pagination;

namespace OrderSystem.Application.Orders.Services;

public interface IOrderService
{
    Task<OrderResponse> CreateAsync(
        CreateOrderRequest request,
        string currentUserId,
        CancellationToken cancellationToken
    );

    Task<OrderResponse?> GetByIdAsync(
        Guid id,
        string? currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken
    );

    Task<PagedResponse<OrderResponse>> GetAllAsync(
        GetOrdersQuery query,
        string? currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken
    );

    Task<OrderResponse> CancelAsync(
        Guid id,
        CancelOrderRequest request,
        string currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken
    );

    Task<IReadOnlyList<OrderStatusHistoryResponse>> GetStatusHistoryAsync(
        Guid orderId,
        string? currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken
    );

    Task<OrderResponse> RetryPaymentAsync(
        Guid id,
        string currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken
    );

    Task<IReadOnlyList<UserOrderHistoryResponse>> GetUserHistoryAsync(
        string currentUserId,
        CancellationToken cancellationToken
    );

    Task<IReadOnlyList<EmailNotificationHistoryResponse>> GetEmailHistoryAsync(
        Guid orderId,
        string? currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken
    );

    Task<OrderSummaryResponse> GetSummaryAsync(bool isAdmin, CancellationToken cancellationToken);
}
