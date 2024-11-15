using System;
using System.Linq;

namespace SlimLib.Auth.Azure.Permissions;

public class GraphPermission(string resource, string operation, string constraint)
{
    public const string OperationReadWrite = "ReadWrite";
    public const string OperationRead = "Read";
    public const string OperationReadBasic = "ReadBasic";

    public static GraphPermission Create(string permission)
    {
        var split = permission.Split('.');

        if (split.Length == 2) return new(split[0], split[1], "");
        if (split.Length == 3) return new(split[0], split[1], split[2]);

        throw new ArgumentException("Invalid permission format.");
    }

    public string Resource { get; } = resource;
    public string Operation { get; } = operation;
    public string Constraint { get; } = constraint;

    public GraphPermission WithOperation(string operation) => new(Resource, operation, Constraint);

    public override string ToString() => $"{Resource}.{Operation}.{Constraint}";

    public bool IsSatisfiedBy(ActualPermissionList actualPermissions) => actualPermissions.Any(IsSatisfiedBy);

    public bool IsSatisfiedBy(GraphPermission permission)
    {
        // Check if the resource, operation, and constraint match exactly
        if (permission.Resource == Resource && permission.Operation == Operation && permission.Constraint == Constraint)
        {
            return true;
        }

        // Check if the operation is "Read" and the permission has "ReadWrite" operation with the same resource and constraint
        if (Operation == OperationRead && permission.Resource == Resource && permission.Operation is OperationReadWrite && permission.Constraint == Constraint)
        {
            return true;
        }

        // Check if the operation is "ReadBasic" and the permission has either "ReadWrite" or "Read" operation with the same resource and constraint
        if (Operation == OperationReadBasic && permission.Resource == Resource && permission.Operation is OperationReadWrite or OperationRead && permission.Constraint == Constraint)
        {
            return true;
        }

        // If none of the conditions are met, return false
        return false;
    }

    public static bool operator ==(GraphPermission left, GraphPermission right) => left.Equals(right);
    public static bool operator !=(GraphPermission left, GraphPermission right) => !(left == right);

    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        if (obj is not GraphPermission other)
        {
            return false;
        }

        return other.Resource == Resource && other.Operation == Operation && other.Constraint == Constraint;
    }

    public override int GetHashCode() => HashCode.Combine(Resource, Operation, Constraint);

    public void Deconstruct(out string resource, out string operation, out string constraint)
    {
        resource = Resource;
        operation = Operation;
        constraint = Constraint;
    }

    public static implicit operator GraphPermission(string permission) => Create(permission);
}
