using PPTRevive.Application.Common.Interfaces;
using PPTRevive.Application.Common.Models;
using PPTRevive.Domain.Common;
using PPTRevive.Domain.Enums;

namespace PPTRevive.Application.Auth.Command.ChangePassword;

public class ChangePasswordCommand : IRequest<ResponseBase>
{
    public required string OldPassword { get; set; }
    public required string NewPassword { get; set; }
}

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.OldPassword)
            .NotEmpty()
            .WithMessage(AppMessage.PasswordCannotBeEmpty.GetDescription());

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage(AppMessage.PasswordCannotBeEmpty.GetDescription())
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long.")
            .Matches("[A-Z]")
            .WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]")
            .WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]")
            .WithMessage("Password must contain at least one number.")
            .Matches("[^a-zA-Z0-9]")
            .WithMessage("Password must contain at least one special character.");

        RuleFor(x => x)
            .Must(x => x.NewPassword != x.OldPassword)
            .WithMessage("New password must be different from the old password.");
    }
}


public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, ResponseBase>
{
    private readonly IIdentityService _identityService;
    private readonly IUser _currentUser;

    public ChangePasswordCommandHandler(IIdentityService identityService, IUser currentUser)
    {
        _identityService = identityService;
        _currentUser = currentUser;
    }

    private ResponseBase ErrorResponse(string error)
    {
        return new ResponseBase
        {
            Status = false,
            Error = error
        };
    }
    public async Task<ResponseBase> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var validationResponse = ValidatePasswords(request.OldPassword, request.NewPassword);
            if (!validationResponse.Status)
            {
                return validationResponse;
            }

            var changePassword = await _identityService.ChangePassword(_currentUser.Id, request.OldPassword, request.NewPassword);

            return !changePassword
                ? ErrorResponse(AppMessage.PasswordUpdateFailed.GetDescription())
                : new ResponseBase
                {
                    Status = true,
                    Message = AppMessage.PasswordUpdatedSuccessfully.GetDescription(),
                };
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex.Message);
        }
    }

    private ResponseBase ValidatePasswords(string oldPassword, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword))
        {
            return ErrorResponse(AppMessage.PasswordCannotBeEmpty.GetDescription());
        }

        if (oldPassword == newPassword)
        {
            return ErrorResponse(AppMessage.PasswordCannotBeSame.GetDescription());
        }

        if (newPassword.Length < 8 ||
            !newPassword.Any(char.IsUpper) ||
            !newPassword.Any(char.IsLower) ||
            !newPassword.Any(char.IsDigit) ||
            !newPassword.Any(ch => "!@#$%^&*()_+-=[]{}|;:'\",.<>?/".Contains(ch)))
        {
            return ErrorResponse(AppMessage.PasswordStrengthWeak.GetDescription());
        }

        return new ResponseBase
        {
            Status = true
        };
    }
}
