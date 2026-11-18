using FluentValidation;

namespace StepByStepCoding.Application.Features.ExamAttempts.Commands.StartExamAttempt
{
    public sealed class StartExamAttemptCommandValidator : AbstractValidator<StartExamAttemptCommand>
    {
        public StartExamAttemptCommandValidator()
        {
            RuleFor(request => request.ExamId).NotEmpty();
        }
    }
}
