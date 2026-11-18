namespace StepByStepCoding.Application.Contracts.Security
{
    public interface IUserService
    {
        Guid? GetUserId();

        bool IsInRole(string role);
    }
}
