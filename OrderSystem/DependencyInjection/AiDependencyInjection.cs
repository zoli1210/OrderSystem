using OrderSystem.Application.AI.Documents;
using OrderSystem.Application.AI.Knowledge;
using OrderSystem.Application.AI.OrderExplanation;
using OrderSystem.Application.AI.Processing;
using OrderSystem.Application.AI.Providers.OpenAi;
using OrderSystem.Application.AI.Providers.Supabase;
using OrderSystem.Application.AI.Providers.WebContent;
using OrderSystem.Application.AI.Sources;
using OrderSystem.Infrastructure.Options;
using OrderSystem.Modules.AI.HostedServices;

namespace OrderSystem.DependencyInjection;

public static class AiDependencyInjection
{
    public static IServiceCollection AddAiServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddOptions<SupabaseOptions>()
            .Bind(configuration.GetSection(SupabaseOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Url),
                "Supabase:Url is missing."
            )
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.SecretKey),
                "Supabase:SecretKey is missing."
            )
            .ValidateOnStart();

        services
            .AddOptions<OpenAiOptions>()
            .Bind(configuration.GetSection(OpenAiOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ApiKey),
                "OpenAI:ApiKey is missing."
            )
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.EmbeddingModel),
                "OpenAI:EmbeddingModel is missing."
            )
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ChatModel),
                "OpenAI:ChatModel is missing."
            )
            .Validate(
                options => options.DefaultMatchCount > 0 && options.DefaultMatchCount <= 20,
                "OpenAI:DefaultMatchCount must be between 1 and 20."
            )
            .ValidateOnStart();

        services.AddHttpClient<IEmbeddingService, OpenAiEmbeddingService>();
        services.AddHttpClient<IVectorSearchService, SupabaseVectorSearchService>();
        services.AddHttpClient<IChatCompletionService, OpenAiChatCompletionService>();
        services.AddHttpClient<
            IKnowledgeDocumentIngestionService,
            KnowledgeDocumentIngestionService
        >();
        services.AddHttpClient<IKnowledgeSourceSeedService, KnowledgeSourceSeedService>();
        services.AddHttpClient<IKnowledgeSourceQueryService, KnowledgeSourceQueryService>();

        services.AddHttpClient<IHtmlContentExtractor, HtmlContentExtractor>();

        services.AddHttpClient<IKnowledgeSourceRepository, SupabaseKnowledgeSourceRepository>();
        services.AddHttpClient<IKnowledgeDocumentRepository, SupabaseKnowledgeDocumentRepository>();

        services.AddSingleton<ITextChunkingService, TextChunkingService>();

        services.AddScoped<IAiKnowledgeService, AiKnowledgeService>();
        services.AddScoped<IKnowledgeSourceIngestionService, KnowledgeSourceIngestionService>();
        services.AddHostedService<AiKnowledgeIngestionHostedService>();
        services.AddScoped<IOrderExplanationService, OrderExplanationService>();

        return services;
    }
}
