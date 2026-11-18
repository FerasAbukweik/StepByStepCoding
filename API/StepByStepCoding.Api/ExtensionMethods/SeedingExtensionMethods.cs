using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using StepByStepCoding.Application.Contracts.Security;

namespace StepByStepCoding.ExtensionMethods
{
    public static class SeedingExtensionMethods
    {
        public static async Task SeedAdmin(this WebApplication app,
            CancellationToken cancellationToken = default)
        {
            await using var scope = app.Services.CreateAsyncScope();

            await scope.ServiceProvider.GetRequiredService<IAdminSeeder>().Seed(cancellationToken);
        }
    }
}
