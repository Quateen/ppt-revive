using PPTRevive.Domain.Entities;

namespace PPTRevive.Application.Common.Models;
public class LoginAttemptContext
{
    public LoginAttempts Attempt { get; set; } = null!;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
}
