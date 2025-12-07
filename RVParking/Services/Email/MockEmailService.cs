using RVParking.Data;
using RVParking.Services.Environment;
using RVParking.Services.Logging;
using System.Runtime;

namespace RVParking.Services.Email
{
    public class MockEmailService : IEmailService
    {
        private readonly ILogger<MockEmailService> _logger;
        private readonly IAppLogger _appLogger;
        private readonly IEnvironmentInfoService _env;

        public MockEmailService(ILogger<MockEmailService> logger, IAppLogger appLogger, IEnvironmentInfoService env)
        {
            _logger = logger;
            _appLogger = appLogger;
            _env = env;
        }

        public async Task<bool> SendEmailAsync(EmailMessage message)
        {
            _logger.LogInformation("Mock Email Sent - To: {To}, Subject: {Subject}",
                message.Email, message.Subject);
            // prepare applog message
            var alogMsg = $"EmailTo: {message.To} fm: {message.From} Subj: {message.Subject} Msg: {message.Body}";
            var sVia = $"Mock - no Host";
            await _appLogger.LogAsync("Info", $"{sVia}", $"Email sent Details:-{alogMsg}");

            // Simulate email sending
            return await Task.FromResult(true);
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
