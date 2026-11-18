using StepByStepCoding.Application.Contracts.Academic;
using StepByStepCoding.Application.Contracts.Persistence;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Features.Files.Commands.EnsureWorkspaceFile;
using StepByStepCoding.Application.Models;
using StepByStepCoding.Domain.Entities;

namespace StepByStepCoding.Security.Tests.Application.Features.Files
{
    public sealed class EnsureWorkspaceFileCommandHandlerTests : TestBase
    {
        public EnsureWorkspaceFileCommandHandlerTests(ITestOutputHelper output) : base(output) { }

        #region Existing and new files

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Handle_CreatesOnlyWhenNoWorkspaceExists(bool exists)
        {
            // Arrange
            var scope = Fixture.Create<AcademicReadScope>();
            var existing = new WorkspaceFile(scope.UserId);
            var request = Fixture.Create<EnsureWorkspaceFileCommand>();
            var access = new Mock<IAcademicAccess>();
            var repository = new Mock<IWorkspaceFileRepository>();
            var unitOfWork = new Mock<IUnitOfWork>();
            access.Setup(service => service.CurrentScope()).Returns(scope);
            repository.Setup(service => service.GetByUser(scope.UserId, CancellationToken.None)).ReturnsAsync(exists ? existing : null);
            WorkspaceFile? added = null;
            repository.Setup(service => service.Add(It.IsAny<WorkspaceFile>()))
                .Callback<WorkspaceFile>(file => added = file).ReturnsAsync((WorkspaceFile file) => file);
            var handler = new EnsureWorkspaceFileCommandHandler(access.Object, repository.Object, unitOfWork.Object);

            WriteOutput("Arranged input", (nameof(scope), scope), (nameof(exists), exists));

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            WriteOutput("Act result", (nameof(result), result), (nameof(added), added));

            // Assert
            result.Should().Be(exists ? existing.Id : added!.Id);
            if (!exists)
                added!.UserId.Should().Be(scope.UserId);

            repository.Verify(service => service.Add(It.IsAny<WorkspaceFile>()), exists ? Times.Never() : Times.Once());
            unitOfWork.Verify(service => service.Commit(CancellationToken.None), exists ? Times.Never() : Times.Once());
        }

        #endregion

        #region Persistence failures

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Handle_WhenConcurrentCreationConflicts_UsesWinnerOrPreservesConflict(bool winnerExists)
        {
            // Arrange
            var scope = Fixture.Create<AcademicReadScope>();
            var winner = new WorkspaceFile(scope.UserId);
            var request = Fixture.Create<EnsureWorkspaceFileCommand>();
            var access = new Mock<IAcademicAccess>();
            var repository = new Mock<IWorkspaceFileRepository>();
            var unitOfWork = new Mock<IUnitOfWork>();
            access.Setup(service => service.CurrentScope()).Returns(scope);
            repository.SetupSequence(service => service.GetByUser(scope.UserId, CancellationToken.None))
                .ReturnsAsync((WorkspaceFile?)null).ReturnsAsync(winnerExists ? winner : null);
            var failure = new ConflictException(Fixture.Create<string>());
            unitOfWork.Setup(service => service.Commit(CancellationToken.None)).ThrowsAsync(failure);
            var handler = new EnsureWorkspaceFileCommandHandler(access.Object, repository.Object, unitOfWork.Object);
            Guid? result = null;

            WriteOutput("Arranged input", (nameof(scope), scope), (nameof(winnerExists), winnerExists));

            // Act
            var exception = await Record.ExceptionAsync(async () => result = await handler.Handle(request, CancellationToken.None));

            WriteOutput("Act result", (nameof(result), result), (nameof(exception), exception));

            // Assert
            if (winnerExists)
            {
                exception.Should().BeNull();
                result.Should().Be(winner.Id);
            }
            else
                exception.Should().BeSameAs(failure);

            unitOfWork.Verify(service => service.Rollback(CancellationToken.None), Times.Once);
            repository.Verify(service => service.GetByUser(scope.UserId, CancellationToken.None), Times.Exactly(2));
        }

        [Fact]
        public async Task Handle_WhenWriteFails_RollsBackAndRethrows()
        {
            // Arrange
            var access = new Mock<IAcademicAccess>();
            access.Setup(service => service.CurrentScope()).Returns(Fixture.Create<AcademicReadScope>());
            var repository = new Mock<IWorkspaceFileRepository>();
            var unitOfWork = new Mock<IUnitOfWork>();
            var failure = new InvalidOperationException(Fixture.Create<string>());
            repository.Setup(service => service.Add(It.IsAny<WorkspaceFile>())).ThrowsAsync(failure);
            var request = Fixture.Create<EnsureWorkspaceFileCommand>();
            var handler = new EnsureWorkspaceFileCommandHandler(access.Object, repository.Object, unitOfWork.Object);

            WriteOutput("Arranged input", (nameof(request), request));

            // Act
            var result = await Record.ExceptionAsync(() => handler.Handle(request, CancellationToken.None));

            WriteOutput("Act result", (nameof(result), result));

            // Assert
            result.Should().BeSameAs(failure);
            unitOfWork.Verify(service => service.Rollback(CancellationToken.None), Times.Once);
            unitOfWork.Verify(service => service.Commit(It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion
    }
}
