using FluentValidation;

namespace StepByStepCoding.Application.Features.Sections.Commands.DeleteSection
{
    public sealed class DeleteSectionCommandValidator : AbstractValidator<DeleteSectionCommand>
    {
        public DeleteSectionCommandValidator()
        {
            RuleFor(request => request.Id).NotEmpty();
            RuleFor(request => request.Version).NotEmpty();
        }
    }
}
