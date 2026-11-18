using StepByStepCoding.Application.Models.Pagination;
using StepByStepCoding.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Models;
using StepByStepCoding.Domain.Entities;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Persistence.Repositories
{
    public sealed class SubmissionRepository : Repository<Submission, ApplicationDbContext>, ISubmissionRepository
    {
        public SubmissionRepository(ApplicationDbContext context) : base(context) { }

        public Task<PaginatedDTO<Submission>> GetPage(AcademicReadScope scope, Guid? parentId, int page,
            int recordsPerPage, Guid? questionId = null, CancellationToken cancellationToken = default)
        {
            return GetPageWithCount(Visible(scope, parentId, questionId).OrderBy(entity => entity.SubmittedAt).ThenBy(entity => entity.Id),
                page, recordsPerPage, cancellationToken);
        }

        public Task<bool> HasDependents(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        private IQueryable<Submission> Visible(AcademicReadScope scope, Guid? parentId, Guid? questionId)
        {
            var query = context.Submissions.AsNoTracking()
                .Where(entity => ((scope.IsAdmin
                    || (scope.IsInstructor
                    && entity.Attempt!.Exam!.Section!.InstructorId == scope.UserId))
                    || (scope.IsStudent
                    && entity.Attempt!.StudentId == scope.UserId)));

            if (parentId.HasValue)
                query = query.Where(entity => entity.AttemptId == parentId.Value);

            if (questionId.HasValue)
                query = query.Where(entity => entity.QuestionId == questionId.Value);

            return query;
        }
    }
}
