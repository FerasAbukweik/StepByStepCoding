using StepByStepCoding.Application.Models.Pagination;
using StepByStepCoding.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Models;
using StepByStepCoding.Domain.Entities;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Persistence.Repositories
{
    public sealed class EnrollmentRepository : Repository<Enrollment, ApplicationDbContext>, IEnrollmentRepository
    {
        public EnrollmentRepository(ApplicationDbContext context) : base(context) { }

        public Task<PaginatedDTO<Enrollment>> GetPage(AcademicReadScope scope, Guid? parentId, int page,
            int recordsPerPage, CancellationToken cancellationToken = default)
        {
            return GetPageWithCount(Visible(scope, parentId).OrderBy(entity => entity.StudentId).ThenBy(entity => entity.Id),
                page, recordsPerPage, cancellationToken);
        }

        public Task<bool> HasDependents(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        private IQueryable<Enrollment> Visible(AcademicReadScope scope, Guid? parentId)
        {
            var query = context.Enrollments.AsNoTracking()
                .Where(entity => ((scope.IsAdmin
                    || (scope.IsInstructor
                    && entity.Section!.InstructorId == scope.UserId))
                    || (scope.IsStudent
                    && entity.StudentId == scope.UserId)));

            if (parentId.HasValue)
                query = query.Where(entity => entity.SectionId == parentId.Value);

            return query;
        }
    }
}
