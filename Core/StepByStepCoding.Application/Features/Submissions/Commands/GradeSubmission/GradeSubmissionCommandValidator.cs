using FluentValidation;

namespace StepByStepCoding.Application.Features.Submissions.Commands.GradeSubmission
{
    public sealed class GradeSubmissionCommandValidator : AbstractValidator<GradeSubmissionCommand>
    {
        public GradeSubmissionCommandValidator()
        {
            RuleFor(request => request.Id).NotEmpty();
            RuleFor(request => request.Version).NotEmpty();
            RuleFor(request => request.Score).InclusiveBetween(0m, 10000m).Must(value => decimal.Round(value, 2) == value);
            RuleFor(request => request.Feedback).MaximumLength(8000);
        }
    }
}
