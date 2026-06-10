//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Identity;
//using PPTRevive.Application.Common.Interfaces;
//using PPTRevive.Application.Common.Models;
//using PPTRevive.Domain.Common;
//using PPTRevive.Domain.Entities;
//using PPTRevive.Domain.Enums;


//namespace PPTRevive.Application.Users.Commands.CreateUser;
//public class CreateUserCommand : IRequest<ResponseBase>
//{
//    public required string FirstName { get; set; }
//    public required string LastName { get; set; }
//    public required string Email { get; set; }
//    public string PhoneNumber { get; set; } = string.Empty;
//    public DateOnly DateOfBirth { get; set; }
//    public string? Address { get; set; }
//    public int GenderId { get; set; }
//    public int UserTypeId { get; set; }
//    public int RoleId { get; set; }
//}

//public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, ResponseBase>
//{
//    private readonly UserManager<User> _userManager;
//    private readonly IUnitOfWork _unitOfWork;
//    private readonly IDataRepository<UserProfile> _dataRepository;
//    private readonly IDataRepository<UserRole> _roleRepository;
//    private readonly IUser _currentUser;
//    private readonly IEmailSenderRepository _emailSenderRepository;
//    private readonly IHttpContextAccessor _httpContextAccessor;
//    private readonly IApplicationDbContext _dbContext;

//    public CreateUserCommandHandler(UserManager<User> userManager, IUnitOfWork unitOfWork,
//        IDataRepository<UserProfile> dataRepository, IDataRepository<UserRole> roleRepository,
//        IUser currentUser, IEmailSenderRepository emailSenderRepository,
//        IHttpContextAccessor httpContextAccessor, IApplicationDbContext dbContext)
//    {
//        _userManager = userManager;
//        _unitOfWork = unitOfWork;
//        _dataRepository = dataRepository;
//        _roleRepository = roleRepository;
//        _currentUser = currentUser;
//        _emailSenderRepository = emailSenderRepository;
//        _httpContextAccessor = httpContextAccessor;
//        _dbContext = dbContext;
//    }

//    private ResponseBase ErrorResponse(string error)
//    {
//        return new ResponseBase
//        {
//            Status = false,
//            Error = error
//        };
//    }

//    public async Task<ResponseBase> Handle(CreateUserCommand request, CancellationToken cancellationToken)
//    {
//        try
//        {
//            var currentUserWorkSpace = await _dbContext.WorkSpaceUser.FindAsync(_currentUser.Id);
//            if (currentUserWorkSpace == null)
//                return ErrorResponse(AppMessage.WorkspaceNotFound.GetDescription());

//            var createUser = new User
//            {
//                FirstName = request.FirstName,
//                LastName = request.LastName,
//                DisplayName = $"{request.FirstName} {request.LastName}",
//                Email = request.Email,
//                PhoneNumber = request.PhoneNumber,
//                UserName = request.Email,
//                Address = request.Address,
//                UserTypeId = request.UserTypeId,
//                IsProfileCompleted = false,
//                TwoFactorEnabled = true,
//                EmailConfirmed = true,
//                PhoneNumberConfirmed = true,
//                LockoutEnabled = false,
//                CreatedBy = _currentUser.Id,
//                Created = DateTime.UtcNow,
//                LastModifiedBy = _currentUser.Id,
//                LastModified = DateTime.UtcNow,
//            };

//            var result = await _userManager.CreateAsync(createUser);

//            if (!result.Succeeded)
//            {
//                var errorMessage = result.Errors.FirstOrDefault()?.Description ?? AppMessage.UnableToRegisterUser.GetDescription();
//                return ErrorResponse(errorMessage);
//            }

//            var userProfile = new UserProfile
//            {
//                UserId = createUser.Id,
//                FirstName = request.FirstName,
//                LastName = request.LastName,
//                Email = request.Email,
//                Address = request.Address ?? string.Empty,
//                MobileNumber = request.PhoneNumber,
//                DateOfBirth = request.DateOfBirth,
//                Gender = request.GenderId,
//            };

//            var userRole = new UserRole
//            {
//                RoleId = request.RoleId,
//                UserId = createUser.Id
//            };

//            var userWorkSpace = new WorkSpaceUser
//            {
//                UserId = createUser.Id,
//                WorkSpaceId = currentUserWorkSpace.WorkSpaceId,
//                RoleId = request.RoleId,
//                CreatedBy = _currentUser.Id,
//                Created = DateTime.UtcNow,
//                LastModifiedBy = _currentUser.Id,
//                LastModified = DateTime.UtcNow
//            };

//            _dataRepository.Add(userProfile, _currentUser.Id);
//            _roleRepository.Add(userRole, _currentUser.Id);
//            _dbContext.WorkSpaceUser.Add(userWorkSpace);
//            await _unitOfWork.SaveChangesAsync(cancellationToken);

//            // Construct base URL for email
//            var req = _httpContextAccessor?.HttpContext?.Request;
//            var baseUrl = req != null ? $"{req.Scheme}://{req.Host}/complete-registration?userId={createUser.Id}"
//                : "http://tsapp.dev.techlign.com/complete-registration?userId={createUser.Id}";

//            string body = string.Format(@"
//             <!DOCTYPE html>
//             <html lang='en'>
//             <head>
//                 <meta charset='UTF-8'>
//                 <meta http-equiv='X-UA-Compatible' content='IE=edge'>
//                 <meta name='viewport' content='width=device-width, initial-scale=1.0'>
//                 <title>Registration Complete</title>
//             </head>
//             <body>
//                 <p>Complete your registration. <a href='{0}'>Click here</a> to proceed.</p>
//             </body>
//             </html>", baseUrl);

//            await _emailSenderRepository.SendEmailAsync(createUser.Email, "Complete Registration", body);

//            return new ResponseBase
//            {
//                Status = true,
//                Data = new
//                {
//                    UserId = createUser.Id,
//                    UserProfileId = userProfile.Id,
//                    Url = baseUrl
//                },
//                Message = AppMessage.RegistrationEmailSentSuccessfully.GetDescription()
//            };
//        }
//        catch (Exception ex)
//        {
//            return ErrorResponse(ex.Message);
//        }
//    }
//}
