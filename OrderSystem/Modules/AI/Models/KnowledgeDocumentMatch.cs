namespace OrderSystem.Modules.AI.Models;

public class KnowledgeDocumentMatch
{
    public Guid Id { get; set; }

    public string Source { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Url { get; set; }

    public string Content { get; set; } = string.Empty;

    public double Similarity { get; set; }
}
