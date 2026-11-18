using StepByStepCoding.Domain.Entities;

namespace StepByStepCoding.Application.Features.Exams.Queries.GetExamsList
{
    internal static class MapperExtensions
    {
        public static ExamListDTO ToDTO(this Exam entity)
        {
            return new ExamListDTO
            {
                Id = entity.Id,
                Version = entity.Version,
                SectionId = entity.SectionId,
                Title = entity.Title,
                StartsAt = entity.StartsAt,
                Deadline = entity.Deadline,
                DurationMinutes = entity.DurationMinutes,
                Status = entity.Status
            };
        }
    }
}
