using OrderSystem.Infrastructure.Messaging.Messages;

namespace OrderSystem.Infrastructure.Email;

public interface IEmailService
{
    Task SendAsync(EmailNotificationMessage message, CancellationToken cancellationToken);
}
