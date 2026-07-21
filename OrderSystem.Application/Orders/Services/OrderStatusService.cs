using OrderSystem.Application.Orders.Contracts.Requests;
using OrderSystem.Application.Orders.Contracts.Responses;
using OrderSystem.Application.Orders.Tracking;
using OrderSystem.Domain.Entities;
using OrderSystem.Domain.Enums;
using OrderSystem.Infrastructure.Messaging;
using OrderSystem.Infrastructure.Messaging.Messages;
using OrderSystem.Repository.Repositories;

namespace OrderSystem.Application.Orders.Services;

public class OrderStatusService : IOrderStatusService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderStatusHistoryRepository _statusHistoryRepository;
    private readonly ITrackingNumberGenerator _trackingNumberGenerator;
    private readonly IOrderMessageSender _orderMessageSender;

    public OrderStatusService(
        IOrderRepository orderRepository,
        IOrderStatusHistoryRepository statusHistoryRepository,
        ITrackingNumberGenerator trackingNumberGenerator,
        IOrderMessageSender orderMessageSender
    )
    {
        _orderRepository = orderRepository;
        _statusHistoryRepository = statusHistoryRepository;
        _trackingNumberGenerator = trackingNumberGenerator;
        _orderMessageSender = orderMessageSender;
    }

    public async Task<UpdateOrderStatusResponse> UpdateStatusAsync(
        Guid orderId,
        UpdateOrderStatusRequest request,
        string currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken
    )
    {
        if (!isAdmin)
        {
            throw new UnauthorizedAccessException("Only admins can update order status.");
        }

        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);

        if (order is null)
        {
            throw new Exception("Order not found.");
        }

        var previousStatus = order.Status;
        var trackingNumber = GenerateTrackingNumber(request);

        order.ChangeStatus(request.TargetStatus, currentUserId, trackingNumber);

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

        await _orderMessageSender.SendOrderStatusChangedAsync(
            new OrderStatusChangedMessage
            {
                OrderId = order.Id,
                CustomerEmail = order.CustomerEmail,
                PreviousStatus = previousStatus,
                CurrentStatus = order.Status,
                TrackingNumber = order.TrackingNumber,
                Note = request.Note,
            },
            cancellationToken
        );

        return new UpdateOrderStatusResponse
        {
            OrderId = order.Id,
            PreviousStatus = previousStatus,
            CurrentStatus = order.Status,
            TrackingNumber = order.TrackingNumber,
            Message = $"Order status changed from {previousStatus} to {order.Status}.",
        };
    }

    private string? GenerateTrackingNumber(UpdateOrderStatusRequest request)
    {
        if (request.TargetStatus != OrderStatus.Shipped)
        {
            return request.TrackingNumber;
        }

        if (!string.IsNullOrWhiteSpace(request.TrackingNumber))
        {
            return request.TrackingNumber;
        }

        return _trackingNumberGenerator.Generate();
    }
}
