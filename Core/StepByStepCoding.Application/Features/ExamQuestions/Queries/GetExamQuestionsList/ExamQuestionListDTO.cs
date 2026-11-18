using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Application.Features.ExamQuestions.Queries.GetExamQuestionsList
{
    public sealed class ExamQuestionListDTO
    {
        public required Guid Id
        {
            get; init;
        }

        public required Guid Version
        {
            get; init;
        }

        public required Guid ExamId
        {
            get; init;
        }

        public required string Title
        {
            get; init;
        }

        public required string? ImagePath
        {
            get; init;
        }

        public required decimal MaxScore
        {
            get; init;
        }

        public required int DisplayOrder
        {
            get; init;
        }
    }
}
