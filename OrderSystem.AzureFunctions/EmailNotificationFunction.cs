using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OrderSystem.AzureFunctions.Services;

namespace OrderSystem.AzureFunctions;

public class EmailNotificationFunction
{
    private readonly IEmailProcessor _emailProcessor;
    private readonly ILogger<EmailNotificationFunction> _logger;

    public EmailNotificationFunction(
        IEmailProcessor emailProcessor,
        ILogger<EmailNotificationFunction> logger
    )
    {
        _emailProcessor = emailProcessor;
        _logger = logger;
    }

    [Function(nameof(EmailNotificationFunction))]
    public async Task Run(
        [ServiceBusTrigger("email-notification", Connection = "AzureServiceBusConnection")]
            string message,
        CancellationToken cancellationToken
    )
    {
        _logger.LogWarning("EmailNotificationFunction triggered.");

        await _emailProcessor.ProcessAsync(message, cancellationToken);
    }
}
