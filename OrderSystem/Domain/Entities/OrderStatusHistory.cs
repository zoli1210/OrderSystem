using OrderSystem.Domain.Enums;

namespace OrderSystem.Domain.Entities;

public class OrderStatusHistory
{
    public Guid Id { get; private set; }

    public Guid OrderId { get; private set; }

    public OrderStatus FromStatus { get; private set; }

    public OrderStatus ToStatus { get; private set; }

    public DateTime ChangedAtUtc { get; private set; }

    public string? ChangedByUserId { get; private set; }

    public string? Note { get; private set; }

    private OrderStatusHistory() { }

    public OrderStatusHistory(
        Guid orderId,
        OrderStatus fromStatus,
        OrderStatus toStatus,
        string? changedByUserId,
        string? note = null
    )
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        ChangedByUserId = changedByUserId;
        Note = note;
        ChangedAtUtc = DateTime.UtcNow;
    }
}
