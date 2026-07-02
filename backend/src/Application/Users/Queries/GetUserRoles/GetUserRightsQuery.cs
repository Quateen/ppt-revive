using Microsoft.AspNetCore.Identity;
using PPTRevive.Application.Common.Interfaces;
using PPTRevive.Application.Common.Models;
using PPTRevive.Domain.Entities;

namespace PPTRevive.Application.Users.Queries.GetUsersWithPagination;

public record GetUserRightsQuery : IRequest<ResponseBase>
{
    //public int UserId { get; set; }
}

public class GetUserRightsQueryHandler : IRequestHandler<GetUserRightsQuery, ResponseBase>
{
    private readonly IQueryRepository<UserRole> _userRoleRepository;
    private readonly IQueryRepository<Role> _roleRepository;
    private readonly IQueryRepository<RoleRight> _roleRightRepository;
    private readonly IQueryRepository<Right> _rightRepository;
    private readonly UserManager<User> _userManager;
    private readonly IUser _currentUser;

    public GetUserRightsQueryHandler(IQueryRepository<UserRole> userRoleRepository,
        IQueryRepository<Role> roleRepository,
        IQueryRepository<RoleRight> roleRightRepository,
        IQueryRepository<Right> rightRepository,
        UserManager<User> userManager,
        IUser currentUser)
    {
        _userRoleRepository = userRoleRepository;
        _roleRepository = roleRepository;
        _roleRightRepository = roleRightRepository;
        _rightRepository = rightRepository;
        _userManager = userManager;
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

    public async Task<ResponseBase> Handle(GetUserRightsQuery request, CancellationToken cancellationToken)
    {
        var userRole = await _userRoleRepository.GetAsync(x => x.UserId == _currentUser.Id);

        if (userRole == null)
        {
            return ErrorResponse("User role not found.");
        }

        var roleRights = await _roleRightRepository.GetAllAsync(x => x.RoleId == userRole.RoleId);

        var rightIds = roleRights.Select(rr => rr.RightId).ToList();

        var rights = await _rightRepository.GetAllAsync(x => rightIds.Contains(x.Id));

        var userRights = rights.Select(r => new UserRightsDto
        {
            RightId = r.Id,
            Name = r.Name
        }).ToList();

        var response = new UserRoleRights
        {
            UserId = _currentUser.Id,
            RoleId = userRole.RoleId,
            UserRights = userRights
        };

        return new ResponseBase
        {
            Status = true,
            Data = response
        };
    }
}

public class UserRoleRights
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
    public List<UserRightsDto>? UserRights { get; set; }
}

public class UserRightsDto
{
    public int RightId { get; set; }
    public string? Name { get; set; }
}
