using Microsoft.AspNetCore.Identity;
using PPTRevive.Application.Common.Contracts;
using PPTRevive.Application.Common.Interfaces;
using PPTRevive.Application.Common.Models;
using PPTRevive.Domain.Common;
using PPTRevive.Domain.Entities;
using AuthPolicyModel = PPTRevive.Domain.Entities.AuthPolicy;
using PPTRevive.Domain.Enums;
using PPTRevive.Application.Common.Behaviours;

namespace PPTRevive.Application.Auth.Command.Login;

public record LoginCommand : IRequest<ResponseBase>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    //public int UserType { get; set; }
    public string ClientInformation { get; set; } = string.Empty;
    public string? CurrentBrowserTimeZone { get; set; } = string.Empty;
}

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .Matches(@"[A-Z]").WithMessage("Must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Must contain at least one lowercase letter.")
            .Matches(@"\d").WithMessage("Must contain at least one number.")
            .Matches(@"[!@#$%^&*(),.?:{}|<>]").WithMessage("Must contain at least one special character.");

        //RuleFor(x => x.UserType)
        //    .GreaterThan(0).WithMessage("Invalid user type.");
    }
}

public class LoginCommandHandler(
    ITokenRepository tokenRepository,
    UserManager<User> userManager,
    IQueryRepository<AuthPolicyModel> authPolicyRepository,
    IDataRepository<LoginAttempts> loginAttemptsRepository,
    IUnitOfWork unitOfWork
        ) : IRequestHandler<LoginCommand, ResponseBase>
{
    private ResponseBase ErrorResponse(string error)
    {
        return new ResponseBase
        {
            Status = false,
            Error = error
        };
    }
    public async Task<ResponseBase> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
           
            LoginAttempts attempt = new()
            {
                IpAddress = request.IpAddress,
                ClientInformation = request.ClientInformation,
                AttemptDate = DateTime.UtcNow,
                LoginType = (int)LoginType.FreshLogin,
                TwoFacVerified = true
            };

            
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return ResponseHelper.ErrorResponse(AppMessage.EmailOrPasswordIsIncorrect.GetDescription(), (int)AppMessage.EmailOrPasswordIsIncorrect);

          
            if (!user.EmailConfirmed)
            {
                attempt.IsSuccess = false;
                attempt.RejectionType = (int)RejectionType.EmailNotConfirmed;
                attempt.UserId = user.Id;

                await SaveAttempt(attempt); 
                return ResponseHelper.ErrorResponse(AppMessage.EmailNotConfirmed.GetDescription(), (int)AppMessage.EmailNotConfirmed);
            }

            
            if (await userManager.IsLockedOutAsync(user))
            {
                attempt.IsSuccess = false;
                attempt.RejectionType = (int)RejectionType.AccountLocked;
                attempt.UserId = user.Id;

                await SaveAttempt(attempt);
                return ResponseHelper.ErrorResponse(AppMessage.AccountAlreadyLocked.GetDescription(), (int)AppMessage.AccountAlreadyLocked);
            }

          
            var passwordValid = await userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordValid)
            {
                await userManager.AccessFailedAsync(user);

                if (user.AccessFailedCount >= 5)
                {
                    await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddMinutes(15));
                }

                attempt.IsSuccess = false;
                attempt.RejectionType = (int)RejectionType.WrongPassword;
                attempt.UserId = user.Id;

                await SaveAttempt(attempt);
                return ResponseHelper.ErrorResponse(AppMessage.EmailOrPasswordIsIncorrect.GetDescription(), (int)AppMessage.EmailOrPasswordIsIncorrect);
            }

            var policy = await authPolicyRepository.GetAsync(x => x.UserType == user.UserTypeId);
            if (policy == null)
                return ErrorResponse("Authentication policy not found.");

            await userManager.ResetAccessFailedCountAsync(user);

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
