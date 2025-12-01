using Microsoft.Extensions.Configuration;
using RVParking.Data;
using TNZAPI.NET.Core;

namespace RVParking.Services.Email
{
    public class TNZEmailService : IEmailService
    {
        private readonly TNZSettings _settings;
        public TNZEmailService(IConfiguration configuration)
        {
            _settings = configuration.GetSection("TNZ").Get<TNZSettings>()
           ?? throw new InvalidOperationException("TNZ settings not configured");

        }

        public async Task<bool> SendEmailAsync(EmailMessage message)
        {
            await SendEmailAsync(message.To, message.Subject, message.Body);
            return true;
        }

        public Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            // Using TNZ
            var apiUser = new TNZApiUser()  // this shoild come from configuration injection
            {
                AuthToken = _settings.AuthToken
            };
            var client = new TNZApiClient(apiUser);

            var reponseEmail = client.Messaging.Email.SendMessage(
                fromEmail: _settings.fromEmail,
                emailSubject: subject,
                messagePlain: body,
                destination: to
            );
            return Task.FromResult( true );
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
