using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using StepByStepCoding.Application.Contracts.Caching;
using StepByStepCoding.Infrastructure.Options;

namespace StepByStepCoding.Infrastructure.Services
{
    public sealed class RedisService : IRedisService
    {
        private readonly IDistributedCache cache;
        private readonly IConnectionMultiplexer connection;
        private readonly RedisOptions options;

        public RedisService(IDistributedCache cache, IConnectionMultiplexer connection, IOptions<RedisOptions> options)
        {
            this.cache = cache;
            this.connection = connection;
            this.options = options.Value;
        }

        public Task Set<T>(string key, T value, TimeSpan? absoluteExpiration = null,
            TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            var absolute = absoluteExpiration ?? options.DefaultAbsoluteExpiration;
            var sliding = slidingExpiration ?? options.DefaultSlidingExpiration;

            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(absolute, TimeSpan.Zero);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(sliding, TimeSpan.Zero);

            return cache.SetAsync(key, JsonSerializer.SerializeToUtf8Bytes(value), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absolute,
                SlidingExpiration = sliding
            }, cancellationToken);
        }

        public async Task<T?> Get<T>(string key, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            // RedisCache refreshes sliding expiration on a successful read.
            var value = await cache.GetAsync(key, cancellationToken);

            return value is null ? default : JsonSerializer.Deserialize<T>(value);
        }

        public async Task RemoveByPrefix(string prefix, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(prefix);
            var database = connection.GetDatabase();
            var pattern = EscapePattern(options.KeyPrefix + prefix) + "*";

            foreach (var endpoint in connection.GetEndPoints())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var server = connection.GetServer(endpoint);

                if (server.IsReplica)
                    continue;

                // Scan primaries in pages; never load the entire keyspace into memory.
                var batch = new List<RedisKey>(256);

                await foreach (var key in server.KeysAsync(database.Database, pattern, pageSize: 256)
                    .WithCancellation(cancellationToken))
                {
                    batch.Add(key);

                    if (batch.Count == 256)
                    {
                        await DeleteBatch(database, batch, cancellationToken);
                        batch.Clear();
                    }
                }

                if (batch.Count > 0)
                    await DeleteBatch(database, batch, cancellationToken);
            }
        }

        private static Task DeleteBatch(IDatabase database, List<RedisKey> keys, CancellationToken cancellationToken)
        {
            // Individual deletes also work when keys belong to different Redis Cluster slots.
            cancellationToken.ThrowIfCancellationRequested();
            return Task.WhenAll(keys.Select(key => database.KeyDeleteAsync(key)));
        }

        private static string EscapePattern(string value)
        {
            return value.Replace("\\", "\\\\").Replace("*", "\\*").Replace("?", "\\?")
                .Replace("[", "\\[").Replace("]", "\\]");
        }
    }
}
