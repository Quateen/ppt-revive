//using PPTRevive.Application.Common.Interfaces;
//using PPTRevive.Application.Common.Models;

//namespace PPTRevive.Application.EntityTypes.Queries.GetEntityTypesWithPagination;

//public record GetEntityTypesQuery : IRequest<ResponseBase>
//{
//}

//public class GetEntityTypesHandler : IRequestHandler<GetEntityTypesQuery, ResponseBase>
//{
//    private readonly IApplicationDbContext _context;
//    private readonly IMapper _mapper;

//    public GetEntityTypesHandler(IApplicationDbContext context, IMapper mapper)
//    {
//        _context = context;
//        _mapper = mapper;
//    }

//    public async Task<ResponseBase> Handle(GetEntityTypesQuery request, CancellationToken cancellationToken)
//    {
//        //var data = await _context.EntityType
//        //    .OrderByDescending(x => x.Id).ToListAsync();

//        await Task.Delay(100);

//        return new ResponseBase()
//        {
//            Status = true,
//            //Data = data,
//        };
//    }
//}
