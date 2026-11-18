using FluentValidation;

namespace StepByStepCoding.Application.Features.Exams.Commands.CloseExam
{
    public sealed class CloseExamCommandValidator : AbstractValidator<CloseExamCommand>
    {
        public CloseExamCommandValidator()
        {
            RuleFor(request => request.Id).NotEmpty();
            RuleFor(request => request.Version).NotEmpty();
        }
    }
}
