using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OrderSystem.AzureFunctions.Services;

namespace OrderSystem.AzureFunctions;

public class PaymentProcessorFunction
{
    private readonly IPaymentProcessor _paymentProcessor;
    private readonly ILogger<PaymentProcessorFunction> _logger;

    public PaymentProcessorFunction(
        IPaymentProcessor paymentProcessor,
        ILogger<PaymentProcessorFunction> logger
    )
    {
        _paymentProcessor = paymentProcessor;
        _logger = logger;
    }

    [Function(nameof(PaymentProcessorFunction))]
    public async Task Run(
        [ServiceBusTrigger("order-created", Connection = "AzureServiceBusConnection")]
            string message,
        CancellationToken cancellationToken
    )
    {
        _logger.LogWarning("PaymentProcessorFunction triggered.");

        await _paymentProcessor.ProcessAsync(message, cancellationToken);
    }
}
