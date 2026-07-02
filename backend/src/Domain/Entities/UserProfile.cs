
namespace PPTRevive.Domain.Entities;

public class UserProfile : BaseAuditableEntity
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public int Gender { get; set; }
    public virtual User? User { get; set; }
}
