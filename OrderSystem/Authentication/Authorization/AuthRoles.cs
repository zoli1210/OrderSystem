namespace OrderSystem.Authentication.Authorization;

public static class AuthRoles
{
    public const string Admin = "Admin";

    public const string Manager = "Manager";

    public const string TeamLead = "TeamLead";

    public const string Support = "Support";

    public const string User = "User";

    public static readonly IReadOnlyList<string> All = [Admin, Manager, TeamLead, Support, User];
}
