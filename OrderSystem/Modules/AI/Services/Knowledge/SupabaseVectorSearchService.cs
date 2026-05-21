using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using OrderSystem.Infrastructure.Options;
using OrderSystem.Modules.AI.Models;

namespace OrderSystem.Modules.AI.Services.Knowledge;

public class SupabaseVectorSearchService : IVectorSearchService
{
    private readonly HttpClient _httpClient;
    private readonly SupabaseOptions _options;

    public SupabaseVectorSearchService(HttpClient httpClient, IOptions<SupabaseOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<KnowledgeDocumentMatch>> SearchAsync(
        IReadOnlyList<float> embedding,
        int matchCount,
        CancellationToken cancellationToken
    )
    {
        var requestBody = new { query_embedding = embedding, match_count = matchCount };

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_options.Url}/rest/v1/rpc/match_knowledge_documents"
        );

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.SecretKey);

        request.Headers.Add("apikey", _options.SecretKey);

        request.Content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Supabase vector search failed. StatusCode: {response.StatusCode}, Body: {json}"
            );
        }

        return JsonSerializer.Deserialize<List<KnowledgeDocumentMatch>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? [];
    }
}
