using System.Text.Json.Serialization;
using Azure.Messaging.ServiceBus;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using OrderSystem.Infrastructure.Messaging;
using OrderSystem.Infrastructure.Persistence;
using OrderSystem.Infrastructure.Persistence.Repositories;
using OrderSystem.Modules.Email.Services;
using OrderSystem.Modules.Orders.Services;
using OrderSystem.Modules.Orders.Validators;
using OrderSystem.Modules.Payments.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Valitador
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderRequestValidator>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SQLConnection"));
});

// Repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Module services
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Messaging
builder.Services.AddScoped<IQueuePublisher, QueuePublisher>();
builder.Services.AddSingleton<InMemoryOrderMessageQueue>();
builder.Services.AddScoped<IOrderMessageSender, AzureServiceBusOrderMessageSender>();

//builder.Services.AddHostedService<PaymentBackgroundService>();
//builder.Services.AddSingleton<InMemoryDeadLetterQueue>();

// Azure
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connectionString = config["AzureServiceBus:ConnectionString"];

    return new ServiceBusClient(connectionString);
});

// Serialization
builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
