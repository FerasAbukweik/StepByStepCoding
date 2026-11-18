using FluentValidation;

namespace StepByStepCoding.Application.Features.ExamQuestions.Queries.GetExamQuestionDetail
{
    public sealed class GetExamQuestionDetailQueryValidator : AbstractValidator<GetExamQuestionDetailQuery>
    {
        public GetExamQuestionDetailQueryValidator()
        {
            RuleFor(request => request.Id).NotEmpty();
        }
    }
}
