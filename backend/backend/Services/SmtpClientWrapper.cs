using backend.Services.Interfaces;
using System.Net;
using System.Net.Mail;

namespace backend.Services
{
    public class SmtpClientWrapper : ISmtpClientWrapper
    {
        private readonly SmtpClient _client;

        public SmtpClientWrapper(string host, int port, NetworkCredential credentials)
        {
            _client = new SmtpClient(host, port)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = credentials
            };
        }

        public Task SendMailAsync(MailMessage message) => _client.SendMailAsync(message);

        public void Dispose() => _client.Dispose();
    }
}
