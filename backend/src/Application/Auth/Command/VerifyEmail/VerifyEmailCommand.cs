using Microsoft.AspNetCore.Identity;
using PPTRevive.Application.Auth.Command.Register;
using PPTRevive.Application.Common.Behaviours;
using PPTRevive.Application.Common.Interfaces;
using PPTRevive.Application.Common.Models;
using PPTRevive.Domain.Common;
using PPTRevive.Domain.Entities;
using PPTRevive.Domain.Enums;

namespace PPTRevive.Application.Auth.Command.VerifyEmail;
public class VerifyEmailCommand : IRequest<ResponseBase>
{
    public int UserId { get; set; }
    //public required string Token { get; set; }
    public string Code { get; set; } = string.Empty;
}

public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Code)
            .NotNull().WithMessage("Verification Code is required.")
            .NotEmpty().WithMessage("Verification Code is required.");

        //RuleFor(x => x.Token)
        //    .NotNull().WithMessage("Verification token is required.")
        //    .NotEmpty().WithMessage("Verification token is required.");
    }
}

public class VerifyEmailCommandHandler(UserManager<User> userManager, IEmailSenderRepository emailRepo, ITotpService totpService) : IRequestHandler<VerifyEmailCommand, ResponseBase>
{
    public async Task<ResponseBase> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {

        var validator = new VerifyEmailCommandValidator();
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

        //var decodedToken = Uri.UnescapeDataString(request.Token);
        //var result = await userManager.ConfirmEmailAsync(user, decodedToken);

        bool success = totpService.VerifyCode(user.Email ?? string.Empty, user.EmailKey, 600, request.Code);

        if (!success)
            return ResponseHelper.ErrorResponse(AppMessage.InvalidOrExpiredToken.GetDescription(), (int)AppMessage.InvalidOrExpiredToken);

        //if (!result.Succeeded)
        //    return ResponseHelper.ErrorResponse(AppMessage.InvalidOrExpiredToken.GetDescription(), (int)AppMessage.InvalidOrExpiredToken);

        user.EmailConfirmed = true;
        await userManager.UpdateAsync(user);

        await VerificationCompleteEmailAsync(user);

        return new ResponseBase
        {
            Status = true,
            Message = AppMessage.EmailVerificationSuccessful.GetDescription(),
        };
    }

    private async Task VerificationCompleteEmailAsync(User user)
    {
        var subject = "Your Email Has Been Successfully Verified";

        var htmlBody = $"""
            <p>Dear {user.FirstName},</p>

            <p>We're pleased to let you know that your email address has been successfully verified.</p>

            <p>You can now log in and start using your account without any limitations.</p>

            <p>If you have any questions or need help, feel free to contact our support team.</p>

            <p>Best regards,<br/>
            The PPT Revive Team</p>
            """;

        await emailRepo.SendEmailAsync(user.Email ?? string.Empty, subject, htmlBody);
    }
}

