using FluentValidation;

namespace StepByStepCoding.Application.Features.Enrollments.Commands.DeleteEnrollment
{
    public sealed class DeleteEnrollmentCommandValidator : AbstractValidator<DeleteEnrollmentCommand>
    {
        public DeleteEnrollmentCommandValidator()
        {
            RuleFor(request => request.Id).NotEmpty();
            RuleFor(request => request.Version).NotEmpty();
        }
    }
}
