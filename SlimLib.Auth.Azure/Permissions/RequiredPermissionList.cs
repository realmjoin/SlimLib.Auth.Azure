using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SlimLib.Auth.Azure.Permissions;

public class RequiredPermissionList(params RequiredPermission[] permissions) : IReadOnlyList<RequiredPermission>
{
    public RequiredPermission this[int index] => ((IReadOnlyList<RequiredPermission>)permissions)[index];

    public int Count => ((IReadOnlyCollection<RequiredPermission>)permissions).Count;

    public int IndexOf(GraphPermission item)
    {
        return Array.IndexOf(permissions, item);
    }

    public IEnumerator<RequiredPermission> GetEnumerator()
    {
        return ((IEnumerable<RequiredPermission>)permissions).GetEnumerator();
    }

    public RequiredPermission[] IsSatisfiedBy(ActualPermissionList permission)
    {
        return [.. permissions.Where(x => x.IsSatisfiedBy(permission))];
    }

    public override string ToString() => string.Join(" -OR- ", permissions.Select(x => $"({x})"));

    IEnumerator IEnumerable.GetEnumerator()
    {
        return permissions.GetEnumerator();
    }
}
