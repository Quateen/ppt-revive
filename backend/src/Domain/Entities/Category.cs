using System.ComponentModel.DataAnnotations;

namespace PPTRevive.Domain.Entities;

public class Category : BaseAuditableEntity
{
    [StringLength(200)]
    public string? Name { get; set; }
    [StringLength(500)]
    public string? Description { get; set; }
    public int? DisplayOrder { get; set; }
    [StringLength(100)]
    public string? ColorCode { get; set; }
    public int EntityTypeId { get; set; }
    public int? ParentCategoryId { get; set; }
}
