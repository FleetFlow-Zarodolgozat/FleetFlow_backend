using System.Net.Mail;

namespace backend.Services.Interfaces
{
    public interface ISmtpClientWrapper : IDisposable
    {
        Task SendMailAsync(MailMessage message);
    }
}
