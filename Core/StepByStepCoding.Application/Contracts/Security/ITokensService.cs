using StepByStepCoding.Application.Models.Security;
using StepByStepCoding.Application.Common;

namespace StepByStepCoding.Application.Contracts.Security
{
    public interface ITokensService
    {
        Task<TokensDTO> GenerateAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<Result<TokensDTO>> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);
    }
}
