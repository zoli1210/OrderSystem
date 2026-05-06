using OrderSystem.Domain.Entities;
using OrderSystem.Domain.Enums;
using OrderSystem.Infrastructure.Messaging;
using OrderSystem.Infrastructure.Messaging.Messages;
using OrderSystem.Infrastructure.Persistence.Repositories;
using OrderSystem.Modules.Orders.DTOs;
using OrderSystem.Modules.Payments.Services;
using OrderSystem.Shared.Pagination;

namespace OrderSystem.Modules.Orders.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentService _paymentService;
    private readonly IOrderMessageSender _orderMessageSender;

    public OrderService(
        IOrderRepository orderRepository,
        IPaymentService paymentService,
        IOrderMessageSender orderMessageSender
    )
    {
        _orderRepository = orderRepository;
        _paymentService = paymentService;
        _orderMessageSender = orderMessageSender;
    }

    public async Task<OrderResponse> CreateAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken
    )
    {
        var order = new Order(
            request.CustomerName,
            request.CustomerEmail,
            request.TotalAmount,
            request.Currency,
            request.Description
        );

        await _orderRepository.AddAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        await _orderMessageSender.SendOrderCreatedAsync(
            new OrderCreatedMessage
            {
                OrderId = order.Id,
                TotalAmount = order.TotalAmount,
                CustomerEmail = order.CustomerEmail,
            },
            cancellationToken
        );

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
            CustomerEmail = order.CustomerEmail,
            TotalAmount = order.TotalAmount,
            Currency = order.Currency,
            Description = order.Description,
            Status = order.Status,
            CreatedAtUtc = order.CreatedAtUtc,
            EmailSentAtUtc = order.EmailSentAtUtc,
        };
    }

    public async Task<PagedResponse<OrderResponse>> GetAllAsync(
        OrderStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken
    )
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var (items, totalCount) = await _orderRepository.GetAllAsync(
            status,
            page,
            pageSize,
            cancellationToken
        );

        return new PagedResponse<OrderResponse>
        {
            Items = items.Select(MapToResponse).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
        };
    }

    public async Task<OrderResponse> CancelAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);

        if (order is null)
        {
            throw new NotFoundException("Order not found");
        }

        order.Cancel();

        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        return MapToResponse(order);
    }
}
