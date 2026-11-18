using StepByStepCoding.Application.Models.Security;

namespace StepByStepCoding.Application.Contracts.Security
{
    public interface IAccountService
    {
        Task<UserDTO> Me(CancellationToken cancellationToken = default);

        Task ChangePassword(ChangePasswordDTO request, CancellationToken cancellationToken = default);

        Task Logout(CancellationToken cancellationToken = default);
    }
}
