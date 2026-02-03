using RVPark.Data;

namespace RVPark.Services.Email
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(EmailMessage message);
        Task<bool> SendEmailAsync(string to, string subject, string body);

    }
}
