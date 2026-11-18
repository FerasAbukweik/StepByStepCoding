using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using StepByStepCoding.Security.Contracts.Repositories;
using StepByStepCoding.Security.Models;
using StepByStepCoding.Security.Options;
using StepByStepCoding.Security.Services;

namespace StepByStepCoding.Security.Tests.Security
{
    public sealed class RefreshTokenServiceTests : TestBase
    {
        public RefreshTokenServiceTests(ITestOutputHelper output) : base(output) { }

        #region Issue tokens

        [Fact]
        public async Task Create_PersistsHashAndReturnsRawToken()
        {
            // Arrange
            var userId = Fixture.Create<Guid>();
            var options = new JwtOptions { RefreshTokenLifeTime = 60 };
            var repository = new Mock<IRefreshTokenRepository>();
            RefreshToken? saved = null;
            repository.Setup(service => service.Add(It.IsAny<RefreshToken>()))
                .Callback<RefreshToken>(token => saved = token).ReturnsAsync((RefreshToken token) => token);
            var service = new RefreshTokenService(repository.Object, Microsoft.Extensions.Options.Options.Create(options));

            WriteOutput("Arranged input", (nameof(userId), userId), (nameof(options.RefreshTokenLifeTime), options.RefreshTokenLifeTime));

            // Act
            var result = await service.CreateAsync(userId);

            WriteOutput("Act result", (nameof(result), result), (nameof(saved), saved));

            // Assert
            saved.Should().NotBeNull();
            saved!.UserId.Should().Be(userId);
            saved.TokenHash.Should().Be(Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(result.RefreshToken))));
            saved.TokenHash.Should().NotBe(result.RefreshToken);
            saved.ExpiresAt.Should().Be(saved.CreatedAt.AddMinutes(60));
            result.RefreshTokenExpiresAt.Should().Be(saved.ExpiresAt);
            repository.Verify(service => service.Add(It.IsAny<RefreshToken>()), Times.Once);
        }

        #endregion

        #region Consume tokens

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Consume_WithMissingToken_DoesNotCallRepository(string? token)
        {
            // Arrange
            var repository = new Mock<IRefreshTokenRepository>();
            var service = new RefreshTokenService(repository.Object, Microsoft.Extensions.Options.Options.Create(new JwtOptions()));

            WriteOutput("Arranged input", (nameof(token), token));

            // Act
            var result = await service.ConsumeAsync(token!);

            WriteOutput("Act result", (nameof(result), result));

            // Assert
            result.Should().BeNull();
            repository.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Consume_HashesInputAndReturnsOwnerOnlyWhenConsumed(bool consumed)
        {
            // Arrange
            var input = Fixture.Create<string>();
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input)));
            var now = DateTimeOffset.UtcNow;
            var token = new RefreshToken(Fixture.Create<Guid>(), hash, now, now.AddHours(1));
            using var cancellation = new CancellationTokenSource();
            var repository = new Mock<IRefreshTokenRepository>();
            repository.Setup(service => service.Consume(hash, It.IsAny<DateTimeOffset>(), cancellation.Token))
                .ReturnsAsync(consumed ? token : null);
            var service = new RefreshTokenService(repository.Object, Microsoft.Extensions.Options.Options.Create(new JwtOptions()));

            WriteOutput("Arranged input", (nameof(input), input), (nameof(consumed), consumed));

            // Act
            var result = await service.ConsumeAsync(input, cancellation.Token);

            WriteOutput("Act result", (nameof(result), result));

            // Assert
            result.Should().Be(consumed ? token.UserId : null);
            repository.Verify(service => service.Consume(hash, It.IsAny<DateTimeOffset>(), cancellation.Token), Times.Once);
        }

        #endregion
    }
}
