using StepByStepCoding.Application.Models.Security;

namespace StepByStepCoding.Application.Contracts.Security
{
    public interface IAccessTokenService
    {
        Task<AccessTokenResultDTO> GenerateAsync(Guid userId);
    }
}
