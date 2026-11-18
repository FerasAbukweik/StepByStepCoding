using FluentValidation;

namespace StepByStepCoding.Application.Features.ExamAttempts.Commands.FinishExamAttempt
{
    public sealed class FinishExamAttemptCommandValidator : AbstractValidator<FinishExamAttemptCommand>
    {
        public FinishExamAttemptCommandValidator()
        {
            RuleFor(request => request.Id).NotEmpty();
            RuleFor(request => request.Version).NotEmpty();
        }
    }
}
