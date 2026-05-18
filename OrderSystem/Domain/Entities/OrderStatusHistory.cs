using OrderSystem.Domain.Enums;

namespace OrderSystem.Domain.Entities;

public class OrderStatusHistory
{
    public Guid Id { get; private set; }

    public Guid OrderId { get; private set; }

    public OrderStatus FromStatus { get; private set; }

    public OrderStatus ToStatus { get; private set; }

    public DateTime ChangedAtUtc { get; private set; }

    public string ChangedByUserId { get; private set; } = string.Empty;

    private OrderStatusHistory() { }

    public OrderStatusHistory(
        Guid orderId,
        OrderStatus fromStatus,
        OrderStatus toStatus,
        string changedByUserId
    )
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        ChangedAtUtc = DateTime.UtcNow;
        ChangedByUserId = changedByUserId;
    }
}
