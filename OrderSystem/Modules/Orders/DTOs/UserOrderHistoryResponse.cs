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

    public IReadOnlyList<UserOrderStatusHistoryItemResponse> StatusHistory { get; set; } = [];
}
