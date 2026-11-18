using StepByStepCoding.Application.Models.Pagination;
using StepByStepCoding.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Models;
using StepByStepCoding.Domain.Entities;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Persistence.Repositories
{
    public sealed class LectureRepository : Repository<Lecture, ApplicationDbContext>, ILectureRepository
    {
        public LectureRepository(ApplicationDbContext context) : base(context) { }

        public Task<PaginatedDTO<Lecture>> GetPage(AcademicReadScope scope, Guid? parentId, int page,
            int recordsPerPage, CancellationToken cancellationToken = default)
        {
            return GetPageWithCount(Visible(scope, parentId)
                .Include(entity => entity.File).OrderBy(entity => entity.ScheduledStart).ThenBy(entity => entity.Id),
                page, recordsPerPage, cancellationToken);
        }

        public Task<bool> HasDependents(Guid id, CancellationToken cancellationToken = default)
        {
            return context.LectureFiles.AnyAsync(entity => entity.LectureId == id, cancellationToken);
        }

        private IQueryable<Lecture> Visible(AcademicReadScope scope, Guid? parentId)
        {
            var query = context.Lectures.AsNoTracking()
                .Where(entity => (scope.IsAdmin
                    || (scope.IsInstructor
                    && entity.Section!.InstructorId == scope.UserId)
                    || (scope.IsStudent
                    && entity.Section!.Enrollments.Any(e => e.StudentId == scope.UserId
                    && e.Status == EnrollmentStatus.Active))));

            if (parentId.HasValue)
                query = query.Where(entity => entity.SectionId == parentId.Value);

            return query;
        }
    }
}
