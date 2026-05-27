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

    public async Task<string> GenerateOrderExplanationAsync(
        string question,
        string orderContext,
        IReadOnlyList<KnowledgeDocumentMatch> contextDocuments,
        CancellationToken cancellationToken
    )
    {
        var documentationContext = string.Join(
            "\n\n---\n\n",
            contextDocuments
                .Take(3)
                .Select(
                    (document, index) =>
                        $"Source {index + 1}: {document.Title}\nUrl: {document.Url}\nContent:\n{document.Content}"
                )
        );

        var input = $"""
            You are a technical assistant for a .NET and Azure based order processing system.

            Your task is to answer the user's exact question about one specific order.

            Use the structured order data as the source of truth.
            Use the retrieved documentation only as supporting context when it is needed to explain the meaning of a status, transition, asynchronous workflow, payment behavior, email behavior or messaging behavior.

            Most important rule:
            Answer only what the user asked. Do not include extra lifecycle details, email details, process history, or recommended actions unless the question asks for them or they are necessary to avoid a misleading answer.

            Intent handling:
            - If the user asks only for the current status, answer with only the current status and one short meaning sentence.
            - If the user asks what happened so far, summarize the relevant status history.
            - If the user asks whether anything needs to be done, clearly say whether manual action is needed.
            - If the user asks about email, only then mention email notification history.
            - If the user asks why something failed or is stuck, explain the relevant failure/waiting reason based on the available data.
            - If the user asks for a full explanation, lifecycle, process, or troubleshooting, give a structured explanation.
            - If the user asks a narrow question, keep the answer narrow.
            - If the user asks a broad question, provide a broader but still concise answer.

            Do not:
            - Do not mention email activity unless the question asks about email or notification status.
            - Do not mention that no further process is needed unless the question asks about next steps or required action.
            - Do not describe the full order lifecycle unless the question asks for lifecycle/process/history.
            - Do not list every available field.
            - Do not assume the order has failed.
            - Do not invent behavior that is not present in the structured data or documentation.

            Answer length:
            - For simple status questions: maximum 1-2 short sentences.
            - For normal questions: maximum 2-4 short bullet points.
            - For detailed process/troubleshooting questions: use a structured answer, but keep it concise.

            User question:
            {question}

            Structured order data:
            {orderContext}

            Retrieved documentation:
            {documentationContext}
            """;

        var requestBody = new { model = _options.ChatModel, input };

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://api.openai.com/v1/responses"
        );

        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer",
            _options.ApiKey
        );

        request.Content = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(requestBody),
            System.Text.Encoding.UTF8,
            "application/json"
        );

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"OpenAI order explanation request failed. StatusCode: {response.StatusCode}, Body: {json}"
            );
        }

        using var document = System.Text.Json.JsonDocument.Parse(json);

        return ExtractOutputText(document.RootElement);
    }
}
