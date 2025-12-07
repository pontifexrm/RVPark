using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens.Experimental;
using RVParking.Data;
using RVParking.Services.Environment;
using RVParking.Services.Logging;
using System.Net.Mail;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace RVParking.Services.Email
{
    public class SmtpEmailService : IEmailService
    {
        // NOT TESTED YET <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
        //public Task<bool> SendEmailAsync(EmailMessage message)
        //{
        //    // Implement SMTP email sending logic here
        //    return Task.FromResult(true);
        //}

        //public Task<bool> SendEmailAsync(string to, string subject, string body)
        //{
        //    // Implement SMTP email sending logic here
        //    return Task.FromResult(true);
        //}
        private readonly EmailConfiguration _emailConfiguration;
        private readonly IAppLogger _appLogger;
        private readonly IEnvironmentInfoService _env;

        public SmtpEmailService(IOptions<EmailConfiguration> emailConfiguration, IAppLogger appLogger, IEnvironmentInfoService env)
        {
            this._emailConfiguration = emailConfiguration.Value;
            _appLogger = appLogger;
            _env = env;
        }

        public async Task<bool> SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return await (Task<bool>)Execute(email, subject, htmlMessage);
        }
        public async Task<bool> SendEmailAsync(EmailMessage message)
        {
            return await (Task<bool>)Execute(message.Email, message.Subject, message.ConfirmMsg);  // TODO: Might need to change EmailMessage to have a default body
        }
        private async Task Execute(string to, string subject, string htmlMessage)
        {
            string host = _emailConfiguration.Host;
            int port = _emailConfiguration.Port;
            string username = _emailConfiguration.Username;
            string password = _emailConfiguration.Password;
            string from = _emailConfiguration.From;
            string name = _emailConfiguration.Name;
            bool enableSsl = _emailConfiguration.EnableSSL;

            MailAddress sender = new MailAddress(from);
            if (!string.IsNullOrEmpty(name))
                sender = new MailAddress(from, name);
            MailAddress toEmail = new MailAddress(to);
            MailAddress ccEmail = new MailAddress("ron@pontifex.nz");
            using MailMessage message = new MailMessage(sender, toEmail);
            message.Subject = 
                    _env.ShouldDisplayEnvInfo ? $"TEST-{subject}" : subject;
            message.Body = htmlMessage;
            message.Bcc.Add(ccEmail);
            message.HeadersEncoding = Encoding.UTF8;
            message.SubjectEncoding = Encoding.UTF8;
            message.BodyEncoding = Encoding.UTF8;
            message.IsBodyHtml = true;

            using (var smtp = new SmtpClient(host))
            {
                smtp.Port = port;
                smtp.EnableSsl = enableSsl;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.UseDefaultCredentials = false;

                if (!string.IsNullOrEmpty(username))
                    smtp.Credentials = new System.Net.NetworkCredential(username, password);
                try
                {
                    await smtp.SendMailAsync(message);

                    var alogMsg = $"EmailTo: {message.To} fm: {message.From} Subj: {message.Subject} Msg: {message.Body}";
                    var sVia = _env.ShouldDisplayEnvInfo ? $"TEST-SMTP - {_emailConfiguration.Host}" : $"SMTP - {_emailConfiguration.Host}";
                    alogMsg = _env.ShouldDisplayEnvInfo ? $"TEST-{alogMsg}" : alogMsg;

                    await _appLogger.LogAsync("Info", $"{sVia}", $"Email sent Details:-{alogMsg}");

                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
    }
    public class SmtpSettings
    {
        public string Server { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
    }
}
