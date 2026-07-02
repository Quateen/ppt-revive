//using PPTRevive.Application.Common.Behaviours;
//using PPTRevive.Application.Common.Interfaces;
//using PPTRevive.Application.TodoItems.Commands.CreateTodoItem;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
//using Microsoft.Extensions.Logging;
//using Moq;
//using NUnit.Framework;

//namespace PPTRevive.Application.UnitTests.Common.Behaviours;

//public class RequestLoggerTests
//{
//    private Mock<ILogger<CreateTodoItemCommand>> _logger = null!;
//    private Mock<IUser> _user = null!;
//    private Mock<IIdentityService> _identityService = null!;

//    [SetUp]
//    public void Setup()
//    {
//        _logger = new Mock<ILogger<CreateTodoItemCommand>>();
//        _user = new Mock<IUser>();
//        _identityService = new Mock<IIdentityService>();
//    }

//    [Test]
//    public async Task ShouldCallGetUserNameAsyncOnceIfAuthenticated()
//    {
//        // Make this compatible with int
//        //_user.Setup(x => x.Id).Returns(int.Newint().ToString());

//        var requestLogger = new LoggingBehaviour<CreateTodoItemCommand>(_logger.Object, _user.Object, _identityService.Object);

//        await requestLogger.Process(new CreateTodoItemCommand { ListId = 1, Title = "title" }, new CancellationToken());

//        //_identityService.Verify(i => i.GetUserNameAsync(It.IsAny<string>()), Times.Once);
//    }

//    [Test]
//    public async Task ShouldNotCallGetUserNameAsyncOnceIfUnauthenticated()
//    {
//        var requestLogger = new LoggingBehaviour<CreateTodoItemCommand>(_logger.Object, _user.Object, _identityService.Object);

//        await requestLogger.Process(new CreateTodoItemCommand { ListId = 1, Title = "title" }, new CancellationToken());

//        //_identityService.Verify(i => i.GetUserNameAsync(It.IsAny<string>()), Times.Never);
//    }
//}
