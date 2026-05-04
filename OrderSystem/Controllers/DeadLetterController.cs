using Microsoft.AspNetCore.Mvc;
using OrderSystem.Infrastructure.Messaging;

namespace OrderSystem.Controllers;

[ApiController]
[Route("dead-letters")]
public class DeadLettersController : ControllerBase
{
    private readonly InMemoryDeadLetterQueue _deadLetterQueue;
    private readonly InMemoryOrderMessageQueue _orderQueue;

    public DeadLettersController(
        InMemoryDeadLetterQueue deadLetterQueue,
        InMemoryOrderMessageQueue orderQueue
    )
    {
        _deadLetterQueue = deadLetterQueue;
        _orderQueue = orderQueue;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var messages = _deadLetterQueue.GetAll();

        return Ok(messages);
    }

    [HttpPost("{orderId:guid}/retry")]
    public async Task<IActionResult> Retry(Guid orderId, CancellationToken cancellationToken)
    {
        var message = _deadLetterQueue.GetAll().FirstOrDefault(x => x.OrderId == orderId);

        if (message is null)
        {
            return NotFound();
        }

        message.RetryCount = 0;

        await _orderQueue.EnqueueAsync(message, cancellationToken);

        return Accepted(new { message = "Dead-letter message requeued.", orderId });
    }
}
