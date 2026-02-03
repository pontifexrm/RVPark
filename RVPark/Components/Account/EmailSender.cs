using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using RVPark.Data;
using RVPark.Services.Email;
using System.Net.Mail;
using System.Net.NetworkInformation;


namespace RVPark.Components.Account
{
    //: IEmailSender<ApplicationUser>

    public class EmailSender(ILogger<EmailSender> logger, IConfiguration configuration, IEmailService EmailService) : IEmailSender<ApplicationUser>
    {
        private readonly ILogger logger = logger;
        private readonly IConfiguration configuration = configuration;
        private readonly IEmailService emailService = EmailService;


        //public AuthMessageSenderOptions Options { get; set; } = optionsAccessor.Value;

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
            await emailService.SendEmailAsync(toEmail, subject, message);
            logger.LogInformation("Email to {EmailAddress} sent!", toEmail);

        }

        public async Task Execute(string authToken,  string fromEmail, string subject, string message,
            string toEmail)
        {
            //Implemented email sending via this template's email service NB authToken not needed here
            await emailService.SendEmailAsync(toEmail, subject, message);
            logger.LogInformation("Email to {EmailAddress} sent!", toEmail);

        }
    }
}
