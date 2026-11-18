using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StepByStepCoding.Security.ExtensionMethods;

namespace StepByStepCoding.Security
{
    public static class RegisterSecurityServices
    {
        public static IServiceCollection AddSecurityServices(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddOptions(configuration)
                .AddDatabase(configuration)
                .AddIdentity()
                .AddRepositories()
                .AddServices()
                .AddDecorators()
                .AddBackgroundServices()
                .AddJwtAuthentication()
                .AddAuthorization();

            return services;
        }
    }
}
