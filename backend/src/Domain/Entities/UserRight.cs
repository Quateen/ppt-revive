namespace PPTRevive.Domain.Entities;

public class UserRight : BaseAuditableEntity
{
    public int UserId { get; set; }
    public int RightId { get; set; }
    public virtual User User { get; set; } = default!;
    public virtual Right Right { get; set; } = default!;
}
