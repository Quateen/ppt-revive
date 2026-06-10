using System.Web;
using Microsoft.AspNetCore.Identity;
using PPTRevive.Application.Common.Behaviours;
using PPTRevive.Application.Common.Interfaces;
using PPTRevive.Application.Common.Models;
using PPTRevive.Domain.Common;
using PPTRevive.Domain.Entities;
using PPTRevive.Domain.Enums;

namespace PPTRevive.Application.Auth.Command.Register;

public record RegisterCommand : IRequest<ResponseBase>
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        // Set global class-level cascade mode (optional)
        ClassLevelCascadeMode = CascadeMode.Continue;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters.");

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Continue) // Rule-level cascade mode to ensure all checks are run
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"\d").WithMessage("Password must contain at least one number.")
            .Matches(@"[\!\@\#\$\%\^\&\*\(\)\-\+]").WithMessage("Password must contain at least one special character (!@#$%^&*()-+).")
            .Matches(@"^\S+$").WithMessage("Password must not contain spaces.")
            .Must(pw => !new[] { "password", "12345678", "qwerty123", "password123" }.Contains(pw?.ToLower()))
            .WithMessage("Password is too common. Please choose a more secure one.");
    }
}



public class RegisterCommandHandler(IIdentityService identityService, IEmailSenderRepository emailRepo, UserManager<User> userManager,
    ITotpService totpService)
    : IRequestHandler<RegisterCommand, ResponseBase>
{
    public async Task<ResponseBase> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var validator = new RegisterCommandValidator();
            var validationResult = validator.Validate(request);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();

                return new ResponseBase
                {
                    Status = false,
                    Message = "Validation failed.",
                    Error = new ResponseHelper.ErrorDetails
                    {
                        Errors = errors,
                        ErrorCodeId = (int)AppMessage.UnknownError
                    }
                };
            }

            var user = await identityService.GetUserByEmail(request.Email);
            if (user != null)
                return ResponseHelper.ErrorResponse(AppMessage.UserWithSameEmailAlreadyExist.GetDescription(),
                                                    (int)AppMessage.UserWithSameEmailAlreadyExist);

            var userCreateResult = await identityService.CreateUserAsync(request.Name, request.Email, request.Password);

            if (!userCreateResult.Result.Succeeded)
                return ResponseHelper.ErrorResponse(userCreateResult.Result.Errors[0],
                                                    (int)AppMessage.FailedToRegisterUser);

            if (userCreateResult.Result == null || userCreateResult.UserId <= 0)
                return ResponseHelper.ErrorResponse(AppMessage.FailedToRegisterUser.GetDescription(),
                                                    (int)AppMessage.FailedToRegisterUser);

            var newUser = await userManager.FindByIdAsync(userCreateResult.UserId.ToString());
            if (newUser != null)
            {
                await SendVerificationEmailAsync(request.Email, newUser.EmailKey);
            }

            return new ResponseBase
            {
                Status = true,
                Data = userCreateResult.UserId,
                Message = AppMessage.AccountCreatedSuccessfully.GetDescription()
            };
        }
        catch (Exception ex)
        {
            return ResponseHelper.ErrorResponse(ex.Message, (int)AppMessage.UnknownError);
        }
    }

    private async Task SendVerificationEmailAsync(string email, string emailKey)
    {
        var user = await identityService.GetUserByEmail(email);

        string code = totpService.GenerateCode(email, emailKey, 600);

        var subject = "Verify your email address to complete your registration";

        var htmlBody = $@"
        <!DOCTYPE html>
        <html>
        <head><meta charset='UTF-8'><title>Email Verification</title></head>
        <body style='font-family: Arial, sans-serif; background-color: #f9f9f9; padding: 30px;'>
            <table width='100%' cellspacing='0' cellpadding='0'>
                <tr>
                    <td align='center'>
                        <table style='max-width: 600px; background-color: #ffffff; border-radius: 8px; padding: 40px; box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);'>
                            <tr><td align='center' style='padding-bottom: 20px;'><h2 style='color: #333;'>Welcome to PPT Revive 👋</h2></td></tr>
                            <tr>
                                <td>
                                    <p>Hi <strong>{user?.FirstName}</strong>,</p>

                                    <p>Thank you for registering with <strong>PPT Revive</strong>. Please verify your email by using the verification code below:</p>

                                    <p style=""text-align: center; margin: 30px 0;"">
                                        <span style=""display: inline-block; background-color: #f0f4ff; color: #003366; font-size: 24px; font-weight: bold; padding: 12px 24px; border-radius: 6px; letter-spacing: 2px;"">
                                            {code}
                                        </span>
                                    </p>

                                    <p>Enter this code in the app or website to complete your registration.</p>

                                    <p>If you didn't create an account, please ignore this email.</p>

                                    <p style=""margin-top: 40px;"">Best regards,<br/>The PPT Revive Team</p>

                                </td>
                            </tr>
                            <tr><td style='padding-top: 30px; font-size: 12px; color: #777;'>This is an automated message, please do not reply.</td></tr>
                        </table>
                    </td>
                </tr>
            </table>
        </body>
        </html>";

        await emailRepo.SendEmailAsync(email ?? string.Empty, subject, htmlBody);
    }

    //private async Task SendVerificationEmailAsync(string email)
    //{
    //    var user = await identityService.GetUserByEmail(email);

    //    var token = await identityService.CreateEmailConfirmToken(user?.Id ?? 0);
    //    var encodedToken = HttpUtility.UrlEncode(token);

    //    var verificationLink = $"https://localhost:7001/verify-email?userId={user?.Id}&token={encodedToken}";

    //    var subject = "Verify your email address to complete your registration";

    //    var htmlBody = $@"
    //    <!DOCTYPE html>
    //    <html>
    //    <head><meta charset='UTF-8'><title>Email Verification</title></head>
    //    <body style='font-family: Arial, sans-serif; background-color: #f9f9f9; padding: 30px;'>
    //        <table width='100%' cellspacing='0' cellpadding='0'>
    //            <tr>
    //                <td align='center'>
    //                    <table style='max-width: 600px; background-color: #ffffff; border-radius: 8px; padding: 40px; box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);'>
    //                        <tr><td align='center' style='padding-bottom: 20px;'><h2 style='color: #333;'>Welcome to PPT Revive 👋</h2></td></tr>
    //                        <tr>
    //                            <td>
    //                                <p>Hi <strong>{user?.FirstName}</strong>,</p>
    //                                <p>Thank you for registering with <strong>PPT Revive</strong>. Please verify your email by clicking the button below:</p>
    //                                <p style='text-align: center; margin: 30px 0;'>
    //                                    <a href='{verificationLink}' style='background-color: #0066ff; color: #ffffff; padding: 12px 24px; border-radius: 4px; text-decoration: none;'>Verify Email</a>
    //                                </p>
    //                                <p>If the button doesn't work, copy this URL into your browser:</p>
    //                                <p style='word-break: break-all;'><span style='color: #0066ff;'>{verificationLink}</span></p>
    //                                <p>If you didn't create an account, ignore this email.</p>
    //                                <p style='margin-top: 40px;'>Best regards,<br/>The PPT Revive Team</p>
    //                            </td>
    //                        </tr>
    //                        <tr><td style='padding-top: 30px; font-size: 12px; color: #777;'>This is an automated message, please do not reply.</td></tr>
    //                    </table>
    //                </td>
    //            </tr>
    //        </table>
    //    </body>
    //    </html>";

    //    await emailRepo.SendEmailAsync(email ?? string.Empty, subject, htmlBody);
    //}
}

