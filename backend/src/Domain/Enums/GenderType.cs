using System.ComponentModel;

namespace PPTRevive.Domain.Enums;
public enum GenderType
{
    [Description("Others")]
    NotSpecified = 0,
    [Description("Male")]
    Male = 1,
    [Description("Female")]
    Female = 2
}
