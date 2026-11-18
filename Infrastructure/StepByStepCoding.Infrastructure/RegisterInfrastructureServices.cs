using StepByStepCoding.Infrastructure.ExtensionMethods;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace StepByStepCoding.Infrastructure
{
    public static class RegisterInfrastructureServices
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddOptions(configuration)
                .AddRedis(configuration)
                .AddCodeExecution()
                .AddServices()
                .AddDecorators();

            return services;
        }
    }
}
