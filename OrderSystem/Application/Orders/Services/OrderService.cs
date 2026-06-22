using Microsoft.AspNetCore.Http.HttpResults;
using OrderSystem.Application.Orders.Contracts.Queries;
using OrderSystem.Application.Orders.Contracts.Requests;
using OrderSystem.Application.Orders.Contracts.Responses;
using OrderSystem.Application.Orders.Mapping;
using OrderSystem.Common.Pagination;
using OrderSystem.Domain.Entities;
using OrderSystem.Domain.Enums;
using OrderSystem.Infrastructure.Messaging;
using OrderSystem.Infrastructure.Messaging.Messages;
using OrderSystem.Infrastructure.Persistence.Repositories;
using OrderSystem.Modules.Auth.Services;
using OrderSystem.Modules.Orders.Services;

namespace OrderSystem.Application.Orders.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderMessageSender _orderMessageSender;
    private readonly ICurrentUserService _currentUserService;
    private readonly IOrderStatusHistoryRepository _statusHistoryRepository;
    private readonly IEmailNotificationHistoryRepository _emailHistoryRepository;

    public OrderService(
        IOrderRepository orderRepository,
        IOrderMessageSender orderMessageSender,
        ICurrentUserService currentUserService,
        IOrderStatusHistoryRepository statusHistoryRepository,
        IEmailNotificationHistoryRepository emailHistoryRepository
    )
    {
        _orderRepository = orderRepository;
        _orderMessageSender = orderMessageSender;
        _currentUserService = currentUserService;
        _statusHistoryRepository = statusHistoryRepository;
        _emailHistoryRepository = emailHistoryRepository;
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

        return OrderMapper.MapToResponse(order);
    }

    public async Task<OrderResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);

        if (order is null)
        {
            throw new Exception("Order not found");
        }

        EnsureUserCanAccessOrder(order);

        return OrderMapper.MapToResponse(order);
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
            Items = items.Select(OrderMapper.MapToResponse).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
        };
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
            throw new Exception("Order not found");
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
            new OrderStatusHistory(
                order.Id,
                previousStatus,
                order.Status,
                currentUserId,
                request.Reason
            ),
            cancellationToken
        );

        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        return OrderMapper.MapToResponse(order);
    }

    public async Task<IReadOnlyList<OrderStatusHistoryResponse>> GetStatusHistoryAsync(
        Guid orderId,
        CancellationToken cancellationToken
    )
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);

        if (order is null)
        {
            throw new Exception("Order not found");
        }

        EnsureUserCanAccessOrder(order);

        var history = await _statusHistoryRepository.GetByOrderIdAsync(orderId, cancellationToken);

        return history.Select(OrderMapper.MapToStatusHistoryResponse).ToList();
    }

    public async Task<OrderResponse> RetryPaymentAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);

        if (order is null)
        {
            throw new Exception("Order not found");
        }

        EnsureUserCanAccessOrder(order);

        var currentUserId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var previousStatus = order.Status;

        order.RetryPayment(currentUserId);

        await _statusHistoryRepository.AddAsync(
            new OrderStatusHistory(
                order.Id,
                previousStatus,
                order.Status,
                currentUserId,
                "Payment retry started."
            ),
            cancellationToken
        );

        await _orderRepository.UpdateAsync(order, cancellationToken);
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

        return OrderMapper.MapToResponse(order);
    }

    public async Task<IReadOnlyList<UserOrderHistoryResponse>> GetUserHistoryAsync(
        CancellationToken cancellationToken
    )
    {
        var currentUserId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var orders = await _orderRepository.GetByUserIdAsync(currentUserId, cancellationToken);

        if (!orders.Any())
        {
            return [];
        }

        var orderIds = orders.Select(order => order.Id).ToList();

        var histories = await _statusHistoryRepository.GetByOrderIdsAsync(
            orderIds,
            cancellationToken
        );

        var historiesByOrderId = histories
            .GroupBy(history => history.OrderId)
            .ToDictionary(
                group => group.Key,
                group =>
                    group
                        .Select(history => new UserOrderStatusHistoryItemResponse
                        {
                            FromStatus = history.FromStatus,
                            ToStatus = history.ToStatus,
                            ChangedAtUtc = history.ChangedAtUtc,
                            ChangedByUserId = history.ChangedByUserId,
                            Note = history.Note,
                        })
                        .ToList()
            );

        return orders
            .Select(order => new UserOrderHistoryResponse
            {
                OrderId = order.Id,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                TotalAmount = order.TotalAmount,
                Currency = order.Currency,
                Description = order.Description,
                CurrentStatus = order.Status,
                CreatedAtUtc = order.CreatedAtUtc,
                UpdatedAtUtc = order.UpdatedAtUtc,
                TrackingNumber = order.TrackingNumber,
                PreparationStartedAtUtc = order.PreparationStartedAtUtc,
                ReadyForShipmentAtUtc = order.ReadyForShipmentAtUtc,
                ShippedAtUtc = order.ShippedAtUtc,
                DeliveredAtUtc = order.DeliveredAtUtc,
                ReturnedAtUtc = order.ReturnedAtUtc,
                StatusHistory = historiesByOrderId.TryGetValue(order.Id, out var orderHistory)
                    ? orderHistory
                    : [],
            })
            .ToList();
    }

    public async Task<IReadOnlyList<EmailNotificationHistoryResponse>> GetEmailHistoryAsync(
        Guid orderId,
        CancellationToken cancellationToken
    )
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);

        if (order is null)
        {
            throw new Exception("Order not found");
        }

        EnsureUserCanAccessOrder(order);

        var histories = await _emailHistoryRepository.GetByOrderIdAsync(orderId, cancellationToken);

        return histories.Select(OrderMapper.MapToEmailHistoryResponse).ToList();
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

    public async Task<OrderSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAdmin)
        {
            throw new UnauthorizedAccessException("Only admins can view order summary.");
        }

        var utcNow = DateTime.UtcNow;

        var summary = await _orderRepository.GetSummaryAsync(
            todayStartUtc: utcNow.Date,
            last7DaysStartUtc: utcNow.AddDays(-7),
            cancellationToken
        );

        var statusCountsByStatus = summary.StatusCounts.ToDictionary(
            statusCount => statusCount.Status,
            statusCount => statusCount.Count
        );

        var ordersByStatus = Enum.GetValues<OrderStatus>()
            .Select(status => new OrderStatusSummaryResponse
            {
                Status = status,
                Count = statusCountsByStatus.GetValueOrDefault(status),
            })
            .ToList();

        return new OrderSummaryResponse
        {
            TotalOrders = summary.TotalOrders,
            OrdersByStatus = ordersByStatus,
            RevenueByCurrency = summary
                .RevenueByCurrency.Select(revenue => new OrderRevenueSummaryResponse
                {
                    Currency = revenue.Currency,
                    TotalAmount = revenue.TotalAmount,
                    AverageOrderValue = revenue.AverageOrderValue,
                    OrderCount = revenue.OrderCount,
                })
                .ToList(),
            FailedPaymentCount = statusCountsByStatus.GetValueOrDefault(OrderStatus.Failed),
            CancelledOrderCount = statusCountsByStatus.GetValueOrDefault(OrderStatus.Cancelled),
            OrdersCreatedToday = summary.OrdersCreatedToday,
            OrdersCreatedLast7Days = summary.OrdersCreatedLast7Days,
            GeneratedAtUtc = utcNow,
        };
    }
}
