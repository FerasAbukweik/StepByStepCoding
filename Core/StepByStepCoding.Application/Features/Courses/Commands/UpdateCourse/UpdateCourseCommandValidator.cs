using FluentValidation;

namespace StepByStepCoding.Application.Features.Courses.Commands.UpdateCourse
{
    public sealed class UpdateCourseCommandValidator : AbstractValidator<UpdateCourseCommand>
    {
        public UpdateCourseCommandValidator()
        {
            RuleFor(request => request.Id).NotEmpty();
            RuleFor(request => request.Version).NotEmpty();
            RuleFor(request => request.Name).NotEmpty().MaximumLength(150);
        }
    }
}
