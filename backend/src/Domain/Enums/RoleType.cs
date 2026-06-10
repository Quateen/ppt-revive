using System.ComponentModel;

namespace PPTRevive.Domain.Enums;

public enum RoleType
{
    [Description("Workspace")]
    Workspace = 1,
    [Description("Project")]
    Project = 2
}

public enum ApplicationRole
{
    [Role(1, (int)RoleType.Workspace, "Workspace Admin")]
    WorkspaceAdmin = 1,

    [Role(2, (int)RoleType.Workspace, "Workspace Member")]
    WorkspaceMember = 2,

    [Role(3, (int)RoleType.Workspace, "Workspace Guest")]
    WorkspaceGuest = 3,

    [Role(4, (int)RoleType.Project, "Project Admin")]
    ProjectAdmin = 4,

    [Role(5, (int)RoleType.Project, "Project Member")]
    ProjectMember = 5,

    [Role(6, (int)RoleType.Project, "Project Guest")]
    ProjectGuest = 6
}

[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class RoleAttribute : Attribute
{
    public int Id { get; }
    public int RoleTypeId { get; }
    public string? Description { get; }

    public RoleAttribute(int id, int roleTypeId = 0, string? description = null)
    {
        Id = id;
        RoleTypeId = roleTypeId;
        Description = description;
    }
}


public static class RoleEnumAttributeExtensions
{
    public static (int Id, string Name, int Value, int RoleTypeId, string? Description) ToRoleAttribute(this ApplicationRole appRole)
    {
        var fieldInfo = appRole.GetType().GetField(appRole.ToString());
        var attribute = fieldInfo?.GetCustomAttributes(typeof(RoleAttribute), false)
                                 .FirstOrDefault() as RoleAttribute;

        if (attribute == null)
            return (0, string.Empty, 0, 0, null);

        string? roleTypeName = Enum.GetName(typeof(RoleType), attribute.RoleTypeId);
        string displayName = attribute.Description ?? string.Empty;

        if (!string.IsNullOrEmpty(roleTypeName) && !string.IsNullOrEmpty(displayName))
        {
            var readableTitle = displayName.Replace($"{roleTypeName} ", "");
            displayName = $"{readableTitle} ({roleTypeName})";
        }

        return (
            attribute.Id,
            displayName ?? string.Empty,
            (int)appRole,
            attribute.RoleTypeId,
            attribute.Description
        );
    }
}
