using Microsoft.AspNetCore.SignalR;
using StepByStepCoding.Application.Contracts.Realtime;
using StepByStepCoding.Hubs;
using StepByStepCoding.Services;

namespace StepByStepCoding.ExtensionMethods
{
    public static class SignalRExtensionMethods
    {
        public static IServiceCollection AddRealtime(this IServiceCollection services)
        {
            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = false;
                options.MaximumReceiveMessageSize = 1024 * 1024;
            });
            services.AddScoped<ISignalRService, SignalRService>();

            return services;
        }

        public static IEndpointRouteBuilder MapRealtime(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapHub<AppHub>(AppHub.Route, options => options.CloseOnAuthenticationExpiration = true)
                .RequireCors("CorsPolicy");

            return endpoints;
        }
    }
}
