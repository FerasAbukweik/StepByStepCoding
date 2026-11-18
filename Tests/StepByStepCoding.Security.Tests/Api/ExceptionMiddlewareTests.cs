using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.MiddleWares;

namespace StepByStepCoding.Security.Tests.Api
{
    public sealed class ExceptionMiddlewareTests : TestBase
    {
        public ExceptionMiddlewareTests(ITestOutputHelper output) : base(output) { }

        #region Error responses

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Invoke_MapsExceptionsWithoutExposingUnexpectedDetails(bool unexpected)
        {
            // Arrange
            var details = Fixture.Create<string>();
            Exception failure = unexpected ? new InvalidOperationException(details) : new NotFoundException(details);
            var next = new Mock<RequestDelegate>();
            next.Setup(invoke => invoke(It.IsAny<HttpContext>())).Throws(failure);
            var logger = new Mock<ILogger<GlobalExceptionMiddleWare>>();
            var middleware = new GlobalExceptionMiddleWare(next.Object, logger.Object);
            var context = new DefaultHttpContext();
            await using var body = new MemoryStream();
            context.Response.Body = body;

            WriteOutput("Arranged input", (nameof(failure), failure), (nameof(unexpected), unexpected));

            // Act
            await middleware.InvokeAsync(context);
            body.Position = 0;
            var result = await new StreamReader(body).ReadToEndAsync();

            WriteOutput("Act result", ("StatusCode", context.Response.StatusCode), (nameof(result), result));

            // Assert
            context.Response.StatusCode.Should().Be(unexpected ? 500 : 404);
            context.Response.ContentType.Should().Be("application/json");
            if (unexpected)
                result.Should().NotContain(details);
            else
                result.Should().Contain(details);
        }

        #endregion
    }
}
