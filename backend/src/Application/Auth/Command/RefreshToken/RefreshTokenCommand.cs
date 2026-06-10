using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PPTRevive.Application.Common.Contracts;
using PPTRevive.Application.Common.Interfaces;
using PPTRevive.Application.Common.Models;
using PPTRevive.Domain.Common;
using PPTRevive.Domain.Entities;
using PPTRevive.Domain.Enums;
using AuthPolicyModel = PPTRevive.Domain.Entities.AuthPolicy;

namespace PPTRevive.Application.Auth.Command.RefreshToken;
public class RefreshTokenCommand : IRequest<ResponseBase>
{
}

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, ResponseBase>
{
    private readonly ITokenRepository _tokenRepository;
    private readonly UserManager<User> _userManager;
    private readonly IQueryRepository<AuthPolicyModel> _authPolicyRepository;
    private readonly IDataRepository<LoginAttempts> _loginAttemptsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUser _currentUser;
    private readonly ILogger<RefreshTokenHandler> _logger;
    //private readonly IQueryRepository<UserRole> _userRoleRepo;
    //private readonly IQueryRepository<RoleRight> _roleRightRepo;

    public RefreshTokenHandler(
        ITokenRepository tokenRepository,
        UserManager<User> userManager,
        IQueryRepository<AuthPolicyModel> authPolicyRepository,
        IDataRepository<LoginAttempts> loginAttemptsRepository,
        IUnitOfWork unitOfWork,
        IUser currentUser,
        ILogger<RefreshTokenHandler> logger
        //IQueryRepository<UserRole> userRoleRepo,
        //IQueryRepository<RoleRight> roleRightRepo
        )
    {
        _tokenRepository = tokenRepository;
        _userManager = userManager;
        _authPolicyRepository = authPolicyRepository;
        _loginAttemptsRepository = loginAttemptsRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _logger = logger;
        //_userRoleRepo = userRoleRepo;
        //_roleRightRepo = roleRightRepo;
    }

    private ResponseBase ErrorResponse(string error)
    {
        return new ResponseBase
        {
            Status = false,
            Error = error
        };
    }

    public async Task<ResponseBase> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(_currentUser.Id.ToString());
            if (user == null)
            {
                _logger.LogWarning("User not found during refresh token attempt.");
                return ErrorResponse(AppMessage.UserNotFound.GetDescription());
            }

            var attempts = await _loginAttemptsRepository.GetAllAsync(x => x.UserId == user.Id);
            var attempt = attempts.OrderByDescending(x => x.Id).FirstOrDefault();

            if (attempt == null)
            {
                _logger.LogWarning($"No login attempt found for user: {user.Email}");
                return ErrorResponse(AppMessage.RefreshTokenNotFound.GetDescription());
            }

            var policy = await _authPolicyRepository.GetAsync(x => true);
            if (policy == null)
            {
                _logger.LogWarning("Auth policy not found.");
                return ErrorResponse(AppMessage.AuthPolicyTokenNotFound.GetDescription());
            }

            var context = await _tokenRepository.ApplyPolicy(user, attempt, policy, string.Empty);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var authResponse = _tokenRepository.GetAuthResponse(user, context, policy);

            //var userRole = await _userRoleRepo.GetAsync(x => x.UserId == user.Id);
            //if (userRole == null)
            //    return ErrorResponse(AppMessage.UnableToFindRecord.GetDescription());

            //var rights = await _roleRightRepo.GetAllAsync(x => x.RoleId == userRole.RoleId);

            //var rightIds = rights.Select(r => r.RightId).ToList();

            return new ResponseBase
            {
                Status = true,
                Data = new AuthResponse
                {
                    IsSuccess = true,
                    UserId = user.Id,
                    Name = user.DisplayName,
                    Email = user.Email ?? string.Empty,
                    SessionId = context.Attempt.Id,
                    AccessToken = authResponse.AccessToken,
                    RefreshToken = authResponse.RefreshToken,
                    ExpiryDate = authResponse.ExpiryDate,
                    EnforceEmailConfirmation = policy.EnforceEmailConfirmation,
                    EnforceMobileConfirmation = policy.EnforceMobileConfirmation,
                    Enforce2FactorVerification = policy.Enforce2FactorVerification,
                    EnforcePasswordChangeOnFirstLogin = policy.EnforcePasswordChangeOnFirstLogin,
                    EnforceProfileCompletion = policy.EnforceProfileCompletion,
                    //RoleId = userRole.RoleId,
                    //Rights = rightIds
                },
                Message = AppMessage.LoginSuccessful.GetDescription()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during refresh token process.");
            return ErrorResponse("An error occurred while processing your request.");
        }
    }
}
