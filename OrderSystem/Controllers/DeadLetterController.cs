using Microsoft.AspNetCore.Mvc;
using OrderSystem.Infrastructure.Messaging;

namespace OrderSystem.Controllers;

[ApiController]
[Route("dead-letters")]
public class DeadLettersController : ControllerBase
{
    private readonly IDeadLetterService _deadLetterService;

    public DeadLettersController(IDeadLetterService deadLetterService)
    {
        _deadLetterService = deadLetterService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var messages = await _deadLetterService.GetDeadLettersAsync(cancellationToken);

        return Ok(messages);
    }

    [HttpPost("{sequenceNumber:long}/retry")]
    public async Task<IActionResult> Retry(long sequenceNumber, CancellationToken cancellationToken)
    {
        var retried = await _deadLetterService.RetryDeadLetterAsync(
            sequenceNumber,
            cancellationToken
        );

        if (!retried)
        {
            return NotFound(new { message = "Dead-letter message not found.", sequenceNumber });
        }

        return Accepted(new { message = "Dead-letter message requeued.", sequenceNumber });
    }
}
