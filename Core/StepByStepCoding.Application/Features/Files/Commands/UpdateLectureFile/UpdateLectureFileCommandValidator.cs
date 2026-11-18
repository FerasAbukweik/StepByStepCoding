using FluentValidation;

namespace StepByStepCoding.Application.Features.Files.Commands.UpdateLectureFile
{
    public sealed class UpdateLectureFileCommandValidator : AbstractValidator<UpdateLectureFileCommand>
    {
        public UpdateLectureFileCommandValidator()
        {
            RuleFor(request => request.Id).NotEmpty();
            RuleFor(request => request.Version).NotEmpty();
            RuleFor(request => request.FileName).NotEmpty().MaximumLength(250);
            RuleFor(request => request.Language).IsInEnum();
            RuleFor(request => request.SavedContent).NotNull().MaximumLength(100000);
            RuleFor(request => request.SavedRevision).GreaterThanOrEqualTo(0);
        }
    }
}
