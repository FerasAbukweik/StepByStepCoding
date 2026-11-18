using StepByStepCoding.Domain.Entities;

namespace StepByStepCoding.Application.Features.ExamQuestions.Queries.GetExamQuestionsList
{
    internal static class MapperExtensions
    {
        public static ExamQuestionListDTO ToDTO(this ExamQuestion entity)
        {
            return new ExamQuestionListDTO
            {
                Id = entity.Id,
                Version = entity.Version,
                ExamId = entity.ExamId,
                Title = entity.Title,
                ImagePath = entity.ImagePath,
                MaxScore = entity.MaxScore,
                DisplayOrder = entity.DisplayOrder
            };
        }
    }
}
