using StepByStepCoding.Domain.Entities;

namespace StepByStepCoding.Application.Features.Lectures.Queries.GetLecturesList
{
    internal static class MapperExtensions
    {
        public static LectureListDTO ToDTO(this Lecture entity)
        {
            return new LectureListDTO
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
