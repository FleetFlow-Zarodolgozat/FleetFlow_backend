using backend;
using backend.Controllers;
using backend.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;

namespace tests.TestHelpers
{
    public static class TestControllerFactory
    {
        public static OwnDataController Create(FlottakezeloDbContext context, Mock<IEmailService>? emailMock = null)
        {
            var fileService = new Mock<IFileService>();
            var tokenService = new Mock<ITokenService>();
            tokenService.Setup(x => x.GenerateSecureToken()).Returns("test-token");
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>{{"Frontend:BaseUrl", "http://localhost:5174"}}!).Build();
            return new OwnDataController(
                context,
                fileService.Object,
                tokenService.Object,
                emailMock?.Object ?? new Mock<IEmailService>().Object,
                configuration
            );
        }
    }
}
