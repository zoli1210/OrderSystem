using OrderSystem.Infrastructure.Messaging.Messages;

namespace OrderSystem.AzureFunctions.Services;

public interface IOrderStatusEmailProcessor
{
    OrderStatusEmailProcessor? BuildEmail(OrderStatusChangedMessage message);
}
