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
        services.AddScoped<IRolePermissionService, RolePermissionService>();

        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IOrderStatusService, OrderStatusService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IEmailService, AzureCommunicationEmailService>();
        services.AddScoped<ITrackingNumberGenerator, MockTrackingNumberGenerator>();

        return services;
    }
}
