namespace SlimLib.Auth.Azure.Permissions;

public class EvaluatedPermissionList(string name, params RequiredPermission[] permissions) : RequiredPermissionList(permissions)
{
    public string Name { get; } = name;
}
