using OrderSystem.Modules.Auth.DTOs;

namespace OrderSystem.Modules.Auth.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);

    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyList<UserResponse>> GetUsersAsync(CancellationToken cancellationToken);

    Task UpdateUserRoleAsync(
        string userId,
        UpdateUserRoleRequest request,
        CancellationToken cancellationToken
    );
}
