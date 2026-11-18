using StepByStepCoding.Domain.Entities;

namespace StepByStepCoding.Application.Features.Courses.Queries.GetCourseDetail
{
    internal static class MapperExtensions
    {
        public static CourseDetailDTO ToDTO(this Course entity)
        {
            return new CourseDetailDTO
            {
                Id = entity.Id,
                Version = entity.Version,
                Name = entity.Name
            };
        }
    }
}
