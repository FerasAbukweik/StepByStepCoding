using Microsoft.EntityFrameworkCore;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Domain.Entities;
using StepByStepCoding.EntityFrameworkCore.Repositories;

namespace StepByStepCoding.Persistence.Repositories
{
    public sealed class WorkspaceFileRepository : Repository<WorkspaceFile, ApplicationDbContext>, IWorkspaceFileRepository
    {
        public WorkspaceFileRepository(ApplicationDbContext context) : base(context) { }

        public Task<WorkspaceFile?> GetByUser(Guid userId, CancellationToken cancellationToken = default)
        {
            return context.WorkspaceFiles.AsNoTracking().SingleOrDefaultAsync(file => file.UserId == userId, cancellationToken);
        }
    }
}
