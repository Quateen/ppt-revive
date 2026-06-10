//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using PPTRevive.Application.Department.Commands.UpdateDepartment;
//using PPTRevive.Application.Users.Queries.GetUsersWithPagination;

//namespace PPTRevive.Web.Controllers;

//[Route("api/[controller]")]
//[ApiController]
//[Authorize]
//public class UserController : ControllerBase
//{
//    private readonly ISender _sender;

//    public UserController(ISender sender)
//    {
//        _sender = sender;
//    }

//    [HttpGet("get")]
//    public async Task<IActionResult> GetUsers([FromQuery] GetUsersWithPaginationQuery query)
//    {
//        var result = await _sender.Send(query);
//        return result.Status ? Ok(result) : BadRequest(result);
//    }

//    //[HttpPost("create")]
//    //public async Task<IActionResult> CreateUser(CreateUserCommand command)
//    //{
//    //    var result = await _sender.Send(command);
//    //    return result.Status ? Ok(result) : BadRequest(result);
//    //}

//    [HttpPut("update")]
//    public async Task<IActionResult> UpdateUser(UpdateUserCommand command)
//    {
//        var result = await _sender.Send(command);
//        return result.Status ? Ok(result) : BadRequest(result);
//    }

//    [HttpGet("rights")]
//    public async Task<IActionResult> GetUserRights()
//    {
//        var result = await _sender.Send(new GetUserRightsQuery());
//        return result.Status ? Ok(result) : BadRequest(result);
//    }
//}
