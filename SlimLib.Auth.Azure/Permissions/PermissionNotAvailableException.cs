using System;

namespace SlimLib.Auth.Azure.Permissions;

public class PermissionNotAvailableException(RequiredPermissionList requestedPermissions) 
    : Exception($"Unable to create client that can satisfy any of the requested permissions. ({requestedPermissions})")
{
    public RequiredPermissionList RequestedPermissions { get; } = requestedPermissions;
}