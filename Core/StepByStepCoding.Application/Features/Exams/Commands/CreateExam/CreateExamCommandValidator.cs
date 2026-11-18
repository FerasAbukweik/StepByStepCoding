using FluentValidation;

namespace StepByStepCoding.Application.Features.Exams.Commands.CreateExam
{
    public sealed class CreateExamCommandValidator : AbstractValidator<CreateExamCommand>
    {
        public CreateExamCommandValidator()
        {
            RuleFor(request => request.SectionId).NotEmpty();
            RuleFor(request => request.Title).NotEmpty().MaximumLength(250);
            RuleFor(request => request.Instructions).MaximumLength(8000);
            RuleFor(request => request.StartsAt).NotEmpty();
            RuleFor(request => request.Deadline).NotEmpty();
            RuleFor(request => request.DurationMinutes).InclusiveBetween(1, 1440);
            RuleFor(request => request.Deadline).GreaterThan(request => request.StartsAt);
        }
    }
}
