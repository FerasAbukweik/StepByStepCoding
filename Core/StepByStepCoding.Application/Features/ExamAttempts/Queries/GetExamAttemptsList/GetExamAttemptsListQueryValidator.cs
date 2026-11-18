using FluentValidation;

namespace StepByStepCoding.Application.Features.ExamAttempts.Queries.GetExamAttemptsList
{
    public sealed class GetExamAttemptsListQueryValidator : AbstractValidator<GetExamAttemptsListQuery>
    {
        public GetExamAttemptsListQueryValidator()
        {
            RuleFor(request => request.Page).InclusiveBetween(1, int.MaxValue / 100);
            RuleFor(request => request.RecordsPerPage).InclusiveBetween(1, 100);
            RuleFor(request => request.ExamId).Must(id => !id.HasValue || id.Value != Guid.Empty);
        }
    }
}
