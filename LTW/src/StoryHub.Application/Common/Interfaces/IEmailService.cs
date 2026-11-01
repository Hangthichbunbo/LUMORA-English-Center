namespace StoryHub.Application.Common.Interfaces;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string toEmail, string subject, string htmlMessage, CancellationToken cancellationToken = default);
    Task<bool> SendConfirmationEmailAsync(string toEmail, string displayName, string confirmationLink, CancellationToken cancellationToken = default);
}
