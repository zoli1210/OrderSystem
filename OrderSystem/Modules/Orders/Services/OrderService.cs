using OrderSystem.Domain.Entities;
using OrderSystem.Domain.Enums;
using OrderSystem.Infrastructure.Messaging;
using OrderSystem.Infrastructure.Messaging.Messages;
using OrderSystem.Infrastructure.Persistence.Repositories;
using OrderSystem.Modules.Auth.Services;
using OrderSystem.Modules.Orders.DTOs;
using OrderSystem.Modules.Payments.Services;
using OrderSystem.Shared.Pagination;

namespace OrderSystem.Modules.Orders.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentService _paymentService;
    private readonly IOrderMessageSender _orderMessageSender;
    private readonly ICurrentUserService _currentUserService;
    private readonly IOrderStatusHistoryRepository _statusHistoryRepository;

    public OrderService(
        IOrderRepository orderRepository,
        IOrderMessageSender orderMessageSender,
        ICurrentUserService currentUserService,
        IOrderStatusHistoryRepository statusHistoryRepository
    )
    {
        _orderRepository = orderRepository;
        _orderMessageSender = orderMessageSender;
        _currentUserService = currentUserService;
        _statusHistoryRepository = statusHistoryRepository;
    }

    public async Task<OrderResponse> CreateAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken
    )
    {
        var currentUserId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var order = new Order(
            request.CustomerName,
            request.CustomerEmail,
            request.TotalAmount,
            request.Currency,
            request.Description,
            currentUserId
        );

        await _orderRepository.AddAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        await _orderMessageSender.SendOrderCreatedAsync(
            new OrderCreatedMessage
            {
                OrderId = order.Id,
                TotalAmount = order.TotalAmount,
                CustomerEmail = order.CustomerEmail,
            },
            cancellationToken
        );

        return MapToResponse(order);
    }

    public async Task<OrderResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);

        if (order is null)
        {
            throw new NotFoundException("Order not found");
        }

        EnsureUserCanAccessOrder(order);

        return MapToResponse(order);
    }

    private static OrderResponse MapToResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            CustomerName = order.CustomerName,
            CreatedByUserId = order.CreatedByUserId,
            CustomerEmail = order.CustomerEmail,
            TotalAmount = order.TotalAmount,
            Currency = order.Currency,
            Description = order.Description,
            Status = order.Status,
            CreatedAtUtc = order.CreatedAtUtc,
            EmailSentAtUtc = order.EmailSentAtUtc,
            UpdatedAtUtc = order.UpdatedAtUtc,
            UpdatedByUserId = order.UpdatedByUserId,
            CancelledAtUtc = order.CancelledAtUtc,
            CancellationReason = order.CancellationReason,
        };
    }

    public async Task<PagedResponse<OrderResponse>> GetAllAsync(
        GetOrdersQuery query,
        CancellationToken cancellationToken
    )
    {
        var createdByUserId = _currentUserService.IsAdmin ? null : _currentUserService.UserId;

        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var sortBy = NormalizeSortBy(query.SortBy);
        var sortOrder = NormalizeSortOrder(query.SortOrder);

        var (items, totalCount) = await _orderRepository.GetAllAsync(
            query.Status,
            createdByUserId,
            page,
            pageSize,
            sortBy,
            sortOrder,
            cancellationToken
        );

        return new PagedResponse<OrderResponse>
        {
            Items = items.Select(MapToResponse).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
        };
    }

    private static string NormalizeSortBy(string? sortBy)
    {
        var allowedSortFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "createdAtUtc",
            "totalAmount",
            "status",
            "customerName",
        };

        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return "createdAtUtc";
        }

        return allowedSortFields.Contains(sortBy) ? sortBy : "createdAtUtc";
    }

    private static string NormalizeSortOrder(string? sortOrder)
    {
        return string.Equals(sortOrder, "asc", StringComparison.OrdinalIgnoreCase) ? "asc" : "desc";
    }

    public async Task<OrderResponse> CancelAsync(
        Guid id,
        CancelOrderRequest request,
        CancellationToken cancellationToken
    )
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);

        if (order is null)
        {
            throw new NotFoundException("Order not found");
        }

        EnsureUserCanAccessOrder(order);

        var currentUserId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var previousStatus = order.Status;

        order.Cancel(request.Reason, currentUserId);

        await _statusHistoryRepository.AddAsync(
            new OrderStatusHistory(order.Id, previousStatus, order.Status, currentUserId),
            cancellationToken
        );

        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        return MapToResponse(order);
    }

    private void EnsureUserCanAccessOrder(Order order)
    {
        if (_currentUserService.IsAdmin)
        {
            return;
        }

        if (order.CreatedByUserId != _currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("You are not allowed to access this order.");
        }
    }

    public async Task<IReadOnlyList<OrderStatusHistoryResponse>> GetStatusHistoryAsync(
        Guid orderId,
        CancellationToken cancellationToken
    )
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);

        if (order is null)
        {
            throw new NotFoundException("Order not found");
        }

        EnsureUserCanAccessOrder(order);

        var history = await _statusHistoryRepository.GetByOrderIdAsync(orderId, cancellationToken);

        return history.Select(MapToStatusHistoryResponse).ToList();
    }

    private static OrderStatusHistoryResponse MapToStatusHistoryResponse(OrderStatusHistory history)
    {
        return new OrderStatusHistoryResponse
        {
            Id = history.Id,
            OrderId = history.OrderId,
            FromStatus = history.FromStatus,
            ToStatus = history.ToStatus,
            ChangedAtUtc = history.ChangedAtUtc,
            ChangedByUserId = history.ChangedByUserId,
        };
    }
}
