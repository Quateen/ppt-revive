//using PPTRevive.Application.Common.Interfaces;
//using PPTRevive.Application.Common.Models;

//namespace PPTRevive.Application.EntityTypes.Queries.GetEntityTypesWithPagination;

//public record GetEntityTypeCategoriesWithPaginationQuery : IRequest<ResponseBase>
//{
//    public int EntityTypeId { get; init; }
//    //public int PageNumber { get; init; } = 1;
//    //public int PageSize { get; init; } = 10;
//}

//public class GetEntityTypeCategoriesWithPaginationHandler : IRequestHandler<GetEntityTypeCategoriesWithPaginationQuery, ResponseBase>
//{
//    private readonly IApplicationDbContext _context;
//    private readonly IMapper _mapper;

//    public GetEntityTypeCategoriesWithPaginationHandler(IApplicationDbContext context, IMapper mapper)
//    {
//        _context = context;
//        _mapper = mapper;
//    }

//    public async Task<ResponseBase> Handle(GetEntityTypeCategoriesWithPaginationQuery request, CancellationToken cancellationToken)
//    {
//        //var data = await _context.Category
//        //    .Where(x => request.EntityTypeId == 0 || x.EntityTypeId == request.EntityTypeId)
//        //    .OrderByDescending(x => x.Id)
//        //    .ProjectTo<EntityTypeCategoriesDto>(_mapper.ConfigurationProvider).ToListAsync();

//        await Task.Delay(100);

//        return new ResponseBase()
//        {
//            Status = true,
//            //Data = data,
//        };
//    }
//}
