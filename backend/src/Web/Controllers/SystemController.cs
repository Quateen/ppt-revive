//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using PPTRevive.Application.AuthPolicy.Command;
//using PPTRevive.Application.AuthPolicy.Queries;

//namespace PPTRevive.Web.Controllers;

//[Route("api/[controller]")]
//[ApiController]
//[Authorize]
//public class SystemController : ControllerBase
//{
//    private readonly ISender _sender;

//    public SystemController(ISender sender)
//    {
//        _sender = sender;
//    }

//    [HttpGet("get-auth-policy")]
//    public async Task<IActionResult> AuthPolicyGet([FromQuery] AuthPolicyGetQuery query)
//    {
//        var result = await _sender.Send(query);
//        return result.Status ? Ok(result) : BadRequest(result);
//    }

//    [HttpPut("update-auth-policy")]
//    public async Task<IActionResult> ProjectUpdate(AuthPolicyUpdateCommand request)
//    {
//        var result = await _sender.Send(request);
//        return result.Status ? Ok(result) : BadRequest(result);
//    }

//    //[HttpPost("create")]
//    //public async Task<IActionResult> ProjectCreate(ProjectCreateCommand request)
//    //{
//    //    var result = await _sender.Send(request);
//    //    if (result.Status) return Ok(result); else return BadRequest(result);
//    //}

//    //[HttpPut("update")]
//    //public async Task<IActionResult> ProjectUpdate(ProjectUpdateCommand request)
//    //{
//    //    var result = await _sender.Send(request);
//    //    if (result.Status) return Ok(result); else return BadRequest(result);
//    //}

//    //[HttpGet("user")]
//    //public async Task<IActionResult> UserProjectGet()
//    //{
//    //    var query = new UserProjectGetQuery();
//    //    var result = await _sender.Send(query);

//    //    return result.Status ? Ok(result) : BadRequest(result);
//    //}
//}
