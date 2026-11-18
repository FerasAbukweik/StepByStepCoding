using StepByStepCoding.Application.Models.Pagination;
using StepByStepCoding.Domain.Entities;
using StepByStepCoding.Application.Models;

namespace StepByStepCoding.Application.Contracts.Repositories
{
    public interface ILectureFileRepository : IRepository<LectureFile>
    {
        Task<PaginatedDTO<LectureFile>> GetPage(AcademicReadScope scope, Guid? parentId, int page, int recordsPerPage, CancellationToken cancellationToken = default);

        Task<LectureFile?> GetWithEnrollment(Guid fileId, Guid userId, CancellationToken cancellationToken = default);

        Task<LectureFile?> GetByLecture(Guid lectureId, CancellationToken cancellationToken = default);

        Task<bool> HasDependents(Guid id, CancellationToken cancellationToken = default);
    }
}
