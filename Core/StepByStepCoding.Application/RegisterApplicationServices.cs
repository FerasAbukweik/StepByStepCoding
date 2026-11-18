using StepByStepCoding.Application.ExtensionMethods;
using Microsoft.Extensions.DependencyInjection;

namespace StepByStepCoding.Application
{
    public static class RegisterApplicationServices
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services
                .AddServices()
                .AddHandlers()
                .AddValidators()
                .AddDecorators();

            return services;
        }
    }
}
