using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OrderSystem.AzureFunctions.Services;

namespace OrderSystem.AzureFunctions;

public static class OrderFulfillmentAlertActivity
{
    [Function(nameof(OrderFulfillmentAlertActivity))]
    public static Task RunAsync(
        [ActivityTrigger] OrderFulfillmentAlert alert,
        FunctionContext context
    )
    {
        var logger = context.GetLogger(nameof(OrderFulfillmentAlertActivity));

        logger.LogWarning(
            "Fulfillment workflow alert. OrderId: {OrderId}, ExpectedStatus: {ExpectedStatus}, Message: {Message}",
            alert.OrderId,
            alert.ExpectedStatus,
            alert.Message
        );

        return Task.CompletedTask;
    }
}
