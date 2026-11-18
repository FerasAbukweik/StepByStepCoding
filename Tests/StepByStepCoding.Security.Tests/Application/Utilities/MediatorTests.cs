using FluentValidation;
using FluentValidation.Results;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Features.Files.Queries.GetFileDetail;
using StepByStepCoding.Application.Utilities;

namespace StepByStepCoding.Security.Tests.Application.Utilities
{
    public sealed class MediatorTests : TestBase
    {
        public MediatorTests(ITestOutputHelper output) : base(output) { }

        #region Dispatch

        [Fact]
        public async Task Send_ForwardsRequestAndCancellationToken()
        {
            // Arrange
            var request = Fixture.Create<GetFileDetailQuery>();
            var expected = Fixture.Create<FileDetailDTO>();
            using var cancellation = new CancellationTokenSource();
            var handler = new Mock<IRequestHandler<GetFileDetailQuery, FileDetailDTO>>();
            handler.Setup(service => service.Handle(request, cancellation.Token)).ReturnsAsync(expected);
            var provider = new Mock<IServiceProvider>();
            provider.Setup(service => service.GetService(typeof(IRequestHandler<GetFileDetailQuery, FileDetailDTO>)))
                .Returns(handler.Object);
            var mediator = new Mediator(provider.Object);

            WriteOutput("Arranged input", (nameof(request), request));

            // Act
            var result = await mediator.Send(request, cancellation.Token);

            WriteOutput("Act result", (nameof(result), result));

            // Assert
            result.Should().BeSameAs(expected);
            handler.Verify(service => service.Handle(request, cancellation.Token), Times.Once);
        }

        [Fact]
        public async Task Send_WhenHandlerIsMissing_ThrowsMediatorException()
        {
            // Arrange
            var request = Fixture.Create<GetFileDetailQuery>();
            var provider = new Mock<IServiceProvider>();
            var mediator = new Mediator(provider.Object);

            WriteOutput("Arranged input", (nameof(request), request));

            // Act
            var result = await Record.ExceptionAsync(() => mediator.Send(request, CancellationToken.None));

            WriteOutput("Act result", (nameof(result), result));

            // Assert
            result.Should().BeOfType<MediatorException>();
        }

        #endregion

        #region Validation and errors

        [Fact]
        public async Task Send_WhenValidationFails_DoesNotInvokeHandler()
        {
            // Arrange
            var request = Fixture.Create<GetFileDetailQuery>();
            var message = Fixture.Create<string>();
            var handler = new Mock<IRequestHandler<GetFileDetailQuery, FileDetailDTO>>();
            var validator = new Mock<IValidator<GetFileDetailQuery>>();
            validator.Setup(service => service.ValidateAsync(It.IsAny<IValidationContext>(), CancellationToken.None))
                .ReturnsAsync(new ValidationResult([new ValidationFailure(nameof(request.Id), message)]));
            var provider = new Mock<IServiceProvider>();
            provider.Setup(service => service.GetService(typeof(IValidator<GetFileDetailQuery>))).Returns(validator.Object);
            provider.Setup(service => service.GetService(typeof(IRequestHandler<GetFileDetailQuery, FileDetailDTO>)))
                .Returns(handler.Object);
            var mediator = new Mediator(provider.Object);

            WriteOutput("Arranged input", (nameof(request), request), (nameof(message), message));

            // Act
            var result = await Record.ExceptionAsync(() => mediator.Send(request, CancellationToken.None));

            WriteOutput("Act result", (nameof(result), result));

            // Assert
            result.Should().BeOfType<CustomValidationException>().Which.Errors.Should().Contain(message);
            handler.Verify(service => service.Handle(It.IsAny<GetFileDetailQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Send_WhenHandlerThrows_PreservesOriginalException()
        {
            // Arrange
            var request = Fixture.Create<GetFileDetailQuery>();
            var failure = new InvalidOperationException(Fixture.Create<string>());
            var handler = new Mock<IRequestHandler<GetFileDetailQuery, FileDetailDTO>>();
            handler.Setup(service => service.Handle(request, CancellationToken.None)).Throws(failure);
            var provider = new Mock<IServiceProvider>();
            provider.Setup(service => service.GetService(typeof(IRequestHandler<GetFileDetailQuery, FileDetailDTO>)))
                .Returns(handler.Object);
            var mediator = new Mediator(provider.Object);

            WriteOutput("Arranged input", (nameof(request), request));

            // Act
            var result = await Record.ExceptionAsync(() => mediator.Send(request, CancellationToken.None));

            WriteOutput("Act result", (nameof(result), result));

            // Assert
            result.Should().BeSameAs(failure);
        }

        #endregion
    }
}
