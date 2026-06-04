using OrderSystem.Domain.Enums;

namespace OrderSystem.Infrastructure.Messaging.Messages;

public class OrderStatusChangedMessage
{
    public Guid OrderId { get; set; }

    public string CustomerEmail { get; set; } = string.Empty;

    public OrderStatus PreviousStatus { get; set; }

    public OrderStatus CurrentStatus { get; set; }

    public string? TrackingNumber { get; set; }

    public string? Note { get; set; }
}
