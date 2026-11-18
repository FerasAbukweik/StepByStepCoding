using FluentValidation;

namespace StepByStepCoding.Application.Features.Submissions.Queries.GetSubmissionDetail
{
    public sealed class GetSubmissionDetailQueryValidator : AbstractValidator<GetSubmissionDetailQuery>
    {
        public GetSubmissionDetailQueryValidator()
        {
            RuleFor(request => request.Id).NotEmpty();
        }
    }
}
