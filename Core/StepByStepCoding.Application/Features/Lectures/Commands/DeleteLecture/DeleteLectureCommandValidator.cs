using FluentValidation;

namespace StepByStepCoding.Application.Features.Lectures.Commands.DeleteLecture
{
    public sealed class DeleteLectureCommandValidator : AbstractValidator<DeleteLectureCommand>
    {
        public DeleteLectureCommandValidator()
        {
            RuleFor(request => request.Id).NotEmpty();
            RuleFor(request => request.Version).NotEmpty();
        }
    }
}
