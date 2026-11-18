using FluentValidation;

namespace StepByStepCoding.Application.Features.ExamQuestions.Commands.DeleteExamQuestion
{
    public sealed class DeleteExamQuestionCommandValidator : AbstractValidator<DeleteExamQuestionCommand>
    {
        public DeleteExamQuestionCommandValidator()
        {
            RuleFor(request => request.Id).NotEmpty();
            RuleFor(request => request.Version).NotEmpty();
        }
    }
}
