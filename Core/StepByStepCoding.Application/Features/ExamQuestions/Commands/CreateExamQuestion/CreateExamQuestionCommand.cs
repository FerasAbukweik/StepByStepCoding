using StepByStepCoding.Application.Utilities.Caching;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Application.Features.ExamQuestions.Commands.CreateExamQuestion
{
    public sealed class CreateExamQuestionCommand() : CacheInvalidatingRequest<Guid>(CacheGroups.Academic)
    {
        public required Guid ExamId
        {
            get; set;
        }

        public required string Title
        {
            get; set;
        }

        public required string Description
        {
            get; set;
        }

        public required string? ImagePath
        {
            get; set;
        }

        public required decimal MaxScore
        {
            get; set;
        }

        public required int DisplayOrder
        {
            get; set;
        }
    }
}
