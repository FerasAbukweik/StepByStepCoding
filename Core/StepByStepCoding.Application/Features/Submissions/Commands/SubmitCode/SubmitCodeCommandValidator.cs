using FluentValidation;

namespace StepByStepCoding.Application.Features.Submissions.Commands.SubmitCode
{
    public sealed class SubmitCodeCommandValidator : AbstractValidator<SubmitCodeCommand>
    {
        public SubmitCodeCommandValidator()
        {
            RuleFor(request => request.AttemptId).NotEmpty();
            RuleFor(request => request.QuestionId).NotEmpty();
            RuleFor(request => request.SubmittedCode).NotEmpty().MaximumLength(100000);
            RuleFor(request => request.Language).IsInEnum();
        }
    }
}
