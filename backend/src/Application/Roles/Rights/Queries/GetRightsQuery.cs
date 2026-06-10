//using System.Linq.Expressions;
//using PPTRevive.Application.Common.Interfaces;
//using PPTRevive.Application.Common.Models;
//using PPTRevive.Domain.Entities;

//namespace PPTRevive.Application.Roles.Rights.Queries;
//public class GetRightsQuery : IRequest<ResponseBase>
//{
//    public int? RightId {  get; set; }
//    public string? Name { get; set; } = string.Empty;
//    public int PageNumber { get; set; } = 1;
//    public int PageSize { get; set; } = 10;
//}

//public class GetRightsHandler : IRequestHandler<GetRightsQuery, ResponseBase>
//{
//    private readonly IQueryRepository<Right> _queryRepository;

//    public GetRightsHandler(IDataRepository<Right> queryRepository)
//    {
//        _queryRepository = queryRepository;
//    }

//    public async Task<ResponseBase> Handle(GetRightsQuery request, CancellationToken cancellationToken)
//    {
//        // Build query conditions
//        Expression<Func<Right, bool>> conditions = x =>
//        (!request.RightId.HasValue || request.RightId == 0 || x.Id == request.RightId) &&
//        (string.IsNullOrEmpty(request.Name) || x.Name.Contains(request.Name));

//        int totalRecords = await _queryRepository.CountAsync(conditions, cancellationToken);

//        var rights = await _queryRepository.GetAllWithIncludesAsync(
//            conditions: conditions,
//            //include: q => q
//            //    .Include(t => t.RoleRights)
//            //    .ThenInclude(r => r.Right),
//            page: request.PageNumber,
//            count: request.PageSize,
//            cancellationToken: cancellationToken
//        );

//        // Map to DTOs
//        var response = rights.Select(r => new RightsDto
//        {
//            RightId = r.Id,
//            Name = r.Name,
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

//public class RightsDto
//{
//    public int RightId { get; set; }
//    public string? Name { get; set; }
//}
