//using PPTRevive.Application.Common.Interfaces;
//using PPTRevive.Application.Common.Models;

//namespace PPTRevive.Application.Users.Commands.CreateUser;

//public record SetPasswordCommand : IRequest<ResponseBase>
//{
//    public int UserId { get; set; }
//    public required string Password { get; set; }
//}

//public class SetPasswordCommandValidator : AbstractValidator<SetPasswordCommand>
//{
//    public SetPasswordCommandValidator()
//    {

//    }
//}

//public class SetPasswordCommandHandler : IRequestHandler<SetPasswordCommand, ResponseBase>
//{
//    private readonly IApplicationDbContext _context;
//    private readonly IIdentityService _identityService;

//    public SetPasswordCommandHandler(IApplicationDbContext context, IIdentityService identityService)
//    {
//        _context = context;
//        _identityService = identityService;
//    }

//    public async Task<ResponseBase> Handle(SetPasswordCommand request, CancellationToken cancellationToken)
//    {
//        var passwordSet = await _identityService.SetPassword(request.UserId, request.Password);

//        return new ResponseBase()
//        {
//            Status = true,
//            Data = passwordSet,
//        };
//    }
//}
