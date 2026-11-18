using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StepByStepCoding.Application.Enums;
using StepByStepCoding.Application.Contracts.Security;
using StepByStepCoding.Application.Models.Security;
using StepByStepCoding.Security.Models;
using StepByStepCoding.Security.Options;
using StepByStepCoding.Security.Services;

namespace StepByStepCoding.Security.Seeding
{
    public sealed class AdminSeeder : IAdminSeeder
    {
        private readonly UserManager<ApplicationUser> users;
        private readonly AuthService registration;
        private readonly AdminSeedOptions options;
        private readonly ILogger<AdminSeeder> logger;

        public AdminSeeder(
            UserManager<ApplicationUser> users,
            AuthService registration,
            IOptions<AdminSeedOptions> options,
            ILogger<AdminSeeder> logger)
        {
            this.users = users;
            this.registration = registration;
            this.options = options.Value;
            this.logger = logger;
        }

        public async Task Seed(CancellationToken cancellationToken = default)
        {
            if (!options.Enabled)
                return;

            Validator.ValidateObject(options, new ValidationContext(options), validateAllProperties: true);
            cancellationToken.ThrowIfCancellationRequested();

            var existing = await users.FindByEmailAsync(options.Email);

            if (existing is not null)
            {
                // Never promote an existing account or reset its credentials through configuration.
                if (!await users.IsInRoleAsync(existing, nameof(UserRole.Admin)))
                    throw new InvalidOperationException("The configured seed email belongs to a non-admin account. Choose a different seed email.");

                return;
            }

            var result = await registration.Register(new SignupDTO
            {
                FullName = options.FullName,
                Email = options.Email,
                Password = options.Password,
                Roles = [UserRole.Admin]
            }, cancellationToken);

            if (!result.IsSuccess)
                throw new InvalidOperationException("Admin seed registration failed. Check the configured account details and Identity requirements.");

            logger.LogInformation("{Service}.{Method} Seed admin created", nameof(AdminSeeder), nameof(Seed));
        }
    }
}
