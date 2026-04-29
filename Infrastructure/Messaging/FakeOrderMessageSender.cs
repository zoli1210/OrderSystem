using OrderSystem.Infrastructure.Messaging.Messages;

namespace OrderSystem.Infrastructure.Messaging;

public class FakeOrderMessageSender : IOrderMessageSender
{
    private readonly InMemoryOrderMessageQueue _queue;
    private readonly ILogger<FakeOrderMessageSender> _logger;

    public FakeOrderMessageSender(
        InMemoryOrderMessageQueue queue,
        ILogger<FakeOrderMessageSender> logger
    )
    {
        _queue = queue;
        _logger = logger;
    }

    public async Task SendOrderCreatedAsync(
        OrderCreatedMessage message,
        CancellationToken cancellationToken
    )
    {
        await _queue.EnqueueAsync(message, cancellationToken);

        _logger.LogInformation(
            "Order created message enqueued. OrderId: {OrderId}, Amount: {Amount}, Email: {Email}",
            message.OrderId,
            message.TotalAmount,
            message.CustomerEmail
        );
    }
}
