using StepByStepCoding.Domain.Entities;

namespace StepByStepCoding.Application.Features.Sections.Queries.GetSectionsList
{
    internal static class MapperExtensions
    {
        public static SectionListDTO ToDTO(this Section entity, string instructorName)
        {
            return new SectionListDTO
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
