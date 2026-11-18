namespace StepByStepCoding.Application.Contracts.Security
{
    public interface IUserDirectory
    {
        Task<IReadOnlyDictionary<Guid, string>> GetNames(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);

        Task<bool> IsInRole(Guid userId, string role, CancellationToken cancellationToken = default);
    }
}
