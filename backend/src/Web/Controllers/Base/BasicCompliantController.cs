using Microsoft.AspNetCore.Authorization;

namespace PPTRevive.Web.Controllers.Base;

[Authorize(Policy = "basic-compliant")]
public class BasicCompliantController : AnonymousController
{
}
