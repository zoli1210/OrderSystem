using OrderSystem.Infrastructure.Messaging.Messages;

namespace OrderSystem.Infrastructure.Messaging;

public interface IOrderMessageSender
{
    Task SendOrderCreatedAsync(OrderCreatedMessage message, CancellationToken cancellationToken);
}