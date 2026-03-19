using backend.Services;
using backend.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;

namespace tests.Services
{
    public class EmailServiceTests
    {
        [Fact]
        public async Task SendAsync_ShouldNotThrow()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                {"EmailSettings:SmtpHost", "smtp.gmail.com"},
                {"EmailSettings:SmtpPort", "587"},
                {"EmailSettings:From", "test@test.com"},
                {"EmailSettings:DisplayName", "Test"},
                {"EmailSettings:Username", "test@test.com"},
                {"EmailSettings:Password", "test-password"}
            };
            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings!).Build();
            var smtpMock = new Mock<ISmtpClientWrapper>();
            smtpMock.Setup(x => x.SendMailAsync(It.IsAny<System.Net.Mail.MailMessage>())).Returns(Task.CompletedTask);
            var service = new EmailService(config, (host, port, credentials) => smtpMock.Object);
            await service.SendAsync("fleetflow.info@gmail.com", "subject", "<b>hello</b>");
            smtpMock.Verify(x => x.SendMailAsync(It.IsAny<System.Net.Mail.MailMessage>()), Times.Once);
        }
    }
}
