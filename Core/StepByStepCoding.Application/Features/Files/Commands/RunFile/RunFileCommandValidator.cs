using FluentValidation;

namespace StepByStepCoding.Application.Features.Files.Commands.RunFile
{
    public sealed class RunFileCommandValidator : AbstractValidator<RunFileCommand>
    {
        public RunFileCommandValidator()
        {
            RuleFor(request => request.FileId).NotEmpty();
            RuleFor(request => request.Code).NotNull().MaximumLength(100000);
            RuleFor(request => request.Stdin).NotNull().MaximumLength(10000);
        }
    }
}
