using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using OrderSystem.AzureFunctions.Orders.Models;

namespace OrderSystem.AzureFunctions;

public static class OrderFulfillmentOrchestrator
{
    [Function(nameof(OrderFulfillmentOrchestrator))]
    public static async Task RunAsync([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var input = context.GetInput<OrderFulfillmentWorkflowInput>();

        if (input is null)
        {
            throw new InvalidOperationException("Order fulfillment workflow input is missing.");
        }

        await WaitForStatusOrAlertAsync(
            context,
            input.OrderId,
            "Preparing",
            TimeSpan.FromHours(48),
            "Order is paid, but preparation has not started within 48 hours."
        );

        await WaitForStatusOrAlertAsync(
            context,
            input.OrderId,
            "ReadyForShipment",
            TimeSpan.FromHours(24),
            "Order is preparing, but it was not marked as ready for shipment within 24 hours."
        );

        await WaitForStatusOrAlertAsync(
            context,
            input.OrderId,
            "Shipped",
            TimeSpan.FromHours(24),
            "Order is ready for shipment, but it was not shipped within 24 hours."
        );

        await WaitForStatusOrAlertAsync(
            context,
            input.OrderId,
            "Delivered",
            TimeSpan.FromDays(5),
            "Order was shipped, but it was not delivered within 5 days."
        );
    }

    private static async Task WaitForStatusOrAlertAsync(
        TaskOrchestrationContext context,
        Guid orderId,
        string expectedStatus,
        TimeSpan timeout,
        string timeoutMessage
    )
    {
        using var timeoutCancellation = new CancellationTokenSource();

        var statusEventTask = context.WaitForExternalEvent<OrderFulfillmentStatusEvent>(
            expectedStatus
        );

        var timeoutTask = context.CreateTimer(
            context.CurrentUtcDateTime.Add(timeout),
            timeoutCancellation.Token
        );

        var completedTask = await Task.WhenAny(statusEventTask, timeoutTask);

        if (completedTask == statusEventTask)
        {
            timeoutCancellation.Cancel();
            return;
        }

        await context.CallActivityAsync(
            nameof(OrderFulfillmentAlertActivity),
            new OrderFulfillmentAlert
            {
                OrderId = orderId,
                ExpectedStatus = expectedStatus,
                Message = timeoutMessage,
            }
        );

        await statusEventTask;
    }
}
