using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OrderSystem.AzureFunctions.Services;
using OrderSystem.Infrastructure.Messaging;
using OrderSystem.Infrastructure.Messaging.Messages;

namespace OrderSystem.AzureFunctions;

public class OrderStatusChangedFunction
{
    private readonly IOrderStatusEmailProcessor _orderStatusEmailProcessor;
    private readonly IEmailMessageSender _emailMessageSender;
    private readonly ILogger<OrderStatusChangedFunction> _logger;

    public OrderStatusChangedFunction(
        IOrderStatusEmailProcessor orderStatusEmailProcessor,
        IEmailMessageSender emailMessageSender,
        ILogger<OrderStatusChangedFunction> logger
    )
    {
        _orderStatusEmailProcessor = orderStatusEmailProcessor;
        _emailMessageSender = emailMessageSender;
        _logger = logger;
    }

    [Function("OrderStatusChangedFunction")]
    public async Task RunAsync(
        [ServiceBusTrigger("order-status-changed", Connection = "AzureServiceBusConnection")]
            ServiceBusReceivedMessage message,
        CancellationToken cancellationToken
    )
    {
        var body = message.Body.ToString();

        var statusChangedMessage = JsonSerializer.Deserialize<OrderStatusChangedMessage>(
            body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        if (statusChangedMessage is null)
        {
            _logger.LogWarning("Order status changed message could not be deserialized.");
            return;
        }

        var email = _orderStatusEmailProcessor.BuildEmail(statusChangedMessage);

        if (email is null)
        {
            _logger.LogInformation(
                "No order status email configured. OrderId: {OrderId}, Status: {Status}",
                statusChangedMessage.OrderId,
                statusChangedMessage.CurrentStatus
            );

            return;
        }

        await _emailMessageSender.SendEmailNotificationAsync(
            new EmailNotificationMessage
            {
                OrderId = statusChangedMessage.OrderId,
                CustomerEmail = statusChangedMessage.CustomerEmail,
                Subject = email.Subject,
                Body = email.Body,
                EmailType = email.EmailType,
            },
            cancellationToken
        );

        _logger.LogInformation(
            "Order status email notification message sent. OrderId: {OrderId}, Status: {Status}, Recipient: {Recipient}, Subject: {Subject}",
            statusChangedMessage.OrderId,
            statusChangedMessage.CurrentStatus,
            statusChangedMessage.CustomerEmail,
            email.Subject
        );
    }
}
