using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SurveyBasket.Settings;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace SurveyBasket.Services.Implementation
{
    public class EmailService(IOptions<MailSettings> mailSettings) : IEmailSender
    {
        private readonly IOptions<MailSettings> mailSettings = mailSettings;

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var messagee = new MimeMessage
            {
                Sender = MailboxAddress.Parse(mailSettings.Value.Mail),
                Subject = subject

            };
            messagee.To.Add(MailboxAddress.Parse(email));
            var builder = new BodyBuilder
            {
                HtmlBody = htmlMessage
            };
            messagee.Body = builder.ToMessageBody();
            using var smtp = new SmtpClient();
            smtp.Connect(mailSettings.Value.Host, mailSettings.Value.Port, SecureSocketOptions.StartTls);
            smtp.Authenticate(mailSettings.Value.Mail, mailSettings.Value.Password);
            await smtp.SendAsync(messagee);
            smtp.Disconnect(true);
        }
    }
}
