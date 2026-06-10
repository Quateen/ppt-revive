using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using PPTRevive.Application.Common.Contracts;
using PPTRevive.Application.Common.Interfaces;
using PPTRevive.Application.Common.Models;
using PPTRevive.Domain.Entities;

namespace PPTRevive.Infrastructure.Repositories;
public class TokenRepository : ITokenRepository
{
    private readonly UserManager<User> _userManager;
    private readonly IApplicationDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public TokenRepository(
         UserManager<User> userManager,
         IApplicationDbContext dbContext,
        IConfiguration configuration
    )
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task<LoginAttemptContext> ApplyPolicy(User user, LoginAttempts attempt, AuthPolicy policy, string timezone)
    {
        var (token, expiry) = await GetToken(user, attempt, policy, timezone);
        var refreshToken = CheckAuthPolicyCompliance(user, attempt, policy)
            ? GetRefreshToken(user.Id, attempt.Id, policy)
            : string.Empty;

        return new LoginAttemptContext
        {
            Attempt = attempt,
            AccessToken = token,
            ExpiryDate = expiry,
            RefreshToken = refreshToken
        };
    }

    #region CLAIMS PROVIDED IN JWT TOKEN

    //usr: User's unique ID.
    //name: User's email.
    //req: The login attempt ID.
    //timezone: User's timezone (if provided).
    //amr: Indicates MFA if applicable.
    //auth-policy-compliance: Indicates if the user complies with authentication policies.
    //rights: A list of user rights in JSON format (both direct and role-based rights).
    //Role-related claim: The user's role ID, if available.

    #endregion

    private async Task<(string, DateTime)> GetToken(User user, LoginAttempts attempt, AuthPolicy policy, string timezone)
    {
        var claims = new List<Claim>();
        var expires = DateTime.UtcNow.AddMinutes(policy.BaseTokenDurationMinutes);

        claims.Add(new Claim("usr", user.Id.ToString()));
        claims.Add(new Claim("name", user.Email ?? string.Empty));
        claims.Add(new Claim("req", attempt.Id.ToString()));

        if (!string.IsNullOrEmpty(timezone))
        {
            claims.Add(new Claim("timezone", timezone));
        }

        if (attempt.TwoFacVerified)
        {
            claims.Add(new Claim("amr", "mfa"));
        }

        if (CheckAuthPolicyCompliance(user, attempt, policy))
        {
            claims.Add(new Claim("auth-policy-compliance", "true"));
            expires = DateTime.UtcNow.AddMinutes(policy.FullTokenDurationMinutes);

            var userPermissions = await _dbContext.Users
                .Where(u => u.Id == user.Id)
                .Select(u => new
                {
                    u.Id,
                    DirectRights = u.UserRights
                        .Select(ur => ur.Right.Id), // Selecting the Right Ids directly
                    RoleRights = u.UserRoles
                        .SelectMany(ur => ur.Role.RoleRights)
                        .Select(rr => rr.Right.Id), // Selecting the Right Ids directly
                    Role = u.UserRoles
                        .Select(ur => ur.Role.Id) // Selecting the Role Id directly
                        .FirstOrDefault() // Getting the first Role Id (assuming one role)
                })
                .FirstOrDefaultAsync();

            if (userPermissions != null)
            {
                var distinctRights = userPermissions.DirectRights
                    .Union(userPermissions.RoleRights)
                    .Distinct();

                IList<int> rights = distinctRights.ToList();
                string rightsJson = JsonConvert.SerializeObject(rights);
                claims.Add(new Claim("rights", rightsJson, JsonClaimValueTypes.JsonArray));

                // Add RoleId to the claims (if a role is present)
                if (userPermissions.Role != 0) // 0 means no role assigned
                {
                    claims.Add(new Claim(ClaimTypes.Role, userPermissions.Role.ToString()));
                }
            }
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:AdminSecretKey"] ?? string.Empty));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var notBefore = DateTime.UtcNow;

        var token = new JwtSecurityToken(
            _configuration["Jwt:Issuer"],
            _configuration["Jwt:Audience"],
            claims,
            expires: expires,
            notBefore: notBefore,
            signingCredentials: credentials
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }

    private string GetRefreshToken(int userId, int tokenIdentifier, AuthPolicy policy)
    {
        var claims = new List<Claim>
    {
        new Claim("usr", userId.ToString()),
        new Claim("req", tokenIdentifier.ToString())
    };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:AdminSecretKey"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(policy.RefreshTokenDurationMinutes);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool CheckAuthPolicyCompliance(User user, LoginAttempts attempt, AuthPolicy policy)
    {
        if (user.PasswordChangedAt is null && policy.EnforcePasswordChangeOnFirstLogin)
        {
            return false;
        }

        if (policy.Enforce2FactorVerification)
        {
            if (user.TwoFactorEnabled == false)
            {
                return false;
            }

            if (attempt.TwoFacVerified == false)
            {
                return false;
            }
        }

        if (policy.EnforceEmailConfirmation && user.EmailConfirmed == false)
        {
            return false;
        }

        if (policy.EnforceMobileConfirmation && user.PhoneNumberConfirmed == false)
        {
            return false;
        }

        if (policy.EnforceProfileCompletion && user.IsProfileCompleted == false)
        {
            return false;
        }

        return true;
    }

    public AuthResponse GetAuthResponse(User user, LoginAttemptContext context, AuthPolicy policy)
    {
        var response = new AuthResponse
        {
            IsSuccess = true,
            UserId = user.Id,
            SessionId = context.Attempt.Id,
            AccessToken = context.AccessToken,
            RefreshToken = context.RefreshToken,
            ExpiryDate = context.ExpiryDate,
        };

        if (user.PasswordChangedAt is null && policy.EnforcePasswordChangeOnFirstLogin)
        {
            response.EnforcePasswordChangeOnFirstLogin = true;
        }

        if (policy.Enforce2FactorVerification)
        {
            if (user.TwoFactorEnabled == false)
            {
                response.Enforce2FactorConfiguration = true;
            }
            else if (context.Attempt.TwoFacVerified == false)
            {
                response.Enforce2FactorVerification = true;
            }
        }

        if (policy.EnforceEmailConfirmation && user.EmailConfirmed == false)
        {
            response.EnforceEmailConfirmation = true;
        }

        if (policy.EnforceMobileConfirmation && user.PhoneNumberConfirmed == false)
        {
            response.EnforceMobileConfirmation = true;
        }

        if (policy.EnforceProfileCompletion && user.IsProfileCompleted == false)
        {
            response.EnforceProfileCompletion = true;
        }

        return response;
    }
}
