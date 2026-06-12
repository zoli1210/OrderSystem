using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderSystem.Infrastructure.Messaging;
using OrderSystem.Modules.Auth;

namespace OrderSystem.Api.Controllers;

[ApiController]
[Route("dead-letters")]
[Authorize(Roles = AuthRoles.Admin)]
public class DeadLettersController : ControllerBase
{
    private readonly IDeadLetterService _deadLetterService;

    public DeadLettersController(IDeadLetterService deadLetterService)
    {
        _deadLetterService = deadLetterService;
    }

    [HttpGet("orders")]
    public async Task<IActionResult> GetOrderDeadLetters(CancellationToken cancellationToken)
    {
        var messages = await _deadLetterService.GetDeadLettersAsync(
            "order-created",
            cancellationToken
        );

        return Ok(messages);
    }

    [HttpGet("emails")]
    public async Task<IActionResult> GetEmailDeadLetters(CancellationToken cancellationToken)
    {
        var messages = await _deadLetterService.GetDeadLettersAsync(
            "email-notification",
            cancellationToken
        );

        return Ok(messages);
    }

    [HttpPost("orders/{sequenceNumber:long}/retry")]
    public async Task<IActionResult> RetryOrderDeadLetter(
        long sequenceNumber,
        CancellationToken cancellationToken
    )
    {
        var retried = await _deadLetterService.RetryDeadLetterAsync(
            "order-created",
            sequenceNumber,
            cancellationToken
        );

        if (!retried)
        {
            return NotFound(
                new { message = "Order dead-letter message not found.", sequenceNumber }
            );
        }

        return Accepted(new { message = "Order dead-letter message requeued.", sequenceNumber });
    }

    [HttpPost("emails/{sequenceNumber:long}/retry")]
    public async Task<IActionResult> RetryEmailDeadLetter(
        long sequenceNumber,
        CancellationToken cancellationToken
    )
    {
        var retried = await _deadLetterService.RetryDeadLetterAsync(
            "email-notification",
            sequenceNumber,
            cancellationToken
        );

        if (!retried)
        {
            return NotFound(
                new { message = "Email dead-letter message not found.", sequenceNumber }
            );
        }

        return Accepted(new { message = "Email dead-letter message requeued.", sequenceNumber });
    }
}
