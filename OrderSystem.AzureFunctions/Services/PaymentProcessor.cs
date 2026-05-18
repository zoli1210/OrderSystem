using System.Text.Json;
using Microsoft.Extensions.Logging;
using OrderSystem.Domain.Entities;
using OrderSystem.Domain.Enums;
using OrderSystem.Infrastructure.Messaging;
using OrderSystem.Infrastructure.Messaging.Messages;
using OrderSystem.Infrastructure.Persistence.Repositories;
using OrderSystem.Modules.Payments.Services;

namespace OrderSystem.AzureFunctions.Services;

public class PaymentProcessor : IPaymentProcessor
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentService _paymentService;
    private readonly IEmailMessageSender _emailMessageSender;
    private readonly ILogger<PaymentProcessor> _logger;
    private const string SystemUserId = "system";
    private readonly IOrderStatusHistoryRepository _statusHistoryRepository;

    public PaymentProcessor(
        IOrderRepository orderRepository,
        IPaymentService paymentService,
        IEmailMessageSender emailMessageSender,
        IOrderStatusHistoryRepository statusHistoryRepository,
        ILogger<PaymentProcessor> logger
    )
    {
        _orderRepository = orderRepository;
        _paymentService = paymentService;
        _emailMessageSender = emailMessageSender;
        _statusHistoryRepository = statusHistoryRepository;
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

        if (order.Status != OrderStatus.Pending)
        {
            _logger.LogWarning(
                "Payment processing skipped. OrderId: {OrderId}, CurrentStatus: {Status}",
                order.Id,
                order.Status
            );

            return;
        }

        var previousStatus = order.Status;

        order.StartPaymentProcessing(SystemUserId);

        await _statusHistoryRepository.AddAsync(
            new OrderStatusHistory(order.Id, previousStatus, order.Status, SystemUserId),
            cancellationToken
        );

        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        try
        {
            var paymentSuccessful = await _paymentService.ProcessPaymentAsync(
                order.Id,
                order.TotalAmount,
                cancellationToken
            );

            var previousPaymentStatus = order.Status;

            if (paymentSuccessful)
            {
                order.MarkAsPaid(SystemUserId);

                await _emailMessageSender.SendEmailNotificationAsync(
                    new EmailNotificationMessage
                    {
                        OrderId = order.Id,
                        CustomerEmail = order.CustomerEmail,
                        Subject = "Order payment successful",
                        Body = $"Your order {order.Id} has been paid successfully.",
                    },
                    cancellationToken
                );

                _logger.LogInformation(
                    "Payment processed successfully. OrderId: {OrderId}, Status: {Status}",
                    order.Id,
                    order.Status
                );
            }
            else
            {
                order.MarkPaymentAsFailed(SystemUserId);
            }

            await _statusHistoryRepository.AddAsync(
                new OrderStatusHistory(order.Id, previousPaymentStatus, order.Status, SystemUserId),
                cancellationToken
            );

            await _orderRepository.UpdateAsync(order, cancellationToken);
            await _orderRepository.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            var previousFailedStatus = order.Status;

            order.MarkPaymentAsFailed(SystemUserId);

            await _statusHistoryRepository.AddAsync(
                new OrderStatusHistory(order.Id, previousFailedStatus, order.Status, SystemUserId),
                cancellationToken
            );

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
