using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Security.Models;

namespace StepByStepCoding.Security.Contracts.Repositories
{
    public interface IRefreshTokenRepository : IRepository<RefreshToken>
    {
        Task<int> DeleteForUser(Guid userId, CancellationToken cancellationToken = default);

        Task<int> DeleteExpired(DateTimeOffset now, CancellationToken cancellationToken = default);

        Task<RefreshToken?> Consume(string tokenHash, DateTimeOffset now, CancellationToken cancellationToken = default);
    }
}
