using FluentValidation;

namespace StepByStepCoding.Application.Features.Enrollments.Commands.CreateEnrollment
{
    public sealed class CreateEnrollmentCommandValidator : AbstractValidator<CreateEnrollmentCommand>
    {
        public CreateEnrollmentCommandValidator()
        {
            RuleFor(request => request.SectionId).NotEmpty();
            RuleFor(request => request.StudentId).NotEmpty();
        }
    }
}
