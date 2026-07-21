using OrderSystem.Infrastructure.Messaging.Messages;

namespace OrderSystem.AzureFunctions.Orders.Services;

public interface IOrderStatusEmailProcessor
{
    OrderStatusEmailProcessor? BuildEmail(OrderStatusChangedMessage message);
}
