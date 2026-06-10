namespace PPTRevive.Application.Common.Models;
public class RoleDto
{
    public int RoleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<RoleRightDto> RoleRights { get; set; } = new List<RoleRightDto>();
    public DateTimeOffset Created { get; set; }
    public int? CreatedBy { get; set; }
    public DateTimeOffset LastModified { get; set; }
    public int? LastModifiedBy { get; set; }
}

public class RoleRightDto
{
    public int RoleRightId { get; set; }
    public int RightId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
