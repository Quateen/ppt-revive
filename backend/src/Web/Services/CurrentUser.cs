using System.Security.Claims;

using PPTRevive.Application.Common.Interfaces;

namespace PPTRevive.Web.Services;

public class CurrentUser : IUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    //public string? Id => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    public int Id
    {
        get
        {
            var idString = _httpContextAccessor.HttpContext?.User?.FindFirst("usr")?.Value;

            //var idString = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(idString, out int id))
            {
                return id;
            }
            return 0; // or throw an exception if ID is expected to be valid and non-null
        }
    }
}
