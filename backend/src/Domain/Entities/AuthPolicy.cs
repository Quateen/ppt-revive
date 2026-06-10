namespace PPTRevive.Domain.Entities;
public class AuthPolicy : BaseAuditableEntity
{
    public int UserType { get; set; }
    public bool Enforce2FactorVerification { get; set; }
    public bool EnforcePasswordChangeOnFirstLogin { get; set; }
    public bool EnforceBackendActivation { get; set; }
    public bool EnforceEmailConfirmation { get; set; }
    public bool EnforceMobileConfirmation { get; set; }
    public bool EnforceProfileCompletion { get; set; }
    public int BaseTokenDurationMinutes { get; set; }
    public int FullTokenDurationMinutes { get; set; }
    public int RefreshTokenDurationMinutes { get; set; }
}
