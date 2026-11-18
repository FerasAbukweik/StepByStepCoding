using FluentValidation;

namespace StepByStepCoding.Application.Features.Files.Commands.DeleteLectureFile
{
    public sealed class DeleteLectureFileCommandValidator : AbstractValidator<DeleteLectureFileCommand>
    {
        public DeleteLectureFileCommandValidator()
        {
            RuleFor(request => request.Id).NotEmpty();
            RuleFor(request => request.Version).NotEmpty();
        }
    }
}
