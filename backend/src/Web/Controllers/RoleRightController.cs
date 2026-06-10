//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using PPTRevive.Application.Roles.Commands;
//using PPTRevive.Application.Roles.Queries.GetRolesWithPagination;
//using PPTRevive.Application.Roles.Rights.Queries;

//namespace PPTRevive.Web.Controllers;

//[Route("api/[controller]")]
//[ApiController]
//[Authorize]
//public class RoleRightsController : ControllerBase
//{
//    private readonly ISender _sender;

//    public RoleRightsController(ISender sender)
//    {
//        _sender = sender;
//    }

//    [HttpGet("roles")]
//    public async Task<IActionResult> GetRoleRight([FromQuery] GetRolesWithPaginationQuery query)
//    {
//        var result = await _sender.Send(query);
//        return result.Status ? Ok(result) : BadRequest(result);
//    }

//    [HttpPost("roles")]
//    public async Task<IActionResult> CreateRoleRights(CreateRoleRightsCommand request)
//    {
//        var result = await _sender.Send(request);
//        return result.Status ? Ok(result) : BadRequest(result);
//    }

//    [HttpPut("roles")]
//    public async Task<IActionResult> UpdateRoleRights(UpdateRoleRightsCommand request)
//    {
//        var result = await _sender.Send(request);
//        return result.Status ? Ok(result) : BadRequest(result);
//    }

//    #region RIGHTS

//    [HttpGet("rights")]
//    public async Task<IActionResult> GetRights([FromQuery] GetRightsQuery query)
//    {
//        var result = await _sender.Send(query);
//        return result.Status ? Ok(result) : BadRequest(result);
//    }

//    #endregion RIGHTS
//}
