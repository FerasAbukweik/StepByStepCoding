using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Moq.Protected;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Domain.Enums;
using StepByStepCoding.Infrastructure.Options;
using StepByStepCoding.Infrastructure.Services;

namespace StepByStepCoding.Security.Tests.Infrastructure
{
    public sealed class CodeExecutionServiceTests : TestBase
    {
        public CodeExecutionServiceTests(ITestOutputHelper output) : base(output) { }

        #region Runtime requests

        [Theory]
        [InlineData(ProgrammingLanguage.Python, "python", "main.py")]
        [InlineData(ProgrammingLanguage.Cpp, "c++", "main.cpp")]
        [InlineData(ProgrammingLanguage.Java, "java", "Main.java")]
        public async Task Execute_SendsSanitizedFileAndReturnsOutput(ProgrammingLanguage language, string runtime, string fileName)
        {
            // Arrange
            var code = Fixture.Create<string>();
            var stdin = Fixture.Create<string>();
            var stdout = Fixture.Create<string>();
            var handler = new Mock<HttpMessageHandler>();
            JsonElement? sent = null;
            handler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(async (HttpRequestMessage request, CancellationToken token) =>
                {
                    sent = JsonSerializer.Deserialize<JsonElement>(await request.Content!.ReadAsStringAsync(token));
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(JsonSerializer.Serialize(new { run = new { stdout, code = 0 } }), Encoding.UTF8, "application/json")
                    };
                });
            using var client = new HttpClient(handler.Object) { BaseAddress = new Uri("http://runner/") };
            var service = new CodeExecutionService(client, Microsoft.Extensions.Options.Options.Create(new CodeExecutionOptions()));

            WriteOutput("Arranged input", (nameof(language), language), (nameof(fileName), fileName), (nameof(code), code), (nameof(stdin), stdin));

            // Act
            var result = await service.Execute(language, "../" + fileName, code, stdin);

            WriteOutput("Act result", (nameof(result), result), (nameof(sent), sent));

            // Assert
            result.Language.Should().Be(runtime);
            result.Run!.Stdout.Should().Be(stdout);
            result.Run.ExitCode.Should().Be(0);
            sent!.Value.GetProperty("files")[0].GetProperty("name").GetString().Should().Be(fileName);
            sent.Value.GetProperty("files")[0].GetProperty("content").GetString().Should().Be(code);
            sent.Value.GetProperty("stdin").GetString().Should().Be(stdin);
            handler.Protected().Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }

        #endregion

        #region Unavailable execution

        [Theory]
        [InlineData("http-error")]
        [InlineData("invalid-json")]
        [InlineData("network-error")]
        public async Task Execute_WhenRunnerFails_ThrowsApplicationException(string failure)
        {
            // Arrange
            var code = Fixture.Create<string>();
            var handler = new Mock<HttpMessageHandler>();
            var setup = handler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
            if (failure == "network-error")
                setup.ThrowsAsync(new HttpRequestException("Unavailable"));
            else
                setup.ReturnsAsync(new HttpResponseMessage(failure == "http-error" ? HttpStatusCode.BadGateway : HttpStatusCode.OK)
                {
                    Content = new StringContent("invalid", Encoding.UTF8, "application/json")
                });

            using var client = new HttpClient(handler.Object) { BaseAddress = new Uri("http://runner/") };
            var service = new CodeExecutionService(client, Microsoft.Extensions.Options.Options.Create(new CodeExecutionOptions()));

            WriteOutput("Arranged input", (nameof(failure), failure), (nameof(code), code));

            // Act
            var result = await Record.ExceptionAsync(() => service.Execute(ProgrammingLanguage.Python, "main.py", code, ""));

            WriteOutput("Act result", (nameof(result), result));

            // Assert
            result.Should().BeOfType<CodeExecutionUnavailableException>();
        }

        #endregion
    }
}
