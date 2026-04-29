using Microsoft.AspNetCore.Mvc;
using OrderSystem.Infrastructure.Messaging;

namespace OrderSystem.Controllers;

[ApiController]
[Route("dead-letters")]
public class DeadLettersController : ControllerBase
{
    private readonly InMemoryDeadLetterQueue _deadLetterQueue;

    public DeadLettersController(InMemoryDeadLetterQueue deadLetterQueue)
    {
        _deadLetterQueue = deadLetterQueue;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var messages = _deadLetterQueue.GetAll();

        return Ok(messages);
    }
}
