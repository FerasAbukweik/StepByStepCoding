using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StepByStepCoding.Infrastructure.Options;

namespace StepByStepCoding.Infrastructure.ExtensionMethods
{
    public static class InfrastructureOptionsExtensionMethods
    {
        public static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<CodeExecutionOptions>(configuration.GetSection(CodeExecutionOptions.SectionName));
            services.AddOptions<CodeExecutionOptions>()
                .Validate(options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https", "A valid code execution URL is required.")
                .Validate(options => options.CompileTimeoutMilliseconds > 0 && options.RunTimeoutMilliseconds > 0
                    && options.HttpTimeoutSeconds * 1000 > options.CompileTimeoutMilliseconds + options.RunTimeoutMilliseconds,
                    "Code execution timeouts must be positive and fit within the HTTP timeout.")
                .Validate(options => options.CompileMemoryLimitBytes > 0 && options.RunMemoryLimitBytes > 0 && options.JavaRunMemoryLimitBytes > 0,
                    "Code execution memory limits must be positive.")
                .ValidateOnStart();

            services.Configure<RedisOptions>(configuration.GetSection(RedisOptions.SectionName));

            services.AddOptions<RedisOptions>()
                .Validate(options => options.DefaultAbsoluteExpiration > TimeSpan.Zero,
                    "Redis absolute expiration must be positive.")
                .Validate(options => options.DefaultSlidingExpiration > TimeSpan.Zero,
                    "Redis sliding expiration must be positive.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.KeyPrefix),
                    "Redis key prefix is required.")
                .ValidateOnStart();

            return services;
        }
    }
}
