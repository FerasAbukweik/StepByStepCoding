using StepByStepCoding.Application.Utilities.Caching;

namespace StepByStepCoding.Application.Features.Courses.Queries.GetCourseDetail
{
    public sealed class GetCourseDetailQuery() : CacheableRequest<CourseDetailDTO>(CacheGroups.Courses)
    {
        public required Guid Id
        {
            get; set;
        }
    }
}
