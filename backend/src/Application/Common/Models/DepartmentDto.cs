namespace PPTRevive.Application.Common.Models;
public class DepartmentDto
{
    public int DepartmentId { get; set; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; } = string.Empty;
    public List<DepartmentRoleDto> DepartmentRole { get; set; } = new List<DepartmentRoleDto>();
    public DateTimeOffset Created { get; set; }
    public int? CreatedBy { get; set; }
    public DateTimeOffset LastModified { get; set; }
    public int? LastModifiedBy { get; set; }
    public int DepartmentRoleId { get; set; }
    public string RoleNames { get; init; } = string.Empty;
}

public class DepartmentRoleDto
{
    public int DepartmentRoleId { get; set; }
    public int RoleId { get; set; }
    public int DepartmentId { get; set; }
    public string Name { get; init; } = string.Empty;
    public string DepartmentRoleName { get; set; } = string.Empty; 
    public string? Description { get; set; }
    public bool IsDeleted { get; init; } = false;
}
