using backend.Services;
using Microsoft.Extensions.Configuration;

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
                {"EmailSettings:DisplayName", "Test"}
            };
            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings!).AddUserSecrets<EmailServiceTests>().AddEnvironmentVariables().Build();
            var service = new EmailService(config);
            await service.SendAsync(config["EmailSettings:Username"]!, "subject", "<b>hello</b>");
        }
    }
}
