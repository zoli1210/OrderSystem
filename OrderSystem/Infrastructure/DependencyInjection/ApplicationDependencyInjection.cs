using OrderSystem.Modules.Auth.Services;
using OrderSystem.Modules.Email.Services;
using OrderSystem.Modules.Orders.Services;
using OrderSystem.Modules.Payments.Services;

namespace OrderSystem.Infrastructure.DependencyInjection;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IEmailService, AzureCommunicationEmailService>();

        return services;
    }
}
