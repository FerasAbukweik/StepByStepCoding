using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StepByStepCoding.Application.Contracts.Execution;
using StepByStepCoding.Infrastructure.Options;
using StepByStepCoding.Infrastructure.Services;

namespace StepByStepCoding.Infrastructure.ExtensionMethods
{
    public static class CodeExecutionExtensionMethods
    {
        public static IServiceCollection AddCodeExecution(this IServiceCollection services)
        {
            services.AddHttpClient<ICodeExecutionService, CodeExecutionService>((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<CodeExecutionOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
                client.Timeout = TimeSpan.FromSeconds(options.HttpTimeoutSeconds);
                client.MaxResponseContentBufferSize = 512 * 1024;
            });

            return services;
        }
    }
}
