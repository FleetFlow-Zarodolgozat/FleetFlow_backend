using backend.Dtos.Users;
using backend.Models;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using tests.TestHelpers;

namespace tests.Controllers
{
    public class OwnDataControllerTests
    {
        [Fact]
        public async Task GetOwnProfile_ShouldReturnUnauthorized_WhenNoUser()
        {
            var context = TestDbContextFactory.Create();
            var controller = TestControllerFactory.Create(context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            var result = await controller.GetOwnProfile();
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task ForgotPassword_ShouldSendEmail_WhenUserExists()
        {
            var context = TestDbContextFactory.Create();
            context.Users.Add(new User
            {
                Id = 1,
                Email = "test@test.com",
                FullName = "Test User",
                IsActive = true,
                Role = "DRIVER"
            });
            context.SaveChanges();
            var emailMock = new Mock<IEmailService>();
            var controller = TestControllerFactory.Create(context, emailMock);
            var dto = new ForgotPasswordDto
            {
                Email = "test@test.com"
            };
            await controller.ForgotPassword(dto);
            emailMock.Verify(x => x.SendAsync(It.IsAny<string>(),  It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
