namespace StepByStepCoding.Application.Contracts.Security
{
    public interface IAdminSeeder
    {
        Task Seed(CancellationToken cancellationToken = default);
    }
}
