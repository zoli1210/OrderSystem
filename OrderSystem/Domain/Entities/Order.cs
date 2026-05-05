using OrderSystem.Domain.Enums;

namespace OrderSystem.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }

    public string CustomerName { get; private set; } = string.Empty;

    public string CustomerEmail { get; private set; } = string.Empty;

    public decimal TotalAmount { get; private set; }

    public string Currency { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public OrderStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private Order() { }

    public Order(
        string customerName,
        string customerEmail,
        decimal totalAmount,
        string currency,
        string? description
    )
    {
        Id = Guid.NewGuid();
        CustomerName = customerName;
        CustomerEmail = customerEmail;
        TotalAmount = totalAmount;
        Currency = currency;
        Description = description;
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
