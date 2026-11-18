using StepByStepCoding.Domain.Entities;

namespace StepByStepCoding.Application.Contracts.Repositories
{
    public interface IFileRepository : IRepository<CodeFile>
    {
        Task<CodeFile?> GetWithEnrollment(Guid id, Guid userId, CancellationToken cancellationToken = default);
    }
}
