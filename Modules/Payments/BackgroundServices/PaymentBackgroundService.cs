using OrderSystem.Infrastructure.Messaging.Messages;
using OrderSystem.Infrastructure.Persistence.Repositories;
using OrderSystem.Modules.Payments.Services;

namespace OrderSystem.Infrastructure.Messaging;

public class PaymentBackgroundService : BackgroundService
{
    private const int MaxRetryCount = 3;

    private readonly InMemoryOrderMessageQueue _queue;
    private readonly InMemoryDeadLetterQueue _deadLetterQueue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PaymentBackgroundService> _logger;

    public PaymentBackgroundService(
        InMemoryOrderMessageQueue queue,
        InMemoryDeadLetterQueue deadLetterQueue,
        IServiceScopeFactory scopeFactory,
        ILogger<PaymentBackgroundService> logger
    )
    {
        _queue = queue;
        _deadLetterQueue = deadLetterQueue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            OrderCreatedMessage? message = null;

            try
            {
                message = await _queue.DequeueAsync(stoppingToken);

                await ProcessPaymentMessageAsync(message, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                if (message is null)
                {
                    _logger.LogError(ex, "Payment worker failed before message was received.");
                    continue;
                }

                await HandleFailedMessageAsync(message, ex, stoppingToken);
            }
        }
    }

    private async Task ProcessPaymentMessageAsync(
        OrderCreatedMessage message,
        CancellationToken cancellationToken
    )
    {
        using var scope = _scopeFactory.CreateScope();

        var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
        var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();

        var order = await orderRepository.GetByIdAsync(message.OrderId, cancellationToken);

        if (order is null)
        {
            _logger.LogWarning("Order not found. OrderId: {OrderId}", message.OrderId);
            return;
        }

        order.SetPaymentProcessing();
        await orderRepository.UpdateAsync(order, cancellationToken);
        await orderRepository.SaveChangesAsync(cancellationToken);

        var paymentSuccessful = await paymentService.ProcessPaymentAsync(
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

        await orderRepository.UpdateAsync(order, cancellationToken);
        await orderRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Payment processed. OrderId: {OrderId}, Status: {Status}",
            order.Id,
            order.Status
        );
    }

    private async Task HandleFailedMessageAsync(
        OrderCreatedMessage message,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        if (message.RetryCount < MaxRetryCount)
        {
            message.RetryCount++;

            await _queue.EnqueueAsync(message, cancellationToken);

            _logger.LogWarning(
                "Payment failed. Retrying. OrderId: {OrderId}, RetryCount: {RetryCount}",
                message.OrderId,
                message.RetryCount
            );

            return;
        }

        await MoveToDeadLetterAsync(message, cancellationToken);

        _logger.LogError(
            exception,
            "Payment failed permanently. OrderId: {OrderId}",
            message.OrderId
        );
    }

    private async Task MoveToDeadLetterAsync(
        OrderCreatedMessage message,
        CancellationToken cancellationToken
    )
    {
        using var scope = _scopeFactory.CreateScope();

        var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

        var order = await orderRepository.GetByIdAsync(message.OrderId, cancellationToken);

        if (order is not null)
        {
            order.SetPaymentFailed();
            await orderRepository.UpdateAsync(order, cancellationToken);
            await orderRepository.SaveChangesAsync(cancellationToken);
        }

        await _deadLetterQueue.AddAsync(message);
    }
}
