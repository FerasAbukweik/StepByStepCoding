using StepByStepCoding.Application.Contracts.Security;
using StepByStepCoding.Application.Enums;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Models.Security;
using StepByStepCoding.Security.Contracts.Repositories;

namespace StepByStepCoding.Security.Services
{
    public sealed class UserLookupService : IUserLookupService
    {
        private readonly IUserLookupRepository repository;
        private readonly IUserService actor;

        public UserLookupService(IUserLookupRepository repository, IUserService actor)
        {
            this.repository = repository;
            this.actor = actor;
        }

        public Task<IReadOnlyList<UserLookupDTO>> Search(UserRole role, string? search, CancellationToken cancellationToken = default)
        {
            if (!actor.IsInRole(nameof(UserRole.Admin)) && !actor.IsInRole(nameof(UserRole.Instructor)))
                throw new ForbiddenException("You cannot search user accounts.");

            if (role is not (UserRole.Instructor or UserRole.Student))
                throw new CustomValidationException("Choose Instructor or Student.");

            if (role == UserRole.Instructor && !actor.IsInRole(nameof(UserRole.Admin)))
                throw new ForbiddenException("Only administrators can select instructors.");

            if (search?.Length > 250)
                throw new CustomValidationException("Search must not exceed 250 characters.");

            return repository.Search(role.ToString(), search?.Trim(), cancellationToken);
        }
    }
}
