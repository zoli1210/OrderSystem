namespace OrderSystem.Modules.Auth.Services;

public class RolePermissionService : IRolePermissionService
{
    private static readonly Dictionary<string, int> RoleLevels = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        [AuthRoles.Admin] = 500,
        [AuthRoles.Manager] = 400,
        [AuthRoles.TeamLead] = 300,
        [AuthRoles.Support] = 200,
        [AuthRoles.User] = 100,
    };

    public bool CanAssignRole(IReadOnlyCollection<string> currentUserRoles, string targetRole)
    {
        if (!RoleLevels.TryGetValue(targetRole, out var targetRoleLevel))
        {
            return false;
        }

        var currentUserHighestLevel = currentUserRoles
            .Where(RoleLevels.ContainsKey)
            .Select(role => RoleLevels[role])
            .DefaultIfEmpty(0)
            .Max();

        return currentUserHighestLevel > targetRoleLevel;
    }
}
