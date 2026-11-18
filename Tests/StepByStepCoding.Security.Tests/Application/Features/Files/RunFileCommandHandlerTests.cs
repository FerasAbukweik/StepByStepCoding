using StepByStepCoding.Application.Contracts.Academic;
using StepByStepCoding.Application.Contracts.Execution;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Features.Files.Commands.RunFile;
using StepByStepCoding.Application.Models;
using StepByStepCoding.Application.Models.Execution;
using StepByStepCoding.Domain.Entities;

namespace StepByStepCoding.Security.Tests.Application.Features.Files
{
    public sealed class RunFileCommandHandlerTests : TestBase
    {
        public RunFileCommandHandlerTests(ITestOutputHelper output) : base(output) { }

        #region Draft execution

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Handle_RunsDraftOnlyWhenFileExists(bool exists)
        {
            // Arrange
            var scope = Fixture.Create<AcademicReadScope>();
            var file = new WorkspaceFile(scope.UserId);
            var request = Fixture.Build<RunFileCommand>().With(command => command.FileId, file.Id).Create();
            var expected = Fixture.Create<CodeExecutionResultDTO>();
            var repository = new Mock<IFileRepository>();
            var access = new Mock<IAcademicAccess>();
            var execution = new Mock<ICodeExecutionService>();
            access.Setup(service => service.CurrentScope()).Returns(scope);
            repository.Setup(service => service.GetWithEnrollment(file.Id, scope.UserId, CancellationToken.None))
                .ReturnsAsync(exists ? file : null);
            execution.Setup(service => service.Execute(file.Language, file.FileName, request.Code, request.Stdin, CancellationToken.None))
                .ReturnsAsync(expected);
            var handler = new RunFileCommandHandler(repository.Object, access.Object, execution.Object);
            CodeExecutionResultDTO? result = null;

            WriteOutput("Arranged input", (nameof(request), request), (nameof(exists), exists));

            // Act
            var exception = await Record.ExceptionAsync(async () => result = await handler.Handle(request, CancellationToken.None));

            WriteOutput("Act result", (nameof(result), result), (nameof(exception), exception));

            // Assert
            if (exists)
            {
                exception.Should().BeNull();
                result.Should().BeSameAs(expected);
                access.Verify(service => service.ReadFile(file), Times.Once);
                execution.Verify(service => service.Execute(file.Language, file.FileName, request.Code, request.Stdin, CancellationToken.None), Times.Once);
            }
            else
            {
                exception.Should().BeOfType<NotFoundException>();
                execution.VerifyNoOtherCalls();
            }

            file.SavedContent.Should().BeEmpty();
            file.SavedRevision.Should().Be(0);
            repository.Verify(service => service.Update(It.IsAny<CodeFile>()), Times.Never);
        }

        #endregion
    }
}
