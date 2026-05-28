using OrderSystem.Domain.Enums;

namespace OrderSystem.Modules.Orders.DTOs;

public class UserOrderHistoryResponse
{
    public Guid OrderId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string CustomerEmail { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public string Currency { get; set; } = string.Empty;

    public string? Description { get; set; }

    public OrderStatus CurrentStatus { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public string? TrackingNumber { get; set; }

    public DateTime? PreparationStartedAtUtc { get; set; }

    public DateTime? ReadyForShipmentAtUtc { get; set; }

    public DateTime? ShippedAtUtc { get; set; }

    public DateTime? DeliveredAtUtc { get; set; }

    public DateTime? ReturnedAtUtc { get; set; }

    public IReadOnlyList<UserOrderStatusHistoryItemResponse> StatusHistory { get; set; } =
        new List<UserOrderStatusHistoryItemResponse>();
}
