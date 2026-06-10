using System.Linq.Expressions;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using PPTRevive.Application.Common.Interfaces;
using PPTRevive.Application.Common.Models;
using PPTRevive.Domain.Entities;
using PPTRevive.Domain.Enums;
using PPTRevive.Domain.Common;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;

namespace PPTRevive.Infrastructure.Identity;

public class IdentityService(
    UserManager<User> userManager,
    IUserClaimsPrincipalFactory<User> userClaimsPrincipalFactory,
    IAuthorizationService authorizationService,
    IApplicationDbContext dbContext) : IIdentityService
{
    public async Task<string?> GetUserNameAsync(int userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());

        return user?.UserName;
    }

    public async Task<(Result Result, int UserId)> CreateUserAsync(string userName, string email, string password)
    {
        var displayName = userName ?? string.Empty;
        var nameParts = displayName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var firstName = nameParts.Length > 0 ? nameParts[0] : string.Empty;
        var lastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : string.Empty;

        var user = new User
        {
            DisplayName = displayName,
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            //PhoneNumber = "03XXXXXXXXX",
            UserTypeId = 2,// (int)UserType.User,
            AuthKey = TypeExtensions.GenerateRandomPassword(),
            SmsKey = TypeExtensions.GenerateRandomPassword(),
            EmailKey = TypeExtensions.GenerateRandomPassword(),
            TwoFactorEnabled = false,
            EmailConfirmed = false,
            PhoneNumberConfirmed = true,
            IsProfileCompleted = true,
            LockoutEnabled = false,
            CreatedBy = 1,
            Created = DateTime.UtcNow,
            LastModifiedBy = 1,
            LastModified = DateTime.UtcNow,
        };

        var result = await userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            UserProfile profile = new()
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                UserId = user.Id,
                Created = DateTimeOffset.UtcNow
            };

            dbContext.UserProfile.Add(profile);
            await dbContext.SaveChangesAsync(CancellationToken.None);
        }

        return (result.ToApplicationResult(), user.Id);
    }

    public async Task<(Result Result, int UserId)> CreateUserAsync(User user, string password)
    {
        var result = await userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            UserProfile profile = new()
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                UserId = user.Id,
                Created = DateTimeOffset.UtcNow
            };

            dbContext.UserProfile.Add(profile);
            await dbContext.SaveChangesAsync(CancellationToken.None);
        }

        return (result.ToApplicationResult(), user.Id);
    }

    public async Task<(Result Result, int UserId)> CreateUserAsync(string userName, string email, int userType)
    {
        var user = new User
        {
            UserName = userName,
            Email = userName,
            //UserType = userType
        };

        var result = await userManager.CreateAsync(user);

        return (result.ToApplicationResult(), user.Id);
    }

    public async Task<bool> IsInRoleAsync(int userId, string role)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());

        return user != null && await userManager.IsInRoleAsync(user, role);
    }

    public async Task<bool> AuthorizeAsync(int userId, string policyName)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user == null)
        {
            return false;
        }

        var principal = await userClaimsPrincipalFactory.CreateAsync(user);

        var result = await authorizationService.AuthorizeAsync(principal, policyName);

        return result.Succeeded;
    }

    public async Task<Result> DeleteUserAsync(int userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());

        return user != null ? await DeleteUserAsync(user) : Result.Success();
    }

    public async Task<Result> DeleteUserAsync(User user)
    {
        var result = await userManager.DeleteAsync(user);

        return result.ToApplicationResult();
    }

    public async Task<List<UserDto>> GetUsersAsync(int userId = 0)
    {
        await Task.Delay(1);
        var userDtos = new List<UserDto>();
        //var users = await _userManager.Users.ToListAsync();

        //foreach (var user in users)
        //{
        //    var result = await _userManager.HasPasswordAsync(user);
        //    var userTypeName = user.UserType == (int)UserType.Employee ? "Employee" : "Doctor";

        //    var userDto = new UserDto
        //    {
        //        Id = user.Id,
        //        UserName = user.UserName ?? string.Empty,
        //        Email = user.Email,
        //        UserType = user.UserType,
        //        UserTypeName = userTypeName,
        //        JoiningDate = user.JoiningDate,
        //        IsAccountCreated = result
        //    };

        //    userDtos.Add(userDto);
        //}

        //if (userId > 0)
        //    return userDtos.Where(x => x.Id == userId).ToList();

        return userDtos.ToList();
    }

    public async Task<string?> CreateEmailConfirmToken(int userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user != null)
        {
            //if (!user.EmailConfirmed)
            {
                var emailConfirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
                return emailConfirmationToken;
            }
        }

        return null;
    }

    public async Task<bool> CompleteEmailConfirmation(int userId, string token)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user != null)
        {
            var identityResult = await userManager.ConfirmEmailAsync(user, token);
            return identityResult.Succeeded;
        }

        return false;
    }

    public async Task<string?> CreatePasswordResetToken(int userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user != null)
        {
            return await userManager.GeneratePasswordResetTokenAsync(user);
        }

        return null;
    }

    public async Task<bool> CompletePasswordReset(int userId, string token, string newPassword)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user != null)
        {
            var identityResult = await userManager.ResetPasswordAsync(user, token, newPassword);
            return identityResult.Succeeded;
        }

        return false;
    }

    public async Task<bool> SetPassword(int userId, string password)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user != null)
        {
            var identityResult = await userManager.AddPasswordAsync(user, password);
            return identityResult.Succeeded;
        }

        return false;
    }

    public async Task<bool> CreateAccountPassword(int userId, string password)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user != null)
        {
            var hasPassword = await userManager.HasPasswordAsync(user);
            if (!hasPassword)
            {
                var identityResult = await userManager.AddPasswordAsync(user, password);
                return identityResult.Succeeded;
            }
        }

        return false;
    }

    public async Task<UserDto?> GetUserByEmail(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user != null)
        {
            var userDto = new UserDto
            {
                Id = user.Id,
                //Username = user.UserName ?? string.Empty,
                //Email = user.Email,
                //UserType = user.UserType,
                //JoiningDate = user.JoiningDate,
            };
            return userDto;
        }

        return null;
    }
    public async Task<UserDto?> ValidatePassword(string email, string password)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user != null)
        {
            var uservalidate = await userManager.CheckPasswordAsync(user, password);
            if (uservalidate)
            {
                var userDto = new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName ?? string.Empty,
                    LastName = user.LastName ?? string.Empty,
                    Email = user.Email,
                    //JoiningDate = user.JoiningDate,
                };
                return userDto;
            }
        }

        return null;
    }
    public async Task<bool> ChangePassword(int userId, string oldPassword, string newPassword)
    {

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user != null)
        {
            var identityResult = await userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            return identityResult.Succeeded;
        }

        return false;

    }

    public async Task<int> GetUserCountAsync(Expression<Func<User, bool>>? predicate = null)
    {
        return await Task.FromResult(
            predicate == null
                ? userManager.Users.Count()
                : userManager.Users.Count(predicate)
        );
    }
}
