using PPTRevive.Application.Common.Interfaces;
using PPTRevive.Application.Common.Models;

namespace PPTRevive.Application.Users.Queries.GetUsersWithPagination;

public record GetUsersWithPaginationQuery : IRequest<ResponseBase>
{
    public int UserId { get; set; }
    public string? Name { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetUsersWithPaginationQueryHandler : IRequestHandler<GetUsersWithPaginationQuery, ResponseBase>
{
    private readonly IApplicationDbContext _dbContext;

    public GetUsersWithPaginationQueryHandler(
        IApplicationDbContext dbContext
    )
    {
        _dbContext = dbContext;
    }

    public async Task<ResponseBase> Handle(GetUsersWithPaginationQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.UserProfile
            .Where(a => a.User != null)
            .Include(a => a.User!)
                .ThenInclude(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RoleRights)
                            .ThenInclude(rr => rr.Right)
            //.Include(a => a.User!)
            //    .ThenInclude(u => u.WorkSpaceUser)
            //        .ThenInclude(wu => wu.WorkSpace)
            .AsQueryable();

        if (request.UserId != 0)
            query = query.Where(a => a.Id == request.UserId);

        if (!string.IsNullOrEmpty(request.Name))
            query = query.Where(a => a.User!.DisplayName.Contains(request.Name));

        var totalCount = await query.CountAsync(cancellationToken);

        var paginated = await query
            .OrderByDescending(a => a.Id)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var resultList = paginated.Select(a =>
        {
            var user = a.User!;
            var firstUserRole = user.UserRoles.FirstOrDefault();
            var role = firstUserRole?.Role;

            return new UserProfileResponse
            {
                Id = a.Id,
                UserId = a.UserId,
                DisplayName = user.DisplayName,
                FirstName = a.FirstName,
                LastName = a.LastName,
                Address = a.Address,
                Email = a.Email,
                MobileNumber = a.MobileNumber,
                DateOfBirth = a.DateOfBirth,
                Gender = a.Gender,
                //WorkspaceId = user.WorkSpaceUser.FirstOrDefault()?.WorkSpaceId ?? 0,
                //WorkspaceName = user.WorkSpaceUser.FirstOrDefault()?.WorkSpace?.Name ?? string.Empty,
                RoleId = role?.Id ?? 0,
                RoleName = role?.Name ?? string.Empty,
                Created = a.Created,
                CreatedBy = a.CreatedBy,
                LastModified = a.LastModified,
                LastModifiedBy = a.LastModifiedBy,
                UserRights = role?.RoleRights
                    .Where(rr => rr.Right != null)
                    .Select(rr => new UserRightsResponse
                    {
                        Id = rr.Right.Id,
                        Name = rr.Right.Name
                    })
                    .ToList() ?? new()
            };
        }).ToList();

        return new ResponseBase
        {
            Status = true,
            Data = resultList,
            Pagination = new Pagination
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalRecords = totalCount
            }
        };
    }
}

public class UserProfileResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public int WorkspaceId { get; set; }
    public string WorkspaceName { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public int Gender { get; set; }
    public DateTimeOffset Created { get; set; }
    public int? CreatedBy { get; set; }
    public DateTimeOffset LastModified { get; set; }
    public int? LastModifiedBy { get; set; }
    public List<UserRightsResponse>? UserRights { get; set; } = [];
}

public class UserRightsResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
