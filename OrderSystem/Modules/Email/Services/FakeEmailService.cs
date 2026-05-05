using Microsoft.Extensions.Logging;
using OrderSystem.Infrastructure.Messaging.Messages;

namespace OrderSystem.Modules.Email.Services;

public class FakeEmailService : IEmailService
{
    private readonly ILogger<FakeEmailService> _logger;

    public FakeEmailService(ILogger<FakeEmailService> logger)
    {
        _logger = logger;
    }

    public async Task SendAsync(
        EmailNotificationMessage message,
        CancellationToken cancellationToken
    )
    {
        await Task.Delay(300, cancellationToken);

        _logger.LogWarning(
            "Fake email sent. To: {Email}, Subject: {Subject}, OrderId: {OrderId}",
            message.CustomerEmail,
            message.Subject,
            message.OrderId
        );
    }
}
