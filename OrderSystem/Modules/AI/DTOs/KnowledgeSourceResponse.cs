namespace OrderSystem.Modules.AI.DTOs;

public class KnowledgeSourceResponse
{
    public string Title { get; set; } = string.Empty;

    public string? Url { get; set; }

    public double Similarity { get; set; }
}
