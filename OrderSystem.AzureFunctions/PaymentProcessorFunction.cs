using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OrderSystem.Infrastructure.Messaging.Messages;
using OrderSystem.Infrastructure.Persistence.Repositories;
using OrderSystem.Modules.Payments.Services;

namespace OrderSystem.AzureFunctions;

public class PaymentProcessorFunction
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentProcessorFunction> _logger;

    public PaymentProcessorFunction(
        IOrderRepository orderRepository,
        IPaymentService paymentService,
        ILogger<PaymentProcessorFunction> logger
    )
    {
        _orderRepository = orderRepository;
        _paymentService = paymentService;
        _logger = logger;
    }

    [Function(nameof(PaymentProcessorFunction))]
    public async Task Run(
        [ServiceBusTrigger("order-created", Connection = "AzureServiceBusConnection")]
            string message,
        CancellationToken cancellationToken
    )
    {
        var orderMessage = JsonSerializer.Deserialize<OrderCreatedMessage>(message);

        if (orderMessage is null)
        {
            _logger.LogError("Invalid order message received.");
            return;
        }

        var order = await _orderRepository.GetByIdAsync(orderMessage.OrderId, cancellationToken);

        if (order is null)
        {
            _logger.LogError("Order not found. OrderId: {OrderId}", orderMessage.OrderId);
            return;
        }

        order.SetPaymentProcessing();
        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        var paymentSuccessful = await _paymentService.ProcessPaymentAsync(
            order.Id,
            order.TotalAmount,
            cancellationToken
        );

        if (paymentSuccessful)
        {
            order.SetPaid();
        }
        else
        {
            order.SetPaymentFailed();
        }

        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        _logger.LogWarning(
            "Payment processed by Azure Function. OrderId: {OrderId}, Status: {Status}",
            order.Id,
            order.Status
        );
    }
}
