using OrderSystem.Infrastructure.Messaging.Messages;

namespace OrderSystem.Modules.Email.Services;

public interface IEmailService
{
    Task SendAsync(EmailNotificationMessage message, CancellationToken cancellationToken);
}
