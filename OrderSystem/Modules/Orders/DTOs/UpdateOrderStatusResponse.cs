using OrderSystem.Domain.Enums;

namespace OrderSystem.Modules.Orders.DTOs;

public class UpdateOrderStatusResponse
{
    public Guid OrderId { get; set; }

    public OrderStatus PreviousStatus { get; set; }

    public OrderStatus CurrentStatus { get; set; }

    public string? TrackingNumber { get; set; }

    public string Message { get; set; } = string.Empty;
}
