using Microsoft.AspNetCore.Identity;
using PPTRevive.Application.Auth.Command.Register;
using PPTRevive.Application.Common.Behaviours;
using PPTRevive.Application.Common.Interfaces;
using PPTRevive.Application.Common.Models;
using PPTRevive.Domain.Common;
using PPTRevive.Domain.Entities;
using PPTRevive.Domain.Enums;

namespace PPTRevive.Application.Auth.Command.SendOtp;
public class SendOtpCommand : IRequest<ResponseBase>
{
    public int UserId { get; set; }
}

public class SendOtpCommandValidator : AbstractValidator<SendOtpCommand>
{
    public SendOtpCommandValidator()
    {
        RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");
    }
}

public class SendOtpCommandHandler(UserManager<User> userManager, IEmailSenderRepository emailRepo, ITotpService totpService) : IRequestHandler<SendOtpCommand, ResponseBase>
{
    public async Task<ResponseBase> Handle(SendOtpCommand request, CancellationToken cancellationToken)
    {

        var validator = new SendOtpCommandValidator();
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

        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
            return ResponseHelper.ErrorResponse(AppMessage.UserNotFound.GetDescription(), (int)AppMessage.UserNotFound);

        if (user.EmailConfirmed)
            return new ResponseBase
            {
                Status = true,
                Message = AppMessage.EmailAlreadyConfirmed.GetDescription(),
            };

        await SendVerificationEmailAsync(user.Email ?? string.Empty, user.EmailKey);


        return new ResponseBase
        {
            Status = true,
            Message = AppMessage.OTPSentSuccessfully.GetDescription()
        };
    }

    private async Task SendVerificationEmailAsync(string email, string emailKey)
    {
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
                                    <p>Hi,</p>

                                    <p>Please verify your email by using the verification code below:</p>

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
}
