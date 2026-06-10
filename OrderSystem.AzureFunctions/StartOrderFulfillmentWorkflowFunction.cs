using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using OrderSystem.AzureFunctions.Services;

namespace OrderSystem.AzureFunctions;

public class StartOrderFulfillmentWorkflowFunction
{
    private readonly ILogger<StartOrderFulfillmentWorkflowFunction> _logger;

    public StartOrderFulfillmentWorkflowFunction(
        ILogger<StartOrderFulfillmentWorkflowFunction> logger
    )
    {
        _logger = logger;
    }

    [Function(nameof(StartOrderFulfillmentWorkflowFunction))]
    public async Task<HttpResponseData> RunAsync(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "post",
            Route = "orders/{orderId}/fulfillment-workflow/start"
        )]
            HttpRequestData request,
        string orderId,
        [DurableClient] DurableTaskClient durableClient
    )
    {
        if (!Guid.TryParse(orderId, out var parsedOrderId))
        {
            var badRequestResponse = request.CreateResponse(HttpStatusCode.BadRequest);

            await badRequestResponse.WriteAsJsonAsync(new { Message = "Invalid order id." });

            return badRequestResponse;
        }

        var instanceId = BuildInstanceId(parsedOrderId);

        var input = new OrderFulfillmentWorkflowInput { OrderId = parsedOrderId };

        await durableClient.ScheduleNewOrchestrationInstanceAsync(
            nameof(OrderFulfillmentOrchestrator),
            input,
            new StartOrchestrationOptions { InstanceId = instanceId }
        );

        _logger.LogInformation(
            "Order fulfillment workflow started. OrderId: {OrderId}, InstanceId: {InstanceId}",
            parsedOrderId,
            instanceId
        );

        var response = request.CreateResponse(HttpStatusCode.Accepted);

        await response.WriteAsJsonAsync(
            new
            {
                OrderId = parsedOrderId,
                InstanceId = instanceId,
                Message = "Order fulfillment workflow started.",
            }
        );

        return response;
    }

    public static string BuildInstanceId(Guid orderId)
    {
        return $"order-fulfillment-{orderId}";
    }
}
