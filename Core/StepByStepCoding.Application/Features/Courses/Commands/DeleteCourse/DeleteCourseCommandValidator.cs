using FluentValidation;

namespace StepByStepCoding.Application.Features.Courses.Commands.DeleteCourse
{
    public sealed class DeleteCourseCommandValidator : AbstractValidator<DeleteCourseCommand>
    {
        public DeleteCourseCommandValidator()
        {
            RuleFor(request => request.Id).NotEmpty();
            RuleFor(request => request.Version).NotEmpty();
        }
    }
}
