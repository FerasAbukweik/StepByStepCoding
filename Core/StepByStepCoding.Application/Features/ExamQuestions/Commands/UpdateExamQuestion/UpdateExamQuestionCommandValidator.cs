using FluentValidation;

namespace StepByStepCoding.Application.Features.ExamQuestions.Commands.UpdateExamQuestion
{
    public sealed class UpdateExamQuestionCommandValidator : AbstractValidator<UpdateExamQuestionCommand>
    {
        public UpdateExamQuestionCommandValidator()
        {
            RuleFor(request => request.Id).NotEmpty();
            RuleFor(request => request.Version).NotEmpty();
            RuleFor(request => request.Title).NotEmpty().MaximumLength(250);
            RuleFor(request => request.Description).NotEmpty().MaximumLength(20000);
            RuleFor(request => request.ImagePath).MaximumLength(1024);
            RuleFor(request => request.MaxScore).GreaterThan(0m).LessThanOrEqualTo(10000m).Must(value => decimal.Round(value, 2) == value);
            RuleFor(request => request.DisplayOrder).InclusiveBetween(0, 1000);
        }
    }
}
