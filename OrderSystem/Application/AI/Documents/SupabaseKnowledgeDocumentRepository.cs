using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using OrderSystem.Infrastructure.Options;
using OrderSystem.Modules.AI.Models;

namespace OrderSystem.Application.AI.Documents;

public class SupabaseKnowledgeDocumentRepository : IKnowledgeDocumentRepository
{
    private readonly HttpClient _httpClient;
    private readonly SupabaseOptions _supabaseOptions;

    public SupabaseKnowledgeDocumentRepository(
        HttpClient httpClient,
        IOptions<SupabaseOptions> supabaseOptions
    )
    {
        _httpClient = httpClient;
        _supabaseOptions = supabaseOptions.Value;
    }

    public async Task DeleteBySourceIdAsync(Guid sourceId, CancellationToken cancellationToken)
    {
        using var request = CreateRequest(
            HttpMethod.Delete,
            $"{_supabaseOptions.Url}/rest/v1/knowledge_documents?source_id=eq.{sourceId}"
        );

        request.Headers.Add("Prefer", "return=minimal");

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Supabase knowledge document cleanup failed. StatusCode: {response.StatusCode}, Body: {responseBody}"
            );
        }
    }

    public async Task InsertAsync(
        KnowledgeSource source,
        string content,
        IReadOnlyList<float> embedding,
        CancellationToken cancellationToken
    )
    {
        var payload = new
        {
            source_id = source.Id,
            source = source.SourceType,
            title = source.Name,
            url = source.Url,
            content,
            embedding = FormatVector(embedding),
        };

        using var request = CreateRequest(
            HttpMethod.Post,
            $"{_supabaseOptions.Url}/rest/v1/knowledge_documents"
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
                $"Supabase knowledge document insert failed. StatusCode: {response.StatusCode}, Body: {responseBody}"
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

    private static string FormatVector(IReadOnlyList<float> embedding)
    {
        return $"[{string.Join(
            ",",
            embedding.Select(value => value.ToString(CultureInfo.InvariantCulture)))}]";
    }
}
