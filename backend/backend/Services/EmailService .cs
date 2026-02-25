using backend.Services.Interfaces;
using System.Net;
using System.Net.Mail;

namespace backend.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration configuration;
        public EmailService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task SendAsync(string to, string subject, string html)
        {
            var email = configuration.GetSection("EmailSettings");
            var message = new MailMessage();
            message.To.Add(to);
            message.Subject = subject;
            message.Body = html;
            message.IsBodyHtml = true;
            message.From = new MailAddress(email["From"]!, email["DisplayName"]);
            using var smtp = new SmtpClient(email["SmtpHost"], int.Parse(email["SmtpPort"]!));
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(email["Username"], email["Password"]);
            await smtp.SendMailAsync(message);
        }
    }
}
