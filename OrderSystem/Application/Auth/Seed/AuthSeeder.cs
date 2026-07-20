using Microsoft.AspNetCore.Identity;
using OrderSystem.Repository.Persistence.Identity;

namespace OrderSystem.Modules.Auth.Seed;

public static class AuthSeeder
{
    public static async Task SeedAsync(
        IServiceProvider serviceProvider,
        IWebHostEnvironment environment
    )
    {
        using var scope = serviceProvider.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        foreach (var role in AuthRoles.All)
        {
            await EnsureRoleExistsAsync(roleManager, role);
        }

        if (environment.IsDevelopment())
        {
            await SeedDevelopmentAdminUserAsync(userManager, configuration);
        }
    }

    private static async Task SeedDevelopmentAdminUserAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration
    )
    {
        var adminEmail = configuration["AdminUser:Email"];
        var adminPassword = configuration["AdminUser:Password"];

        if (string.IsNullOrWhiteSpace(adminEmail))
        {
            throw new InvalidOperationException("AdminUser:Email is missing.");
        }

        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            throw new InvalidOperationException("AdminUser:Password is missing.");
        }

        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
            };

            var createResult = await userManager.CreateAsync(adminUser, adminPassword);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    createResult.Errors.Select(error => error.Description)
                );

                throw new InvalidOperationException($"Admin user creation failed: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(adminUser, AuthRoles.Admin))
        {
            await userManager.AddToRoleAsync(adminUser, AuthRoles.Admin);
        }
    }

    private static async Task EnsureRoleExistsAsync(
        RoleManager<IdentityRole> roleManager,
        string roleName
    )
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}
