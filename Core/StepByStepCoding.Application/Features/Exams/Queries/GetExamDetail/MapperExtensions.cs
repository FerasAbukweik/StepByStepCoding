using StepByStepCoding.Domain.Entities;

namespace StepByStepCoding.Application.Features.Exams.Queries.GetExamDetail
{
    internal static class MapperExtensions
    {
        public static ExamDetailDTO ToDTO(this Exam entity)
        {
            return new ExamDetailDTO
            {
                Id = entity.Id,
                InstructorId = entity.Section?.InstructorId ?? Guid.Empty,
                Version = entity.Version,
                SectionId = entity.SectionId,
                Title = entity.Title,
                Instructions = entity.Instructions,
                StartsAt = entity.StartsAt,
                Deadline = entity.Deadline,
                DurationMinutes = entity.DurationMinutes,
                Status = entity.Status
            };
        }
    }
}
