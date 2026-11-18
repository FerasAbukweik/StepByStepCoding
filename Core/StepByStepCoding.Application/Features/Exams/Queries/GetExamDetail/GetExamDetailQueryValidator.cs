using FluentValidation;

namespace StepByStepCoding.Application.Features.Exams.Queries.GetExamDetail
{
    public sealed class GetExamDetailQueryValidator : AbstractValidator<GetExamDetailQuery>
    {
        public GetExamDetailQueryValidator()
        {
            RuleFor(request => request.Id).NotEmpty();
        }
    }
}
