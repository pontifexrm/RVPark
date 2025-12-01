using RVParking.Data;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace RVParking.Services.Email
{
    public class MailKitEmailService : IEmailService
    {
        private readonly MailKitSettings _settings;
        private readonly ILogger<MailKitEmailService> _logger;

        public MailKitEmailService(IConfiguration configuration, ILogger<MailKitEmailService> logger)
        {
            _settings = configuration.GetSection("MailKit").Get<MailKitSettings>()
                        ?? throw new InvalidOperationException("MailKit settings not configured");
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(EmailMessage message)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(_settings.Name, _settings.From));
                email.To.Add(new MailboxAddress("", message.Email));
                email.Subject = message.Subject;

                email.Body = new TextPart(message.IsHtml ? TextFormat.Html : TextFormat.Plain)
                {
                    Text = message.Message
                };

                using var client = new SmtpClient();

                // Configure SSL/TLS based on settings
                var secureSocketOptions = _settings.EnableSsl
                    ? (_settings.Port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls)
                    : SecureSocketOptions.None;

                await client.ConnectAsync(_settings.Server, _settings.Port, secureSocketOptions);

                // Note: MailKit doesn't use UseDefaultCredentials like System.Net.Mail
                if (!string.IsNullOrEmpty(_settings.Username))
                {
                    await client.AuthenticateAsync(_settings.Username, _settings.Password);
                }

                await client.SendAsync(email);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully to {Recipient}", message.To);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Recipient}", message.To);
                return false;
            }
        }

        public Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            return SendEmailAsync(new EmailMessage
            {
                To = to,
                Subject = subject,
                Body = body,
                IsHtml = true
            });
        }
    }

    public class MailKitSettings
    {
        public string Server { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string From { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
        public bool IsHtml { get; set; } = true;
    }

}
