using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Models.Security;
using StepByStepCoding.Security.Models;

namespace StepByStepCoding.Security.Contracts.Repositories
{
    public interface IUserLookupRepository : IRepository<ApplicationUser>
    {
        Task<IReadOnlyList<UserLookupDTO>> Search(string role, string? search, CancellationToken cancellationToken);
    }
}
