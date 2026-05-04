using System.Threading.Channels;
using OrderSystem.Infrastructure.Messaging.Messages;

namespace OrderSystem.Infrastructure.Messaging;

public class InMemoryOrderMessageQueue
{
    private readonly Channel<OrderCreatedMessage> _channel =
        Channel.CreateUnbounded<OrderCreatedMessage>();

    public async Task EnqueueAsync(OrderCreatedMessage message, CancellationToken cancellationToken)
    {
        await _channel.Writer.WriteAsync(message, cancellationToken);
    }

    public async Task<OrderCreatedMessage> DequeueAsync(CancellationToken cancellationToken)
    {
        return await _channel.Reader.ReadAsync(cancellationToken);
    }
}
