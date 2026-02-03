using RVPark.Data;

namespace RVPark.Services.Email
{
    internal class SendGridEmailService : IEmailService
    {
        public Task<bool> SendEmailAsync(EmailMessage message)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            throw new NotImplementedException();
        }
    }
}