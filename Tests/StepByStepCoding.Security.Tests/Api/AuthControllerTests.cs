using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StepByStepCoding.Application.Common;
using StepByStepCoding.Application.Contracts.Security;
using StepByStepCoding.Application.Models.Security;
using StepByStepCoding.Contracts.Cookie;
using StepByStepCoding.Controllers;

namespace StepByStepCoding.Security.Tests.Api
{
    public sealed class AuthControllerTests : TestBase
    {
        public AuthControllerTests(ITestOutputHelper output) : base(output) { }

        #region Login cookies

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Login_RegistersCookiesOnlyWhenAuthenticationSucceeds(bool success)
        {
            // Arrange
            var request = Fixture.Create<LoginDTO>();
            var tokens = Fixture.Create<TokensDTO>();
            var auth = new Mock<IAuthService>();
            var cookies = new Mock<ICookieService>();
            auth.Setup(service => service.LoginAsync(request, CancellationToken.None))
                .ReturnsAsync(success ? Result<TokensDTO>.Success(tokens)
                    : Result<TokensDTO>.Failure("Invalid credentials", HttpStatusCode.Unauthorized));
            var controller = new AuthController(auth.Object, cookies.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
            };

            WriteOutput("Arranged input", (nameof(request), request), (nameof(success), success));

            // Act
            var result = await controller.Login(request, CancellationToken.None);

            WriteOutput("Act result", ("StatusCode", ((IStatusCodeActionResult)result).StatusCode));

            // Assert
            ((IStatusCodeActionResult)result).StatusCode.Should().Be(success ? 200 : 401);
            cookies.Verify(service => service.RegisterTokens(tokens), success ? Times.Once() : Times.Never());
            controller.Response.Headers.CacheControl.ToString().Should().Be("no-store");
        }

        #endregion

        #region Missing refresh cookie

        [Fact]
        public async Task RefreshToken_WithMissingCookie_ReturnsUnauthorizedWithoutCallingAuth()
        {
            // Arrange
            var auth = new Mock<IAuthService>();
            var cookies = new Mock<ICookieService>();
            cookies.Setup(service => service.GetRefreshToken()).Returns((string?)null);
            var controller = new AuthController(auth.Object, cookies.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
            };

            WriteOutput("Arranged input", ("RefreshToken", (string?)null));

            // Act
            var result = await controller.RefreshToken(CancellationToken.None);

            WriteOutput("Act result", ("StatusCode", ((ObjectResult)result).StatusCode));

            // Assert
            result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(401);
            auth.VerifyNoOtherCalls();
            cookies.Verify(service => service.RegisterTokens(It.IsAny<TokensDTO>()), Times.Never);
        }

        #endregion
    }
}
