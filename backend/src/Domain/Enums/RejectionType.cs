namespace PPTRevive.Domain.Enums;

public enum RejectionType
{
    None = 0,
    WrongPassword = 1,
    WrongCode = 2,
    RequestExpired = 3,
    AccountLocked = 4,
    EmailNotConfirmed=5,
}
