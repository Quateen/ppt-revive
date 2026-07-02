using Microsoft.AspNetCore.Identity;
using PPTRevive.Application.Common.Behaviours;
using PPTRevive.Application.Common.Interfaces;
using PPTRevive.Application.Common.Models;
using PPTRevive.Domain.Common;
using PPTRevive.Domain.Entities;
using PPTRevive.Domain.Enums;

namespace PPTRevive.Application.Auth.Command.ChangeEmail;
public class EmailChangeCommand : IRequest<ResponseBase>
{
    public required string NewEmail { get; set; }
    public required string CurrentPassword { get; set; }
}
public class EmailChangeCommandValidator : AbstractValidator<EmailChangeCommand>
{
    public EmailChangeCommandValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required.");

        RuleFor(x => x.NewEmail)
            .NotEmpty().WithMessage("New email is required.")
            .EmailAddress().WithMessage("Invalid email format.");
    }
}
public class EmailChangeCommandHandler(
    UserManager<User> userManager,
    IUser currentUser,
    IApplicationDbContext dbContext)
    : IRequestHandler<EmailChangeCommand, ResponseBase>
{
    public async Task<ResponseBase> Handle(EmailChangeCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(currentUser.Id.ToString());
        if (user == null)
            return ResponseHelper.ErrorResponse(AppMessage.UserNotFound.GetDescription(), (int)AppMessage.UserNotFound);

        var passwordValid = await userManager.CheckPasswordAsync(user, request.CurrentPassword);
        if (!passwordValid)
            return ResponseHelper.ErrorResponse(AppMessage.IncorrectCurrentPassword.GetDescription(), (int)AppMessage.IncorrectCurrentPassword);

        
        var token = await userManager.GenerateChangeEmailTokenAsync(user, request.NewEmail);

      
        var result = await userManager.ChangeEmailAsync(user, request.NewEmail, token);
        if (!result.Succeeded)
        {
            var errorMessages = string.Join(", ", result.Errors.Select(e => e.Description));
            return ResponseHelper.ErrorResponse(AppMessage.IncorrectCurrentPassword.GetDescription(), (int)AppMessage.IncorrectCurrentPassword);
        }

        
        user.UserName = request.NewEmail;
        user.NormalizedUserName = request.NewEmail.ToUpperInvariant();
        await userManager.UpdateAsync(user);

        
        var userProfile = await dbContext.UserProfile
            .FirstOrDefaultAsync(x => x.UserId == user.Id, cancellationToken);

        if (userProfile != null)
        {
            userProfile.Email = request.NewEmail;
            dbContext.UserProfile.Update(userProfile);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return new ResponseBase
        {
            Status = true,
            Message = AppMessage.EmailUpdatedSuccessfully.GetDescription(),
         
        };
    }
}
