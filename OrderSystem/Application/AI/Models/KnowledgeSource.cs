using System.Text.Json.Serialization;

namespace OrderSystem.Modules.AI.Models;

public class KnowledgeSource
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Url { get; set; }

    [JsonPropertyName("source_type")]
    public string SourceType { get; set; } = string.Empty;

    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; }

    [JsonPropertyName("refresh_interval_hours")]
    public int? RefreshIntervalHours { get; set; }

    [JsonPropertyName("last_ingested_at")]
    public DateTime? LastIngestedAt { get; set; }

    [JsonPropertyName("last_ingestion_status")]
    public string? LastIngestionStatus { get; set; }

    [JsonPropertyName("last_error")]
    public string? LastError { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
