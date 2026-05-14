using OrderSystem.Domain.Enums;

namespace OrderSystem.Modules.Orders.DTOs;

public class OrderResponse
{
    public Guid Id { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string CreatedByUserId { get; set; } = string.Empty;

    public string CustomerEmail { get; set; } = string.Empty;

    public DateTime? EmailSentAtUtc { get; set; }

    public decimal TotalAmount { get; set; }

    public string Currency { get; set; } = string.Empty;

    public string? Description { get; set; }

    public OrderStatus Status { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
