namespace PPTRevive.Domain.Entities;

public class RoleRight : BaseAuditableEntity
{
    public int RoleId { get; set; }
    public int RightId { get; set; }
    public virtual Role Role { get; set; } = default!;
    public virtual Right Right { get; set; } = default!;
}
