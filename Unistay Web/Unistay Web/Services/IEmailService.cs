namespace Unistay_Web.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendVerificationEmailAsync(string to, string verificationLink);
        Task SendPasswordResetEmailAsync(string to, string resetLink);
    }
}
