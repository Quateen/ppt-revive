using System.ComponentModel.DataAnnotations;

namespace PPTRevive.Domain.Entities;

public class Right : BaseAuditableEntity
{
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    public virtual ICollection<RoleRight> RoleRights { get; set; } = new List<RoleRight>();
    public virtual ICollection<UserRight> UserRights { get; set; } = new List<UserRight>();
}
