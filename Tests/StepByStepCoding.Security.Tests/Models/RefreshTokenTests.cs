using StepByStepCoding.Security.Models;

namespace StepByStepCoding.Security.Tests.Models
{
    public sealed class RefreshTokenTests : TestBase
    {
        public RefreshTokenTests(ITestOutputHelper output) : base(output)
        {
        }

        #region Constructor

        [Fact]
        public void Constructor_WithValidValues_CreatesVersion7IdAndUtcDates()
        {
            // Arrange
            var now = Fixture.Create<DateTimeOffset>().ToOffset(TimeSpan.FromHours(3));
            var userId = Fixture.Create<Guid>();

            WriteOutput("Arranged input", (nameof(now), now), (nameof(userId), userId));

            // Act
            var token = new RefreshToken(userId, new string('A', 64), now, now.AddMinutes(10));

            WriteOutput(
                "Act result",
                (nameof(token), token),
                ("token.Id.Version", token.Id.Version),
                ("token.CreatedAt.Offset", token.CreatedAt.Offset),
                ("token.ExpiresAt.Offset", token.ExpiresAt.Offset));

            // Assert
            token.Id.Version.Should().Be(7);
            token.UserId.Should().Be(userId);
            token.CreatedAt.Offset.Should().Be(TimeSpan.Zero);
            token.ExpiresAt.Offset.Should().Be(TimeSpan.Zero);
            token.CreatedAt.Should().Be(now);
        }

        [Fact]
        public void Constructor_WithEmptyUserId_Throws()
        {
            // Arrange
            var now = Fixture.Create<DateTimeOffset>();

            WriteOutput("Arranged input", (nameof(now), now));

            // Act
            var operationException1 = Record.Exception(() => new RefreshToken(Guid.Empty, new string('A', 64), now, now.AddMinutes(10)));

            WriteOutput("Act result", (nameof(operationException1), operationException1));

            // Assert
            operationException1.Should().BeOfType<ArgumentException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("invalid")]
        [InlineData("ZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZ")]
        public void Constructor_WithInvalidHash_Throws(string? hash)
        {
            // Arrange
            var now = Fixture.Create<DateTimeOffset>();

            WriteOutput("Arranged input", (nameof(hash), hash), (nameof(now), now));

            // Act
            var operationException1 = Record.Exception(() => new RefreshToken(Fixture.Create<Guid>(), hash!, now, now.AddMinutes(10)));

            WriteOutput("Act result", (nameof(operationException1), operationException1));

            // Assert
            operationException1.Should().BeOfType<ArgumentException>();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Constructor_WithInvalidExpiration_Throws(int minutes)
        {
            // Arrange
            var now = Fixture.Create<DateTimeOffset>();

            WriteOutput("Arranged input", (nameof(minutes), minutes), (nameof(now), now));

            // Act
            var operationException1 = Record.Exception(() => new RefreshToken(Fixture.Create<Guid>(), new string('A', 64), now, now.AddMinutes(minutes)));

            WriteOutput("Act result", (nameof(operationException1), operationException1));

            // Assert
            operationException1.Should().BeOfType<ArgumentException>();
        }

        #endregion

    }
}
