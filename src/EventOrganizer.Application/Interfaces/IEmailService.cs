namespace EventOrganizer.Application.Interfaces;

public interface IEmailService
{
    Task SendConfirmationEmailAsync(string toEmail, string toName, string confirmationLink, CancellationToken cancellationToken = default);
}
