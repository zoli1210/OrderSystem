using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using OrderSystem.AzureFunctions.Services;
using OrderSystem.Domain.Enums;
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

    [Function(nameof(OrderStatusChangedFunction))]
    public async Task RunAsync(
        [ServiceBusTrigger("order-status-changed", Connection = "AzureServiceBusConnection")]
            ServiceBusReceivedMessage message,
        [DurableClient] DurableTaskClient durableClient,
        CancellationToken cancellationToken
    )
    {
        var body = message.Body.ToString();

        _logger.LogInformation(
            "OrderStatusChangedFunction triggered. MessageId: {MessageId}, SequenceNumber: {SequenceNumber}, DeliveryCount: {DeliveryCount}, Body: {Body}",
            message.MessageId,
            message.SequenceNumber,
            message.DeliveryCount,
            body
        );

        var statusChangedMessage = JsonSerializer.Deserialize<OrderStatusChangedMessage>(
            body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        if (statusChangedMessage is null)
        {
            _logger.LogWarning("Order status changed message could not be deserialized.");
            return;
        }

        await HandleFulfillmentWorkflowAsync(
            durableClient,
            statusChangedMessage,
            cancellationToken
        );

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
            "Order status email notification message sent. OrderId: {OrderId}, Status: {Status}, Recipient: {Recipient}, Subject: {Subject}, EmailType: {EmailType}",
            statusChangedMessage.OrderId,
            statusChangedMessage.CurrentStatus,
            statusChangedMessage.CustomerEmail,
            email.Subject,
            email.EmailType
        );
    }

    private async Task HandleFulfillmentWorkflowAsync(
        DurableTaskClient durableClient,
        OrderStatusChangedMessage message,
        CancellationToken cancellationToken
    )
    {
        if (message.CurrentStatus == OrderStatus.Paid)
        {
            await StartFulfillmentWorkflowAsync(durableClient, message, cancellationToken);

            return;
        }

        if (!IsFulfillmentWorkflowEventStatus(message.CurrentStatus))
        {
            return;
        }

        await NotifyFulfillmentWorkflowAsync(durableClient, message, cancellationToken);
    }

    private async Task StartFulfillmentWorkflowAsync(
        DurableTaskClient durableClient,
        OrderStatusChangedMessage message,
        CancellationToken cancellationToken
    )
    {
        var instanceId = StartOrderFulfillmentWorkflowFunction.BuildInstanceId(message.OrderId);

        try
        {
            var existingInstance = await durableClient.GetInstanceAsync(
                instanceId,
                getInputsAndOutputs: false,
                cancellationToken
            );

            if (existingInstance is not null)
            {
                _logger.LogInformation(
                    "Fulfillment workflow start skipped because an instance already exists. OrderId: {OrderId}, InstanceId: {InstanceId}, RuntimeStatus: {RuntimeStatus}",
                    message.OrderId,
                    instanceId,
                    existingInstance.RuntimeStatus
                );

                return;
            }

            await durableClient.ScheduleNewOrchestrationInstanceAsync(
                nameof(OrderFulfillmentOrchestrator),
                new OrderFulfillmentWorkflowInput
                {
                    OrderId = message.OrderId,
                    CustomerEmail = message.CustomerEmail,
                },
                new StartOrchestrationOptions { InstanceId = instanceId },
                cancellationToken
            );

            _logger.LogInformation(
                "Fulfillment workflow started. OrderId: {OrderId}, InstanceId: {InstanceId}",
                message.OrderId,
                instanceId
            );
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Fulfillment workflow could not be started. OrderId: {OrderId}, InstanceId: {InstanceId}",
                message.OrderId,
                instanceId
            );

            throw;
        }
    }

    private async Task NotifyFulfillmentWorkflowAsync(
        DurableTaskClient durableClient,
        OrderStatusChangedMessage message,
        CancellationToken cancellationToken
    )
    {
        var instanceId = StartOrderFulfillmentWorkflowFunction.BuildInstanceId(message.OrderId);

        try
        {
            await durableClient.RaiseEventAsync(
                instanceId,
                message.CurrentStatus.ToString(),
                new OrderFulfillmentStatusEvent
                {
                    OrderId = message.OrderId,
                    Status = message.CurrentStatus,
                    ChangedAtUtc = DateTime.UtcNow,
                    Note = message.Note,
                },
                cancellationToken
            );

            _logger.LogInformation(
                "Fulfillment workflow event raised. OrderId: {OrderId}, InstanceId: {InstanceId}, EventName: {EventName}",
                message.OrderId,
                instanceId,
                message.CurrentStatus
            );
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Fulfillment workflow event could not be raised. OrderId: {OrderId}, Status: {Status}",
                message.OrderId,
                message.CurrentStatus
            );

            throw;
        }
    }

    private static bool IsFulfillmentWorkflowEventStatus(OrderStatus status)
    {
        return status
            is OrderStatus.Preparing
                or OrderStatus.ReadyForShipment
                or OrderStatus.Shipped
                or OrderStatus.Delivered;
    }
}
