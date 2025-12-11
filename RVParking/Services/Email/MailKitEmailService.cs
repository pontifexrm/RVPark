using RVParking.Data;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using Microsoft.Extensions.Configuration;
using RVParking.Services.Environment;
using RVParking.Services.Logging;

namespace RVParking.Services.Email
{
    /// <summary>
    /// This is used for many mail provider mainly with SMTP We have it here for SmarterASP Email Provider.
    /// </summary>

    public class MailKitEmailService : IEmailService
    {
        private readonly MailKitSettings _settings;
        private readonly ILogger<MailKitEmailService> _logger;
        private readonly IAppLogger _appLogger;
        private IEnvironmentInfoService _env;

        public MailKitEmailService(IConfiguration configuration, ILogger<MailKitEmailService> logger, IAppLogger appLogger, IEnvironmentInfoService env)
        {
            _settings = configuration.GetSection("MailKit").Get<MailKitSettings>()
                        ?? throw new InvalidOperationException("MailKit settings not configured");
            _logger = logger;
            _appLogger = appLogger;
            _env = env;
        }

        public async Task<bool> SendEmailAsync(EmailMessage message)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(_settings.DisplayName, _settings.From));
                email.To.Add(new MailboxAddress("", message.To));
                email.Subject = _env.ShouldDisplayEnvInfo ? $"TEST-{message.Subject}" : message.Subject;

                email.Body = new TextPart(message.IsHtml ? TextFormat.Html : TextFormat.Plain)
                {
                    Text = message.Body
                };


                using var client = new MailKit.Net.Smtp.SmtpClient();
                // Ignore certificate validation errors
                client.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

                // Configure SSL/TLS based on settings
                var secureSocketOptions = _settings.EnableSsl
                    ? (_settings.Port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls)
                    : SecureSocketOptions.None;

                await client.ConnectAsync(_settings.Host, _settings.Port, secureSocketOptions);

                // Note: MailKit doesn't use UseDefaultCredentials like System.Net.Mail
                if (!string.IsNullOrEmpty(_settings.Username))
                {
                    await client.AuthenticateAsync(_settings.Username, _settings.Password);
                }

                await client.SendAsync(email);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully to {Recipient}", message.To);

                // prepare applog message
                var alogMsg = $"EmailTo: {email.To} fm: {email.From} Subj: {email.Subject} Msg: {email.Body}";
                var sVia = _env.ShouldDisplayEnvInfo ? $"TEST-MailKit - {_settings.Host}" :$"MailKit - {_settings.Host}";
                alogMsg = _env.ShouldDisplayEnvInfo ? $"TEST-{alogMsg}" : alogMsg ;

                await _appLogger.LogAsync("Info", $"{sVia}", $"Email sent Details:-{alogMsg}");
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
                IsHtml = false
            });
        }
    }

    public class MailKitSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string From { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
        public bool IsHtml { get; set; } = true;
    }

}
