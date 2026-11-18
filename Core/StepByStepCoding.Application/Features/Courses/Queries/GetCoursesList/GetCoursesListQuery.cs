using StepByStepCoding.Application.Utilities.Caching;
using StepByStepCoding.Application.Models.Pagination;

namespace StepByStepCoding.Application.Features.Courses.Queries.GetCoursesList
{
    public sealed class GetCoursesListQuery() : CacheablePageRequest<PaginatedDTO<CourseListDTO>>(CacheGroups.Courses)
    {
    }
}
