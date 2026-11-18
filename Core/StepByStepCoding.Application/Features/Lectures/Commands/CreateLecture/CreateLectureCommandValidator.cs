using FluentValidation;

namespace StepByStepCoding.Application.Features.Lectures.Commands.CreateLecture
{
    public sealed class CreateLectureCommandValidator : AbstractValidator<CreateLectureCommand>
    {
        public CreateLectureCommandValidator()
        {
            RuleFor(request => request.FileName).NotEmpty().MaximumLength(250).When(request => request.FileName is not null);
            RuleFor(request => request.Language).IsInEnum();
            RuleFor(request => request.InitialCode).NotNull().MaximumLength(100000);

            RuleFor(request => request.SectionId).NotEmpty();
            RuleFor(request => request.Title).NotEmpty().MaximumLength(250);
            RuleFor(request => request.ScheduledStart).NotEmpty();
        }
    }
}
