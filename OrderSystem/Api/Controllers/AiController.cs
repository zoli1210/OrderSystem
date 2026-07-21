using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderSystem.Application.AI.Documents;
using OrderSystem.Application.AI.Knowledge;
using OrderSystem.Application.AI.OrderExplanation;
using OrderSystem.Authentication.Authorization;
using OrderSystem.Modules.AI.DTOs;

namespace OrderSystem.Api.Controllers;

[ApiController]
[Route("ai")]
[Authorize]
public class AiController : ControllerBase
{
    private readonly IAiKnowledgeService _aiKnowledgeService;
    private readonly IKnowledgeDocumentIngestionService _ingestionService;
    private readonly IOrderExplanationService _orderExplanationService;
    private readonly ILogger<AiController> _logger;

    public AiController(
        IAiKnowledgeService aiKnowledgeService,
        IKnowledgeDocumentIngestionService ingestionService,
        IOrderExplanationService orderExplanationService,
        ILogger<AiController> logger
    )
    {
        _aiKnowledgeService = aiKnowledgeService;
        _ingestionService = ingestionService;
        _orderExplanationService = orderExplanationService;
        _logger = logger;
    }

    [HttpPost("knowledge/ask")]
    public async Task<IActionResult> AskKnowledge(
        AskKnowledgeRequest request,
        CancellationToken cancellationToken
    )
    {
        var response = await _aiKnowledgeService.AskAsync(request, cancellationToken);

        return Ok(response);
    }

    [HttpPost("knowledge/documents")]
    [Authorize(Roles = AuthRoles.Admin)]
    public async Task<IActionResult> CreateKnowledgeDocument(
        CreateKnowledgeDocumentRequest request,
        CancellationToken cancellationToken
    )
    {
        await _ingestionService.CreateAsync(request, cancellationToken);

        return Accepted(new { message = "Knowledge document created." });
    }

    [HttpPost("orders/{orderId:guid}/explain")]
    public async Task<IActionResult> ExplainOrder(
        Guid orderId,
        ExplainOrderRequest request,
        CancellationToken cancellationToken
    )
    {
        _logger.LogInformation("AI order explanation requested. OrderId: {OrderId}", orderId);

        var response = await _orderExplanationService.ExplainAsync(
            orderId,
            request,
            cancellationToken
        );

        return Ok(response);
    }
}
