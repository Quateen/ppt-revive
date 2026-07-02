using PPTRevive.Application.Common.Contracts;
using PPTRevive.Application.Common.Models;
using PPTRevive.Domain.Entities;
using AuthPolicyModel = PPTRevive.Domain.Entities.AuthPolicy;

namespace PPTRevive.Application.Common.Interfaces;
public interface ITokenRepository
{
    Task<LoginAttemptContext> ApplyPolicy(User user, LoginAttempts attempt, AuthPolicyModel policy, string timezone);
    AuthResponse GetAuthResponse(User user, LoginAttemptContext context, AuthPolicyModel policy);
    bool CheckAuthPolicyCompliance(User user, LoginAttempts attempt, AuthPolicyModel policy);
}
