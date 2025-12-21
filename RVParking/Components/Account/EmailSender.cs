using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Net.NetworkInformation;
using TNZAPI.NET;
using TNZAPI.NET.Core;
using TNZAPI.NET.Api.Addressbook.Contact.Dto;
using Microsoft.AspNetCore.Identity.UI.Services;
using RVParking.Data;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;


namespace RVParking.Components.Account
{
    //: IEmailSender<ApplicationUser>
    public class EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor, ILogger<EmailSender> logger, IConfiguration configuration) : IEmailSender<ApplicationUser>
    {
        private readonly ILogger logger = logger;
        private readonly IConfiguration configuration = configuration;

        public AuthMessageSenderOptions Options { get; set; } = optionsAccessor.Value;

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
        // TODO: these need to be changed to use rvpark's email service
        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            string authToken = string.Empty; string fromEmail = string.Empty;
            if (string.IsNullOrEmpty(Options.AuthToken))
            {
                authToken = configuration["TNZAPI:AuthToken"];
                fromEmail = configuration["TNZAPI:fromEmail"];
            }
            else
            {
                authToken = Options.AuthToken;
                fromEmail = Options.fromEmail;
            }
            await Execute(authToken, fromEmail, subject, message, toEmail);
        }

        public async Task Execute(string authToken,  string fromEmail, string subject, string message,
            string toEmail)
        {
            var apiUser = new TNZApiUser() { AuthToken = authToken };
            var client = new TNZApiClient(apiUser);
            await client.Messaging.Email.SendMessageAsync(
                                           fromEmail: fromEmail,
                                           emailSubject: subject,
                                           messageHTML: message,
                                           destination: toEmail
                                         );

            logger.LogInformation("Email to {EmailAddress} sent!", toEmail);
        }
    }
}
