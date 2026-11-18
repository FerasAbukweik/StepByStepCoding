using FluentValidation;

namespace StepByStepCoding.Application.Features.Files.Commands.CreateLectureFile
{
    public sealed class CreateLectureFileCommandValidator : AbstractValidator<CreateLectureFileCommand>
    {
        public CreateLectureFileCommandValidator()
        {
            RuleFor(request => request.LectureId).NotEmpty();
            RuleFor(request => request.FileName).NotEmpty().MaximumLength(250);
            RuleFor(request => request.Language).IsInEnum();
            RuleFor(request => request.SavedContent).NotNull().MaximumLength(100000);
            RuleFor(request => request.SavedRevision).GreaterThanOrEqualTo(0);
        }
    }
}
