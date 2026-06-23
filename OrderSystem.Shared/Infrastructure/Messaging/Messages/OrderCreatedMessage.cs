namespace OrderSystem.Infrastructure.Messaging.Messages;

public class OrderCreatedMessage
{
    public Guid OrderId { get; set; }
    public decimal TotalAmount { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;

    public int RetryCount { get; set; } = 0;

    public bool IsResolved { get; set; } = false;
}
