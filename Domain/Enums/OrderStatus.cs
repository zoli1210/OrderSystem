namespace OrderSystem.Domain.Enums;

public enum OrderStatus
{
    Pending = 1,
    PaymentProcessing = 2,
    Paid = 3,
    PaymentFailed = 4,
    Cancelled = 5
}