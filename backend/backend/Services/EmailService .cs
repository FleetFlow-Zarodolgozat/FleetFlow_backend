using backend.Services.Interfaces;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;

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
            message.IsBodyHtml = true;
            message.From = new MailAddress(email["From"]!, email["DisplayName"]);
            var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fleetflow_logo_jpg.jpg");
            if (File.Exists(logoPath) && html.Contains("cid:fleetflow-logo", StringComparison.OrdinalIgnoreCase))
            {
                var view = AlternateView.CreateAlternateViewFromString(html, null, MediaTypeNames.Text.Html);
                var logo = new LinkedResource(logoPath, MediaTypeNames.Image.Jpeg)
                {
                    ContentId = "fleetflow-logo",
                    TransferEncoding = TransferEncoding.Base64
                };
                view.LinkedResources.Add(logo);
                message.AlternateViews.Add(view);
            }
            else
                message.Body = html;
            using var smtp = new SmtpClient(email["SmtpHost"], int.Parse(email["SmtpPort"]!));
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(email["Username"], email["Password"]);
            await smtp.SendMailAsync(message);
        }
    }
}
