using StepByStepCoding.Application.Contracts.Academic;
using StepByStepCoding.Application.Contracts.Persistence;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Features.Files.Commands.SaveChanges;
using StepByStepCoding.Application.Models;
using StepByStepCoding.Application.Models.Realtime;
using StepByStepCoding.Domain.Entities;

namespace StepByStepCoding.Security.Tests.Application.Features.Files
{
    public sealed class SaveChangesCommandHandlerTests : TestBase
    {
        public SaveChangesCommandHandlerTests(ITestOutputHelper output) : base(output) { }

        #region Save and validation

        [Theory]
        [InlineData("success")]
        [InlineData("missing")]
        [InlineData("forbidden")]
        [InlineData("stale")]
        [InlineData("commit-failure")]
        public async Task Handle_ValidatesAccessAndRevisionBeforeSaving(string scenario)
        {
            // Arrange
            var scope = Fixture.Create<AcademicReadScope>();
            var file = new WorkspaceFile(scope.UserId);
            var text = Fixture.Create<string>();
            var request = new SaveChangesCommand
            {
                FileId = file.Id,
                BatchId = Fixture.Create<Guid>(),
                BaseRevision = scenario == "stale" ? 1 : 0,
                ChangeSets = [new CodeChangeSet { Changes = [new CodeEdit { RangeOffset = 0, RangeLength = 0, Text = text }] }]
            };
            var access = new Mock<IAcademicAccess>();
            var repository = new Mock<IFileRepository>();
            var unitOfWork = new Mock<IUnitOfWork>();
            access.Setup(service => service.CurrentScope()).Returns(scope);
            repository.Setup(service => service.GetWithEnrollment(file.Id, scope.UserId, CancellationToken.None))
                .ReturnsAsync(scenario == "missing" ? null : file);
            var failure = new InvalidOperationException(Fixture.Create<string>());
            if (scenario == "forbidden")
                access.Setup(service => service.ManageFile(file)).Throws(new ForbiddenException("Denied"));
            if (scenario == "commit-failure")
                unitOfWork.Setup(service => service.Commit(CancellationToken.None)).ThrowsAsync(failure);

            var handler = new SaveChangesCommandHandler(access.Object, repository.Object, unitOfWork.Object);
            CodeChangesDTO? result = null;

            WriteOutput("Arranged input", (nameof(request), request), (nameof(scenario), scenario));

            // Act
            var exception = await Record.ExceptionAsync(async () => result = await handler.Handle(request, CancellationToken.None));

            WriteOutput("Act result", (nameof(result), result), (nameof(exception), exception));

            // Assert
            switch (scenario)
            {
                case "success":
                    exception.Should().BeNull();
                    file.SavedContent.Should().Be(text);
                    file.SavedRevision.Should().Be(1);
                    result.Should().BeEquivalentTo(new
                    {
                        request.FileId, request.BatchId, request.BaseRevision, Revision = 1L, request.ChangeSets
                    });
                    break;
                case "missing": exception.Should().BeOfType<NotFoundException>(); break;
                case "forbidden": exception.Should().BeOfType<ForbiddenException>(); break;
                case "stale": exception.Should().BeOfType<ConflictException>(); break;
                default: exception.Should().BeSameAs(failure); break;
            }

            var writes = scenario is "success" or "commit-failure" ? Times.Once() : Times.Never();
            repository.Verify(service => service.Update(file), writes);
            unitOfWork.Verify(service => service.Commit(CancellationToken.None), writes);
            unitOfWork.Verify(service => service.Rollback(CancellationToken.None),
                scenario == "commit-failure" ? Times.Once() : Times.Never());
        }

        #endregion
    }
}
