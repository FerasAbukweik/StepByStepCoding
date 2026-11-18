using StepByStepCoding.Application.Utilities.Caching;
using StepByStepCoding.Application.Models.Pagination;

namespace StepByStepCoding.Application.Features.Submissions.Queries.GetSubmissionDetail
{
    public sealed class GetSubmissionDetailQuery() : CacheableRequest<SubmissionDetailDTO>(CacheGroups.Submissions)
    {
        public required Guid Id
        {
            get; set;
        }
    }
}
