namespace OrderSystem.Modules.AI.DTOs;

public class CreateKnowledgeDocumentRequest
{
    public string Source { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Url { get; set; }

    public string Content { get; set; } = string.Empty;
}
