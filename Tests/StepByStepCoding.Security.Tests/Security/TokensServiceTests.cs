using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StepByStepCoding.Application.Common;
using StepByStepCoding.Application.Contracts.Persistence;
using StepByStepCoding.Application.Contracts.Security;
using StepByStepCoding.Application.Models.Security;
using StepByStepCoding.Security.Models;
using StepByStepCoding.Security.Services;

namespace StepByStepCoding.Security.Tests.Security
{
    public sealed class TokensServiceTests : TestBase
    {
        private readonly Mock<IAccessTokenService> accessTokens = new();
        private readonly Mock<IRefreshTokenService> refreshTokens = new();
        private readonly Mock<IUnitOfWork<ApplicationSecurityDbContext>> unitOfWork = new();
        private readonly Mock<UserManager<ApplicationUser>> users;
        private readonly TokensService service;

        public TokensServiceTests(ITestOutputHelper output) : base(output)
        {
            users = new Mock<UserManager<ApplicationUser>>(
                new Mock<IUserStore<ApplicationUser>>().Object,
                Microsoft.Extensions.Options.Options.Create(new IdentityOptions()), new Mock<IPasswordHasher<ApplicationUser>>().Object,
                Array.Empty<IUserValidator<ApplicationUser>>(), Array.Empty<IPasswordValidator<ApplicationUser>>(),
                new Mock<ILookupNormalizer>().Object, new IdentityErrorDescriber(),
                new Mock<IServiceProvider>().Object, new Mock<ILogger<UserManager<ApplicationUser>>>().Object);
            service = new TokensService(accessTokens.Object, refreshTokens.Object, unitOfWork.Object, users.Object);
        }

        #region Issuance

        [Fact]
        public async Task Generate_CombinesAccessAndRefreshTokens()
        {
            // Arrange
            var userId = Fixture.Create<Guid>();
            var access = Fixture.Create<AccessTokenResultDTO>();
            var refresh = Fixture.Create<RefreshTokenResultDTO>();
            accessTokens.Setup(service => service.GenerateAsync(userId)).ReturnsAsync(access);
            refreshTokens.Setup(service => service.CreateAsync(userId, CancellationToken.None)).ReturnsAsync(refresh);

            WriteOutput("Arranged input", (nameof(userId), userId));

            // Act
            var result = await service.GenerateAsync(userId);

            WriteOutput("Act result", (nameof(result), result));

            // Assert
            result.Should().BeEquivalentTo(new
            {
                access.AccessToken, access.AccessTokenExpiresAt, refresh.RefreshToken, refresh.RefreshTokenExpiresAt
            });
            refreshTokens.Verify(service => service.CreateAsync(userId, CancellationToken.None), Times.Once);
        }

        #endregion

        #region Rotation

        [Theory]
        [InlineData("consumed")]
        [InlineData("missing-user")]
        [InlineData("locked")]
        [InlineData("success")]
        public async Task Refresh_RotatesOnlyForValidUnlockedUser(string scenario)
        {
            // Arrange
            var token = Fixture.Create<string>();
            var userId = Fixture.Create<Guid>();
            var user = Fixture.Build<ApplicationUser>().OmitAutoProperties().With(entity => entity.Id, userId).Create();
            var access = Fixture.Create<AccessTokenResultDTO>();
            var refresh = Fixture.Create<RefreshTokenResultDTO>();
            refreshTokens.Setup(service => service.ConsumeAsync(token, CancellationToken.None))
                .ReturnsAsync(scenario == "consumed" ? null : userId);
            users.Setup(manager => manager.FindByIdAsync(userId.ToString())).ReturnsAsync(scenario == "missing-user" ? null : user);
            users.Setup(manager => manager.IsLockedOutAsync(user)).ReturnsAsync(scenario == "locked");
            accessTokens.Setup(service => service.GenerateAsync(userId)).ReturnsAsync(access);
            refreshTokens.Setup(service => service.CreateAsync(userId, CancellationToken.None)).ReturnsAsync(refresh);
            bool? shouldCommit = null;
            unitOfWork.Setup(work => work.ExecuteInTransaction(It.IsAny<Func<CancellationToken, Task<Result<TokensDTO>>>>(),
                    It.IsAny<Func<Result<TokensDTO>, bool>>(), CancellationToken.None))
                .Returns(async (Func<CancellationToken, Task<Result<TokensDTO>>> operation,
                    Func<Result<TokensDTO>, bool> commit, CancellationToken cancellationToken) =>
                {
                    var result = await operation(cancellationToken);
                    shouldCommit = commit(result);
                    return result;
                });

            WriteOutput("Arranged input", (nameof(token), token), (nameof(scenario), scenario));

            // Act
            var result = await service.RefreshAsync(token);

            WriteOutput("Act result", (nameof(result), result), (nameof(shouldCommit), shouldCommit));

            // Assert
            result.IsSuccess.Should().Be(scenario == "success");
            shouldCommit.Should().Be(result.IsSuccess);
            if (result.IsSuccess)
            {
                result.Value!.AccessToken.Should().Be(access.AccessToken);
                result.Value.RefreshToken.Should().Be(refresh.RefreshToken);
            }
            else
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            refreshTokens.Verify(service => service.CreateAsync(userId, CancellationToken.None),
                scenario == "success" ? Times.Once() : Times.Never());
            refreshTokens.Verify(service => service.ConsumeAsync(token, CancellationToken.None), Times.Once);
        }

        #endregion
    }
}
