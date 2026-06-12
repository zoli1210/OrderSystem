using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using OrderSystem.Infrastructure.Options;
using OrderSystem.Modules.AI.Models;

namespace OrderSystem.Application.AI.Sources;

public class KnowledgeSourceQueryService : IKnowledgeSourceQueryService
{
    private readonly HttpClient _httpClient;
    private readonly SupabaseOptions _supabaseOptions;

    public KnowledgeSourceQueryService(
        HttpClient httpClient,
        IOptions<SupabaseOptions> supabaseOptions
    )
    {
        _httpClient = httpClient;
        _supabaseOptions = supabaseOptions.Value;
    }

    public async Task<IReadOnlyList<KnowledgeSource>> GetSourcesDueForIngestionAsync(
        CancellationToken cancellationToken
    )
    {
        using var request = CreateRequest(
            HttpMethod.Get,
            $"{_supabaseOptions.Url}/rest/v1/knowledge_sources?is_active=eq.true&select=*"
        );

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Supabase knowledge sources query failed. StatusCode: {response.StatusCode}, Body: {responseBody}"
            );
        }

        var sources =
            JsonSerializer.Deserialize<List<KnowledgeSource>>(
                responseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? [];

        var now = DateTime.UtcNow;

        return sources
            .Where(source => source.IsActive)
            .Where(source => !string.IsNullOrWhiteSpace(source.Url))
            .Where(source =>
                source.LastIngestedAt is null
                || source.RefreshIntervalHours is null
                || source.LastIngestedAt.Value.AddHours(source.RefreshIntervalHours.Value) <= now
            )
            .ToList();
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
