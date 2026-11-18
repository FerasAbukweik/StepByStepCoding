using Microsoft.Extensions.Logging;
using StepByStepCoding.Application.Decorators;
using StepByStepCoding.Application.Features.Courses.Commands.DeleteCourse;
using StepByStepCoding.Application.Features.Files.Queries.GetFileDetail;
using StepByStepCoding.Application.Utilities;

namespace StepByStepCoding.Security.Tests.Decorators
{
    public sealed class LoggingDecoratorTests : TestBase
    {
        public LoggingDecoratorTests(ITestOutputHelper output) : base(output) { }

        #region Response handlers

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Handle_LogsOutcomeAndPreservesResponseOrException(bool fails)
        {
            // Arrange
            var request = Fixture.Create<GetFileDetailQuery>();
            var expected = Fixture.Create<FileDetailDTO>();
            var failure = new InvalidOperationException(Fixture.Create<string>());
            var inner = new Mock<IRequestHandler<GetFileDetailQuery, FileDetailDTO>>();
            var logger = new Mock<ILogger<LoggingDecorator<GetFileDetailQuery, FileDetailDTO>>>();
            if (fails)
                inner.Setup(handler => handler.Handle(request, CancellationToken.None)).ThrowsAsync(failure);
            else
                inner.Setup(handler => handler.Handle(request, CancellationToken.None)).ReturnsAsync(expected);

            var decorator = new LoggingDecorator<GetFileDetailQuery, FileDetailDTO>(inner.Object, logger.Object);
            FileDetailDTO? result = null;

            WriteOutput("Arranged input", (nameof(request), request), (nameof(fails), fails));

            // Act
            var exception = await Record.ExceptionAsync(async () => result = await decorator.Handle(request, CancellationToken.None));

            WriteOutput("Act result", (nameof(result), result), (nameof(exception), exception));

            // Assert
            if (fails)
                exception.Should().BeSameAs(failure);
            else
            {
                exception.Should().BeNull();
                result.Should().BeSameAs(expected);
            }

            logger.Verify(service => service.Log(fails ? LogLevel.Error : LogLevel.Information,
                It.IsAny<EventId>(), It.Is<It.IsAnyType>((state, _) => state.ToString()!.StartsWith("LoggingDecorator.Handle")),
                It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), fails ? Times.Once() : Times.Exactly(2));
            inner.Verify(handler => handler.Handle(request, CancellationToken.None), Times.Once);
        }

        #endregion

        #region Void handlers

        [Fact]
        public async Task Handle_VoidHandler_ForwardsCancellationToken()
        {
            // Arrange
            var request = Fixture.Create<DeleteCourseCommand>();
            using var cancellation = new CancellationTokenSource();
            var inner = new Mock<IRequestHandler<DeleteCourseCommand>>();
            var logger = new Mock<ILogger<LoggingDecorator<DeleteCourseCommand>>>();
            var decorator = new LoggingDecorator<DeleteCourseCommand>(inner.Object, logger.Object);

            WriteOutput("Arranged input", (nameof(request), request));

            // Act
            var result = await Record.ExceptionAsync(() => decorator.Handle(request, cancellation.Token));

            WriteOutput("Act result", (nameof(result), result));

            // Assert
            result.Should().BeNull();
            inner.Verify(handler => handler.Handle(request, cancellation.Token), Times.Once);
        }

        #endregion
    }
}
