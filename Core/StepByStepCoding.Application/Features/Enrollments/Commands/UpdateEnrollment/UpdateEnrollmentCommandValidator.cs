using FluentValidation;

namespace StepByStepCoding.Application.Features.Enrollments.Commands.UpdateEnrollment
{
    public sealed class UpdateEnrollmentCommandValidator : AbstractValidator<UpdateEnrollmentCommand>
    {
        public UpdateEnrollmentCommandValidator()
        {
            RuleFor(request => request.Id).NotEmpty();
            RuleFor(request => request.Version).NotEmpty();
        }
    }
}
