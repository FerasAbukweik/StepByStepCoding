using StepByStepCoding.Application.Models.Pagination;
using StepByStepCoding.Domain.Entities;
using StepByStepCoding.Application.Models;

namespace StepByStepCoding.Application.Contracts.Repositories
{
    public interface IExamQuestionRepository : IRepository<ExamQuestion>
    {
        Task<PaginatedDTO<ExamQuestion>> GetPage(AcademicReadScope scope, Guid? parentId, int page, int recordsPerPage, CancellationToken cancellationToken = default);

        Task<int> Count(AcademicReadScope scope, Guid? parentId, CancellationToken cancellationToken = default);

        Task<bool> HasDependents(Guid id, CancellationToken cancellationToken = default);
    }
}
