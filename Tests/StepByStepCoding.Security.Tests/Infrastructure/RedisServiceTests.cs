using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using StepByStepCoding.Infrastructure.Options;
using StepByStepCoding.Infrastructure.Services;

namespace StepByStepCoding.Security.Tests.Infrastructure
{
    public sealed class RedisServiceTests : TestBase
    {
        public RedisServiceTests(ITestOutputHelper output) : base(output) { }

        #region Set and get

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Set_UsesConfiguredOrExplicitExpirations(bool overrideDefaults)
        {
            // Arrange
            var key = Fixture.Create<string>();
            var value = Fixture.Create<string>();
            var options = new RedisOptions();
            TimeSpan? absolute = overrideDefaults ? TimeSpan.FromMinutes(20) : null;
            TimeSpan? sliding = overrideDefaults ? TimeSpan.FromMinutes(5) : null;
            var cache = new Mock<IDistributedCache>();
            byte[]? stored = null;
            DistributedCacheEntryOptions? expiration = null;
            cache.Setup(service => service.SetAsync(key, It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), CancellationToken.None))
                .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>((_, data, settings, _) =>
                {
                    stored = data;
                    expiration = settings;
                }).Returns(Task.CompletedTask);
            var service = new RedisService(cache.Object, new Mock<IConnectionMultiplexer>().Object, Microsoft.Extensions.Options.Options.Create(options));

            WriteOutput("Arranged input", (nameof(key), key), (nameof(value), value), (nameof(absolute), absolute), (nameof(sliding), sliding));

            // Act
            await service.Set(key, value, absolute, sliding);

            WriteOutput("Act result", (nameof(stored), stored), (nameof(expiration), expiration));

            // Assert
            JsonSerializer.Deserialize<string>(stored!).Should().Be(value);
            expiration!.AbsoluteExpirationRelativeToNow.Should().Be(absolute ?? options.DefaultAbsoluteExpiration);
            expiration.SlidingExpiration.Should().Be(sliding ?? options.DefaultSlidingExpiration);
            cache.Verify(service => service.SetAsync(key, It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), CancellationToken.None), Times.Once);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Get_ReturnsDeserializedValueOrNull(bool found)
        {
            // Arrange
            var key = Fixture.Create<string>();
            var value = Fixture.Create<string>();
            var cache = new Mock<IDistributedCache>();
            cache.Setup(service => service.GetAsync(key, CancellationToken.None))
                .ReturnsAsync(found ? JsonSerializer.SerializeToUtf8Bytes(value) : null);
            var service = new RedisService(cache.Object, new Mock<IConnectionMultiplexer>().Object, Microsoft.Extensions.Options.Options.Create(new RedisOptions()));

            WriteOutput("Arranged input", (nameof(key), key), (nameof(found), found));

            // Act
            var result = await service.Get<string>(key);

            WriteOutput("Act result", (nameof(result), result));

            // Assert
            result.Should().Be(found ? value : null);
            cache.Verify(service => service.GetAsync(key, CancellationToken.None), Times.Once);
        }

        #endregion

        #region Prefix invalidation

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task RemoveByPrefix_EscapesPrefixAndSkipsReplicas(bool replica)
        {
            // Arrange
            var endpoint = new DnsEndPoint("localhost", 6379);
            var keys = Fixture.CreateMany<string>(3).Select(value => (RedisKey)value).ToArray();
            var database = new Mock<IDatabase>();
            var server = new Mock<IServer>();
            var connection = new Mock<IConnectionMultiplexer>();
            database.SetupGet(service => service.Database).Returns(2);
            server.SetupGet(service => service.IsReplica).Returns(replica);
            server.Setup(service => service.KeysAsync(2, It.IsAny<RedisValue>(), 256, 0, 0, CommandFlags.None))
                .Returns(keys.ToAsyncEnumerable());
            database.Setup(service => service.KeyDeleteAsync(It.IsAny<RedisKey>(), CommandFlags.None)).ReturnsAsync(true);
            connection.Setup(service => service.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(database.Object);
            connection.Setup(service => service.GetEndPoints(It.IsAny<bool>())).Returns([endpoint]);
            connection.Setup(service => service.GetServer(endpoint, It.IsAny<object>())).Returns(server.Object);
            var service = new RedisService(new Mock<IDistributedCache>().Object, connection.Object,
                Microsoft.Extensions.Options.Options.Create(new RedisOptions { KeyPrefix = "App:" }));
            const string prefix = "files:[1]";

            WriteOutput("Arranged input", (nameof(prefix), prefix), (nameof(replica), replica), (nameof(keys), keys.Select(key => key.ToString()).ToArray()));

            // Act
            var result = await Record.ExceptionAsync(() => service.RemoveByPrefix(prefix));

            WriteOutput("Act result", (nameof(result), result));

            // Assert
            result.Should().BeNull();
            server.Verify(service => service.KeysAsync(2, (RedisValue)@"App:files:\[1\]*", 256, 0, 0, CommandFlags.None),
                replica ? Times.Never() : Times.Once());
            foreach (var key in keys)
                database.Verify(service => service.KeyDeleteAsync(key, CommandFlags.None), replica ? Times.Never() : Times.Once());
        }

        #endregion
    }
}
