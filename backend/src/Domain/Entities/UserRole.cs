namespace PPTRevive.Domain.Entities;

public class UserRole : BaseAuditableEntity
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
    public virtual User User { get; set; } = default!;
    public virtual Role Role { get; set; } = default!;
}
