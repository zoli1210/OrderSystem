using OrderSystem.Domain.Enums;

namespace OrderSystem.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }

    public string CustomerName { get; private set; } = string.Empty;

    public string CreatedByUserId { get; private set; } = string.Empty;

    public string CustomerEmail { get; private set; } = string.Empty;

    public DateTime? EmailSentAtUtc { get; private set; }

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
        string? description,
        string createdByUserId
    )
    {
        Id = Guid.NewGuid();
        CustomerName = customerName;
        CustomerEmail = customerEmail;
        TotalAmount = totalAmount;
        Currency = currency;
        Description = description;
        CreatedByUserId = createdByUserId;
        Status = OrderStatus.Pending;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void StartPaymentProcessing()
    {
        if (Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Payment processing cannot be started from status {Status}."
            );
        }

        Status = OrderStatus.PaymentProcessing;
    }

    public void MarkAsPaid()
    {
        if (Status != OrderStatus.PaymentProcessing)
        {
            throw new InvalidOperationException(
                $"Order cannot be marked as paid from status {Status}."
            );
        }

        Status = OrderStatus.Paid;
    }

    public void MarkPaymentAsFailed()
    {
        if (Status != OrderStatus.PaymentProcessing)
        {
            throw new InvalidOperationException(
                $"Payment cannot be marked as failed from status {Status}."
            );
        }

        Status = OrderStatus.PaymentFailed;
    }

    public void Cancel()
    {
        if (Status is OrderStatus.Paid or OrderStatus.PaymentProcessing or OrderStatus.Cancelled)
        {
            throw new InvalidOperationException($"Order cannot be cancelled from status {Status}.");
        }

        Status = OrderStatus.Cancelled;
    }

    public bool IsEmailSent()
    {
        return EmailSentAtUtc.HasValue;
    }

    public void MarkEmailAsSent()
    {
        if (EmailSentAtUtc.HasValue)
        {
            return;
        }

        EmailSentAtUtc = DateTime.UtcNow;
    }
}
