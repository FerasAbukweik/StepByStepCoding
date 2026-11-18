using FluentValidation;

namespace StepByStepCoding.Application.Features.Exams.Commands.UpdateExam
{
    public sealed class UpdateExamCommandValidator : AbstractValidator<UpdateExamCommand>
    {
        public UpdateExamCommandValidator()
        {
            RuleFor(request => request.Id).NotEmpty();
            RuleFor(request => request.Version).NotEmpty();
            RuleFor(request => request.Title).NotEmpty().MaximumLength(250);
            RuleFor(request => request.Instructions).MaximumLength(8000);
            RuleFor(request => request.StartsAt).NotEmpty();
            RuleFor(request => request.Deadline).NotEmpty();
            RuleFor(request => request.DurationMinutes).InclusiveBetween(1, 1440);
            RuleFor(request => request.Deadline).GreaterThan(request => request.StartsAt);
        }
    }
}
