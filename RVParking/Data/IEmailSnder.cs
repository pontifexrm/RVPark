namespace RVParking.Data;
public interface IEmailSnder
{
    Task SendEmailAsync(string email, string subject, string htmlMessage);
}