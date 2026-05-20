using OrderSystem.Domain.Enums;

namespace OrderSystem.Domain.Entities;

public class EmailNotificationHistory
{
    public Guid Id { get; private set; }

    public Guid OrderId { get; private set; }

    public string Recipient { get; private set; } = string.Empty;

    public string Subject { get; private set; } = string.Empty;

    public string Body { get; private set; } = string.Empty;

    public EmailNotificationStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? SentAtUtc { get; private set; }

    public DateTime? FailedAtUtc { get; private set; }

    public string? ErrorMessage { get; private set; }

    private EmailNotificationHistory() { }

    public EmailNotificationHistory(Guid orderId, string recipient, string subject, string body)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        Recipient = recipient;
        Subject = subject;
        Body = body;
        Status = EmailNotificationStatus.Pending;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void MarkAsSent()
    {
        Status = EmailNotificationStatus.Sent;
        SentAtUtc = DateTime.UtcNow;
        FailedAtUtc = null;
        ErrorMessage = null;
    }

    public void MarkAsFailed(string errorMessage)
    {
        Status = EmailNotificationStatus.Failed;
        FailedAtUtc = DateTime.UtcNow;
        ErrorMessage = errorMessage;
    }
}
