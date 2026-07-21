using OrderSystem.Application.Orders.Services;
using OrderSystem.Application.Orders.Tracking;
using OrderSystem.Authentication.Application;
using OrderSystem.Infrastructure.Email;
using OrderSystem.Infrastructure.Payments;

namespace OrderSystem.DependencyInjection;

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
