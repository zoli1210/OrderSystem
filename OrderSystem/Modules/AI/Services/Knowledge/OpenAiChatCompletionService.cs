using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using OrderSystem.Infrastructure.Options;
using OrderSystem.Modules.AI.Models;

namespace OrderSystem.Modules.AI.Services.Knowledge;

public class OpenAiChatCompletionService : IChatCompletionService
{
    private readonly HttpClient _httpClient;
    private readonly OpenAiOptions _options;

    public OpenAiChatCompletionService(HttpClient httpClient, IOptions<OpenAiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<string> GenerateAnswerAsync(
        string question,
        IReadOnlyList<KnowledgeDocumentMatch> contextDocuments,
        CancellationToken cancellationToken
    )
    {
        var context = string.Join(
            "\n\n---\n\n",
            contextDocuments.Select(
                (document, index) =>
                    $"Source {index + 1}: {document.Title}\nUrl: {document.Url}\nContent:\n{document.Content}"
            )
        );

        var input = $"""
            You are a technical support assistant for a .NET and Azure based order processing system.

            Answer the user's question using only the provided context.
            If the answer is not available in the context, say that the available documentation does not contain enough information.

            User question:
            {question}

            Retrieved context:
            {context}
            """;

        var requestBody = new { model = _options.ChatModel, input };

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://api.openai.com/v1/responses"
        );

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

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
                $"OpenAI chat request failed. StatusCode: {response.StatusCode}, Body: {json}"
            );
        }

        using var document = JsonDocument.Parse(json);

        return ExtractOutputText(document.RootElement);
    }

    private static string ExtractOutputText(JsonElement root)
    {
        if (root.TryGetProperty("output_text", out var outputText))
        {
            return outputText.GetString() ?? string.Empty;
        }

        if (!root.TryGetProperty("output", out var output))
        {
            return string.Empty;
        }

        foreach (var outputItem in output.EnumerateArray())
        {
            if (!outputItem.TryGetProperty("content", out var content))
            {
                continue;
            }

            foreach (var contentItem in content.EnumerateArray())
            {
                if (contentItem.TryGetProperty("text", out var text))
                {
                    return text.GetString() ?? string.Empty;
                }
            }
        }

        return string.Empty;
    }
}
