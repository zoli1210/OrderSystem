using OrderSystem.Domain.Enums;

namespace OrderSystem.Application.Orders.Contracts.Responses;

public class OrderResponse
{
    public Guid Id { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string CreatedByUserId { get; set; } = string.Empty;

    public string CustomerEmail { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public string Currency { get; set; } = string.Empty;

    public string? Description { get; set; }

    public OrderStatus Status { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? EmailSentAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public string? UpdatedByUserId { get; set; }

    public DateTime? CancelledAtUtc { get; set; }

    public string? CancellationReason { get; set; }

    public string? TrackingNumber { get; set; }

    public DateTime? PreparationStartedAtUtc { get; set; }

    public DateTime? ReadyForShipmentAtUtc { get; set; }

    public DateTime? ShippedAtUtc { get; set; }

    public DateTime? DeliveredAtUtc { get; set; }

    public DateTime? ReturnedAtUtc { get; set; }
}
