using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PPTRevive.Application.Common.Models;
using PPTRevive.Domain.Enums;

namespace PPTRevive.Web.Shared;

public class HasRightAttribute : AuthorizeAttribute, IAuthorizationFilter
{
    private readonly ApplicationRights _requiredRight;
    public HasRightAttribute(ApplicationRights requiredRight)
    {
        _requiredRight = requiredRight;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var response = new ResponseBase();

        var user = context.HttpContext.User;

        var ra = _requiredRight.ToRightAttribute();

        var rightsClaims = user.FindAll("rights").Select(c => c.Value).ToList();

        // Check if the user has the required right
        if (!rightsClaims.Contains(ra.Id.ToString()))
        {
            response.Error = 403;
            response.Message = $"User does not have right to: {Domain.Common.TypeExtensions.AddSpaces(ra.Name)}";
            context.Result = new JsonResult(response)
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}
