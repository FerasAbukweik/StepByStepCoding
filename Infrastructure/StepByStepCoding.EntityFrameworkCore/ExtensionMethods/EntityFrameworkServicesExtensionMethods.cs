using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StepByStepCoding.Application.Contracts.Persistence;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.EntityFrameworkCore.Repositories;
using StepByStepCoding.EntityFrameworkCore.UnitsOfWork;

namespace StepByStepCoding.EntityFrameworkCore.ExtensionMethods
{
    public static class EntityFrameworkServicesExtensionMethods
    {
        public static IServiceCollection AddEntityFrameworkServices(this IServiceCollection services)
        {
            // Both layers call this; context-specific contracts keep their scoped instances separate.
            services.TryAddScoped(typeof(IRepository<,>), typeof(Repository<,>));
            services.TryAddScoped(typeof(IUnitOfWork<>), typeof(EfUnitOfWork<>));

            return services;
        }
    }
}
