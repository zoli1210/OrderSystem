using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OrderSystem.Authentication.Authorization;
using OrderSystem.Authentication.Contracts;
using OrderSystem.Repository.Persistence.Identity;

namespace OrderSystem.Authentication.Application;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly ICurrentUserService _currentUserService;
    private readonly IRolePermissionService _rolePermissionService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration,
        ICurrentUserService currentUserService,
        IRolePermissionService rolePermissionService
    )
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _currentUserService = currentUserService;
        _rolePermissionService = rolePermissionService;
    }

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken
    )
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);

        if (existingUser is not null)
        {
            throw new InvalidOperationException("User with this email already exists.");
        }

        var user = new ApplicationUser { UserName = request.Email, Email = request.Email };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(error => error.Description));
            throw new InvalidOperationException(errors);
        }

        await EnsureRoleExistsAsync(AuthRoles.User);
        await _userManager.AddToRoleAsync(user, AuthRoles.User);

        return await GenerateTokenAsync(user);
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken
    )
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);

        if (!passwordValid)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        return await GenerateTokenAsync(user);
    }

    private async Task<AuthResponse> GenerateTokenAsync(ApplicationUser user)
    {
        var jwtKey = _configuration["Jwt:Key"];
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];

        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            throw new InvalidOperationException("Jwt:Key is missing.");
        }

        var expiresAtUtc = DateTime.UtcNow.AddHours(1);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
        };

        var roles = await _userManager.GetRolesAsync(user);

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials
        );

        return new AuthResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAtUtc = expiresAtUtc,
        };
    }

    private async Task EnsureRoleExistsAsync(string roleName)
    {
        var roleExists = await _roleManager.RoleExistsAsync(roleName);

        if (!roleExists)
        {
            await _roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    public async Task<IReadOnlyList<UserResponse>> GetUsersAsync(
        CancellationToken cancellationToken
    )
    {
        var users = await _userManager
            .Users.OrderBy(user => user.Email)
            .ToListAsync(cancellationToken);

        var responses = new List<UserResponse>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            responses.Add(
                new UserResponse
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    Roles = roles.ToList(),
                }
            );
        }

        return responses;
    }

    public async Task UpdateUserRoleAsync(
        string userId,
        UpdateUserRoleRequest request,
        CancellationToken cancellationToken
    )
    {
        if (!AuthRoles.All.Contains(request.Role, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Invalid role.");
        }

        var currentUserId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var currentUser = await _userManager.FindByIdAsync(currentUserId);

        if (currentUser is null)
        {
            throw new UnauthorizedAccessException("Current user not found.");
        }

        var currentUserRoles = await _userManager.GetRolesAsync(currentUser);

        if (!_rolePermissionService.CanAssignRole(currentUserRoles.ToList(), request.Role))
        {
            throw new UnauthorizedAccessException("You are not allowed to assign this role.");
        }

        var targetUser = await _userManager.FindByIdAsync(userId);

        if (targetUser is null)
        {
            throw new InvalidOperationException("Target user not found.");
        }

        var targetUserRoles = await _userManager.GetRolesAsync(targetUser);

        await EnsureRoleExistsAsync(request.Role);

        if (targetUserRoles.Any())
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(targetUser, targetUserRoles);

            if (!removeResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    removeResult.Errors.Select(error => error.Description)
                );
                throw new InvalidOperationException(errors);
            }
        }

        var addResult = await _userManager.AddToRoleAsync(targetUser, request.Role);

        if (!addResult.Succeeded)
        {
            var errors = string.Join("; ", addResult.Errors.Select(error => error.Description));
            throw new InvalidOperationException(errors);
        }
    }
}
