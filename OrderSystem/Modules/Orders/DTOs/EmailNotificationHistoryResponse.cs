using OrderSystem.Domain.Enums;

namespace OrderSystem.Modules.Orders.DTOs;

public class EmailNotificationHistoryResponse
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public string Recipient { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public EmailNotificationStatus Status { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? SentAtUtc { get; set; }

    public DateTime? FailedAtUtc { get; set; }

    public string? ErrorMessage { get; set; }
}
