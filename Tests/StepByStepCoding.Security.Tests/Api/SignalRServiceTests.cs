using Microsoft.AspNetCore.SignalR;
using StepByStepCoding.Application.Models.Realtime;
using StepByStepCoding.Hubs;
using StepByStepCoding.Services;

namespace StepByStepCoding.Security.Tests.Api
{
    public sealed class SignalRServiceTests : TestBase
    {
        public SignalRServiceTests(ITestOutputHelper output) : base(output) { }

        #region File group broadcasts

        [Fact]
        public async Task UpdateCode_SendsChangesToTheWholeFileGroup()
        {
            // Arrange
            var changes = Fixture.Create<CodeChangesDTO>();
            var group = $"file:{changes.FileId:N}";
            var client = new Mock<IAppClient>();
            var clients = new Mock<IHubClients<IAppClient>>();
            var hub = new Mock<IHubContext<AppHub, IAppClient>>();
            client.Setup(service => service.UpdateCode(changes)).Returns(Task.CompletedTask);
            clients.Setup(service => service.Group(group)).Returns(client.Object);
            hub.SetupGet(service => service.Clients).Returns(clients.Object);
            var service = new SignalRService(hub.Object);

            WriteOutput("Arranged input", (nameof(changes), changes), (nameof(group), group));

            // Act
            var result = await Record.ExceptionAsync(() => service.UpdateCode(changes));

            WriteOutput("Act result", (nameof(result), result));

            // Assert
            result.Should().BeNull();
            clients.Verify(service => service.Group(group), Times.Once);
            client.Verify(service => service.UpdateCode(changes), Times.Once);
            clients.VerifyNoOtherCalls();
        }

        #endregion
    }
}
