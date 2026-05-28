namespace OrderSystem.Domain.Enums;

public enum OrderStatus
{
    Pending = 0,
    PaymentProcessing = 10,
    Paid = 20,

    Preparing = 30,
    ReadyForShipment = 40,
    Shipped = 50,
    Delivered = 60,

    Failed = 70,
    Cancelled = 80,
    Returned = 90,
}
