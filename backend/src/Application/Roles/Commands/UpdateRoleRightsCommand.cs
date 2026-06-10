//using PPTRevive.Application.Common.Interfaces;
//using PPTRevive.Application.Common.Models;
//using PPTRevive.Domain.Common;
//using PPTRevive.Domain.Entities;
//using PPTRevive.Domain.Enums;

//namespace PPTRevive.Application.Roles.Commands;
//public class UpdateRoleRightsCommand : IRequest<ResponseBase>
//{
//    public int RoleId { get; set; }
//    public string Name { get; set; } = string.Empty;
//    public string? Description { get; set; }
//    public List<UpdateRoleRightsModel>? RoleRights { get; set; }
//}

//public class UpdateRoleRightsModel
//{
//    public int RoleRightId { get; set; }
//    public int RightId { get; set; }
//    public bool IsDeleted { get; set; }
//}

//public class UpdateRoleRightsHandler : IRequestHandler<UpdateRoleRightsCommand, ResponseBase>
//{
//    private readonly IDataRepository<Role> _roleRepository;
//    private readonly IDataRepository<RoleRight> _roleRightRepository;
//    private readonly IUnitOfWork _unitOfWork;
//    private readonly IUser _currentUser;

//    public UpdateRoleRightsHandler(IDataRepository<Role> roleRepository, IDataRepository<RoleRight> roleRightRepository, IUnitOfWork unitOfWork, IUser currentUser)
//    {
//        _roleRepository = roleRepository;
//        _roleRightRepository = roleRightRepository;
//        _unitOfWork = unitOfWork;
//        _currentUser = currentUser;
//    }

//    private ResponseBase ErrorResponse(string error)
//    {
//        //_appLogger.Debug($"ErrorResponse Msg: {error}");
//        return new ResponseBase
//        {
//            Status = false,
//            Error = error
//        };
//    }

//    public async Task<ResponseBase> Handle(UpdateRoleRightsCommand request, CancellationToken cancellationToken)
//    {
//        try
//        {
//            var role = await _roleRepository.GetAsync(x => x.Id == request.RoleId, cancellationToken);

//            if (role == null)
//                return ErrorResponse(AppMessage.UnableToFindRecord.GetDescription());

//            role.Name = request.Name;
//            role.Description = request.Description;

//            _roleRepository.Attach(role, _currentUser.Id);

//            await _unitOfWork.SaveChangesAsync(cancellationToken);

//            await UpdateRoleRights(request, role, cancellationToken);

//            return new ResponseBase
//            {
//                Status = true,
//                Message = AppMessage.RecordUpdatedSuccessfully.GetDescription()
//            };
//        }
//        catch (Exception ex)
//        {
//            //_appLogger.Error(ex);
//            return ErrorResponse(ex.Message);
//        }
//    }

//    private async Task UpdateRoleRights(UpdateRoleRightsCommand request, Role? role, CancellationToken cancellationToken)
//    {
//        if (request.RoleRights != null && role != null)
//        {
//            foreach (var item in request.RoleRights)
//            {
//                if (item.RoleRightId == 0 && item.RightId > 0 && !item.IsDeleted)
//                {
//                    // Add new RoleRight
//                    var newRoleRight = new RoleRight
//                    {
//                        RightId = item.RightId,
//                        RoleId = role.Id
//                    };

//                    _roleRightRepository.Add(newRoleRight, _currentUser.Id);
//                }
//                else if (item.RoleRightId > 0 && item.RightId > 0 && !item.IsDeleted)
//                {
//                    // Update existing RoleRight
//                    var existingRoleRight = await _roleRightRepository.GetAsync(
//                        x => x.Id == item.RoleRightId && x.RoleId == role.Id && x.RightId == item.RightId,
//                        cancellationToken
//                    );

//                    if (existingRoleRight != null)
//                    {
//                        existingRoleRight.RightId = item.RightId;
//                        existingRoleRight.RoleId = role.Id;

//                        _roleRightRepository.Attach(existingRoleRight, _currentUser.Id);
//                    }
//                }
//                else if (item.RoleRightId > 0 && item.RightId > 0 && item.IsDeleted)
//                {
//                    // Delete existing RoleRight
//                    var existingRoleRight = await _roleRightRepository.GetAsync(
//                        x => x.Id == item.RoleRightId,
//                        cancellationToken
//                    );

//                    if (existingRoleRight != null)
//                    {
//                        _roleRightRepository.Delete(existingRoleRight);
//                    }
//                }
//            }
//        }

//        await _unitOfWork.SaveChangesAsync(cancellationToken);
//    }
//}
