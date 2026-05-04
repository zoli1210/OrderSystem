using OrderSystem.Infrastructure.Messaging.Messages;

namespace OrderSystem.Infrastructure.Messaging;

public class InMemoryDeadLetterQueue
{
    private readonly List<OrderCreatedMessage> _messages = new();

    public Task AddAsync(OrderCreatedMessage message)
    {
        _messages.Add(message);
        return Task.CompletedTask;
    }

    public IReadOnlyList<OrderCreatedMessage> GetAll()
    {
        return _messages.AsReadOnly();
    }

    public bool Remove(Guid orderId)
    {
        var message = _messages.FirstOrDefault(x => x.OrderId == orderId);

        if (message is null)
        {
            return false;
        }

        _messages.Remove(message);

        return true;
    }
}
