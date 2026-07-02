namespace PPTRevive.Domain.Enums;
public enum ApplicationRights
{
    // Workspace
    [RightAttribute(1, roleIds: [1])]
    ViewWorkspace = 1,

    [RightAttribute(2, roleIds: [1, 2, 3, 4, 5, 6])]
    ViewWorkspaceById = 2,

    [RightAttribute(3, roleIds: [1])]
    AddWorkspace = 3,

    [RightAttribute(4, roleIds: [1])]
    EditWorkspace = 4,

    [RightAttribute(5, roleIds: [1])]
    AddNewWorkspaceUser = 5,

    [RightAttribute(6, roleIds: [1])]
    RemoveWorkspaceUser = 6,

    [RightAttribute(7, roleIds: [1])]
    RemoveWorkspace = 7,

    //Project
    [RightAttribute(8, roleIds: [1])]
    ViewProject = 8,

    [RightAttribute(9, roleIds: [1, 2, 3, 4, 5, 6])]
    ViewProjectById = 9,

    [RightAttribute(10, roleIds: [1, 2])]
    AddProject = 10,

    [RightAttribute(11, roleIds: [1, 2])]
    EditProject = 11,

    [RightAttribute(12, roleIds: [1, 2])]
    AddProjectUser = 12,

    [RightAttribute(13, roleIds: [1])]
    RemoveProjectUser = 13,

    // Tickets
    [RightAttribute(14, roleIds: [1, 2, 3, 4, 5, 6])]
    ViewTicket = 14,

    [RightAttribute(15, roleIds: [1, 2, 3, 4, 5, 6])]
    ViewTicketById = 15,

    [RightAttribute(16, roleIds: [1, 2, 4, 5])]
    AddTicket = 16,

    [RightAttribute(17, roleIds: [1, 2, 4, 5])]
    EditTicket = 17,

    [RightAttribute(18, roleIds: [1, 2, 4, 5])]
    AssignTicket = 18,

    // Roles
    [RightAttribute(19, roleIds: [1, 2, 4, 5])]
    ViewRole = 19,

    [RightAttribute(20, roleIds: [1, 2, 4, 5])]
    ViewRoleById = 20,

    [RightAttribute(21, roleIds: [1])]
    AddRole = 21,

    [RightAttribute(22, roleIds: [1])]
    EditRole = 22,

    // Users
    [RightAttribute(23, roleIds: [1, 2, 3, 4, 5, 6])]
    ViewUser = 23,

    [RightAttribute(24, roleIds: [1, 2, 3, 4, 5, 6])]
    ViewUserById = 24,

    [RightAttribute(25, roleIds: [1, 2])]
    AddUser = 25,

    [RightAttribute(26, roleIds: [1, 2])]
    EditUser = 26,

    // Settings
    [RightAttribute(27, roleIds: [1])]
    ViewSettings = 27,

    [RightAttribute(28, roleIds: [1])]
    ViewSettingsById = 28,

    [RightAttribute(29, roleIds: [1])]
    AddSettings = 29,

    [RightAttribute(30, roleIds: [1])]
    EditSettings = 30,
}

[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class RightAttribute : Attribute
{
    public int Id { get; }
    public int[] RoleIds { get; }

    public RightAttribute(int id, int[] roleIds)
    {
        Id = id;
        RoleIds = roleIds;
    }
}

public static class RightEnumAttributeExtensions
{
    public static (int Id, string Name, int Value, int[] RoleIds) ToRightAttribute(this ApplicationRights right)
    {
        var fieldInfo = right.GetType().GetField(right.ToString());
        var attribute = fieldInfo?.GetCustomAttributes(typeof(RightAttribute), false)
                                 .FirstOrDefault() as RightAttribute;

        return attribute != null
            ? (attribute.Id, right.ToReadableString(), (int)right, attribute.RoleIds)
            : (0, string.Empty, 0, Array.Empty<int>());
    }
}

