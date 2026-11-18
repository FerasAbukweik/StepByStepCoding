using StepByStepCoding.Application.Models.Pagination;
using StepByStepCoding.Domain.Entities;
using StepByStepCoding.Application.Models;

namespace StepByStepCoding.Application.Contracts.Repositories
{
    public interface ILectureRepository : IRepository<Lecture>
    {
        Task<PaginatedDTO<Lecture>> GetPage(AcademicReadScope scope, Guid? parentId, int page, int recordsPerPage, CancellationToken cancellationToken = default);

        Task<bool> HasDependents(Guid id, CancellationToken cancellationToken = default);
    }
}
