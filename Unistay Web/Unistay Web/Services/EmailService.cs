namespace Unistay_Web.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            // TODO: Implement email sending logic
            _logger.LogInformation($"Sending email to {to} with subject: {subject}");
            await Task.CompletedTask;
        }

        public async Task SendVerificationEmailAsync(string to, string verificationLink)
        {
            var subject = "Xác thực tài khoản Unistay";
            var body = $"Vui lòng click vào link sau để xác thực tài khoản: {verificationLink}";
            await SendEmailAsync(to, subject, body);
        }

        public async Task SendPasswordResetEmailAsync(string to, string resetLink)
        {
            var subject = "Đặt lại mật khẩu Unistay";
            var body = $"Vui lòng click vào link sau để đặt lại mật khẩu: {resetLink}";
            await SendEmailAsync(to, subject, body);
        }
    }
}
