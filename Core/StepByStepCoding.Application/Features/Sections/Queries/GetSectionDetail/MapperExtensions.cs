using StepByStepCoding.Domain.Entities;

namespace StepByStepCoding.Application.Features.Sections.Queries.GetSectionDetail
{
    internal static class MapperExtensions
    {
        public static SectionDetailDTO ToDTO(this Section entity, string instructorName)
        {
            return new SectionDetailDTO
            {
                Id = entity.Id,
                Version = entity.Version,
                CourseId = entity.CourseId,
                InstructorId = entity.InstructorId,
                Name = entity.Name,
                CourseName = entity.Course?.Name ?? "",
                InstructorName = instructorName
            };
        }
    }
}
