using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
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
/// funnel is pre-login). If Mailchimp is configured the lead is upserted into the
/// audience; either way it is recorded via structured logging (persisted by the Serilog
/// MySQL sink). Capture is best-effort — a downstream failure never blocks the visitor.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
[EnableRateLimiting("ppt-anon")]
public class LeadController : ControllerBase
{
    private readonly ILogger<LeadController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public LeadController(ILogger<LeadController> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost]
    public async Task<IActionResult> Capture([FromBody] LeadRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { status = false, error = "A valid email is required." });

        var email = request.Email.Trim();
        var source = request.Source ?? "unknown";

        // Always record the lead (queryable in the Logs table).
        _logger.LogWarning("LEAD_CAPTURE email={Email} source={Source}", email, source);

        // Push to Mailchimp when configured; failure must not break the funnel.
        await TryAddToMailchimp(email, source);

        return Ok(new { status = true, message = "You're on the list." });
    }

    private async Task TryAddToMailchimp(string email, string source)
    {
        var apiKey = _configuration["Mailchimp:ApiKey"];
        var listId = _configuration["Mailchimp:AudienceId"];
        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(listId))
            return; // Mailchimp not configured — logging above is the system of record.

        // The data-center prefix is the segment after the '-' in the API key (e.g. us21).
        var dc = apiKey.Contains('-') ? apiKey[(apiKey.LastIndexOf('-') + 1)..] : null;
        if (string.IsNullOrWhiteSpace(dc))
        {
            _logger.LogWarning("Mailchimp API key has no data-center suffix; skipping.");
            return;
        }

        try
        {
            // Idempotent upsert: PUT members/{md5(lowercased email)}.
            var subscriberHash = Convert.ToHexString(
                MD5.HashData(Encoding.UTF8.GetBytes(email.ToLowerInvariant()))).ToLowerInvariant();

            var url = $"https://{dc}.api.mailchimp.com/3.0/lists/{listId}/members/{subscriberHash}";
            var body = new
            {
                email_address = email,
                status_if_new = "subscribed",
                status = "subscribed",
                tags = new[] { $"ppt-revive:{source}" }
            };

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(Encoding.ASCII.GetBytes($"anystring:{apiKey}")));

            using var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await client.PutAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Mailchimp upsert failed ({Status}): {Error}", response.StatusCode, err);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Mailchimp upsert threw for {Email}.", email);
        }
    }
}
