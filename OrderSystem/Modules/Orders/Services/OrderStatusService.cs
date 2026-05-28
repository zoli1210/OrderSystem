using OrderSystem.Domain.Entities;
using OrderSystem.Infrastructure.Persistence.Repositories;
using OrderSystem.Modules.Auth.Services;
using OrderSystem.Modules.Orders.DTOs;

namespace OrderSystem.Modules.Orders.Services;

public class OrderStatusService : IOrderStatusService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderStatusHistoryRepository _statusHistoryRepository;
    private readonly ICurrentUserService _currentUserService;

    public OrderStatusService(
        IOrderRepository orderRepository,
        IOrderStatusHistoryRepository statusHistoryRepository,
        ICurrentUserService currentUserService
    )
    {
        _orderRepository = orderRepository;
        _statusHistoryRepository = statusHistoryRepository;
        _currentUserService = currentUserService;
    }

    public async Task<UpdateOrderStatusResponse> UpdateStatusAsync(
        Guid orderId,
        UpdateOrderStatusRequest request,
        CancellationToken cancellationToken
    )
    {
        if (!_currentUserService.IsAdmin)
        {
            throw new UnauthorizedAccessException("Only admins can update order status.");
        }

        var currentUserId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);

        if (order is null)
        {
            throw new NotFoundException("Order not found.");
        }

        var previousStatus = order.Status;

        order.ChangeStatus(request.TargetStatus, currentUserId, request.TrackingNumber);

        await _statusHistoryRepository.AddAsync(
            new OrderStatusHistory(
                order.Id,
                previousStatus,
                order.Status,
                currentUserId,
                request.Note
            ),
            cancellationToken
        );

        await _orderRepository.UpdateAsync(order, cancellationToken);

        return new UpdateOrderStatusResponse
        {
            OrderId = order.Id,
            PreviousStatus = previousStatus,
            CurrentStatus = order.Status,
            TrackingNumber = order.TrackingNumber,
            Message = $"Order status changed from {previousStatus} to {order.Status}.",
        };
    }
}
