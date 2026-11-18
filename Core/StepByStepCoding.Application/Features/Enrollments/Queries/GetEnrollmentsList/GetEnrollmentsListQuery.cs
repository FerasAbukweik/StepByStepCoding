using StepByStepCoding.Application.Utilities.Caching;
using StepByStepCoding.Application.Models.Pagination;

namespace StepByStepCoding.Application.Features.Enrollments.Queries.GetEnrollmentsList
{
    public sealed class GetEnrollmentsListQuery() : CacheablePageRequest<PaginatedDTO<EnrollmentListDTO>>(CacheGroups.Enrollments)
    {
        public Guid? SectionId
        {
            get; set;
        }
    }
}
