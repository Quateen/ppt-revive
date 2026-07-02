using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using PPTRevive.Domain.Common;
using PPTRevive.Domain.Entities;
using PPTRevive.Domain.Enums;
using UserRoleModel = PPTRevive.Domain.Entities.UserRole;

namespace PPTRevive.Infrastructure.Data;
public class DataSeeder
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;

    public DataSeeder(
        ApplicationDbContext dbContext,
        UserManager<User> userManager,
        IConfiguration configuration
    )
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _configuration = configuration;
    }

    public void Seed()
    {
        try
        {
            SeedUsers();
            SeedAuthPolicy();
            //SeedRoles();
            //SeedRights();
            //SeedUserRoles();
            //SeedRoleRights();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Seeding failed: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }

    private bool CheckTablesExist(string tableName)
    {
        if (!_dbContext.Database.CanConnect())
            return false;

        var connection = _dbContext.Database.GetDbConnection();
        var dbName = new MySqlConnectionStringBuilder(connection.ConnectionString).Database;

        var sqlCommand = $"SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = '{dbName}' AND table_name = '{tableName}';";
        using (var command = connection.CreateCommand())
        {
            command.CommandText = sqlCommand;
            _dbContext.Database.OpenConnection();

            var result = command.ExecuteScalar();
            return Convert.ToInt32(result) > 0;
        }
    }

    #region USER

    private void SeedUsers()
    {
        if (!CheckTablesExist("user"))
        {
            Console.WriteLine("Users table does not exist yet. Skipping seeding...");
            return;
        }

        var usersToSeed = new List<(string Email, string FirstName, string LastName, string DisplayName, UserType UserType)>
        {
            //("admin@ts.com", "Admin", "TS", "Admin TS", UserType.Admin),
            ("app@user.com", "App", "User", "App User", UserType.User),
            //("user2@ts.com", "User 2", "TS", "User TS", UserType.User),
            //("user3@ts.com", "User 3", "TS", "User TS", UserType.User),
            //("user4@ts.com", "User 4", "TS", "User TS", UserType.User),
            //("guest@ts.com", "Guest", "TS", "Guest TS", UserType.Guest),
        };

        foreach (var (email, firstName, lastName, displayName, userType) in usersToSeed)
        {
            SeedUser(email, firstName, lastName, displayName, userType);
        }
    }

    private void SeedUser(string email, string firstName, string lastName, string displayName, UserType userType)
    {
        if (!CheckTablesExist("userprofile"))
        {
            Console.WriteLine("User Profile table does not exist yet. Skipping seeding...");
            return;
        }

        var existingUser = _userManager.FindByEmailAsync(email).GetAwaiter().GetResult();
        if (existingUser != null)
        {
            Console.WriteLine($"{displayName} already exists. Skipping seeding...");
            return;
        }

        var user = new User
        {
            FirstName = firstName,
            LastName = lastName,
            DisplayName = displayName,
            Email = email,
            UserName = email,
            PhoneNumber = "03XXXXXXXXX",
            UserTypeId = (int)userType,
            Address = "21st Jump Street",
            AuthKey = TypeExtensions.GenerateRandomPassword(),
            SmsKey = TypeExtensions.GenerateRandomPassword(),
            EmailKey = TypeExtensions.GenerateRandomPassword(),
            TwoFactorEnabled = true,
            EmailConfirmed = true,
            PhoneNumberConfirmed = true,
            IsProfileCompleted = true,
            LockoutEnabled = false,
            CreatedBy = 1,
            Created = DateTime.UtcNow,
            LastModifiedBy = 1,
            LastModified = DateTime.UtcNow,
        };

        // Seed the admin with a configured password, or a strong random one that is
        // logged once for first-login rotation — never a hardcoded default.
        var seedPassword = _configuration["Seed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(seedPassword))
        {
            seedPassword = TypeExtensions.GenerateRandomPassword();
            Console.WriteLine($"[Seed] Generated admin password for {email}: {seedPassword} — change it after first login.");
        }

        var result = _userManager.CreateAsync(user, seedPassword).GetAwaiter().GetResult();

        if (result.Succeeded)
        {
            Console.WriteLine($"{displayName} seeded successfully.");

            CreateUserProfileIfNotExists(user, firstName, lastName, email);
        }
        else
        {
            Console.WriteLine($"Failed to seed {displayName}:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"- {error.Code}: {error.Description}");
            }
        }
    }

    private void CreateUserProfileIfNotExists(User user, string firstName, string lastName, string email)
    {
        var userProfileExists = _dbContext.UserProfile.Any(up => up.UserId == user.Id);
        if (userProfileExists)
        {
            Console.WriteLine($"{user.DisplayName} profile already exists. Skipping seeding...");
            return;
        }

        var userProfile = new UserProfile
        {
            UserId = user.Id,
            FirstName = firstName,
            LastName = lastName,
            Address = "21st Jump Street",
            Email = email,
            MobileNumber = "03XXXXXXXXX",
            DateOfBirth = new DateOnly(1990, 1, 1),
            CreatedBy = 1,
            Created = DateTime.UtcNow,
            LastModifiedBy = 1,
            LastModified = DateTime.UtcNow,
        };

        _dbContext.UserProfile.Add(userProfile);
        _dbContext.SaveChanges();
        Console.WriteLine($"{user.DisplayName} profile seeded successfully.");
    }

    #endregion USER

    #region AUTH POLICY

    private void SeedAuthPolicy()
    {
        try
        {
            if (!CheckTablesExist("authpolicy"))
            {
                Console.WriteLine("AuthPolicies table does not exist. Skipping seeding...");
                return;
            }

            // Seed policies for each user type
            //SeedPolicyIfNotExist(UserType.Admin);
            SeedPolicyIfNotExist(UserType.User);
            //SeedPolicyIfNotExist(UserType.Guest);

            _dbContext.SaveChanges();
        }
        catch (Exception ex)
        {
            // Log exception for debugging or future analysis
            Console.WriteLine($"An error occurred while seeding AuthPolicies: {ex.Message}");
        }
    }

    private void SeedPolicyIfNotExist(UserType userType)
    {
        var existingPolicy = _dbContext.AuthPolicy.FirstOrDefault(ap => ap.UserType == (int)userType);

        if (existingPolicy == null)
        {
            existingPolicy = new AuthPolicy
            {
                UserType = (int)userType,
                CreatedBy = 1,  // Ideally should come from logged-in user or configuration
                Created = DateTime.UtcNow
            };

            _dbContext.AuthPolicy.Add(existingPolicy);
        }

        SetPolicyDefaults(existingPolicy);

        Console.WriteLine($"Policy for {userType} seeded successfully.");
    }

    private void SetPolicyDefaults(AuthPolicy policy)
    {
        policy.Enforce2FactorVerification = false;
        policy.EnforcePasswordChangeOnFirstLogin = false;
        policy.EnforceBackendActivation = false;
        policy.EnforceEmailConfirmation = false;
        policy.EnforceMobileConfirmation = false;
        policy.EnforceProfileCompletion = false;
        policy.BaseTokenDurationMinutes = 360;
        policy.FullTokenDurationMinutes = 1440;
        policy.RefreshTokenDurationMinutes = 2880;
    }

    #endregion AUTH POLICY

    #region ROLE, RIGHTS & USER ROLES/RIGHTS

    //public void SeedRoles()
    //{
    //    if (!CheckTablesExist("role"))
    //    {
    //        Console.WriteLine("Role table does not exist yet. Skipping seeding...");
    //        return;
    //    }

    //    var rolesToSeed = Enum.GetValues(typeof(ApplicationRole))
    //        .Cast<ApplicationRole>()
    //        .Select(role =>
    //        {
    //            var attr = role.ToRoleAttribute();
    //            return new Role
    //            {
    //                Id = attr.Id,
    //                Name = attr.Name,
    //                TypeId = attr.RoleTypeId,
    //                Description = attr.Description
    //            };
    //        }).ToList();

    //    var dbSet = _dbContext.Set<Role>();
    //    var existingRoles = dbSet.ToList();

    //    foreach (var seedRole in rolesToSeed)
    //    {
    //        var existing = existingRoles.FirstOrDefault(r => r.Id == seedRole.Id);

    //        if (existing == null)
    //        {
    //            dbSet.Add(seedRole);
    //        }
    //        else
    //        {
    //            if (existing.Name != seedRole.Name ||
    //                existing.TypeId != seedRole.TypeId ||
    //                existing.Description != seedRole.Description)
    //            {
    //                existing.Name = seedRole.Name;
    //                existing.TypeId = seedRole.TypeId;
    //                existing.Description = seedRole.Description;
    //            }
    //        }
    //    }

    //    _dbContext.SaveChanges();
    //}

    //public void SeedRights()
    //{
    //    if (!CheckTablesExist("right"))
    //    {
    //        Console.WriteLine("Right table does not exist yet. Skipping seeding...");
    //        return;
    //    }
    //    var rightsToSeed = Enum.GetValues(typeof(ApplicationRights))
    //        .Cast<ApplicationRights>()
    //        .Select(right =>
    //        {
    //            var attr = right.ToRightAttribute();
    //            return new Right
    //            {
    //                Id = attr.Id,
    //                Name = attr.Name,
    //                Description = attr.Name
    //            };
    //        }).ToList();

    //    var dbSet = _dbContext.Set<Right>();
    //    var existingRights = dbSet.ToList();
    //    foreach (var seedRight in rightsToSeed)
    //    {
    //        var existing = existingRights.FirstOrDefault(r => r.Id == seedRight.Id);
    //        if (existing == null)
    //        {
    //            dbSet.Add(seedRight);
    //        }
    //        else
    //        {
    //            if (existing.Name != seedRight.Name ||
    //                existing.Description != seedRight.Description)
    //            {
    //                existing.Name = seedRight.Name;
    //                existing.Description = seedRight.Description;
    //            }
    //        }
    //    }
    //    _dbContext.SaveChanges();
    //}

    //private void SeedUserRoles()
    //{
    //    if (!CheckTablesExist("userrole"))
    //    {
    //        Console.WriteLine("UserRoles table does not exist yet. Skipping seeding...");
    //        return;
    //    }

    //    var usersToRoles = new List<(string Email, ApplicationRole Role)>
    //    {
    //        ("admin@ts.com", ApplicationRole.WorkspaceAdmin),
    //        ("user@ts.com", ApplicationRole.WorkspaceMember),
    //        ("guest@ts.com", ApplicationRole.WorkspaceGuest),
    //        ("user2@ts.com", ApplicationRole.ProjectMember),
    //        ("user3@ts.com", ApplicationRole.ProjectMember),
    //        ("user4@ts.com", ApplicationRole.ProjectMember)
    //    };

    //    foreach (var (email, roleEnum) in usersToRoles)
    //    {
    //        var user = _userManager.FindByEmailAsync(email).GetAwaiter().GetResult();
    //        if (user == null)
    //        {
    //            Console.WriteLine($"User with email {email} not found. Skipping role assignment...");
    //            continue;
    //        }

    //        var role = _dbContext.Role.FirstOrDefaultAsync(r => r.Id == (int)roleEnum).GetAwaiter().GetResult();
    //        if (role == null)
    //        {
    //            Console.WriteLine($"Role {roleEnum} not found. Skipping role assignment...");
    //            continue;
    //        }

    //        var existing = _dbContext.UserRole
    //            .Any(ur => ur.UserId == user.Id && ur.RoleId == role.Id);

    //        if (existing)
    //        {
    //            Console.WriteLine($"{roleEnum} already assigned to {user.DisplayName}. Skipping...");
    //            continue;
    //        }

    //        var userRole = new UserRole
    //        {
    //            UserId = user.Id,
    //            RoleId = role.Id
    //        };

    //        _dbContext.UserRole.Add(userRole);
    //        _dbContext.SaveChanges();

    //        Console.WriteLine($"Successfully assigned {roleEnum} role to {user.DisplayName}");
    //    }
    //}

    //public void SeedRoleRights()
    //{
    //    if (!CheckTablesExist("roleright"))
    //    {
    //        Console.WriteLine("RoleRight table does not exist yet. Skipping seeding...");
    //        return;
    //    }

    //    var roleRightDbSet = _dbContext.Set<RoleRight>();
    //    var existingRoleRights = roleRightDbSet.ToList();

    //    var desiredRoleRights = Enum.GetValues(typeof(ApplicationRights))
    //        .Cast<ApplicationRights>()
    //        .SelectMany(right =>
    //        {
    //            var attr = right.ToRightAttribute();
    //            return attr.RoleIds.Select(roleId => new RoleRight
    //            {
    //                RoleId = roleId,
    //                RightId = attr.Id
    //            });
    //        })
    //        .ToList();

    //    var toAdd = desiredRoleRights
    //        .Where(rr => !existingRoleRights.Any(er => er.RoleId == rr.RoleId && er.RightId == rr.RightId))
    //        .ToList();

    //    var toRemove = existingRoleRights
    //        .Where(er => !desiredRoleRights.Any(rr => rr.RoleId == er.RoleId && rr.RightId == er.RightId))
    //        .ToList();

    //    if (toRemove.Any())
    //        roleRightDbSet.RemoveRange(toRemove);

    //    if (toAdd.Any())
    //        roleRightDbSet.AddRange(toAdd);

    //    if (toAdd.Any() || toRemove.Any())
    //        _dbContext.SaveChanges();

    //    Console.WriteLine($"Role Rights seeded successfully");
    //}

    #endregion ROLE, RIGHTS & USER ROLES/RIGHTS
}
