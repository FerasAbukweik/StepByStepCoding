using FluentValidation;

namespace StepByStepCoding.Application.Features.Sections.Commands.UpdateSection
{
    public sealed class UpdateSectionCommandValidator : AbstractValidator<UpdateSectionCommand>
    {
        public UpdateSectionCommandValidator()
        {
            RuleFor(request => request.Id).NotEmpty();
            RuleFor(request => request.Version).NotEmpty();
            RuleFor(request => request.InstructorId).NotEmpty();
            RuleFor(request => request.Name).NotEmpty().MaximumLength(150);
        }
    }
}
