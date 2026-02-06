using RVPark.Data;
using RVPark.Services.Environment;
using RVPark.Services.Logging;
using TNZAPI.NET.Core;

namespace RVPark.Services.Email
{
    public class TNZEmailService : IEmailService
    {
        private readonly TNZSettings _settings;
        private readonly IAppLogger _applogger;
        private readonly IEnvironmentInfoService _env;


        public TNZEmailService(IConfiguration configuration, IAppLogger appLogger, IEnvironmentInfoService env)
        {
            _settings = configuration.GetSection("TNZ").Get<TNZSettings>()
           ?? throw new InvalidOperationException("TNZ settings not configured");

            _applogger = appLogger;
            _env = env;
        }

        public async Task<bool> SendEmailAsync(EmailMessage message)
        {
            await SendEmailAsync(message.To, message.Subject, message.Body);
            return true;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            // Using TNZ
            var apiUser = new TNZApiUser()  // this shoild come from configuration injection
            {
                AuthToken = _settings.AuthToken
            };
            var client = new TNZApiClient(apiUser);

            var reponseEmail = client.Messaging.Email.SendMessage(
                fromEmail: _settings.fromEmail,
                emailSubject: _env.ShouldDisplayEnvInfo ? $"TEST-{subject}" : subject,
                messagePlain: body,
                destination: to
            );
            // prepare applog message
            var alogMsg = $"EmailTo: {to} fm: {_settings.fromEmail} Subj: {subject} Msg: {body}";
            alogMsg = _env.ShouldDisplayEnvInfo ? $"TEST-{alogMsg}" : alogMsg;

            var sVia = _env.ShouldDisplayEnvInfo ? $"TEST-TNZApiUser" : $"TNZApiUser";
            await _applogger.LogAsync("Info", $"{sVia}", $"Email sent Details:-{alogMsg}");

            return true;
        }
    }
    public class TNZSettings
    {
        public string AuthToken { get; set; } = string.Empty;
        public string fromEmail { get; set; } = string.Empty;
        public string CCemail { get; set; } = string.Empty;
        public string SMSrept { get; set; } = string.Empty;

    }

}
