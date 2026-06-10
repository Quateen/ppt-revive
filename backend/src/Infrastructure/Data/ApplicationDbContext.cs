using System.Reflection;
using System.Reflection.Emit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PPTRevive.Application.Common.Interfaces;
using PPTRevive.Domain.Entities;

namespace PPTRevive.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    public new DbSet<User> Users => base.Set<User>();
    public DbSet<Role> Role => Set<Role>();
    public DbSet<Right> Right => Set<Right>();
    public DbSet<RoleRight> RoleRight => Set<RoleRight>();
    public DbSet<UserRole> UserRole => Set<UserRole>();
    public DbSet<UserRight> UserRight => Set<UserRight>();
    public DbSet<UserProfile> UserProfile => Set<UserProfile>();
    public DbSet<AuthPolicy> AuthPolicy => Set<AuthPolicy>();
    public DbSet<LoginAttempts> LoginAttempts => Set<LoginAttempts>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<UserSubscription> UserSubscriptions => Set<UserSubscription>();

    //public DbSet<Category> Category => Set<Category>();
    //public DbSet<EntityType> EntityType => Set<EntityType>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Exclude unnecessary tables by mapping them to null
        builder.Entity<IdentityRole<int>>().ToTable((string?)null); // Role table
        builder.Entity<IdentityUserRole<int>>().ToTable((string?)null); // UserRole table
        builder.Entity<IdentityUserClaim<int>>().ToTable((string?)null); // UserClaim table
        builder.Entity<IdentityUserToken<int>>().ToTable((string?)null); // UserToken table
        builder.Entity<IdentityRoleClaim<int>>().ToTable((string?)null); // RoleClaim table
        builder.Entity<IdentityUserLogin<int>>().ToTable((string?)null); // UserLogin table (if unused)

        // Map User to the default AspNetUsers table
        builder.Entity<User>(entity =>
        {
            entity.ToTable("User"); // Optional: Rename the table if needed
        });

        // User
        builder.Entity<User>(entity =>
        {
            entity.HasOne(u => u.UserProfile)                   // Principal entity
                .WithOne(up => up.User)                         // Dependent entity
                .HasForeignKey<UserProfile>(up => up.UserId)    // Specify foreign key in UserProfile
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(u => u.UserRights)           // Principal entity
                .WithOne(ur => ur.User)                 // Dependent entity
                .HasForeignKey(ur => ur.UserId)         // Specify foreign key in UserProfile
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(u => u.UserRoles)            // Principal entity
                .WithOne(ur => ur.User)                 // Dependent entity
                .HasForeignKey(ur => ur.UserId)         // Specify foreign key in UserProfile
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(u => u.UserSubscriptions)
                  .WithOne(us => us.User)
                  .HasForeignKey(us => us.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

        });

        builder.Entity<Role>(entity =>
        {
            entity.HasMany(r => r.RoleRights)           // Principal entity
                .WithOne(rr => rr.Role)                 // Dependent entity
                .HasForeignKey(rr => rr.RoleId)         // Specify foreign key in UserProfile
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(r => r.UserRoles)            // Principal entity
                .WithOne(ur => ur.Role)                 // Dependent entity
                .HasForeignKey(ur => ur.RoleId)         // Specify foreign key in UserProfile
                .OnDelete(DeleteBehavior.NoAction);

        });

        builder.Entity<Right>(entity =>
        {
            entity.HasMany(r => r.RoleRights)           // Principal entity
                .WithOne(rr => rr.Right)                // Dependent entity
                .HasForeignKey(rr => rr.RightId)        // Specify foreign key in UserProfile
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(r => r.UserRights)           // Principal entity
                .WithOne(ur => ur.Right)                // Dependent entity
                .HasForeignKey(ur => ur.RightId)        // Specify foreign key in UserProfile
                .OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                  .ValueGeneratedNever(); // disables identity/auto-increment

            entity.HasMany(s => s.UserSubscriptions)
                  .WithOne(us => us.Subscription)
                  .HasForeignKey(us => us.SubscriptionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<UserSubscription>(entity =>
        {
            entity.Property(e => e.Id)
                  .ValueGeneratedOnAdd();

            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Subscription)
                  .WithMany()
                  .HasForeignKey(e => e.SubscriptionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
