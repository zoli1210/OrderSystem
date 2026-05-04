using OrderSystem.Domain.Entities;
using OrderSystem.Infrastructure.Messaging;
using OrderSystem.Infrastructure.Messaging.Messages;
using OrderSystem.Infrastructure.Persistence.Repositories;
using OrderSystem.Modules.Orders.DTOs;
using OrderSystem.Modules.Payments.Services;

namespace OrderSystem.Modules.Orders.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentService _paymentService;
    private readonly IOrderMessageSender _orderMessageSender;

    public OrderService(IOrderRepository orderRepository, IPaymentService paymentService, IOrderMessageSender orderMessageSender)
    {
        _orderRepository = orderRepository;
        _paymentService = paymentService;
        _orderMessageSender = orderMessageSender;
    }

    public async Task<OrderResponse> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var order = new Order(
            request.CustomerName,
            request.TotalAmount);

        await _orderRepository.AddAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        //order.SetPaymentProcessing();

        //await _orderRepository.UpdateAsync(order, cancellationToken);
        //await _orderRepository.SaveChangesAsync(cancellationToken);

        //var paymentSuccessful = await _paymentService.ProcessPaymentAsync(
        //    order.Id,
        //    order.TotalAmount,
        //    cancellationToken);

        //if (paymentSuccessful)
        //{
        //    order.SetPaid();
        //}
        //else
        //{
        //    order.SetPaymentFailed();
        //}

        //await _orderRepository.UpdateAsync(order, cancellationToken);
        //await _orderRepository.SaveChangesAsync(cancellationToken);

        await _orderMessageSender.SendOrderCreatedAsync( new OrderCreatedMessage
        {
            OrderId = order.Id,
            TotalAmount = order.TotalAmount,
            CustomerEmail = request.CustomerEmail
        },
        cancellationToken);

        return MapToResponse(order);
    }

    public async Task<OrderResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);

        if (order is null)
        {
            throw new NotFoundException("Order not found");
        }

        return MapToResponse(order);
    }

    private static OrderResponse MapToResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            CustomerName = order.CustomerName,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            CreatedAtUtc = order.CreatedAtUtc
        };
    }
}