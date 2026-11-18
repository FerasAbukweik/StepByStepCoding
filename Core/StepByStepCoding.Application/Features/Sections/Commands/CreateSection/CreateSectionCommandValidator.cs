using FluentValidation;

namespace StepByStepCoding.Application.Features.Sections.Commands.CreateSection
{
    public sealed class CreateSectionCommandValidator : AbstractValidator<CreateSectionCommand>
    {
        public CreateSectionCommandValidator()
        {
            RuleFor(request => request.CourseId).NotEmpty();
            RuleFor(request => request.InstructorId).NotEmpty();
            RuleFor(request => request.Name).NotEmpty().MaximumLength(150);
        }
    }
}
