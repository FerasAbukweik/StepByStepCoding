using StepByStepCoding.ExtensionMethods;

namespace StepByStepCoding;

public static class RegisterApiServices
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions(configuration)
            .AddApiControllers()
            .AddRequestLimits()
            .AddRealtime()
            .AddAuthenticationCookies()
            .AddCors(configuration)
            .AddServices()
            .AddDecorators();

        return services;
    }
}
