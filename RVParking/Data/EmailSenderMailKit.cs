using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using System.Net.Mail;

namespace RVParking.Data;

public class EmailSenderMailKit : IEmailSnder
{
    private readonly EmailConfiguration _emailConfiguration;

    public EmailSenderMailKit(IOptions<EmailConfiguration> emailConfiguration)
    {
        this._emailConfiguration = emailConfiguration.Value;
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        return Execute(email, subject, htmlMessage);
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

        var email = new MimeMessage();

        var sender = MailboxAddress.Parse(from);
        var BccEmail = MailboxAddress.Parse("rvpark@pontifex.nz");
        if (!string.IsNullOrEmpty(name))
            sender.Name = name;
        email.Sender = sender;
        email.From.Add(sender);
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;
        email.Body = new TextPart(TextFormat.Plain) { Text = htmlMessage };
        email.Bcc.Add(BccEmail);


        using (var smtp = new MailKit.Net.Smtp.SmtpClient())
        {
            smtp.Timeout = 10000; // 10secs
            try
            {
                await smtp.ConnectAsync(host, port, enableSsl);
                if (!string.IsNullOrEmpty(username))
                    smtp.Authenticate(username, password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}