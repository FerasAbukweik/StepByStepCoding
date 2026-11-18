using FluentValidation;

namespace StepByStepCoding.Application.Features.Courses.Commands.CreateCourse
{
    public sealed class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
    {
        public CreateCourseCommandValidator()
        {
            RuleFor(request => request.Name).NotEmpty().MaximumLength(150);
        }
    }
}
