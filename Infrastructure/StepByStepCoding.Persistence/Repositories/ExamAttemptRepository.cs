using StepByStepCoding.Application.Models.Pagination;
using StepByStepCoding.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Models;
using StepByStepCoding.Domain.Entities;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Persistence.Repositories
{
    public sealed class ExamAttemptRepository : Repository<ExamAttempt, ApplicationDbContext>, IExamAttemptRepository
    {
        public ExamAttemptRepository(ApplicationDbContext context) : base(context) { }

        public Task<PaginatedDTO<ExamAttempt>> GetPage(AcademicReadScope scope, Guid? parentId, int page,
            int recordsPerPage, CancellationToken cancellationToken = default)
        {
            return GetPageWithCount(Visible(scope, parentId).OrderBy(entity => entity.StartedAt).ThenBy(entity => entity.Id),
                page, recordsPerPage, cancellationToken);
        }

        public Task<bool> HasDependents(Guid id, CancellationToken cancellationToken = default)
        {
            return context.Submissions.AnyAsync(entity => entity.AttemptId == id, cancellationToken);
        }

        private IQueryable<ExamAttempt> Visible(AcademicReadScope scope, Guid? parentId)
        {
            var query = context.ExamAttempts.AsNoTracking()
                .Where(entity => ((scope.IsAdmin
                    || (scope.IsInstructor
                    && entity.Exam!.Section!.InstructorId == scope.UserId))
                    || (scope.IsStudent
                    && entity.StudentId == scope.UserId)));

            if (parentId.HasValue)
                query = query.Where(entity => entity.ExamId == parentId.Value);

            return query;
        }
    }
}
