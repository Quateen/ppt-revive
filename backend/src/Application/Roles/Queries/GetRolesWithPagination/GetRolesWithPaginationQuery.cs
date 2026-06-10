//using System.Linq.Expressions;
//using PPTRevive.Application.Common.Interfaces;
//using PPTRevive.Application.Common.Models;
//using PPTRevive.Domain.Entities;

//namespace PPTRevive.Application.Roles.Queries.GetRolesWithPagination;

//public record GetRolesWithPaginationQuery : IRequest<ResponseBase>
//{
//    public int? RoleId { get; init; }
//    public int PageNumber { get; init; } = 1;
//    public int PageSize { get; init; } = 10;
//}

//public class GetRolesWithPaginationQueryHandler : IRequestHandler<GetRolesWithPaginationQuery, ResponseBase>
//{
//    private readonly IQueryRepository<Role> _queryRepository;

//    public GetRolesWithPaginationQueryHandler(IDataRepository<Role> queryRepository)
//    {
//        _queryRepository = queryRepository;
//    }

//    public async Task<ResponseBase> Handle(GetRolesWithPaginationQuery request, CancellationToken cancellationToken)
//    {
//        // Build query conditions
//        Expression<Func<Role, bool>> conditions = x =>
//        (!request.RoleId.HasValue || request.RoleId == 0 || x.Id == request.RoleId);

//        int totalRecords = await _queryRepository.CountAsync(conditions, cancellationToken);

//        var roles = await _queryRepository.GetAllWithIncludesAsync(
//            conditions: conditions,
//            include: q => q
//                .Include(t => t.RoleRights)
//                .ThenInclude(r => r.Right),
//            page: request.PageNumber,
//            count: request.PageSize,
//            cancellationToken: cancellationToken
//        );

//        // Map to DTOs
//        var response = roles.Select(role => new RoleDto
//        {
//            RoleId = role.Id,
//            Name = role.Name,
//            Description = role.Description,
//            Created = role.Created,
//            CreatedBy = role.CreatedBy,
//            LastModified = role.LastModified,
//            LastModifiedBy = role.LastModifiedBy,
//            RoleRights = role.RoleRights.Select(rr => new RoleRightDto
//            {
//                RoleRightId = rr.Id,
//                RightId = rr.Right.Id,
//                Name = rr.Right.Name,
//                Description = rr.Right.Description
//            }).ToList()
//        }).ToList();

//        return new ResponseBase
//        {
//            Status = true,
//            Data = response,
//            Pagination = new Pagination
//            {
//                PageNumber = request.PageNumber,
//                PageSize = request.PageSize,
//                TotalRecords = totalRecords,
//            },
//        };
//    }
//}
