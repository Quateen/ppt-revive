using Microsoft.AspNetCore.Authorization;

namespace PPTRevive.Web.Controllers.Base;

[Authorize(Policy = "full-compliant")]

public class FullCompliantController : BasicCompliantController
{
}
