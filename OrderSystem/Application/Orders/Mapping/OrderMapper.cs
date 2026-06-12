using OrderSystem.Application.Orders.Contracts.Responses;
using OrderSystem.Domain.Entities;

namespace OrderSystem.Application.Orders.Mapping;

public static class OrderMapper
{
    public static OrderResponse MapToResponse(Order order)
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
            TrackingNumber = order.TrackingNumber,
            PreparationStartedAtUtc = order.PreparationStartedAtUtc,
            ReadyForShipmentAtUtc = order.ReadyForShipmentAtUtc,
            ShippedAtUtc = order.ShippedAtUtc,
            DeliveredAtUtc = order.DeliveredAtUtc,
            ReturnedAtUtc = order.ReturnedAtUtc,
        };
    }

    public static OrderStatusHistoryResponse MapToStatusHistoryResponse(OrderStatusHistory history)
    {
        return new OrderStatusHistoryResponse
        {
            Id = history.Id,
            OrderId = history.OrderId,
            FromStatus = history.FromStatus,
            ToStatus = history.ToStatus,
            ChangedAtUtc = history.ChangedAtUtc,
            ChangedByUserId = history.ChangedByUserId,
            Note = history.Note,
        };
    }

    public static EmailNotificationHistoryResponse MapToEmailHistoryResponse(
        EmailNotificationHistory history
    )
    {
        return new EmailNotificationHistoryResponse
        {
            Id = history.Id,
            OrderId = history.OrderId,
            Recipient = history.Recipient,
            Subject = history.Subject,
            Status = history.Status,
            CreatedAtUtc = history.CreatedAtUtc,
            SentAtUtc = history.SentAtUtc,
            FailedAtUtc = history.FailedAtUtc,
            ErrorMessage = history.ErrorMessage,
        };
    }
}
