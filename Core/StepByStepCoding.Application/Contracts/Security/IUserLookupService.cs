using StepByStepCoding.Application.Enums;
using StepByStepCoding.Application.Models.Security;

namespace StepByStepCoding.Application.Contracts.Security
{
    public interface IUserLookupService
    {
        Task<IReadOnlyList<UserLookupDTO>> Search(UserRole role, string? search, CancellationToken cancellationToken = default);
    }
}
