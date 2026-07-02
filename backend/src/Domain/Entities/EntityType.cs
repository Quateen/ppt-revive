using System.ComponentModel.DataAnnotations;

namespace PPTRevive.Domain.Entities;

public class EntityType : BaseAuditableEntity
{
    [StringLength(200)]
    public string? Name { get; set; }
    public bool IsActive { get; set; }
    public int? EntityTypeParentId { get; set; }
}
