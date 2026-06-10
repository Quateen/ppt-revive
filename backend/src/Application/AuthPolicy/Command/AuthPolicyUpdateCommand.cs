//using PPTRevive.Application.Common.Interfaces;
//using PPTRevive.Application.Common.Models;
//using PPTRevive.Domain.Common;
//using PPTRevive.Domain.Enums;

//namespace PPTRevive.Application.AuthPolicy.Command;
//public class AuthPolicyUpdateCommand : IRequest<ResponseBase>
//{
//    public int UserTypeId { get; set; }
//    public bool Enforce2FactorVerification { get; set; }
//    public bool EnforcePasswordChangeOnFirstLogin { get; set; }
//    public bool EnforceBackendActivation { get; set; }
//    public bool EnforceEmailConfirmation { get; set; }
//    public bool EnforceMobileConfirmation { get; set; }
//    public bool EnforceProfileCompletion { get; set; }
//    public int BaseTokenDurationMinutes { get; set; }
//    public int FullTokenDurationMinutes { get; set; }
//    public int RefreshTokenDurationMinutes { get; set; }
//}

//public class AuthPolicyUpdateCommandHandler : IRequestHandler<AuthPolicyUpdateCommand, ResponseBase>
//{
//    private readonly IApplicationDbContext _dbContext;
//    private readonly IUser _currentUser;

//    public AuthPolicyUpdateCommandHandler(
//            IApplicationDbContext dbContext,
//            IUser currentUser
//        )
//    {
//        _dbContext = dbContext;
//        _currentUser = currentUser;
//    }

//    private ResponseBase ErrorResponse(string error)
//    {
//        return new ResponseBase
//        {
//            Status = false,
//            Error = error
//        };
//    }

//    public async Task<ResponseBase> Handle(AuthPolicyUpdateCommand request, CancellationToken cancellationToken)
//    {
//        var authPolicy = await _dbContext.AuthPolicy.FindAsync(request.UserTypeId, cancellationToken);
//        if (authPolicy is null)
//            return ErrorResponse(AppMessage.UnableToFindRecord.GetDescription());

//        authPolicy.Enforce2FactorVerification = request.Enforce2FactorVerification;
//        authPolicy.EnforcePasswordChangeOnFirstLogin = request.EnforcePasswordChangeOnFirstLogin;
//        authPolicy.EnforceBackendActivation = request.EnforceBackendActivation;
//        authPolicy.EnforceEmailConfirmation = request.EnforceEmailConfirmation;
//        authPolicy.EnforceMobileConfirmation = request.EnforceMobileConfirmation;
//        authPolicy.EnforceProfileCompletion = request.EnforceProfileCompletion;
//        authPolicy.BaseTokenDurationMinutes = request.BaseTokenDurationMinutes;
//        authPolicy.FullTokenDurationMinutes = request.FullTokenDurationMinutes;
//        authPolicy.RefreshTokenDurationMinutes = request.RefreshTokenDurationMinutes;
//        authPolicy.LastModifiedBy = _currentUser.Id;
//        authPolicy.LastModified = DateTime.UtcNow;

//        _dbContext.AuthPolicy.Update(authPolicy);
//        await _dbContext.SaveChangesAsync(cancellationToken);

//        return new ResponseBase
//        {
//            Status = true,
//            Message = AppMessage.RecordUpdatedSuccessfully.GetDescription()
//        };
//    }
//}
