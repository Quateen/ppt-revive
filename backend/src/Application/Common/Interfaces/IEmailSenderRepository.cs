namespace PPTRevive.Application.Common.Interfaces;
public interface IEmailSenderRepository
{
    Task SendEmailAsync(string recipientEmail, string subject, string body);
}
