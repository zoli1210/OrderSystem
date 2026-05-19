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

    public DateTime? UpdatedAtUtc { get; private set; }

    public string? UpdatedByUserId { get; private set; }

    public DateTime? CancelledAtUtc { get; private set; }

    public string? CancellationReason { get; private set; }

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
        UpdatedAtUtc = CreatedAtUtc;
        UpdatedByUserId = createdByUserId;
    }

    public void StartPaymentProcessing(string updatedByUserId)
    {
        if (Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Payment processing cannot be started from status {Status}."
            );
        }

        Status = OrderStatus.PaymentProcessing;
        SetUpdated(updatedByUserId);
    }

    public void MarkAsPaid(string updatedByUserId)
    {
        if (Status != OrderStatus.PaymentProcessing)
        {
            throw new InvalidOperationException(
                $"Order cannot be marked as paid from status {Status}."
            );
        }

        Status = OrderStatus.Paid;
        SetUpdated(updatedByUserId);
    }

    public void MarkPaymentAsFailed(string updatedByUserId)
    {
        if (Status != OrderStatus.PaymentProcessing)
        {
            throw new InvalidOperationException(
                $"Payment cannot be marked as failed from status {Status}."
            );
        }

        Status = OrderStatus.PaymentFailed;
        SetUpdated(updatedByUserId);
    }

    public void Cancel(string reason, string updatedByUserId)
    {
        if (Status is OrderStatus.Paid or OrderStatus.PaymentProcessing or OrderStatus.Cancelled)
        {
            throw new InvalidOperationException($"Order cannot be cancelled from status {Status}.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidOperationException("Cancellation reason is required.");
        }

        Status = OrderStatus.Cancelled;
        CancelledAtUtc = DateTime.UtcNow;
        CancellationReason = reason;
        SetUpdated(updatedByUserId);
    }

    public void RetryPayment(string updatedByUserId)
    {
        if (Status != OrderStatus.PaymentFailed)
        {
            throw new InvalidOperationException(
                $"Payment retry cannot be started from status {Status}."
            );
        }

        Status = OrderStatus.Pending;
        SetUpdated(updatedByUserId);
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

    private void SetUpdated(string updatedByUserId)
    {
        UpdatedAtUtc = DateTime.UtcNow;
        UpdatedByUserId = updatedByUserId;
    }
}
