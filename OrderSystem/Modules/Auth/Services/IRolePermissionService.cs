namespace OrderSystem.Modules.Auth.Services;

public interface IRolePermissionService
{
    bool CanAssignRole(IReadOnlyCollection<string> currentUserRoles, string targetRole);
}
