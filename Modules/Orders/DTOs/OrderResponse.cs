using OrderSystem.Domain.Enums;

namespace OrderSystem.Modules.Orders.DTOs;

public class OrderResponse
{
    public Guid Id { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public OrderStatus Status { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
