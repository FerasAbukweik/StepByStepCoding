using FluentValidation;

namespace StepByStepCoding.Application.Features.Exams.Commands.PublishExam
{
    public sealed class PublishExamCommandValidator : AbstractValidator<PublishExamCommand>
    {
        public PublishExamCommandValidator()
        {
            RuleFor(request => request.Id).NotEmpty();
            RuleFor(request => request.Version).NotEmpty();
        }
    }
}
