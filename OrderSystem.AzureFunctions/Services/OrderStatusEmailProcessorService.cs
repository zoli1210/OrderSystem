using OrderSystem.Domain.Enums;
using OrderSystem.Infrastructure.Messaging.Messages;

namespace OrderSystem.AzureFunctions.Services;

public class OrderStatusEmailProcessorService : IOrderStatusEmailProcessor
{
    public OrderStatusEmailProcessor? BuildEmail(OrderStatusChangedMessage message)
    {
        var orderReference = message.OrderId.ToString("N")[..8].ToUpperInvariant();

        return message.CurrentStatus switch
        {
            OrderStatus.Preparing => new OrderStatusEmailProcessor
            {
                Subject = $"Order #{orderReference} is being prepared",
                EmailType = "Preparing",
                Body = $"""
                    Hi,

                    Your order is now being prepared.

                    Order ID: {message.OrderId}

                    We will notify you again when your order is ready for shipment.
                    """,
            },

            OrderStatus.ReadyForShipment => new OrderStatusEmailProcessor
            {
                Subject = $"Order #{orderReference} is ready for shipment",
                EmailType = "ReadyForShipment",
                Body = $"""
                    Hi,

                    Your order has been packed and is ready to be handed over to the courier.

                    Order ID: {message.OrderId}

                    We will notify you again when the package has been shipped.
                    """,
            },

            OrderStatus.Shipped => new OrderStatusEmailProcessor
            {
                Subject = $"Order #{orderReference} has been shipped",
                EmailType = "Shipped",
                Body = $"""
                    Hi,

                    Your order has been handed over to the courier.

                    Order ID: {message.OrderId}
                    Tracking number: {message.TrackingNumber}

                    You can use the tracking number to follow the delivery status.
                    """,
            },

            OrderStatus.Delivered => new OrderStatusEmailProcessor
            {
                Subject = $"Order #{orderReference} has been delivered",
                EmailType = "Delivered",
                Body = $"""
                    Hi,

                    Your order has been delivered successfully.

                    Order ID: {message.OrderId}

                    Thank you for your order.
                    """,
            },

            _ => null,
        };
    }
}
