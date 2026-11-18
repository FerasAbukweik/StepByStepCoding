using StepByStepCoding.Domain.Entities;

namespace StepByStepCoding.Application.Features.ExamQuestions.Queries.GetExamQuestionDetail
{
    internal static class MapperExtensions
    {
        public static ExamQuestionDetailDTO ToDTO(this ExamQuestion entity)
        {
            return new ExamQuestionDetailDTO
            {
                Id = entity.Id,
                Version = entity.Version,
                ExamId = entity.ExamId,
                Title = entity.Title,
                Description = entity.Description,
                ImagePath = entity.ImagePath,
                MaxScore = entity.MaxScore,
                DisplayOrder = entity.DisplayOrder
            };
        }
    }
}
