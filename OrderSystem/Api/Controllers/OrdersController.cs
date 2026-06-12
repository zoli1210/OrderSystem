using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderSystem.Modules.Orders.DTOs;
using OrderSystem.Modules.Orders.Services;

namespace OrderSystem.Api.Controllers;

[ApiController]
[Route("orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IOrderStatusService _orderStatusService;

    public OrdersController(IOrderService orderService, IOrderStatusService orderStatusService)
    {
        _orderService = orderService;
        _orderStatusService = orderStatusService;
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> Create(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken
    )
    {
        var response = await _orderService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var response = await _orderService.GetByIdAsync(id, cancellationToken);

        if (response is null)
        {
            return NotFound();
        }

        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] GetOrdersQuery query,
        CancellationToken cancellationToken
    )
    {
        var orders = await _orderService.GetAllAsync(query, cancellationToken);

        return Ok(orders);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(
        Guid id,
        CancelOrderRequest request,
        CancellationToken cancellationToken
    )
    {
        var response = await _orderService.CancelAsync(id, request, cancellationToken);

        return Ok(response);
    }

    [HttpGet("{id:guid}/status-history")]
    public async Task<IActionResult> GetStatusHistory(Guid id, CancellationToken cancellationToken)
    {
        var history = await _orderService.GetStatusHistoryAsync(id, cancellationToken);

        return Ok(history);
    }

    [HttpPost("{id:guid}/retry-payment")]
    public async Task<IActionResult> RetryPayment(Guid id, CancellationToken cancellationToken)
    {
        var response = await _orderService.RetryPaymentAsync(id, cancellationToken);

        return Accepted(response);
    }

    [HttpGet("user-history")]
    public async Task<IActionResult> GetUserHistory(CancellationToken cancellationToken)
    {
        var history = await _orderService.GetUserHistoryAsync(cancellationToken);

        return Ok(history);
    }

    [HttpGet("{id:guid}/email-history")]
    public async Task<IActionResult> GetEmailHistory(Guid id, CancellationToken cancellationToken)
    {
        var history = await _orderService.GetEmailHistoryAsync(id, cancellationToken);

        return Ok(history);
    }

    [HttpPatch("{orderId:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid orderId,
        UpdateOrderStatusRequest request,
        CancellationToken cancellationToken
    )
    {
        var response = await _orderStatusService.UpdateStatusAsync(
            orderId,
            request,
            cancellationToken
        );

        return Ok(response);
    }
}
