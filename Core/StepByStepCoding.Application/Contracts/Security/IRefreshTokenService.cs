using StepByStepCoding.Application.Models.Security;

namespace StepByStepCoding.Application.Contracts.Security
{
    public interface IRefreshTokenService
    {
        Task<RefreshTokenResultDTO> CreateAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<Guid?> ConsumeAsync(string token, CancellationToken cancellationToken = default);
    }
}
