using FluentValidation;

namespace StepByStepCoding.Application.Features.Submissions.Queries.GetSubmissionsList
{
    public sealed class GetSubmissionsListQueryValidator : AbstractValidator<GetSubmissionsListQuery>
    {
        public GetSubmissionsListQueryValidator()
        {
            RuleFor(request => request.Page).InclusiveBetween(1, int.MaxValue / 100);
            RuleFor(request => request.RecordsPerPage).InclusiveBetween(1, 100);
            RuleFor(request => request.AttemptId).Must(id => !id.HasValue || id.Value != Guid.Empty);
        }
    }
}
