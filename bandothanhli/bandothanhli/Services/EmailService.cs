using MailKit.Net.Smtp;
using MimeKit;

namespace bandothanhli.Services
{
    public interface IEmailService
    {
        Task GuiEmailAsync(string toEmail, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task GuiEmailAsync(string toEmail, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config["Email:TuEmail"]));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;
            email.Body = new TextPart("html") { Text = body };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_config["Email:Host"], int.Parse(_config["Email:Port"]), true);
            await smtp.AuthenticateAsync(_config["Email:TaiKhoan"], _config["Email:MatKhau"]);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}