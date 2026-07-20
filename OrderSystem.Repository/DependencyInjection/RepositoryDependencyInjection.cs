using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderSystem.Repository.Persistence;
using OrderSystem.Repository.Repositories;

namespace OrderSystem.Repository.DependencyInjection;

public static class RepositoryDependencyInjection
{
    public static IServiceCollection AddRepository(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionString =
            configuration.GetConnectionString("SQLConnection")
            ?? configuration["SqlConnectionString"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("SQLConnection or SqlConnectionString is missing.");
        }

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(
                connectionString,
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                }
            );
        });

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderStatusHistoryRepository, OrderStatusHistoryRepository>();

        services.AddScoped<
            IEmailNotificationHistoryRepository,
            EmailNotificationHistoryRepository
        >();

        return services;
    }
}
