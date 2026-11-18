using Microsoft.Extensions.Logging;
using StepByStepCoding.Application.Contracts.Realtime;
using StepByStepCoding.Decorators;

namespace StepByStepCoding.Security.Tests.Decorators
{
    public sealed class SignalRServiceLoggingDecoratorTests : TestBase
    {
        public SignalRServiceLoggingDecoratorTests(ITestOutputHelper output) : base(output) { }

        #region Notification delivery

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task LectureEnded_DoesNotReportCommittedWriteAsFailedWhenTransportFails(bool fails)
        {
            // Arrange
            var lectureId = Fixture.Create<Guid>();
            var fileId = Fixture.Create<Guid>();
            var inner = new Mock<ISignalRService>();
            var logger = new Mock<ILogger<SignalRServiceLoggingDecorator>>();
            if (fails)
                inner.Setup(service => service.LectureEnded(lectureId, fileId)).ThrowsAsync(new InvalidOperationException("Unavailable"));

            var decorator = new SignalRServiceLoggingDecorator(inner.Object, logger.Object);

            WriteOutput("Arranged input", (nameof(lectureId), lectureId), (nameof(fileId), fileId), (nameof(fails), fails));

            // Act
            var result = await Record.ExceptionAsync(() => decorator.LectureEnded(lectureId, fileId));

            WriteOutput("Act result", (nameof(result), result));

            // Assert
            result.Should().BeNull();
            inner.Verify(service => service.LectureEnded(lectureId, fileId), Times.Once);
            logger.Verify(service => service.Log(fails ? LogLevel.Warning : LogLevel.Debug,
                It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        #endregion
    }
}
