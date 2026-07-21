using OrderSystem.Domain.Enums;

namespace OrderSystem.Application.Orders.Contracts.Responses;

public class UserOrderStatusHistoryItemResponse
{
    public OrderStatus FromStatus { get; set; }

    public OrderStatus ToStatus { get; set; }

    public DateTime ChangedAtUtc { get; set; }

    public string ChangedByUserId { get; set; } = string.Empty;

    public string? Note { get; set; }
}
