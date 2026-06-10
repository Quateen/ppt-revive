using Microsoft.AspNetCore.Identity;
using PPTRevive.Application.Common.Interfaces;
using PPTRevive.Application.Common.Models;
using PPTRevive.Domain.Common;
using PPTRevive.Domain.Entities;
using PPTRevive.Domain.Enums;

namespace PPTRevive.Application.Users.Commands.CompleteRegistration;
public class CompleteRegistrationCommand : IRequest<ResponseBase>
{
    public int Id { get; set; }
    public required string Password { get; set; }
}

public class CompleteRegistrationCommandHandler : IRequestHandler<CompleteRegistrationCommand, ResponseBase>
{
    private readonly UserManager<User> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteRegistrationCommandHandler(UserManager<User> userManager, IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    private ResponseBase ErrorResponse(string error)
    {
        return new ResponseBase
        {
            Status = false,
            Error = error
        };
    }

    public async Task<ResponseBase> Handle(CompleteRegistrationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var getUser = await _userManager.FindByIdAsync(request.Id.ToString());

            if (getUser == null)
                return ErrorResponse(AppMessage.UnableToFindRecord.GetDescription());

            getUser.EmailConfirmed = true;
            getUser.PhoneNumberConfirmed = !string.IsNullOrEmpty(getUser.PhoneNumber);
            getUser.AuthKey = TypeExtensions.GenerateRandomPassword();
            getUser.EmailKey = TypeExtensions.GenerateRandomPassword();
            getUser.SmsKey = TypeExtensions.GenerateRandomPassword();
            getUser.PasswordChangedAt = DateTime.UtcNow;
            getUser.EmailConfirmed = true;
            getUser.LastModified = DateTime.UtcNow;

            // Set Password
            var passwordResult = await _userManager.AddPasswordAsync(getUser, request.Password);
            if (!passwordResult.Succeeded)
                return ErrorResponse(string.Join("; ", passwordResult.Errors.Select(e => e.Description)));

            // Update User in DB
            var updateResult = await _userManager.UpdateAsync(getUser);
            if (!updateResult.Succeeded)
                return ErrorResponse(string.Join("; ", updateResult.Errors.Select(e => e.Description)));

            return new ResponseBase
            {
                Status = true,
                Message = AppMessage.RegistrationCompletedSuccessfully.GetDescription()
            };
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex.Message);
        }
    }
}
