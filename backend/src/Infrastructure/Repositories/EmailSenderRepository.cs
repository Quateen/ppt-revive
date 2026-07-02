using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PPTRevive.Application.Common.Interfaces;

namespace PPTRevive.Infrastructure.Repositories;

public class EmailSenderRepository : IEmailSenderRepository
{
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly bool _useSsl;
    private readonly ILogger<EmailSenderRepository> _logger;

    public EmailSenderRepository(IConfiguration configuration, ILogger<EmailSenderRepository> logger)
    {
        _logger = logger;
        // No credential defaults: SMTP settings must come from configuration/secrets.
        _smtpServer = configuration["Smtp:Host"] ?? string.Empty;
        _smtpPort = int.TryParse(configuration["Smtp:Port"], out int port) ? port : 587;
        _smtpUsername = configuration["Smtp:Username"] ?? string.Empty;
        _smtpPassword = configuration["Smtp:Password"] ?? string.Empty;
        _useSsl = bool.TryParse(configuration["Smtp:UseSSL"], out bool ssl) ? ssl : true;
    }

    public async Task SendEmailAsync(string recipientEmail, string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(_smtpServer) || string.IsNullOrWhiteSpace(_smtpUsername) || string.IsNullOrWhiteSpace(_smtpPassword))
        {
            _logger.LogError("SMTP is not configured (Smtp:Host/Username/Password); cannot send email to {Recipient}.", recipientEmail);
            throw new InvalidOperationException("Email delivery is not configured.");
        }

        try
        {
            using (var client = new SmtpClient(_smtpServer, _smtpPort))
            {
                client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                client.EnableSsl = _useSsl;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpUsername, "PPTRevive Admin"),
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
