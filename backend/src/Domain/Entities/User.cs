using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace PPTRevive.Domain.Entities;

public class User : IdentityUser<int>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Address { get; set; } = string.Empty;
    public DateTime? LastLoginDate { get; set; }
    public DateTime? PasswordChangedAt { get; set; }
    public int UserTypeId { get; set; }
    public string AuthKey { get; set; } = string.Empty;
    public string SmsKey { get; set; } = string.Empty;
    public string EmailKey { get; set; } = string.Empty;
    public bool IsProfileCompleted { get; set; }
    public int? LastLoginAttempt { get; set; }
    [StringLength(200)]
    public string? GooglePicture { get; set; }
    [StringLength(200)]
    public string? GoogleAud { get; set; }
    [StringLength(200)]
    public string? GoogleAzp { get; set; }
    [StringLength(200)]
    public string? GoogleExp { get; set; }
    [StringLength(200)]
    public string? GoogleIat { get; set; }
    [StringLength(200)]
    public string? GoogleIss { get; set; }
    [StringLength(200)]
    public string? GoogleSub { get; set; }
    [StringLength(200)]
    public string? SocialApp { get; set; }

    // User Management Navigation Properties
    public virtual UserProfile UserProfile { get; set; } = default!;
    public virtual ICollection<UserRight> UserRights { get; set; } = new List<UserRight>();
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<UserSubscription> UserSubscriptions { get; set; } = default!;

    #region BASE ENTITY

    public DateTimeOffset Created { get; set; }
    public int? CreatedBy { get; set; }
    public DateTimeOffset LastModified { get; set; }
    public int? LastModifiedBy { get; set; }

    #endregion BASE ENTITY
}
