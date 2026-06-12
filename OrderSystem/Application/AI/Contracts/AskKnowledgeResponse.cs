namespace OrderSystem.Modules.AI.DTOs;

public class AskKnowledgeResponse
{
    public string Answer { get; set; } = string.Empty;

    public IReadOnlyList<KnowledgeSourceResponse> Sources { get; set; } = [];
}
