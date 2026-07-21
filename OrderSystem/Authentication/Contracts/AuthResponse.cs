namespace OrderSystem.Authentication.Contracts;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }
}
