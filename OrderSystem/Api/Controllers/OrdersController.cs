using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderSystem.Application.Orders.Contracts.Queries;
using OrderSystem.Application.Orders.Contracts.Requests;
using OrderSystem.Application.Orders.Contracts.Responses;
using OrderSystem.Application.Orders.Services;
using OrderSystem.Authentication.Application;
using OrderSystem.Authentication.Authorization;

namespace OrderSystem.Api.Controllers;

[ApiController]
[Route("orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IOrderStatusService _orderStatusService;
    private readonly ICurrentUserService _currentUserService;

    public OrdersController(
        IOrderService orderService,
        IOrderStatusService orderStatusService,
        ICurrentUserService currentUserService
    )
    {
        _orderService = orderService;
        _orderStatusService = orderStatusService;
        _currentUserService = currentUserService;
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> Create(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken
    )
    {
        var response = await _orderService.CreateAsync(
            request,
            GetRequiredUserId(),
            cancellationToken
        );

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var response = await _orderService.GetByIdAsync(
            id,
            _currentUserService.UserId,
            _currentUserService.IsAdmin,
            cancellationToken
        );

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
        var orders = await _orderService.GetAllAsync(
            query,
            _currentUserService.UserId,
            _currentUserService.IsAdmin,
            cancellationToken
        );

        return Ok(orders);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(
        Guid id,
        [FromBody] CancelOrderRequest request,
        CancellationToken cancellationToken
    )
    {
        var response = await _orderService.CancelAsync(
            id,
            request,
            GetRequiredUserId(),
            _currentUserService.IsAdmin,
            cancellationToken
        );

        return Ok(response);
    }

    [HttpGet("{id:guid}/status-history")]
    public async Task<IActionResult> GetStatusHistory(Guid id, CancellationToken cancellationToken)
    {
        var history = await _orderService.GetStatusHistoryAsync(
            id,
            _currentUserService.UserId,
            _currentUserService.IsAdmin,
            cancellationToken
        );

        return Ok(history);
    }

    [HttpPost("{id:guid}/retry-payment")]
    public async Task<IActionResult> RetryPayment(Guid id, CancellationToken cancellationToken)
    {
        var response = await _orderService.RetryPaymentAsync(
            id,
            GetRequiredUserId(),
            _currentUserService.IsAdmin,
            cancellationToken
        );

        return Accepted(response);
    }

    [HttpGet("user-history")]
    public async Task<IActionResult> GetUserHistory(CancellationToken cancellationToken)
    {
        var history = await _orderService.GetUserHistoryAsync(
            GetRequiredUserId(),
            cancellationToken
        );

        return Ok(history);
    }

    [HttpGet("{id:guid}/email-history")]
    public async Task<IActionResult> GetEmailHistory(Guid id, CancellationToken cancellationToken)
    {
        var history = await _orderService.GetEmailHistoryAsync(
            id,
            _currentUserService.UserId,
            _currentUserService.IsAdmin,
            cancellationToken
        );

        return Ok(history);
    }

    [HttpPatch("{orderId:guid}/status")]
    [Authorize(Roles = AuthRoles.Admin)]
    public async Task<IActionResult> UpdateStatus(
        Guid orderId,
        [FromBody] UpdateOrderStatusRequest request,
        CancellationToken cancellationToken
    )
    {
        var response = await _orderStatusService.UpdateStatusAsync(
            orderId,
            request,
            GetRequiredUserId(),
            _currentUserService.IsAdmin,
            cancellationToken
        );

        return Ok(response);
    }

    [HttpGet("summary")]
    [Authorize(Roles = AuthRoles.Admin)]
    public async Task<ActionResult<OrderSummaryResponse>> GetSummary(
        CancellationToken cancellationToken
    )
    {
        var response = await _orderService.GetSummaryAsync(
            _currentUserService.IsAdmin,
            cancellationToken
        );

        return Ok(response);
    }

    private string GetRequiredUserId()
    {
        var currentUserId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        return currentUserId;
    }
}
