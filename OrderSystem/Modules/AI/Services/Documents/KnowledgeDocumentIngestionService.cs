using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using OrderSystem.Infrastructure.Options;
using OrderSystem.Modules.AI.DTOs;
using OrderSystem.Modules.AI.Services.Knowledge;

namespace OrderSystem.Modules.AI.Services.Documents;

public class KnowledgeDocumentIngestionService : IKnowledgeDocumentIngestionService
{
    private readonly HttpClient _httpClient;
    private readonly IEmbeddingService _embeddingService;
    private readonly SupabaseOptions _supabaseOptions;

    public KnowledgeDocumentIngestionService(
        HttpClient httpClient,
        IEmbeddingService embeddingService,
        IOptions<SupabaseOptions> supabaseOptions
    )
    {
        _httpClient = httpClient;
        _embeddingService = embeddingService;
        _supabaseOptions = supabaseOptions.Value;
    }

    public async Task CreateAsync(
        CreateKnowledgeDocumentRequest request,
        CancellationToken cancellationToken
    )
    {
        var embedding = await _embeddingService.CreateEmbeddingAsync(
            request.Content,
            cancellationToken
        );

        var payload = new
        {
            source = request.Source,
            title = request.Title,
            url = request.Url,
            content = request.Content,
            embedding = FormatVector(embedding),
        };

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_supabaseOptions.Url}/rest/v1/knowledge_documents"
        );

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _supabaseOptions.SecretKey
        );

        httpRequest.Headers.Add("apikey", _supabaseOptions.SecretKey);
        httpRequest.Headers.Add("Prefer", "return=minimal");

        httpRequest.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Supabase document insert failed. StatusCode: {response.StatusCode}, Body: {responseBody}"
            );
        }
    }

    private static string FormatVector(IReadOnlyList<float> embedding)
    {
        return $"[{string.Join(",", embedding.Select(value => value.ToString(System.Globalization.CultureInfo.InvariantCulture)))}]";
    }
}
