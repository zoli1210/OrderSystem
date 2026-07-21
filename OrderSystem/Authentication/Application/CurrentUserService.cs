using System.Security.Claims;
using OrderSystem.Authentication.Authorization;

namespace OrderSystem.Authentication.Application;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    public bool IsAdmin => _httpContextAccessor.HttpContext?.User.IsInRole(AuthRoles.Admin) == true;
}
