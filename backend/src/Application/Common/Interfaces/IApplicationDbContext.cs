using PPTRevive.Domain.Entities;
using AuthPolicyModel = PPTRevive.Domain.Entities.AuthPolicy;

namespace PPTRevive.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Role { get; }
    DbSet<Right> Right { get; }
    DbSet<RoleRight> RoleRight { get; }
    DbSet<UserRole> UserRole { get; }
    DbSet<UserRight> UserRight { get; }
    DbSet<UserProfile> UserProfile { get; }
    public DbSet<LoginAttempts> LoginAttempts { get; }
    public DbSet<AuthPolicyModel> AuthPolicy { get; }
    public DbSet<Subscription> Subscriptions { get; }
    public DbSet<UserSubscription> UserSubscriptions { get; }

    //DbSet<Category> Category { get; }
    //DbSet<EntityType> EntityType { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
