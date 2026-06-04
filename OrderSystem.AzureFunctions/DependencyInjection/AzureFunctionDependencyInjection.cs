using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderSystem.AzureFunctions.Services;
using OrderSystem.Infrastructure.Messaging;
using OrderSystem.Infrastructure.Persistence;
using OrderSystem.Infrastructure.Persistence.Repositories;
using OrderSystem.Modules.Email.Services;
using OrderSystem.Modules.Payments.Services;

namespace OrderSystem.AzureFunctions.DependencyInjection;

public static class AzureFunctionsDependencyInjection
{
    public static IServiceCollection AddAzureFunctionServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var sqlConnectionString = configuration["SqlConnectionString"];

        if (string.IsNullOrWhiteSpace(sqlConnectionString))
        {
            throw new InvalidOperationException("SqlConnectionString is missing.");
        }

        var serviceBusConnectionString = configuration["AzureServiceBusConnection"];

        if (string.IsNullOrWhiteSpace(serviceBusConnectionString))
        {
            throw new InvalidOperationException("AzureServiceBusConnection is missing.");
        }

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(sqlConnectionString);
        });

        services.AddSingleton(_ => new ServiceBusClient(
            serviceBusConnectionString,
            new ServiceBusClientOptions { TransportType = ServiceBusTransportType.AmqpWebSockets }
        ));

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderStatusHistoryRepository, OrderStatusHistoryRepository>();
        services.AddScoped<
            IEmailNotificationHistoryRepository,
            EmailNotificationHistoryRepository
        >();

        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IPaymentProcessor, PaymentProcessor>();

        services.AddScoped<IEmailMessageSender, AzureServiceBusEmailMessageSender>();

        services.AddScoped<IEmailService, AzureCommunicationEmailService>();
        services.AddScoped<IEmailProcessor, EmailProcessor>();
        services.AddScoped<IOrderStatusEmailProcessor, OrderStatusEmailProcessorService>();

        return services;
    }
}
