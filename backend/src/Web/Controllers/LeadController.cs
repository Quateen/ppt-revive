using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace PPTRevive.Web.Controllers;

public class LeadRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>Where the lead was captured (e.g. "landing", "post-download").</summary>
    public string? Source { get; set; }
}

/// <summary>
/// Captures founding-member leads from the free tier. Anonymous by design (the whole
/// funnel is pre-login). Leads are recorded via structured logging, which the Serilog
/// MySQL sink persists to the Logs table; swap the body for a Mailchimp/ConvertKit/
/// Membership.io call to push straight into the email platform.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
[EnableRateLimiting("ppt-anon")]
public class LeadController : ControllerBase
{
    private readonly ILogger<LeadController> _logger;

    public LeadController(ILogger<LeadController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public IActionResult Capture([FromBody] LeadRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { status = false, error = "A valid email is required." });

        _logger.LogWarning("LEAD_CAPTURE email={Email} source={Source}",
            request.Email.Trim(), request.Source ?? "unknown");

        return Ok(new { status = true, message = "You're on the list." });
    }
}
