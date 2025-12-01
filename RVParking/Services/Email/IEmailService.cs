using RVParking.Data;

namespace RVParking.Services.Email
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(EmailMessage message);
        Task<bool> SendEmailAsync(string to, string subject, string body);

    }
}
