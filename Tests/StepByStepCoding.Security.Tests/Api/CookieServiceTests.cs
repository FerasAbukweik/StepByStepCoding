using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using StepByStepCoding.Application.Models.Security;
using StepByStepCoding.Options;
using StepByStepCoding.Services;

namespace StepByStepCoding.Security.Tests.Api
{
    public sealed class CookieServiceTests : TestBase
    {
        public CookieServiceTests(ITestOutputHelper output) : base(output) { }

        #region Token cookies

        [Theory]
        [InlineData("")]
        [InlineData("/codeclass")]
        public void RegisterTokens_SetsHttpOnlyCookiesWithCorrectPaths(string pathBase)
        {
            // Arrange
            var tokens = Fixture.Create<TokensDTO>();
            var responseCookies = new Mock<IResponseCookies>();
            var accessor = new Mock<IHttpContextAccessor> { DefaultValue = DefaultValue.Mock };
            accessor.SetupGet(service => service.HttpContext!.Request.PathBase).Returns(new PathString(pathBase));
            accessor.SetupGet(service => service.HttpContext!.Response.Cookies).Returns(responseCookies.Object);
            var captured = new Dictionary<string, CookieOptions>();
            responseCookies.Setup(cookies => cookies.Append(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CookieOptions>()))
                .Callback<string, string, CookieOptions>((name, _, options) => captured[name] = options);
            var service = new CookieService(accessor.Object, Microsoft.Extensions.Options.Options.Create(new CookieKeys()));

            WriteOutput("Arranged input", (nameof(tokens), tokens), (nameof(pathBase), pathBase));

            // Act
            service.RegisterTokens(tokens);

            WriteOutput("Act result", (nameof(captured), captured));

            // Assert
            captured.Should().HaveCount(2);
            captured.Values.Should().OnlyContain(options => options.HttpOnly && options.Secure && options.SameSite == SameSiteMode.Strict);
            captured["access_token"].Path.Should().Be(pathBase == "" ? "/" : pathBase);
            captured["refresh_token"].Path.Should().Be(pathBase + "/api/auth/refresh-token");
            captured["access_token"].Expires.Should().Be(tokens.AccessTokenExpiresAt);
            responseCookies.Verify(cookies => cookies.Append("access_token", tokens.AccessToken, It.IsAny<CookieOptions>()), Times.Once);
            responseCookies.Verify(cookies => cookies.Append("refresh_token", tokens.RefreshToken, It.IsAny<CookieOptions>()), Times.Once);
        }

        #endregion
    }
}
