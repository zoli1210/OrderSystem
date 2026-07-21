using OrderSystem.Domain.Enums;

namespace OrderSystem.Application.Orders.Contracts.Responses;

public class OrderStatusHistoryResponse
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public OrderStatus FromStatus { get; set; }

    public OrderStatus ToStatus { get; set; }

    public DateTime? ChangedAtUtc { get; set; }

    public string? ChangedByUserId { get; set; }

    public string? Note { get; set; }
}
