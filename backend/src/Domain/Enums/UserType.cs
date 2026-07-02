using System.ComponentModel;

namespace PPTRevive.Domain.Enums;
public enum UserType
{
    [Description("Admin")]
    Admin = 1,
    [Description("User")]
    User = 2,
    [Description("Guest")]
    Guest = 3
}
