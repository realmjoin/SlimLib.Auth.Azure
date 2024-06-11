using System.Collections.Generic;

namespace SlimLib.Auth.Azure.Permissions;

internal interface ISlimClientWithPermissions
{
    public IReadOnlyList<RequiredPermission> Permissions { get; }
}
