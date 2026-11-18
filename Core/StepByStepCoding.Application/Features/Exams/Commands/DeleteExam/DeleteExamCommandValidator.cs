using FluentValidation;

namespace StepByStepCoding.Application.Features.Exams.Commands.DeleteExam
{
    public sealed class DeleteExamCommandValidator : AbstractValidator<DeleteExamCommand>
    {
        public DeleteExamCommandValidator()
        {
            RuleFor(request => request.Id).NotEmpty();
            RuleFor(request => request.Version).NotEmpty();
        }
    }
}
