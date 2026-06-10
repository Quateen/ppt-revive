using Microsoft.AspNetCore.Identity;
using PPTRevive.Application.Common.Interfaces;
using PPTRevive.Application.Common.Models;
using PPTRevive.Domain.Common;
using PPTRevive.Domain.Entities;
using PPTRevive.Domain.Enums;

namespace PPTRevive.Application.Department.Commands.UpdateDepartment;
public class UpdateUserCommand : IRequest<ResponseBase>
{
    public required int Id { get; set; }
    public required string FirstName { get; set; }
    public string? LastName { get; set; }
    public required string Email { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public string? Address { get; set; }
}

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, ResponseBase>
{
    private readonly UserManager<User> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDataRepository<UserProfile> _dataRepository;
    private readonly IUser _currentUser;

    public UpdateUserCommandHandler(UserManager<User> userManager, IUnitOfWork unitOfWork, IDataRepository<UserProfile> dataRepository,
    IUser currentUser)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _dataRepository = dataRepository;
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

    public async Task<ResponseBase> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var getUser = await _userManager.FindByIdAsync(request.Id.ToString());

        if (getUser == null)
            return ErrorResponse(AppMessage.UnableToFindRecord.GetDescription());

        getUser.FirstName = request.FirstName;
        getUser.LastName = request.LastName ?? string.Empty;
        getUser.DisplayName = $"{request.FirstName} {request.LastName}";
        getUser.Email = request.Email;
        getUser.UserName = request.Email;
        getUser.PhoneNumber = request.PhoneNumber;

        var updateUser = await _userManager.UpdateAsync(getUser);

        var getUserProfile = await _dataRepository.GetAsync(up => up.UserId == getUser.Id);
        if (getUserProfile != null)
        {
            getUserProfile.FirstName = request.FirstName;
            getUserProfile.LastName = request.LastName ?? string.Empty;
            getUserProfile.Address = request.Address ?? string.Empty;
            getUserProfile.Email = request.Email;
            getUserProfile.DateOfBirth = request.DateOfBirth;

            _dataRepository.Attach(getUserProfile, _currentUser.Id);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ResponseBase
        {
            Status = true,
            Message = AppMessage.RecordUpdatedSuccessfully.GetDescription()
        };
    }
}

