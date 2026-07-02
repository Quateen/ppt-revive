using System.Web;
using Microsoft.Extensions.Configuration;
using PPTRevive.Application.Common.Behaviours;
using PPTRevive.Application.Common.Interfaces;
using PPTRevive.Application.Common.Models;
using PPTRevive.Domain.Common;
using PPTRevive.Domain.Enums;

namespace PPTRevive.Application.Auth.Command.ForgetPassword;

public record ForgetPasswordCommand : IRequest<ResponseBase>
{
    public required string Email { get; set; }
}

public class ForgetPasswordCommandValidator : AbstractValidator<ForgetPasswordCommand>
{
    public ForgetPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
       .NotEmpty().WithMessage("Email is required.")
       .EmailAddress().WithMessage(AppMessage.EmailIsIncorrect.GetDescription());
    }
}


public class ForgetPasswordCommandHandler : IRequestHandler<ForgetPasswordCommand, ResponseBase>
{
    private readonly IIdentityService _identityService;
    private readonly IEmailSenderRepository _emailSenderRepository;
    private readonly IConfiguration _configuration;

    public ForgetPasswordCommandHandler(IIdentityService identityService, IEmailSenderRepository emailSenderRepository, IConfiguration configuration)
    {
        _identityService = identityService;
        _emailSenderRepository = emailSenderRepository;
        _configuration = configuration;
    }

    private ResponseBase ErrorResponse(string error)
    {
        return new ResponseBase
        {
            Status = false,
            Error = error
        };
    }
    public async Task<ResponseBase> Handle(ForgetPasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {

            var validator = new ForgetPasswordCommandValidator();
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

            var user = await _identityService.GetUserByEmail(request.Email);
            if (user == null)
                return ErrorResponse(AppMessage.EmailIsIncorrect.GetDescription());

            var unEncodedToken = await _identityService.CreatePasswordResetToken(user.Id);
            var token = HttpUtility.UrlEncode(unEncodedToken);

            var feBaseUrl = _configuration.GetValue<string>("AppConfig:FEBaseURL");
            var forgetPasswordUrl = string.Format("{0}/reset-password?token={1}&userId={2}", feBaseUrl, token, user.Id);

            string body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Reset Your Password</title>
</head>
<body style='font-family: Arial, sans-serif; background-color: #f9f9f9; padding: 30px;'>
    <table width='100%' cellspacing='0' cellpadding='0'>
        <tr>
            <td align='center'>
                <table style='max-width: 600px; width: 100%; background-color: #ffffff; border-radius: 8px; padding: 40px; box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);'>
                    <tr>
                        <td align='center' style='padding-bottom: 20px;'>
                            <h2 style='color: #333;'>Password Reset Request</h2>
                        </td>
                    </tr>
                    <tr>
                        <td style='text-align: justify; color: #333;'>
                            <p>Hi <strong>{user?.FirstName}</strong>,</p>

                            <p>We received a request to reset your password. To proceed, please click the button below:</p>

                            <p style='text-align: center; margin: 30px 0;'>
                                <a href='{forgetPasswordUrl}' style='
                                    display: inline-block;
                                    padding: 12px 24px;
                                    font-size: 16px;
                                    color: #ffffff;
                                    background-color: #007BFF;
                                    text-decoration: none;
                                    border-radius: 6px;
                                '>Reset Password</a>
                            </p>

                            <p>If the button above doesn't work, you can also copy and paste the following link into your browser:</p>

                            <p style='word-break: break-all; color: #007BFF;'>
                                <a href='{forgetPasswordUrl}' style='color: #007BFF;'>{forgetPasswordUrl}</a>
                            </p>

                            <p>If you didn't request a password reset, you can safely ignore this email.</p>

                            <p style='margin-top: 40px;'>Best regards,<br/>The PPT Revive Team</p>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding-top: 30px; font-size: 12px; color: #777; text-align: center;'>
                            This is an automated message, please do not reply.
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";




            // SEND Registration Complete Email
            await _emailSenderRepository.SendEmailAsync(request.Email ?? string.Empty, "Reset Password", body);

            return new ResponseBase()
            {
                Status = true,
                Message = AppMessage.PasswordResetEmailSent.GetDescription()
            };

        }
        catch (Exception ex)
        {
            return ErrorResponse(ex.Message);
        }
    }
}

public class ForgetPasswordResponse
{
    public int UserId { get; set; }
    public string? UnencodeToken { get; set; }
    public string? Token { get; set; }
}
