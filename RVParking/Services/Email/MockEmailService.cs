using RVParking.Data;

namespace RVParking.Services.Email
{
    public class MockEmailService : IEmailService
    {
        private readonly ILogger<MockEmailService> _logger;

        public MockEmailService(ILogger<MockEmailService> logger)
        {
            _logger = logger;
        }

        public Task<bool> SendEmailAsync(EmailMessage message)
        {
            _logger.LogInformation("Mock Email Sent - To: {To}, Subject: {Subject}",
                message.Email, message.Subject);

            // Simulate email sending
            return Task.FromResult(true);
        }

        public Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            return SendEmailAsync(new EmailMessage
            {
                Email = to,
                Subject = subject,
                Message = body
            });
        }

    }
}
