using StepByStepCoding.Application.Utilities.Caching;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Application.Features.Courses.Commands.CreateCourse
{
    public sealed class CreateCourseCommand() : CacheInvalidatingRequest<Guid>(CacheGroups.Academic)
    {
        public required string Name
        {
            get; set;
        }
    }
}
