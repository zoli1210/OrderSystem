using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using OrderSystem.Infrastructure.Options;
using OrderSystem.Modules.AI.DTOs;

namespace OrderSystem.Application.AI.Sources;

public class KnowledgeSourceSeedService : IKnowledgeSourceSeedService
{
    private const string SeedFilePath =
        "Infrastructure/Supabase/KnowledgeSources/knowledge-sources.json";

    private readonly HttpClient _httpClient;
    private readonly SupabaseOptions _supabaseOptions;
    private readonly IWebHostEnvironment _environment;

    public KnowledgeSourceSeedService(
        HttpClient httpClient,
        IOptions<SupabaseOptions> supabaseOptions,
        IWebHostEnvironment environment
    )
    {
        _httpClient = httpClient;
        _supabaseOptions = supabaseOptions.Value;
        _environment = environment;
    }

    public async Task<KnowledgeSourceSeedResponse> SeedAsync(CancellationToken cancellationToken)
    {
        var filePath = Path.Combine(_environment.ContentRootPath, SeedFilePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Knowledge source seed file was not found.", filePath);
        }

        var json = await File.ReadAllTextAsync(filePath, cancellationToken);

        var seedItems =
            JsonSerializer.Deserialize<List<KnowledgeSourceSeedItem>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? [];

        var created = 0;
        var skipped = 0;

        foreach (var item in seedItems)
        {
            if (string.IsNullOrWhiteSpace(item.Name))
            {
                skipped++;
                continue;
            }

            var exists = await SourceExistsAsync(item, cancellationToken);

            if (exists)
            {
                skipped++;
                continue;
            }

            await CreateSourceAsync(item, cancellationToken);

            created++;
        }

        return new KnowledgeSourceSeedResponse
        {
            Total = seedItems.Count,
            Created = created,
            Skipped = skipped,
        };
    }

    private async Task<bool> SourceExistsAsync(
        KnowledgeSourceSeedItem item,
        CancellationToken cancellationToken
    )
    {
        var queryUrl = item.Url is not null
            ? $"{_supabaseOptions.Url}/rest/v1/knowledge_sources?url=eq.{Uri.EscapeDataString(item.Url)}&select=id"
            : $"{_supabaseOptions.Url}/rest/v1/knowledge_sources?name=eq.{Uri.EscapeDataString(item.Name)}&select=id";

        using var request = CreateRequest(HttpMethod.Get, queryUrl);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Supabase knowledge source existence check failed. StatusCode: {response.StatusCode}, Body: {responseBody}"
            );
        }

        using var document = JsonDocument.Parse(responseBody);

        return document.RootElement.ValueKind == JsonValueKind.Array
            && document.RootElement.GetArrayLength() > 0;
    }

    private async Task CreateSourceAsync(
        KnowledgeSourceSeedItem item,
        CancellationToken cancellationToken
    )
    {
        var payload = new
        {
            name = item.Name,
            url = item.Url,
            source_type = item.SourceType,
            refresh_interval_hours = item.RefreshIntervalHours,
        };

        using var request = CreateRequest(
            HttpMethod.Post,
            $"{_supabaseOptions.Url}/rest/v1/knowledge_sources"
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
                $"Supabase knowledge source insert failed. StatusCode: {response.StatusCode}, Body: {responseBody}"
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
