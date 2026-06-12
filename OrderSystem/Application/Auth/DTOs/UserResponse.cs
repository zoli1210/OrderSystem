namespace OrderSystem.Modules.Auth.DTOs;

public class UserResponse
{
    public string Id { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public IReadOnlyList<string> Roles { get; set; } = [];
}
