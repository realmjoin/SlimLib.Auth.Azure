using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SlimLib.Auth.Azure.Permissions;

public class RequiredPermission(params GraphPermission[] permissions) : IReadOnlyList<GraphPermission>
{
    public int Count => permissions.Length;

    public GraphPermission this[int index] => permissions[index];

    public bool IsSatisfiedBy(ActualPermissionList actualPermissions)
    {
        foreach (var permission in permissions)
        {
            if (!actualPermissions.Any(permission.IsSatisfiedBy))
            {
                return false;
            }
        }

        return true;
    }

    public override string ToString() => string.Join(" -AND- ", permissions.Select(x => x.ToString()));

    public static bool operator ==(RequiredPermission left, RequiredPermission right) => left.Equals(right);
    public static bool operator !=(RequiredPermission left, RequiredPermission right) => !(left == right);

    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        if (obj is not RequiredPermission other)
        {
            return false;
        }

        if (Count != other.Count)
        {
            return false;
        }

        return !permissions.Except(other).Any();
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();

        foreach (var permission in permissions)
        {
            hash.Add(permission);
        }

        return hash.ToHashCode();
    }

    public IEnumerator<GraphPermission> GetEnumerator() => ((IEnumerable<GraphPermission>)permissions).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static implicit operator RequiredPermission(string permission) => new(permission);
    public static implicit operator RequiredPermission((string permission1, string permission2) list) => new(list.permission1, list.permission2);
}
