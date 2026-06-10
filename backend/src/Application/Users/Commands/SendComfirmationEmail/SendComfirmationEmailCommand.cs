//using PPTRevive.Application.Common.Interfaces;
//using PPTRevive.Application.Common.Models;
//using PPTRevive.Domain.Common;
//using PPTRevive.Domain.Entities;
//using PPTRevive.Domain.Enums;

//namespace PPTRevive.Application.Users.Commands.CreateUser;

//public record SendComfirmationEmailCommand : IRequest<ResponseBase>
//{
//    public int UserId { get; set; }
//    public required string Password { get; set; }
//    public int RoleId { get; set; }
//}

//public class SendComfirmationEmailCommandValidator : AbstractValidator<SendComfirmationEmailCommand>
//{
//    public SendComfirmationEmailCommandValidator()
//    {

//    }
//}

//public class SendComfirmationEmailCommandHandler : IRequestHandler<SendComfirmationEmailCommand, ResponseBase>
//{
//    private readonly IApplicationDbContext _context;
//    private readonly IIdentityService _identityService;

//    public SendComfirmationEmailCommandHandler(IApplicationDbContext context, IIdentityService identityService)
//    {
//        _context = context;
//        _identityService = identityService;
//    }

//    public async Task<ResponseBase> Handle(SendComfirmationEmailCommand request, CancellationToken cancellationToken)
//    {
//        var token = await _identityService.CreateEmailConfirmToken(request.UserId);
//        // var user = await _identityService.GetUsersAsync(request.UserId);

//        // Create User Role Entry Here
//        var role = new UserRole();
//        role.UserId = request.UserId;
//        role.RoleId = request.RoleId;

//        _context.UserRole.Add(role);
//        await _context.SaveChangesAsync(cancellationToken);

//        // Send email here


//        return new ResponseBase()
//        {
//            Status = true,
//            Message = AppMessage.RecordCreatedSuccessfully.GetDescription()
//        };
//    }
//}
