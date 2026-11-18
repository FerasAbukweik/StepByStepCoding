using FluentValidation;

namespace StepByStepCoding.Application.Features.Lectures.Commands.StartLecture
{
    public sealed class StartLectureCommandValidator : AbstractValidator<StartLectureCommand>
    {
        public StartLectureCommandValidator()
        {
            RuleFor(request => request.Id).NotEmpty();
            RuleFor(request => request.Version).NotEmpty();
        }
    }
}
