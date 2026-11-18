using StepByStepCoding.Application.Models.Security;
using StepByStepCoding.Application.Common;

namespace StepByStepCoding.Application.Contracts.Security
{
    public interface IAuthService
    {
        Task<Result> SignupAsync(SignupDTO request, CancellationToken cancellationToken = default);

        Task<Result<TokensDTO>> LoginAsync(LoginDTO request, CancellationToken cancellationToken = default);

        Task<Result<TokensDTO>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    }
}
