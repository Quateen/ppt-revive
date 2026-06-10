using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using PPTRevive.Application.Common.Behaviours;
using PPTRevive.Application.Common.Contracts;
using PPTRevive.Application.Common.Interfaces;
using PPTRevive.Application.Common.Models;
using PPTRevive.Domain.Common;
using PPTRevive.Domain.Entities;
using PPTRevive.Domain.Enums;
using AuthPolicyModel = PPTRevive.Domain.Entities.AuthPolicy;

namespace PPTRevive.Application.Auth.Command.GoogleLogin;
public class GoogleLoginCommand : IRequest<ResponseBase>
{
    public string SocialLogin { get; set; } = string.Empty;
    public string? GivenName { get; set; }
    public string? GoogleLocal { get; set; }
    public string? FamilyName { get; set; }
    public string? GooglePicture { get; set; }
    public string? GoogleAud { get; set; }
    public string? GoogleAzp { get; set; }
    public string? GoogleExp { get; set; }
    public string? GoogleIat { get; set; }
    public string? GoogleIss { get; set; }
    public string? GoogleSub { get; set; }
    public string? GoogleName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? IdToken { get; set; }
    public string? DeviceToken { get; set; }
    public string? CurrentBrowserTimeZone { get; set; } = string.Empty;
}

public class GoogleLoginCommandHandler(
    UserManager<User> userManager,
    IIdentityService identityService,
    ITokenRepository tokenRepository,
    IQueryRepository<AuthPolicyModel> authPolicyRepository,
    IDataRepository<LoginAttempts> loginAttemptsRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<GoogleLoginCommand, ResponseBase>
{

    private ResponseBase ErrorResponse(string error)
    {
        return new ResponseBase
        {
            Status = false,
            Error = error
        };
    }

    public async Task<ResponseBase> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.SocialLogin != "google")
            {
                return ResponseHelper.ErrorResponse(AppMessage.EmailOrPasswordIsIncorrect.GetDescription(),
                                                    (int)AppMessage.EmailOrPasswordIsIncorrect);
            }

            // Verify the Google ID token
            var jwtToken = new JwtSecurityToken(request.IdToken);
            var payload = jwtToken.Payload;

            if (payload == null)
            {
                return ResponseHelper.ErrorResponse(AppMessage.InvalidGoogleIdToken.GetDescription(),
                                                    (int)AppMessage.InvalidGoogleIdToken);
            }

            var email = payload["email"].ToString();
            if (email != request.Email)
            {
                return ResponseHelper.ErrorResponse(AppMessage.EmailMismatch.GetDescription(),
                                                    (int)AppMessage.EmailMismatch);
            }

            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                var newUser = new User
                {
                    DisplayName = request.GivenName ?? string.Empty,
                    UserName = request.Email,
                    Email = request.Email,
                    EmailConfirmed = true,
                    FirstName = string.Empty,
                    LastName = request.FamilyName ?? string.Empty,
                    //PhoneNumber = "03XXXXXXXXX",
                    UserTypeId = 2,// (int)UserType.User,
                    AuthKey = TypeExtensions.GenerateRandomPassword(),
                    SmsKey = TypeExtensions.GenerateRandomPassword(),
                    EmailKey = TypeExtensions.GenerateRandomPassword(),
                    TwoFactorEnabled = false,
                    PhoneNumberConfirmed = true,
                    IsProfileCompleted = true,
                    LockoutEnabled = false,
                    CreatedBy = 1,
                    Created = DateTime.UtcNow,
                    LastModifiedBy = 1,
                    LastModified = DateTime.UtcNow,
                    GooglePicture = request.GooglePicture,
                    GoogleAud = request.GoogleAud,
                    GoogleAzp = request.GoogleAzp,
                    GoogleExp = request.GoogleExp,
                    GoogleIat = request.GoogleIat,
                    GoogleIss = request.GoogleIss,
                    GoogleSub = request.GoogleSub,
                    SocialApp = request.SocialLogin,
                };

                var userCreateResult = await identityService.CreateUserAsync(newUser, "Asdf@1234");

                if (!userCreateResult.Result.Succeeded)
                    return ResponseHelper.ErrorResponse(userCreateResult.Result.Errors[0],
                                                        (int)AppMessage.FailedToRegisterUser);

                user = newUser;
                user.Id = userCreateResult.UserId;
            }

            LoginAttempts attempt = new()
            {
                IpAddress = string.Empty,
                ClientInformation = string.Empty,
                AttemptDate = DateTime.UtcNow,
                LoginType = (int)LoginType.FreshLogin,
                TwoFacVerified = true
            };

            var policy = await authPolicyRepository.GetAsync(x => x.UserType == user.UserTypeId);
            if (policy == null)
                return ErrorResponse("Authentication policy not found.");
            var context = await tokenRepository.ApplyPolicy(user, attempt, policy, request.CurrentBrowserTimeZone ?? string.Empty);
            context.Attempt.IsSuccess = true;
            context.Attempt.UserId = user.Id;

            user.LastLoginAttempt = context.Attempt.Id;
            user.LastLoginDate = DateTime.UtcNow;
            user.LastModifiedBy = user.Id;

            await SaveAttempt(context.Attempt);

            var authResponse = tokenRepository.GetAuthResponse(user, context, policy);

            return new ResponseBase()
            {
                Status = true,
                Data = new AuthResponse
                {
                    IsSuccess = true,
                    UserId = user.Id,
                    Name = user.DisplayName,
                    Email = user.Email ?? string.Empty,
                    SessionId = attempt.Id,
                    AccessToken = authResponse.AccessToken,
                    RefreshToken = authResponse.RefreshToken,
                    ExpiryDate = authResponse.ExpiryDate,
                    EnforceEmailConfirmation = policy.EnforceEmailConfirmation,
                    EnforceMobileConfirmation = policy.EnforceMobileConfirmation,
                    Enforce2FactorVerification = policy.Enforce2FactorVerification,
                    EnforcePasswordChangeOnFirstLogin = policy.EnforcePasswordChangeOnFirstLogin,
                    EnforceProfileCompletion = policy.EnforceProfileCompletion,
                },
                Message = AppMessage.LoginSuccessful.GetDescription()
            };

        }
        catch (Exception ex)
        {
            return ResponseHelper.ErrorResponse(ex.Message, (int)AppMessage.UnknownError);
        }
    }

    private async Task SaveAttempt(LoginAttempts attempt)
    {
        attempt.CreatedBy = attempt.UserId;
        attempt.LastModifiedBy = attempt.UserId;

        loginAttemptsRepository.Add(attempt, attempt.UserId);
        await unitOfWork.SaveChangesAsync(default);
    }
}
