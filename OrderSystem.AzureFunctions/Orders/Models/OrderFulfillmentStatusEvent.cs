using OrderSystem.Domain.Enums;

namespace OrderSystem.AzureFunctions.Orders.Models;

public class OrderFulfillmentStatusEvent
{
    public Guid OrderId { get; set; }

    public OrderStatus Status { get; set; }

    public DateTime ChangedAtUtc { get; set; }

    public string? Note { get; set; }
}
