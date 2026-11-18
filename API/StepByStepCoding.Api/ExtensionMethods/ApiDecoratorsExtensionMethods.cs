using StepByStepCoding.Application.Contracts.Realtime;
using StepByStepCoding.Decorators;

namespace StepByStepCoding.ExtensionMethods
{
    public static class ApiDecoratorsExtensionMethods
    {
        public static IServiceCollection AddDecorators(this IServiceCollection services)
        {
            services.Decorate<ISignalRService, SignalRServiceLoggingDecorator>();

            return services;
        }
    }
}
