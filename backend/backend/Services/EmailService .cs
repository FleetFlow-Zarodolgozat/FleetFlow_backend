using backend.Services.Interfaces;
using System.Net;
using System.Net.Mail;

namespace backend.Services
{
    public class EmailService : IEmailService
    {
        public async Task SendAsync(string to, string subject, string html)
        {
            var message = new MailMessage();
            message.To.Add(to);
            message.Subject = subject;
            message.Body = html;
            message.IsBodyHtml = true;
            message.From = new MailAddress("fleetflow.info@gmail.com", "FleetFlow2006$");
            using var smtp = new SmtpClient("smtp.gmail.com", 465);
            smtp.EnableSsl = true;
            await smtp.SendMailAsync(message);
        }
    }
}
