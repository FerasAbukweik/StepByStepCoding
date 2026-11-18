using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using StepByStepCoding.Infrastructure.Options;

namespace StepByStepCoding.Infrastructure.ExtensionMethods
{
    public static class RedisExtensionMethods
    {
        public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IConnectionMultiplexer>(_ =>
            {
                var connectionString = configuration.GetConnectionString("Redis");

                if (string.IsNullOrWhiteSpace(connectionString))
                    throw new InvalidOperationException("ConnectionStrings:Redis is required.");

                var settings = ConfigurationOptions.Parse(connectionString);
                settings.AbortOnConnectFail = false;

                return ConnectionMultiplexer.Connect(settings);
            });

            services.AddStackExchangeRedisCache(_ => { });

            services.AddOptions<RedisCacheOptions>()
                .Configure<IConnectionMultiplexer, IOptions<RedisOptions>>((options, connection, settings) =>
                {
                    options.InstanceName = settings.Value.KeyPrefix;
                    options.ConnectionMultiplexerFactory = () => Task.FromResult(connection);
                });

            return services;
        }
    }
}
