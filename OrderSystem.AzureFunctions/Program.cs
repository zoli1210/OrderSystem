using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderSystem.Infrastructure.Persistence;
using OrderSystem.Infrastructure.Persistence.Repositories;
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

            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IPaymentService, PaymentService>();
        }
    )
    .Build();

host.Run();
