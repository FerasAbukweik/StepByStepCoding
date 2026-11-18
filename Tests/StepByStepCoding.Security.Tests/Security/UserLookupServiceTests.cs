using StepByStepCoding.Application.Contracts.Security;
using StepByStepCoding.Application.Enums;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Models.Security;
using StepByStepCoding.Security.Contracts.Repositories;
using StepByStepCoding.Security.Services;

namespace StepByStepCoding.Security.Tests.Security
{
    public sealed class UserLookupServiceTests : TestBase
    {
        public UserLookupServiceTests(ITestOutputHelper output) : base(output) { }

        #region Staff lookup permissions

        [Theory]
        [InlineData("Admin", UserRole.Student, true)]
        [InlineData("Admin", UserRole.Instructor, true)]
        [InlineData("Instructor", UserRole.Student, true)]
        [InlineData("Instructor", UserRole.Instructor, false)]
        [InlineData("Student", UserRole.Student, false)]
        public async Task Search_QueriesOnlyWhenActorCanSelectRequestedRole(string actorRole, UserRole role, bool permitted)
        {
            // Arrange
            var search = Fixture.Create<string>();
            IReadOnlyList<UserLookupDTO> expected = Fixture.CreateMany<UserLookupDTO>(2).ToArray();
            var repository = new Mock<IUserLookupRepository>();
            var actor = new Mock<IUserService>();
            actor.Setup(user => user.IsInRole(It.IsAny<string>())).Returns((string requested) => requested == actorRole);
            repository.Setup(service => service.Search(role.ToString(), search, CancellationToken.None)).ReturnsAsync(expected);
            var service = new UserLookupService(repository.Object, actor.Object);
            IReadOnlyList<UserLookupDTO>? result = null;

            WriteOutput("Arranged input", (nameof(actorRole), actorRole), (nameof(role), role), (nameof(search), search));

            // Act
            var exception = await Record.ExceptionAsync(async () => result = await service.Search(role, $" {search} "));

            WriteOutput("Act result", (nameof(result), result), (nameof(exception), exception));

            // Assert
            if (permitted)
            {
                exception.Should().BeNull();
                result.Should().BeSameAs(expected);
            }
            else
                exception.Should().BeOfType<ForbiddenException>();

            repository.Verify(service => service.Search(role.ToString(), search, CancellationToken.None),
                permitted ? Times.Once() : Times.Never());
        }

        [Fact]
        public async Task Search_WithUnsupportedRole_DoesNotQueryRepository()
        {
            // Arrange
            var actor = new Mock<IUserService>();
            actor.Setup(user => user.IsInRole("Admin")).Returns(true);
            var repository = new Mock<IUserLookupRepository>();
            var service = new UserLookupService(repository.Object, actor.Object);
            var search = Fixture.Create<string>();

            WriteOutput("Arranged input", (nameof(search), search), ("Role", UserRole.Admin));

            // Act
            var result = await Record.ExceptionAsync(() => service.Search(UserRole.Admin, search));

            WriteOutput("Act result", (nameof(result), result));

            // Assert
            result.Should().BeOfType<CustomValidationException>();
            repository.VerifyNoOtherCalls();
        }

        #endregion
    }
}
