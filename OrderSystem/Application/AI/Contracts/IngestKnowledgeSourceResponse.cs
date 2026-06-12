namespace OrderSystem.Modules.AI.DTOs;

public class IngestKnowledgeSourceResponse
{
    public Guid SourceId { get; set; }

    public string SourceName { get; set; } = string.Empty;

    public int ChunkCount { get; set; }

    public string Status { get; set; } = string.Empty;
}
