using StepByStepCoding.Application.Models.Pagination;
using StepByStepCoding.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Models;
using StepByStepCoding.Domain.Entities;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Persistence.Repositories
{
    public sealed class SectionRepository : Repository<Section, ApplicationDbContext>, ISectionRepository
    {
        public SectionRepository(ApplicationDbContext context) : base(context) { }

        public Task<PaginatedDTO<Section>> GetPage(AcademicReadScope scope, Guid? parentId, int page,
            int recordsPerPage, CancellationToken cancellationToken = default)
        {
            return GetPageWithCount(Visible(scope, parentId).Include(entity => entity.Course).OrderBy(entity => entity.Name).ThenBy(entity => entity.Id),
                page, recordsPerPage, cancellationToken);
        }

        public async Task<bool> HasDependents(Guid id, CancellationToken cancellationToken = default)
        {
            return await context.Enrollments.AnyAsync(entity => entity.SectionId == id, cancellationToken)
                || await context.Lectures.AnyAsync(entity => entity.SectionId == id, cancellationToken)
                || await context.Exams.AnyAsync(entity => entity.SectionId == id, cancellationToken);
        }

        private IQueryable<Section> Visible(AcademicReadScope scope, Guid? parentId)
        {
            var query = context.Sections.AsNoTracking()
                .Where(entity => (scope.IsAdmin
                    || (scope.IsInstructor
                    && entity.InstructorId == scope.UserId)
                    || (scope.IsStudent
                    && entity.Enrollments.Any(e => e.StudentId == scope.UserId
                    && e.Status == EnrollmentStatus.Active))));

            if (parentId.HasValue)
                query = query.Where(entity => entity.CourseId == parentId.Value);

            return query;
        }
    }
}
