namespace OrderSystem.Infrastructure.Messaging.DTOs;

public class DeadLetterMessageResponse
{
    public string MessageId { get; set; } = string.Empty;

    public long SequenceNumber { get; set; }

    public int DeliveryCount { get; set; }

    public DateTimeOffset EnqueuedTimeUtc { get; set; }

    public string Body { get; set; } = string.Empty;

    public string? DeadLetterReason { get; set; }

    public string? DeadLetterErrorDescription { get; set; }
}
