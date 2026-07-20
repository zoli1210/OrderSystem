using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using OrderSystem.Api.HealthCheck;
using OrderSystem.Infrastructure.DependencyInjection;
using OrderSystem.Modules.AI.Seed;
using OrderSystem.Modules.Auth.Seed;
using OrderSystem.Modules.Orders.Validators;
using OrderSystem.Repository.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter JWT token. Example: Bearer {your token}",
        }
    );

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                []
            },
        }
    );
});

// Validation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderRequestValidator>();

// Project services
builder.Services.AddRepository(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddMessaging(builder.Configuration);
builder.Services.AddAuth(builder.Configuration);
builder.Services.AddProjectHealthChecks();
builder.Services.AddAiServices(builder.Configuration);

builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();

await AuthSeeder.SeedAsync(app.Services, app.Environment);

await AiKnowledgeSeeder.SeedAsync(app.Services, app.Logger);

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks(
    "/health",
    new HealthCheckOptions { ResponseWriter = HealthCheckResponse.WriteResponseAsync }
);

app.Run();
