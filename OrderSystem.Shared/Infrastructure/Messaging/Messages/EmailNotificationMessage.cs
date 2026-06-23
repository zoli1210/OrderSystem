namespace OrderSystem.Infrastructure.Messaging.Messages;

public class EmailNotificationMessage
{
    public Guid OrderId { get; set; }

    public string CustomerEmail { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public string EmailType { get; set; } = string.Empty;
}
