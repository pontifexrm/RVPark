using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Net.NetworkInformation;
using TNZAPI.NET;
using TNZAPI.NET.Core;
using TNZAPI.NET.Api.Addressbook.Contact.Dto;
using Microsoft.AspNetCore.Identity.UI.Services;
using RVParking.Data;
using System.Net.Mail;


namespace RVParking.Components.Account
{
    //: IEmailSender<ApplicationUser>
    public class EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor, ILogger<EmailSender> logger) : IEmailSender<ApplicationUser>
    {
        private readonly ILogger logger = logger;

        public AuthMessageSenderOptions Options { get; } = optionsAccessor.Value;

        public Task SendConfirmationLinkAsync(ApplicationUser user, string email,
                string confirmationLink) => SendEmailAsync(email, "Confirm your email",
                                            "<html lang=\"en\"><head></head><body>Please confirm your account by " +
                                            $"<a href='{confirmationLink}'>clicking here</a>.</body></html>");

        public Task SendPasswordResetLinkAsync(ApplicationUser user, string email,
            string resetLink) => SendEmailAsync(email, "Reset your password",
            "<html lang=\"en\"><head></head><body>Please reset your password by " +
            $"<a href='{resetLink}'>clicking here</a>.</body></html>");

       public Task SendPasswordResetCodeAsync(ApplicationUser user, string email,
            string resetCode) => SendEmailAsync(email, "Reset your password",
            "<html lang=\"en\"><head></head><body>Please reset your password " +
            $"using the following code:<br>{resetCode}</body></html>");

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            if (string.IsNullOrEmpty(Options.AuthToken))
            {
                throw new Exception("Null EmailAuthKey");
            }

            await Execute(Options.AuthToken, subject, message, toEmail);
        }

        public async Task Execute(string authToken, string subject, string message,
            string toEmail)
        {
            var apiUser = new TNZApiUser() { AuthToken = authToken };
            var client = new TNZApiClient(apiUser);
//            var reponseEmail = client.Messaging.Email.SendMessageAsync(
            await client.Messaging.Email.SendMessageAsync(
                                           fromEmail: Options.fromEmail,  
                                           emailSubject: subject,
                                           messageHTML: message,
                                           destination: toEmail
                                         );



            logger.LogInformation("Email to {EmailAddress} sent!", toEmail);
        }

    }
}
