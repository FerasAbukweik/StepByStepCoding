using FluentValidation;

namespace StepByStepCoding.Application.Features.ExamQuestions.Commands.CreateExamQuestion
{
    public sealed class CreateExamQuestionCommandValidator : AbstractValidator<CreateExamQuestionCommand>
    {
        public CreateExamQuestionCommandValidator()
        {
            RuleFor(request => request.ExamId).NotEmpty();
            RuleFor(request => request.Title).NotEmpty().MaximumLength(250);
            RuleFor(request => request.Description).NotEmpty().MaximumLength(20000);
            RuleFor(request => request.ImagePath).MaximumLength(1024);
            RuleFor(request => request.MaxScore).GreaterThan(0m).LessThanOrEqualTo(10000m).Must(value => decimal.Round(value, 2) == value);
            RuleFor(request => request.DisplayOrder).InclusiveBetween(0, 1000);
        }
    }
}
