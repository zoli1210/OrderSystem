namespace OrderSystem.Modules.AI.DTOs;

public class KnowledgeSourceSeedItem
{
    public string Name { get; set; } = string.Empty;

    public string? Url { get; set; }

    public string SourceType { get; set; } = "Url";

    public int? RefreshIntervalHours { get; set; }
}
