using System.Text.Json;
using Microsoft.Extensions.Logging;
using OrderSystem.Infrastructure.Messaging.Messages;
using OrderSystem.Modules.Email.Services;

namespace OrderSystem.AzureFunctions.Services;

public class EmailProcessor : IEmailProcessor
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailProcessor> _logger;

    public EmailProcessor(IEmailService emailService, ILogger<EmailProcessor> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task ProcessAsync(string message, CancellationToken cancellationToken)
    {
        var emailMessage = JsonSerializer.Deserialize<EmailNotificationMessage>(message);

        if (emailMessage is null)
        {
            throw new InvalidOperationException("Invalid email notification message received.");
        }

        await _emailService.SendAsync(emailMessage, cancellationToken);

        _logger.LogWarning(
            "Email notification processed. OrderId: {OrderId}, Email: {Email}",
            emailMessage.OrderId,
            emailMessage.CustomerEmail
        );
    }
}
