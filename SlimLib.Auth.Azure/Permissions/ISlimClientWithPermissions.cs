using System.Collections.Generic;

namespace SlimLib.Auth.Azure.Permissions;

public interface ISlimClientWithPermissions
{
    public IReadOnlyList<RequiredPermission> Permissions { get; }
}
