using System.Net;
using System.Net.Mail;
using PPTRevive.Application.Common.Interfaces;

namespace PPTRevive.Infrastructure.Services;

public class MailService : IMailService
{
    private readonly string _smtpServer = "smtp.zoho.com"; // Your SMTP server address
    private readonly int _smtpPort = 587; // SMTP port (e.g., 587 for TLS, 465 for SSL)
    private readonly string _smtpUsername = "mail@strahlenstudios.com"; // Your email address
    private readonly string _smtpPassword = "Asdf@1234"; // Your email password

    public async Task SendEmailAsync(string recipientEmail, string subject, string body)
    {
        try
        {
            using (var client = new SmtpClient(_smtpServer, _smtpPort))
            {
                client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                client.EnableSsl = true; // Enable SSL or TLS

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpUsername, "AIC Admin"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true // Set to true if the body is HTML
                };

                // Adding recipient
                mailMessage.To.Add(recipientEmail);

                // Add a plain text alternative view if you're sending HTML email
                if (mailMessage.IsBodyHtml)
                {
                    var plainTextBody = "This is a plain text version of the email body.";
                    var plainTextView = AlternateView.CreateAlternateViewFromString(plainTextBody, null, "text/plain");
                    mailMessage.AlternateViews.Add(plainTextView);

                    var htmlView = AlternateView.CreateAlternateViewFromString(body, null, "text/html");
                    mailMessage.AlternateViews.Add(htmlView);
                }

                // Proper headers
                mailMessage.Headers.Add("X-Mailer", "DotNetCore");
                mailMessage.Headers.Add("X-Priority", "3"); // Normal priority
                mailMessage.Headers.Add("X-MSMail-Priority", "Normal");

                // Adding custom headers for better delivery
                mailMessage.Headers.Add("Return-Path", _smtpUsername);

                await client.SendMailAsync(mailMessage);

                Console.WriteLine("Email sent successfully.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending email: {ex.Message}");
        }
    }
}
