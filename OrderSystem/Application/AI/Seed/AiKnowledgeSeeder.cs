using OrderSystem.Application.AI.Sources;

namespace OrderSystem.Modules.AI.Seed;

public static class AiKnowledgeSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, ILogger logger)
    {
        using var scope = serviceProvider.CreateScope();

        try
        {
            var seedService =
                scope.ServiceProvider.GetRequiredService<IKnowledgeSourceSeedService>();

            var result = await seedService.SeedAsync(CancellationToken.None);

            logger.LogInformation(
                "AI knowledge sources seeded. Total: {Total}, Created: {Created}, Skipped: {Skipped}",
                result.Total,
                result.Created,
                result.Skipped
            );
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "AI knowledge source seeding failed. Application startup will continue."
            );
        }
    }
}
