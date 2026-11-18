using StepByStepCoding.Application.Models.Pagination;
using StepByStepCoding.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Models;
using StepByStepCoding.Domain.Entities;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Persistence.Repositories
{
    public sealed class LectureFileRepository : Repository<LectureFile, ApplicationDbContext>, ILectureFileRepository
    {
        public LectureFileRepository(ApplicationDbContext context) : base(context) { }

        public Task<PaginatedDTO<LectureFile>> GetPage(AcademicReadScope scope, Guid? parentId, int page,
            int recordsPerPage, CancellationToken cancellationToken = default)
        {
            return GetPageWithCount(Visible(scope, parentId).OrderBy(entity => entity.FileName).ThenBy(entity => entity.Id),
                page, recordsPerPage, cancellationToken);
        }

        public Task<LectureFile?> GetWithEnrollment(Guid fileId, Guid userId, CancellationToken cancellationToken = default)
        {
            return GetById(fileId,
                [
                    entity => entity.Lecture!.Section!.Enrollments
                        .Where(enrollment => enrollment.StudentId == userId && enrollment.Status == EnrollmentStatus.Active)
                ], cancellationToken);
        }

        public Task<LectureFile?> GetByLecture(Guid lectureId, CancellationToken cancellationToken = default)
        {
            return context.LectureFiles.AsNoTracking()
                .SingleOrDefaultAsync(entity => entity.LectureId == lectureId, cancellationToken);
        }

        public Task<bool> HasDependents(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        private IQueryable<LectureFile> Visible(AcademicReadScope scope, Guid? parentId)
        {
            var query = context.LectureFiles.AsNoTracking()
                .Where(entity => ((scope.IsAdmin
                    || (scope.IsInstructor
                    && entity.Lecture!.Section!.InstructorId == scope.UserId))
                    || ((scope.IsAdmin
                    || (scope.IsInstructor
                    && entity.Lecture!.Section!.InstructorId == scope.UserId)
                    || (scope.IsStudent
                    && entity.Lecture!.Section!.Enrollments.Any(e => e.StudentId == scope.UserId
                    && e.Status == EnrollmentStatus.Active)))
                    && entity.Lecture!.Status != LectureStatus.Scheduled)));

            if (parentId.HasValue)
                query = query.Where(entity => entity.LectureId == parentId.Value);

            return query;
        }
    }
}
