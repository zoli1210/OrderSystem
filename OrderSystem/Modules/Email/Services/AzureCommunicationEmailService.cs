using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrderSystem.Infrastructure.Messaging.Messages;

namespace OrderSystem.Modules.Email.Services;

public class AzureCommunicationEmailService : IEmailService
{
    private readonly EmailClient _emailClient;
    private readonly string _senderAddress;
    private readonly ILogger<AzureCommunicationEmailService> _logger;

    public AzureCommunicationEmailService(
        IConfiguration configuration,
        ILogger<AzureCommunicationEmailService> logger
    )
    {
        var connectionString = configuration["CommunicationServices:ConnectionString"];
        var senderAddress = configuration["CommunicationServices:SenderAddress"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "CommunicationServices:ConnectionString is missing."
            );
        }

        if (string.IsNullOrWhiteSpace(senderAddress))
        {
            throw new InvalidOperationException("CommunicationServices:SenderAddress is missing.");
        }

        _emailClient = new EmailClient(connectionString);
        _senderAddress = senderAddress;
        _logger = logger;
    }

    public async Task SendAsync(
        EmailNotificationMessage message,
        CancellationToken cancellationToken
    )
    {
        var emailMessage = new EmailMessage(
            senderAddress: _senderAddress,
            recipients: new EmailRecipients(new[] { new EmailAddress(message.CustomerEmail) }),
            content: new EmailContent(message.Subject) { PlainText = message.Body }
        );

        var operation = await _emailClient.SendAsync(
            WaitUntil.Completed,
            emailMessage,
            cancellationToken
        );

        _logger.LogWarning(
            "Email sent via Azure Communication Services. OperationId: {OperationId}, To: {Email}, OrderId: {OrderId}",
            operation.Id,
            message.CustomerEmail,
            message.OrderId
        );
    }
}
