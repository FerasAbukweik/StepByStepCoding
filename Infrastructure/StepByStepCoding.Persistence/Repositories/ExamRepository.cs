using StepByStepCoding.Application.Models.Pagination;
using StepByStepCoding.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Models;
using StepByStepCoding.Domain.Entities;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Persistence.Repositories
{
    public sealed class ExamRepository : Repository<Exam, ApplicationDbContext>, IExamRepository
    {
        public ExamRepository(ApplicationDbContext context) : base(context) { }

        public Task<PaginatedDTO<Exam>> GetPage(AcademicReadScope scope, Guid? parentId, int page,
            int recordsPerPage, CancellationToken cancellationToken = default)
        {
            return GetPageWithCount(Visible(scope, parentId).OrderBy(entity => entity.StartsAt).ThenBy(entity => entity.Id),
                page, recordsPerPage, cancellationToken);
        }

        public async Task<bool> HasDependents(Guid id, CancellationToken cancellationToken = default)
        {
            return await context.ExamQuestions.AnyAsync(entity => entity.ExamId == id, cancellationToken)
                || await context.ExamAttempts.AnyAsync(entity => entity.ExamId == id, cancellationToken);
        }

        private IQueryable<Exam> Visible(AcademicReadScope scope, Guid? parentId)
        {
            var query = context.Exams.AsNoTracking()
                .Where(entity => ((scope.IsAdmin
                    || (scope.IsInstructor
                    && entity.Section!.InstructorId == scope.UserId))
                    || ((scope.IsAdmin
                    || (scope.IsInstructor
                    && entity.Section!.InstructorId == scope.UserId)
                    || (scope.IsStudent
                    && entity.Section!.Enrollments.Any(e => e.StudentId == scope.UserId
                    && e.Status == EnrollmentStatus.Active)))
                    && entity.Status != ExamStatus.Draft)));

            if (parentId.HasValue)
                query = query.Where(entity => entity.SectionId == parentId.Value);

            return query;
        }
    }
}
