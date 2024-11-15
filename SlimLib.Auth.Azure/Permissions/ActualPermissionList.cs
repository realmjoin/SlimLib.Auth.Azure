using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SlimLib.Auth.Azure.Permissions;

public class ActualPermissionList(params GraphPermission[] permissions) : IReadOnlyList<GraphPermission>
{
    public GraphPermission this[int index] => ((IReadOnlyList<GraphPermission>)permissions)[index];

    public int Count => ((IReadOnlyCollection<GraphPermission>)permissions).Count;

    public static ActualPermissionList Create(IEnumerable<string> permissions)
    {
        return new([.. permissions.Select(GraphPermission.Create)]);
    }

    public int IndexOf(GraphPermission item)
    {
        return Array.IndexOf(permissions, item);
    }

    public IEnumerator<GraphPermission> GetEnumerator()
    {
        return ((IEnumerable<GraphPermission>)permissions).GetEnumerator();
    }

    public override string ToString() => $"({string.Join(", ", permissions.Select(x => x.ToString()))})";

    IEnumerator IEnumerable.GetEnumerator()
    {
        return permissions.GetEnumerator();
    }
}
