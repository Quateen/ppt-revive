//using PPTRevive.Application.Common.Interfaces;
//using PPTRevive.Application.Common.Models;
//using PPTRevive.Domain.Common;
//using PPTRevive.Domain.Entities;
//using PPTRevive.Domain.Enums;

//namespace PPTRevive.Application.Users.Commands.CreateUser;

//public record CompleteEmailComfirmationCommand : IRequest<ResponseBase>
//{
//    public int UserId { get; set; }
//    public required string Token { get; set; }
//}

//public class CompleteEmailComfirmationCommandValidator : AbstractValidator<CompleteEmailComfirmationCommand>
//{
//    public CompleteEmailComfirmationCommandValidator()
//    {

//    }
//}

//public class CompleteEmailComfirmationCommandHandler : IRequestHandler<CompleteEmailComfirmationCommand, ResponseBase>
//{
//    private readonly IApplicationDbContext _context;
//    private readonly IIdentityService _identityService;

//    public CompleteEmailComfirmationCommandHandler(IApplicationDbContext context, IIdentityService identityService)
//    {
//        _context = context;
//        _identityService = identityService;
//    }

//    public async Task<ResponseBase> Handle(CompleteEmailComfirmationCommand request, CancellationToken cancellationToken)
//    {
//        var emailConfirmed = await _identityService.CompleteEmailConfirmation(request.UserId, request.Token);
//        if (emailConfirmed)
//        {
//            // Send create password token
//        }
//        else
//        {
//            // show error
//        }

//        // Send email here


//        return new ResponseBase()
//        {
//            Status = true,
//            Message = AppMessage.RecordCreatedSuccessfully.GetDescription()
//        };
//    }
//}
