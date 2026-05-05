using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderSystem.AzureFunctions.Services;
using OrderSystem.Infrastructure.Messaging;
using OrderSystem.Infrastructure.Persistence;
using OrderSystem.Infrastructure.Persistence.Repositories;
using OrderSystem.Modules.Email.Services;
using OrderSystem.Modules.Payments.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(
        (context, services) =>
        {
            var connectionString = context.Configuration["SqlConnectionString"];

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "SqlConnectionString is missing from local.settings.json."
                );
            }

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            services.AddSingleton(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var connectionString = configuration["AzureServiceBusConnection"];

                return new ServiceBusClient(connectionString);
            });

            services.AddScoped<IEmailService, FakeEmailService>();
            services.AddScoped<IEmailProcessor, EmailProcessor>();
            services.AddScoped<IPaymentProcessor, PaymentProcessor>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IEmailMessageSender, AzureServiceBusEmailMessageSender>();
        }
    )
    .Build();

host.Run();
