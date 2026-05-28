using OrderSystem.Domain.Enums;

namespace OrderSystem.Modules.Orders.DTOs;

public class UpdateOrderStatusRequest
{
    public OrderStatus TargetStatus { get; set; }

    public string? Note { get; set; }

    public string? TrackingNumber { get; set; }
}
