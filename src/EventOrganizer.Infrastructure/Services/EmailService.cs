using EventOrganizer.Application.Interfaces;
using EventOrganizer.Infrastructure.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EventOrganizer.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<SmtpSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendConfirmationEmailAsync(
        string toEmail,
        string toName,
        string confirmationLink,
        CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = "Confirm your Event Organizer account";

        message.Body = new TextPart("html")
        {
            Text = $"""
                <h2>Welcome to Event Organizer!</h2>
                <p>Hi {toName},</p>
                <p>Please confirm your email address by clicking the link below:</p>
                <p><a href="{confirmationLink}" style="background:#1976d2;color:#fff;padding:10px 20px;border-radius:4px;text-decoration:none;">Confirm Email</a></p>
                <p>This link expires in 24 hours.</p>
                <p>If you did not create an account, please ignore this email.</p>
                """
        };

        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.Host, _settings.Port,
                _settings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None,
                cancellationToken);

            if (!string.IsNullOrEmpty(_settings.Username))
                await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send confirmation email to {Email}", toEmail);
            // Log the link in development so confirmation still works without SMTP
            _logger.LogWarning("Confirmation link for {Email}: {Link}", toEmail, confirmationLink);
            throw;
        }
    }
}
