using System;
using System.Collections.Generic;

namespace SlimLib.Auth.Azure.Permissions;

public class PermissionNotAvailableException : Exception
{
    public PermissionNotAvailableException(params RequiredPermission[] requestedPermissions)
        : base($"Unable to create client that can satisfy any of the requested permissions. ({RequiredPermission.ToString(requestedPermissions)})")
    {
        RequestedPermissions = requestedPermissions;
    }

    public RequiredPermission[] RequestedPermissions { get; }
}