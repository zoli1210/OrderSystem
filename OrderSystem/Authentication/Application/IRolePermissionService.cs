namespace OrderSystem.Authentication.Application;

public interface IRolePermissionService
{
    bool CanAssignRole(IReadOnlyCollection<string> currentUserRoles, string targetRole);
}
