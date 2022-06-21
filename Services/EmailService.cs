using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using DotnetEmailServiceAWSSES.Helpers;

namespace LogAppServer.Services
{
    public interface IEmailService
    {
        void Send(string to, string subject, string html, string from = null);
    }

    public class EmailService : IEmailService
    {
        private readonly AppSettings _appSettings;

        public EmailService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public void Send(string to, string subject, string html, string from)
        {
            // create message
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(from));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Html) { Text = html };
            // send email
            using var smtp = new SmtpClient();
            smtp.Connect("email-smtp.ap-south-1.amazonaws.com", 587, SecureSocketOptions.StartTls);
            smtp.Authenticate("AKIAXQLWDQZWEDALXU75", "BO4ckfDegy0Bz39goQEt8CdmyHHca/loypqdb7XHMoTa");
            smtp.Send(email);
            smtp.Disconnect(true);
        }
    }
}