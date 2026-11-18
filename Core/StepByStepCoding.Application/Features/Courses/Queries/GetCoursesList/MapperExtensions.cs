using StepByStepCoding.Domain.Entities;

namespace StepByStepCoding.Application.Features.Courses.Queries.GetCoursesList
{
    internal static class MapperExtensions
    {
        public static CourseListDTO ToDTO(this Course entity)
        {
            return new CourseListDTO
            {
                Id = entity.Id,
                Version = entity.Version,
                Name = entity.Name
            };
        }
    }
}
