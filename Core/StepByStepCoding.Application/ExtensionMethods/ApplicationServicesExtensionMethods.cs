using StepByStepCoding.Application.Contracts.Caching;
using StepByStepCoding.Application.Decorators;
using StepByStepCoding.Application.Contracts.Academic;
using StepByStepCoding.Application.Contracts.Realtime;
using StepByStepCoding.Application.Services;
using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using StepByStepCoding.Application.Utilities;

namespace StepByStepCoding.Application.ExtensionMethods
{
    public static class ApplicationServicesExtensionMethods
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddLogging();

            services.AddScoped(provider => new Lazy<IRedisService>(() => provider.GetRequiredService<IRedisService>()));
            services.AddScoped<AcademicCachePolicy>();
            services.AddScoped(provider => new Lazy<AcademicCachePolicy>(() => provider.GetRequiredService<AcademicCachePolicy>()));
            services.AddScoped<IMediator, Mediator>();
            services.AddScoped<IAcademicAccess, AcademicAccess>();
            services.AddSingleton(TimeProvider.System);

            return services;
        }

        public static IServiceCollection AddHandlers(this IServiceCollection services, params Assembly[] assemblies)
        {
            if (assemblies.Length == 0)
                assemblies = [typeof(Mediator).Assembly];

            services.Scan(scan => scan
                .FromAssemblies(assemblies)
                .AddClasses(classes => classes.AssignableTo(typeof(IRequestHandler<,>))
                    .Where(type => !type.IsGenericTypeDefinition), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
                .AddClasses(classes => classes.AssignableTo(typeof(IRequestHandler<>))
                    .Where(type => !type.IsGenericTypeDefinition), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            return services;
        }

        public static IServiceCollection AddDecorators(this IServiceCollection services)
        {
            services.Decorate(typeof(IRequestHandler<,>), typeof(CachingDecorator<,>));
            services.Decorate(typeof(IRequestHandler<>), typeof(CachingDecorator<>));
            services.Decorate(typeof(IRequestHandler<,>), typeof(LoggingDecorator<,>));
            services.Decorate(typeof(IRequestHandler<>), typeof(LoggingDecorator<>));

            return services;
        }

        public static IServiceCollection AddValidators(this IServiceCollection services, params Assembly[] assemblies)
        {
            if (assemblies.Length == 0)
                assemblies = [typeof(Mediator).Assembly];

            foreach (var assembly in assemblies)
                services.AddValidatorsFromAssembly(assembly, ServiceLifetime.Scoped, includeInternalTypes: true);

            return services;
        }
    }
}
