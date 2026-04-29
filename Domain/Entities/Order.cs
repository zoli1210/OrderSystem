using OrderSystem.Domain.Enums;

namespace OrderSystem.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }

    public string CustomerName { get; private set; } = string.Empty;

    public decimal TotalAmount { get; private set; }

    public OrderStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private Order()
    {
    }

    public Order(string customerName, decimal totalAmount)
    {
        Id = Guid.NewGuid();
        CustomerName = customerName;
        TotalAmount = totalAmount;
        Status = OrderStatus.Pending;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void SetPaymentProcessing()
    {
        Status = OrderStatus.PaymentProcessing;
    }

    public void SetPaid()
    {
        Status = OrderStatus.Paid;
    }

    public void SetPaymentFailed()
    {
        Status = OrderStatus.PaymentFailed;
    }

    public void Cancel()
    {
        Status = OrderStatus.Cancelled;
    }
}