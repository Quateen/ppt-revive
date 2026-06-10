using System.Linq.Expressions;
using PPTRevive.Application.Common.Models;
using PPTRevive.Domain.Entities;

namespace PPTRevive.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(int userId);

    Task<bool> IsInRoleAsync(int userId, string role);

    Task<bool> AuthorizeAsync(int userId, string policyName);

    Task<(Result Result, int UserId)> CreateUserAsync(string userName, string email, string password);
    Task<(Result Result, int UserId)> CreateUserAsync(User user, string password);
    Task<(Result Result, int UserId)> CreateUserAsync(string userName, string email, int userType);
    Task<Result> DeleteUserAsync(int userId);
    Task<List<UserDto>> GetUsersAsync(int userId = 0);
    Task<string?> CreateEmailConfirmToken(int userId);
    Task<bool> CompleteEmailConfirmation(int userId, string token);
    Task<string?> CreatePasswordResetToken(int userId);
    Task<bool> CompletePasswordReset(int userId, string token, string newPassword);
    Task<bool> SetPassword(int userId, string password);
    Task<bool> CreateAccountPassword(int userId, string password);
    Task<UserDto?> GetUserByEmail(string email);
    Task<UserDto?> ValidatePassword(string email, string password);
    Task<bool> ChangePassword(int userId, string oldPassword, string newPassword);
    Task<int> GetUserCountAsync(Expression<Func<User, bool>>? predicate = null);
}
