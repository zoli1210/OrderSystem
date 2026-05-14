namespace OrderSystem.Modules.Auth.Services;

public interface ICurrentUserService
{
    string? UserId { get; }

    bool IsAuthenticated { get; }

    bool IsAdmin { get; }
}
