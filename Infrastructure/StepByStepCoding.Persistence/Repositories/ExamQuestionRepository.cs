using StepByStepCoding.Application.Models.Pagination;
using StepByStepCoding.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;
using StepByStepCoding.Application.Contracts.Repositories;
using StepByStepCoding.Application.Models;
using StepByStepCoding.Domain.Entities;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Persistence.Repositories
{
    public sealed class ExamQuestionRepository : Repository<ExamQuestion, ApplicationDbContext>, IExamQuestionRepository
    {
        public ExamQuestionRepository(ApplicationDbContext context) : base(context) { }

        public Task<PaginatedDTO<ExamQuestion>> GetPage(AcademicReadScope scope, Guid? parentId, int page,
            int recordsPerPage, CancellationToken cancellationToken = default)
        {
            return GetPageWithCount(Visible(scope, parentId).OrderBy(entity => entity.DisplayOrder).ThenBy(entity => entity.Id),
                page, recordsPerPage, cancellationToken);
        }

        public Task<int> Count(AcademicReadScope scope, Guid? parentId, CancellationToken cancellationToken = default)
        {
            return Visible(scope, parentId).CountAsync(cancellationToken);
        }

        public Task<bool> HasDependents(Guid id, CancellationToken cancellationToken = default)
        {
            return context.Submissions.AnyAsync(entity => entity.QuestionId == id, cancellationToken);
        }

        private IQueryable<ExamQuestion> Visible(AcademicReadScope scope, Guid? parentId)
        {
            var query = context.ExamQuestions.AsNoTracking()
                .Where(entity => ((scope.IsAdmin
                    || (scope.IsInstructor
                    && entity.Exam!.Section!.InstructorId == scope.UserId))
                    || ((scope.IsAdmin
                    || (scope.IsInstructor
                    && entity.Exam!.Section!.InstructorId == scope.UserId)
                    || (scope.IsStudent
                    && entity.Exam!.Section!.Enrollments.Any(e => e.StudentId == scope.UserId
                    && e.Status == EnrollmentStatus.Active)))
                    && entity.Exam!.Status != ExamStatus.Draft
                    && entity.Exam!.StartsAt <= scope.Now)));

            if (parentId.HasValue)
                query = query.Where(entity => entity.ExamId == parentId.Value);

            return query;
        }
    }
}
