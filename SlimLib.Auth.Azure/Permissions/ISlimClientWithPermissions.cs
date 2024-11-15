namespace SlimLib.Auth.Azure.Permissions;

public interface ISlimClientWithPermissions
{
    public RequiredPermissionList Permissions { get; }
}
