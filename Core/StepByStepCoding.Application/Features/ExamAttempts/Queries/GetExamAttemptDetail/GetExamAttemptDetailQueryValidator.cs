using FluentValidation;

namespace StepByStepCoding.Application.Features.ExamAttempts.Queries.GetExamAttemptDetail
{
    public sealed class GetExamAttemptDetailQueryValidator : AbstractValidator<GetExamAttemptDetailQuery>
    {
        public GetExamAttemptDetailQueryValidator()
        {
            RuleFor(request => request.Id).NotEmpty();
        }
    }
}
