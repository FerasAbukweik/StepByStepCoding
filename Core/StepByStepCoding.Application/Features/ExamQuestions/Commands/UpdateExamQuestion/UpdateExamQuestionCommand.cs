using StepByStepCoding.Application.Utilities.Caching;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Application.Features.ExamQuestions.Commands.UpdateExamQuestion
{
    public sealed class UpdateExamQuestionCommand() : CacheInvalidatingRequest(CacheGroups.Academic)
    {
        public required Guid Id
        {
            get; set;
        }

        public required Guid Version
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
