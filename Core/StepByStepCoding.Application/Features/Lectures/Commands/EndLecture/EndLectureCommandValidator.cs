using FluentValidation;

namespace StepByStepCoding.Application.Features.Lectures.Commands.EndLecture
{
    public sealed class EndLectureCommandValidator : AbstractValidator<EndLectureCommand>
    {
        public EndLectureCommandValidator()
        {
            RuleFor(request => request.Id).NotEmpty();
            RuleFor(request => request.Version).NotEmpty();
        }
    }
}
