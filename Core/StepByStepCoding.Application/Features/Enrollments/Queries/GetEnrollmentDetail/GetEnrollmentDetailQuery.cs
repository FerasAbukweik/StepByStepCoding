using StepByStepCoding.Application.Utilities.Caching;
using StepByStepCoding.Application.Models.Pagination;

namespace StepByStepCoding.Application.Features.Enrollments.Queries.GetEnrollmentDetail
{
    public sealed class GetEnrollmentDetailQuery() : CacheableRequest<EnrollmentDetailDTO>(CacheGroups.Enrollments)
    {
        public required Guid Id
        {
            get; set;
        }
    }
}
