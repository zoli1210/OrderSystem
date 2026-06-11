using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrderSystem.AzureFunctions.Services;
using OrderSystem.Infrastructure.Messaging;
using OrderSystem.Infrastructure.Messaging.Messages;

namespace OrderSystem.AzureFunctions;

public class OrderFulfillmentAlertActivity
{
    private readonly IEmailMessageSender _emailMessageSender;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OrderFulfillmentAlertActivity> _logger;

    public OrderFulfillmentAlertActivity(
        IEmailMessageSender emailMessageSender,
        IConfiguration configuration,
        ILogger<OrderFulfillmentAlertActivity> logger
    )
    {
        _emailMessageSender = emailMessageSender;
        _configuration = configuration;
        _logger = logger;
    }

    [Function(nameof(OrderFulfillmentAlertActivity))]
    public async Task RunAsync(
        [ActivityTrigger] OrderFulfillmentAlert alert,
        CancellationToken cancellationToken
    )
    {
        var alertEmail = _configuration["FulfillmentAlertEmail"];

        if (string.IsNullOrWhiteSpace(alertEmail))
        {
            _logger.LogWarning(
                "Fulfillment alert email is not configured. OrderId: {OrderId}, ExpectedStatus: {ExpectedStatus}, Message: {Message}",
                alert.OrderId,
                alert.ExpectedStatus,
                alert.Message
            );

            return;
        }

        await _emailMessageSender.SendEmailNotificationAsync(
            new EmailNotificationMessage
            {
                OrderId = alert.OrderId,
                CustomerEmail = alertEmail,
                Subject = $"Fulfillment alert - order {alert.OrderId}",
                Body = $"""
                Fulfillment workflow alert.

                Order ID: {alert.OrderId}
                Expected status: {alert.ExpectedStatus}

                Message:
                {alert.Message}
                """,
                EmailType = $"FulfillmentAlert_{alert.ExpectedStatus}",
            },
            cancellationToken
        );

        _logger.LogWarning(
            "Fulfillment alert email queued. OrderId: {OrderId}, ExpectedStatus: {ExpectedStatus}, Recipient: {Recipient}",
            alert.OrderId,
            alert.ExpectedStatus,
            alertEmail
        );
    }
}
