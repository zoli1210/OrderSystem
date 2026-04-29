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
}
