namespace OrderSystem.Application.Models;

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

    public string? TrackingNumber { get; private set; }

    public DateTime? PreparationStartedAtUtc { get; private set; }

    public DateTime? ReadyForShipmentAtUtc { get; private set; }

    public DateTime? ShippedAtUtc { get; private set; }

    public DateTime? DeliveredAtUtc { get; private set; }

    public DateTime? ReturnedAtUtc { get; private set; }

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
        ChangeStatus(OrderStatus.PaymentProcessing, updatedByUserId);
    }

    public void MarkAsPaid(string updatedByUserId)
    {
        ChangeStatus(OrderStatus.Paid, updatedByUserId);
    }

    public void MarkPaymentAsFailed(string updatedByUserId)
    {
        ChangeStatus(OrderStatus.Failed, updatedByUserId);
    }

    public void RetryPayment(string updatedByUserId)
    {
        ChangeStatus(OrderStatus.Pending, updatedByUserId);
    }

    public void StartPreparing(string updatedByUserId)
    {
        ChangeStatus(OrderStatus.Preparing, updatedByUserId);
    }

    public void MarkAsReadyForShipment(string updatedByUserId)
    {
        ChangeStatus(OrderStatus.ReadyForShipment, updatedByUserId);
    }

    public void MarkAsShipped(string updatedByUserId, string trackingNumber)
    {
        if (string.IsNullOrWhiteSpace(trackingNumber))
        {
            throw new InvalidOperationException(
                "Tracking number is required when marking an order as shipped."
            );
        }

        ChangeStatus(OrderStatus.Shipped, updatedByUserId, trackingNumber);
    }

    public void MarkAsDelivered(string updatedByUserId)
    {
        ChangeStatus(OrderStatus.Delivered, updatedByUserId);
    }

    public void MarkAsReturned(string updatedByUserId)
    {
        ChangeStatus(OrderStatus.Returned, updatedByUserId);
    }

    public void Cancel(string reason, string updatedByUserId)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidOperationException("Cancellation reason is required.");
        }

        ChangeStatus(OrderStatus.Cancelled, updatedByUserId);

        CancelledAtUtc = DateTime.UtcNow;
        CancellationReason = reason;
    }

    public void ChangeStatus(
        OrderStatus targetStatus,
        string updatedByUserId,
        string? trackingNumber = null
    )
    {
        if (Status == targetStatus)
        {
            return;
        }

        if (!IsValidTransition(Status, targetStatus))
        {
            throw new InvalidOperationException(
                $"Invalid order status transition from {Status} to {targetStatus}."
            );
        }

        Status = targetStatus;

        switch (targetStatus)
        {
            case OrderStatus.Preparing:
                PreparationStartedAtUtc = DateTime.UtcNow;
                break;

            case OrderStatus.ReadyForShipment:
                ReadyForShipmentAtUtc = DateTime.UtcNow;
                break;

            case OrderStatus.Shipped:
                if (string.IsNullOrWhiteSpace(trackingNumber))
                {
                    throw new InvalidOperationException(
                        "Tracking number is required when marking an order as shipped."
                    );
                }

                ShippedAtUtc = DateTime.UtcNow;
                TrackingNumber = trackingNumber;
                break;

            case OrderStatus.Delivered:
                DeliveredAtUtc = DateTime.UtcNow;
                break;

            case OrderStatus.Returned:
                ReturnedAtUtc = DateTime.UtcNow;
                break;
        }

        SetUpdated(updatedByUserId);
    }

    private static bool IsValidTransition(OrderStatus currentStatus, OrderStatus targetStatus)
    {
        return currentStatus switch
        {
            OrderStatus.Pending => targetStatus
                is OrderStatus.PaymentProcessing
                    or OrderStatus.Cancelled,

            OrderStatus.PaymentProcessing => targetStatus is OrderStatus.Paid or OrderStatus.Failed,

            OrderStatus.Failed => targetStatus is OrderStatus.Pending or OrderStatus.Cancelled,

            OrderStatus.Paid => targetStatus is OrderStatus.Preparing or OrderStatus.Cancelled,

            OrderStatus.Preparing => targetStatus
                is OrderStatus.ReadyForShipment
                    or OrderStatus.Cancelled,

            OrderStatus.ReadyForShipment => targetStatus
                is OrderStatus.Shipped
                    or OrderStatus.Cancelled,

            OrderStatus.Shipped => targetStatus is OrderStatus.Delivered,

            OrderStatus.Delivered => targetStatus is OrderStatus.Returned,

            OrderStatus.Cancelled => false,

            OrderStatus.Returned => false,

            _ => false,
        };
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
