using StepByStepCoding.Domain.Entities;

namespace StepByStepCoding.Application.Contracts.Repositories
{
    public interface IWorkspaceFileRepository : IRepository<WorkspaceFile>
    {
        Task<WorkspaceFile?> GetByUser(Guid userId, CancellationToken cancellationToken = default);
    }
}
