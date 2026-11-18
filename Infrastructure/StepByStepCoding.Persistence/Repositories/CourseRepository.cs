using StepByStepCoding.Application.Models.Pagination;
using StepByStepCoding.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Models;
using StepByStepCoding.Domain.Entities;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Persistence.Repositories
{
    public sealed class CourseRepository : Repository<Course, ApplicationDbContext>, ICourseRepository
    {
        public CourseRepository(ApplicationDbContext context) : base(context) { }

        public Task<PaginatedDTO<Course>> GetPage(AcademicReadScope scope, Guid? parentId, int page,
            int recordsPerPage, CancellationToken cancellationToken = default)
        {
            return GetPageWithCount(Visible(scope, parentId).OrderBy(entity => entity.Name).ThenBy(entity => entity.Id),
                page, recordsPerPage, cancellationToken);
        }

        public Task<bool> HasDependents(Guid id, CancellationToken cancellationToken = default)
        {
            return context.Sections.AnyAsync(entity => entity.CourseId == id, cancellationToken);
        }

        private IQueryable<Course> Visible(AcademicReadScope scope, Guid? parentId)
        {
            var query = context.Courses.AsNoTracking()
                .Where(entity => true);

            return query;
        }
    }
}
