using OrderSystem.Domain.Enums;

namespace OrderSystem.Application.Orders.Contracts.Requests;

public class UpdateOrderStatusRequest
{
    public OrderStatus TargetStatus { get; set; }

    public string? Note { get; set; }

    public string? TrackingNumber { get; set; }
}
