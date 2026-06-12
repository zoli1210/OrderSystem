using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using OrderSystem.Infrastructure.Options;
using OrderSystem.Modules.AI.Models;

namespace OrderSystem.Application.AI.Sources;

public class SupabaseKnowledgeSourceRepository : IKnowledgeSourceRepository
{
    private readonly HttpClient _httpClient;
    private readonly SupabaseOptions _supabaseOptions;

    public SupabaseKnowledgeSourceRepository(
        HttpClient httpClient,
        IOptions<SupabaseOptions> supabaseOptions
    )
    {
        _httpClient = httpClient;
        _supabaseOptions = supabaseOptions.Value;
    }

    public async Task<KnowledgeSource?> GetByIdAsync(
        Guid sourceId,
        CancellationToken cancellationToken
    )
    {
        using var request = CreateRequest(
            HttpMethod.Get,
            $"{_supabaseOptions.Url}/rest/v1/knowledge_sources?id=eq.{sourceId}&select=*"
        );

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Supabase knowledge source query failed. StatusCode: {response.StatusCode}, Body: {responseBody}"
            );
        }

        var sources =
            JsonSerializer.Deserialize<List<KnowledgeSource>>(
                responseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? [];

        return sources.FirstOrDefault();
    }

    public Task MarkAsSucceededAsync(Guid sourceId, CancellationToken cancellationToken)
    {
        return UpdateStatusAsync(sourceId, "Succeeded", null, cancellationToken);
    }

    public Task MarkAsFailedAsync(
        Guid sourceId,
        string errorMessage,
        CancellationToken cancellationToken
    )
    {
        return UpdateStatusAsync(sourceId, "Failed", errorMessage, cancellationToken);
    }

    private async Task UpdateStatusAsync(
        Guid sourceId,
        string status,
        string? errorMessage,
        CancellationToken cancellationToken
    )
    {
        var payload = new
        {
            last_ingested_at = DateTime.UtcNow,
            last_ingestion_status = status,
            last_error = errorMessage,
            updated_at = DateTime.UtcNow,
        };

        using var request = CreateRequest(
            HttpMethod.Patch,
            $"{_supabaseOptions.Url}/rest/v1/knowledge_sources?id=eq.{sourceId}"
        );

        request.Headers.Add("Prefer", "return=minimal");

        request.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Supabase knowledge source status update failed. StatusCode: {response.StatusCode}, Body: {responseBody}"
            );
        }
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string url)
    {
        var request = new HttpRequestMessage(method, url);

        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _supabaseOptions.SecretKey
        );

        request.Headers.Add("apikey", _supabaseOptions.SecretKey);

        return request;
    }
}
