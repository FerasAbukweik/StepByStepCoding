using StepByStepCoding.Application.Models.Pagination;
using StepByStepCoding.Domain.Entities;
using StepByStepCoding.Application.Models;

namespace StepByStepCoding.Application.Contracts.Repositories
{
    public interface ISubmissionRepository : IRepository<Submission>
    {
        Task<PaginatedDTO<Submission>> GetPage(AcademicReadScope scope, Guid? parentId, int page,
            int recordsPerPage, Guid? questionId = null, CancellationToken cancellationToken = default);

        Task<bool> HasDependents(Guid id, CancellationToken cancellationToken = default);
    }
}
