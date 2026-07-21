using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderSystem.Authentication.Application;
using OrderSystem.Authentication.Authorization;
using OrderSystem.Authentication.Contracts;

namespace OrderSystem.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        RegisterRequest request,
        CancellationToken cancellationToken
    )
    {
        var response = await _authService.RegisterAsync(request, cancellationToken);

        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginRequest request,
        CancellationToken cancellationToken
    )
    {
        var response = await _authService.LoginAsync(request, cancellationToken);

        return Ok(response);
    }

    [HttpGet("users")]
    [Authorize(Roles = AuthRoles.Admin)]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
    {
        var users = await _authService.GetUsersAsync(cancellationToken);

        return Ok(users);
    }

    [HttpPut("users/{userId}/role")]
    [Authorize(Roles = $"{AuthRoles.Admin},{AuthRoles.Manager},{AuthRoles.TeamLead}")]
    public async Task<IActionResult> UpdateUserRole(
        string userId,
        UpdateUserRoleRequest request,
        CancellationToken cancellationToken
    )
    {
        await _authService.UpdateUserRoleAsync(userId, request, cancellationToken);

        return NoContent();
    }
}
