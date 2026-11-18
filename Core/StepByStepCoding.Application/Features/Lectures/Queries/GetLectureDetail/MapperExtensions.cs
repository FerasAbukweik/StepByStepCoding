using StepByStepCoding.Domain.Entities;

namespace StepByStepCoding.Application.Features.Lectures.Queries.GetLectureDetail
{
    internal static class MapperExtensions
    {
        public static LectureDetailDTO ToDTO(this Lecture entity)
        {
            return new LectureDetailDTO
            {
                Id = entity.Id,
                Version = entity.Version,
                SectionId = entity.SectionId,
                FileId = entity.File?.Id,
                Title = entity.Title,
                ScheduledStart = entity.ScheduledStart,
                StartedAt = entity.StartedAt,
                EndedAt = entity.EndedAt,
                Status = entity.Status
            };
        }
    }
}
