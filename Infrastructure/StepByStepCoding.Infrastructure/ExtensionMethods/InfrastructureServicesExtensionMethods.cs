using StepByStepCoding.Application.Contracts.Execution;
using StepByStepCoding.Infrastructure.Decorators;
using Microsoft.Extensions.DependencyInjection;
using StepByStepCoding.Application.Contracts.Caching;
using StepByStepCoding.Infrastructure.Services;

namespace StepByStepCoding.Infrastructure.ExtensionMethods
{
    public static class InfrastructureServicesExtensionMethods
    {
        public static IServiceCollection AddDecorators(this IServiceCollection services)
        {
            services.Decorate<ICodeExecutionService, CodeExecutionLoggingDecorator>();
            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IRedisService, RedisService>();

            return services;
        }
    }
}
