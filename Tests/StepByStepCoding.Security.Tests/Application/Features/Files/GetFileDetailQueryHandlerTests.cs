using StepByStepCoding.Application.Contracts.Academic;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Features.Files.Queries.GetFileDetail;
using StepByStepCoding.Application.Models;
using StepByStepCoding.Domain.Entities;

namespace StepByStepCoding.Security.Tests.Application.Features.Files
{
    public sealed class GetFileDetailQueryHandlerTests : TestBase
    {
        public GetFileDetailQueryHandlerTests(ITestOutputHelper output) : base(output)
        {
        }

        #region Successful reads

        [Fact]
        public async Task Handle_WithAccessibleFile_ReturnsSavedCodeAndChecksAccess()
        {
            // Arrange
            var userId = Fixture.Create<Guid>();
            var file = new WorkspaceFile(userId);
            var request = Fixture.Build<GetFileDetailQuery>().With(query => query.Id, file.Id).Create();
            using var cancellation = new CancellationTokenSource();
            var access = new Mock<IAcademicAccess>(MockBehavior.Strict);
            var repository = new Mock<IFileRepository>(MockBehavior.Strict);
            access.Setup(service => service.CurrentScope()).Returns(new AcademicReadScope(userId, false, false, true, DateTimeOffset.UtcNow));
            access.Setup(service => service.ReadFile(file));
            repository.Setup(service => service.GetWithEnrollment(file.Id, userId, cancellation.Token)).ReturnsAsync(file);
            var handler = new GetFileDetailQueryHandler(access.Object, repository.Object);

            WriteOutput("Arranged input", (nameof(request), request), (nameof(file), file), (nameof(userId), userId));

            // Act
            var result = await handler.Handle(request, cancellation.Token);

            WriteOutput("Act result", (nameof(result), result));

            // Assert
            result.Should().BeEquivalentTo(new { file.Id, file.Version, file.UserId, file.FileName, file.Language, file.SavedContent, file.SavedRevision });
            result.LectureId.Should().BeNull();
            repository.Verify(service => service.GetWithEnrollment(file.Id, userId, cancellation.Token), Times.Once);
            access.Verify(service => service.ReadFile(file), Times.Once);
            repository.VerifyNoOtherCalls();
        }

        #endregion

        #region Missing and forbidden files

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Handle_WithMissingOrForbiddenFile_PreservesFailure(bool forbidden)
        {
            // Arrange
            var userId = Fixture.Create<Guid>();
            var file = new WorkspaceFile(userId);
            var request = Fixture.Build<GetFileDetailQuery>().With(query => query.Id, file.Id).Create();
            var access = new Mock<IAcademicAccess>(MockBehavior.Strict);
            var repository = new Mock<IFileRepository>(MockBehavior.Strict);
            access.Setup(service => service.CurrentScope()).Returns(new AcademicReadScope(userId, false, false, true, DateTimeOffset.UtcNow));
            repository.Setup(service => service.GetWithEnrollment(request.Id, userId, CancellationToken.None)).ReturnsAsync(forbidden ? file : null);
            var failure = new ForbiddenException("Access denied.");
            if (forbidden)
                access.Setup(service => service.ReadFile(file)).Throws(failure);
            var handler = new GetFileDetailQueryHandler(access.Object, repository.Object);

            WriteOutput("Arranged input", (nameof(request), request), (nameof(forbidden), forbidden), (nameof(userId), userId));

            // Act
            var result = await Record.ExceptionAsync(() => handler.Handle(request, CancellationToken.None));

            WriteOutput("Act result", (nameof(result), result));

            // Assert
            if (forbidden)
            {
                result.Should().BeSameAs(failure);
                access.Verify(service => service.ReadFile(file), Times.Once);
            }
            else
            {
                result.Should().BeOfType<NotFoundException>();
                access.Verify(service => service.ReadFile(It.IsAny<CodeFile>()), Times.Never);
            }

            repository.Verify(service => service.GetWithEnrollment(request.Id, userId, CancellationToken.None), Times.Once);
            repository.VerifyNoOtherCalls();
        }

        #endregion

    }
}
