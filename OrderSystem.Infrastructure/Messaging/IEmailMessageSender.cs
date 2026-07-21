using OrderSystem.Infrastructure.Messaging.Messages;

namespace OrderSystem.Infrastructure.Messaging;

public interface IEmailMessageSender
{
    Task SendEmailNotificationAsync(
        EmailNotificationMessage message,
        CancellationToken cancellationToken
    );
}
