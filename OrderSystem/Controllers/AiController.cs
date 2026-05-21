using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderSystem.Modules.AI.DTOs;
using OrderSystem.Modules.AI.Services.Documents;
using OrderSystem.Modules.AI.Services.Knowledge;
using OrderSystem.Modules.Auth;

namespace OrderSystem.Controllers;

[ApiController]
[Route("ai")]
[Authorize]
public class AiController : ControllerBase
{
    private readonly IAiKnowledgeService _aiKnowledgeService;
    private readonly IKnowledgeDocumentIngestionService _ingestionService;

    public AiController(
        IAiKnowledgeService aiKnowledgeService,
        IKnowledgeDocumentIngestionService ingestionService
    )
    {
        _aiKnowledgeService = aiKnowledgeService;
        _ingestionService = ingestionService;
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
}
