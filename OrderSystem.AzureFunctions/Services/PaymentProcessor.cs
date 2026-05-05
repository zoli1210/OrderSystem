using System.Text.Json;
using Microsoft.Extensions.Logging;
using OrderSystem.Infrastructure.Messaging.Messages;
using OrderSystem.Infrastructure.Persistence.Repositories;
using OrderSystem.Modules.Payments.Services;

namespace OrderSystem.AzureFunctions.Services;

public class PaymentProcessor : IPaymentProcessor
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentProcessor> _logger;

    public PaymentProcessor(
        IOrderRepository orderRepository,
        IPaymentService paymentService,
        ILogger<PaymentProcessor> logger
    )
    {
        _orderRepository = orderRepository;
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task ProcessAsync(string message, CancellationToken cancellationToken)
    {
        var orderMessage = JsonSerializer.Deserialize<OrderCreatedMessage>(message);

        if (orderMessage is null)
        {
            throw new InvalidOperationException("Invalid order message received.");
        }

        var order = await _orderRepository.GetByIdAsync(orderMessage.OrderId, cancellationToken);

        if (order is null)
        {
            throw new InvalidOperationException(
                $"Order not found. OrderId: {orderMessage.OrderId}"
            );
        }

        order.SetPaymentProcessing();
        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        try
        {
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
        }
        catch
        {
            order.SetPaymentFailed();
            await _orderRepository.UpdateAsync(order, cancellationToken);
            await _orderRepository.SaveChangesAsync(cancellationToken);

            throw;
        }

        _logger.LogWarning(
            "Payment processed. OrderId: {OrderId}, Status: {Status}",
            order.Id,
            order.Status
        );
    }
}
