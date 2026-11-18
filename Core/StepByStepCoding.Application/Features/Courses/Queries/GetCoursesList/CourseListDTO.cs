using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Application.Features.Courses.Queries.GetCoursesList
{
    public sealed class CourseListDTO
    {
        public required Guid Id
        {
            get; init;
        }

        public required Guid Version
        {
            get; init;
        }

        public required string Name
        {
            get; init;
        }
    }
}
