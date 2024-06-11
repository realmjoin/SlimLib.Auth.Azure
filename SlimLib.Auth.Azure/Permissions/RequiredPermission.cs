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

    public static List<RequiredPermission> AppendReadWrite(IEnumerable<RequiredPermission> permissions)
    {
        var final = new List<RequiredPermission>();

        foreach (var permission in permissions)
        {
            if (permission.Count == 1)
            {
                final.Add(permission[0]);
                final.Add(Replace(permission[0]));
            }
            else if (permission.Count == 2)
            {
                final.Add(new(permission[0], permission[1]));
                final.Add(new(permission[0], Replace(permission[1])));
                final.Add(new(Replace(permission[0]), permission[1]));
                final.Add(new(Replace(permission[0]), Replace(permission[1])));
            }
            else
            {
                throw new NotSupportedException("Only single or double permissions are supported.");
            }
        }

        return final;
    }

    public static List<RequiredPermission> PrependReadWrite(IEnumerable<RequiredPermission> permissions)
    {
        var final = AppendReadWrite(permissions);

        final.Reverse();

        return final;
    }

    private static string Replace(string str) => str.Replace(".Read.", ".ReadWrite.");

    public static implicit operator RequiredPermission(string permission) => new(permission);
    public static implicit operator RequiredPermission((string Permission1, string Permission2) permissions) => new(permissions.Permission1, permissions.Permission2);
}
