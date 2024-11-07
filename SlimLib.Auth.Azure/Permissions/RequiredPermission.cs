using System;
using System.Collections;
using System.Collections.Generic;

namespace SlimLib.Auth.Azure.Permissions;

public record RequiredPermission : IReadOnlyList<string>
{
    private readonly string permission1;
    private readonly string permission2;

    public RequiredPermission(string permission)
    {
        permission1 = permission;
        permission2 = "";
    }

    public RequiredPermission(string permission1, string permission2)
    {
        this.permission1 = permission1;
        this.permission2 = permission2;
    }

    public int Count => string.IsNullOrEmpty(permission2) ? 1 : 2;

    public string this[int index] => index switch
    {
        0 => permission1,
        1 => permission2,
        _ => throw new IndexOutOfRangeException()
    };

    public IEnumerator<string> GetEnumerator()
    {
        yield return permission1;
        if (Count == 2) yield return permission2;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => Count == 2 ? $"{permission1} -AND- {permission2}" : permission1;

    public static string ToString(IEnumerable<RequiredPermission> permissions) => string.Join(" -OR- ", permissions);

    public static IEnumerable<RequiredPermission> AppendReadWrite(ICollection<RequiredPermission> permissions)
    {
        foreach (var permission in permissions)
        {
            if (permission.Count == 1)
            {
                yield return permission[0];

                var replace1 = new RequiredPermission(Replace(permission[0]));

                if (!permissions.Contains(replace1))
                    yield return replace1;
            }
            else if (permission.Count == 2)
            {
                yield return new(permission[0], permission[1]);

                var replace01 = new RequiredPermission(permission[0], Replace(permission[1]));

                if (!permissions.Contains(replace01))
                    yield return new(permission[0], Replace(permission[1]));

                var replace10 = new RequiredPermission(Replace(permission[0]), permission[1]);

                if (!permissions.Contains(replace10))
                    yield return replace10;

                var replace11 = new RequiredPermission(Replace(permission[0]), Replace(permission[1]));

                if (!permissions.Contains(replace11))
                    yield return replace11;
            }
            else
            {
                throw new NotSupportedException("Only single or double permissions are supported.");
            }
        }
    }

    private static string Replace(string str) => str.Replace(".Read.", ".ReadWrite.");

    public static implicit operator RequiredPermission(string permission) => new(permission);
    public static implicit operator RequiredPermission((string Permission1, string Permission2) permissions) => new(permissions.Permission1, permissions.Permission2);
}
