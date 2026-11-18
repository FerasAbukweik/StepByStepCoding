using StepByStepCoding.Application.Contracts.Security;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Services;
using StepByStepCoding.Domain.Entities;

namespace StepByStepCoding.Security.Tests.Application.Services
{
    public sealed class AcademicAccessTests : TestBase
    {
        public AcademicAccessTests(ITestOutputHelper output) : base(output) { }

        #region Authentication

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void CurrentScope_WithMissingOrEmptyUserId_Throws(bool empty)
        {
            // Arrange
            var user = new Mock<IUserService>();
            user.Setup(service => service.GetUserId()).Returns(empty ? Guid.Empty : null);
            var clock = new Mock<TimeProvider>();
            var access = new AcademicAccess(user.Object, clock.Object);

            WriteOutput("Arranged input", (nameof(empty), empty));

            // Act
            var result = Record.Exception(() => access.CurrentScope());

            WriteOutput("Act result", (nameof(result), result));

            // Assert
            result.Should().BeOfType<UnauthenticatedException>();
        }

        #endregion

        #region Section access

        [Theory]
        [InlineData("Admin", false, true)]
        [InlineData("Instructor", true, true)]
        [InlineData("Instructor", false, false)]
        [InlineData("Student", true, false)]
        public void ManageSection_RequiresAdminOrAssignedInstructor(string role, bool ownsSection, bool permitted)
        {
            // Arrange
            var userId = Fixture.Create<Guid>();
            var section = new Section(Fixture.Create<Guid>(), ownsSection ? userId : Fixture.Create<Guid>(), Fixture.Create<string>());
            var user = new Mock<IUserService>();
            user.Setup(service => service.GetUserId()).Returns(userId);
            user.Setup(service => service.IsInRole(It.IsAny<string>())).Returns((string requested) => requested == role);
            var clock = new Mock<TimeProvider>();
            var access = new AcademicAccess(user.Object, clock.Object);

            WriteOutput("Arranged input", (nameof(role), role), (nameof(section), section), (nameof(userId), userId));

            // Act
            var result = Record.Exception(() => access.ManageSection(section));

            WriteOutput("Act result", (nameof(result), result));

            // Assert
            if (permitted)
                result.Should().BeNull();
            else
                result.Should().BeOfType<ForbiddenException>();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ReadSection_RequiresActiveStudentEnrollment(bool active)
        {
            // Arrange
            var userId = Fixture.Create<Guid>();
            var section = new Section(Fixture.Create<Guid>(), Fixture.Create<Guid>(), Fixture.Create<string>());
            var enrollment = new Enrollment(section.Id, userId);
            enrollment.Update(active);
            section.Enrollments.Add(enrollment);
            var user = new Mock<IUserService>();
            user.Setup(service => service.GetUserId()).Returns(userId);
            user.Setup(service => service.IsInRole(It.IsAny<string>())).Returns((string role) => role == "Student");
            var access = new AcademicAccess(user.Object, new Mock<TimeProvider>().Object);

            WriteOutput("Arranged input", (nameof(userId), userId), (nameof(active), active));

            // Act
            var result = Record.Exception(() => access.ReadSection(section));

            WriteOutput("Act result", (nameof(result), result));

            // Assert
            if (active)
                result.Should().BeNull();
            else
                result.Should().BeOfType<ForbiddenException>();
        }

        #endregion

        #region Workspace ownership

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Workspace_ReadAndManage_RequireFileOwner(bool owner)
        {
            // Arrange
            var userId = Fixture.Create<Guid>();
            var file = new WorkspaceFile(owner ? userId : Fixture.Create<Guid>());
            var user = new Mock<IUserService>();
            user.Setup(service => service.GetUserId()).Returns(userId);
            var access = new AcademicAccess(user.Object, new Mock<TimeProvider>().Object);

            WriteOutput("Arranged input", (nameof(userId), userId), (nameof(file), file));

            // Act
            var readResult = Record.Exception(() => access.ReadFile(file));
            var manageResult = Record.Exception(() => access.ManageFile(file));

            WriteOutput("Act result", (nameof(readResult), readResult), (nameof(manageResult), manageResult));

            // Assert
            if (owner)
            {
                readResult.Should().BeNull();
                manageResult.Should().BeNull();
            }
            else
            {
                readResult.Should().BeOfType<ForbiddenException>();
                manageResult.Should().BeOfType<ForbiddenException>();
            }
        }

        #endregion
    }
}
