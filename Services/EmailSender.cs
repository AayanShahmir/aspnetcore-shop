using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;
using System.Net.Mail;
using System.Threading.Tasks;

namespace BIsm2.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public EmailSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string email, string conlink)
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_config["EmailSetts:From"]));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = "Test mail via MailKit";
            message.Body = new TextPart(TextFormat.Html)
            {
                Text = $"<p>This email is sent by XYZ. Please verify your account by clicking this link: <a href='{conlink}'>Verify Account</a></p>"
            };

            using var smtp = new MailKit.Net.Smtp.SmtpClient();


            // Gmail SMTP example — replace with your provider if needed
            await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_config["EmailSetts:UserName"], _config["EmailSetts:Password"]);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);
        }
    }
}

