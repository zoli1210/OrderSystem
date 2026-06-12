namespace OrderSystem.Modules.AI.DTOs;

public class AskKnowledgeRequest
{
    public string Question { get; set; } = string.Empty;

    public int? MatchCount { get; set; }
}
