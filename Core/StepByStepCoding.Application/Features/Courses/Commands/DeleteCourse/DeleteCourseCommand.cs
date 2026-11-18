using StepByStepCoding.Application.Utilities.Caching;

namespace StepByStepCoding.Application.Features.Courses.Commands.DeleteCourse
{
    public sealed class DeleteCourseCommand() : CacheInvalidatingRequest(CacheGroups.Academic)
    {
        public required Guid Id
        {
            get; set;
        }

        public required Guid Version
        {
            get; set;
        }
    }
}
