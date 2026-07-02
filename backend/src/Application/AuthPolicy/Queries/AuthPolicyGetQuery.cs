//using System.Linq.Expressions;
//using PPTRevive.Application.Common.Interfaces;
//using PPTRevive.Application.Common.Models;
//using PPTRevive.Domain.Common;
//using PPTRevive.Domain.Enums;
//using AuthPolicyModel = PPTRevive.Domain.Entities.AuthPolicy;

//namespace PPTRevive.Application.AuthPolicy.Queries;
//public class AuthPolicyGetQuery : IRequest<ResponseBase>
//{
//    public int AuthPolicyId { get; set; }
//    public int UserTypeId { get; set; }
//    public int PageNumber { get; set; }
//    public int PageSize { get; set; }
//}

//public class AuthPolicyGetQueryHandler : IRequestHandler<AuthPolicyGetQuery, ResponseBase>
//{
//    private readonly IApplicationDbContext _dbContext;
//    private readonly IQueryRepository<AuthPolicyModel> _authPolicyRepo;

//    public AuthPolicyGetQueryHandler(
//        IApplicationDbContext dbContext,
//        IQueryRepository<AuthPolicyModel> authPolicyRepo)
//    {
//        _dbContext = dbContext;
//        _authPolicyRepo = authPolicyRepo;
//    }

//    public async Task<ResponseBase> Handle(AuthPolicyGetQuery request, CancellationToken cancellationToken)
//    {
//        Expression<Func<AuthPolicyModel, bool>> filter = a =>
//            (request.AuthPolicyId == 0 || a.Id == request.AuthPolicyId) &&
//            (request.UserTypeId == 0 || a.UserType == request.UserTypeId);

//        Expression<Func<AuthPolicyModel, AuthPolicyResponse>> projection = a => new AuthPolicyResponse
//        {
//            AuthPolicyId = a.Id,
//            UserTypeId = a.UserType,
//            UserType = ((UserType)a.UserType).GetDescription(),
//            Enforce2FactorVerification = a.Enforce2FactorVerification,
//            EnforcePasswordChangeOnFirstLogin = a.EnforcePasswordChangeOnFirstLogin,
//            EnforceBackendActivation = a.EnforceBackendActivation,
//            EnforceEmailConfirmation = a.EnforceEmailConfirmation,
//            EnforceMobileConfirmation = a.EnforceMobileConfirmation,
//            EnforceProfileCompletion = a.EnforceProfileCompletion,
//            BaseTokenDurationMinutes = a.BaseTokenDurationMinutes,
//            FullTokenDurationMinutes = a.FullTokenDurationMinutes,
//            RefreshTokenDurationMinutes = a.RefreshTokenDurationMinutes,
//            UpdatedAt = a.LastModified
//        };

//        var (authPolicies, totalCount) = await _authPolicyRepo.GetAllWithCountAsync(
//            conditions: filter,
//            columns: projection,
//            orderBy: "Id",
//            ascending: true,
//            page: request.PageNumber > 0 ? request.PageNumber : 1,
//            count: request.PageSize > 0 ? request.PageSize : 20,
//            CancellationToken.None
//        );

//        return new ResponseBase
//        {
//            Status = true,
//            Data = authPolicies,
//            Pagination = new Pagination
//            {
//                PageNumber = request.PageNumber,
//                PageSize = request.PageSize,
//                TotalRecords = totalCount
//            }
//        };
//    }
//}

//public class AuthPolicyResponse
//{
//    public int AuthPolicyId { get; set; }
//    public int UserTypeId { get; set; }
//    public string UserType { get; set; } = string.Empty;
//    public bool Enforce2FactorVerification { get; set; }
//    public bool EnforcePasswordChangeOnFirstLogin { get; set; }
//    public bool EnforceBackendActivation { get; set; }
//    public bool EnforceEmailConfirmation { get; set; }
//    public bool EnforceMobileConfirmation { get; set; }
//    public bool EnforceProfileCompletion { get; set; }
//    public int BaseTokenDurationMinutes { get; set; }
//    public int FullTokenDurationMinutes { get; set; }
//    public int RefreshTokenDurationMinutes { get; set; }
//    public DateTimeOffset UpdatedAt { get; set; }
//}
