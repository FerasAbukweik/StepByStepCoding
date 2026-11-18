using StepByStepCoding.Application.Utilities.Caching;
using StepByStepCoding.Application.Models.Pagination;

namespace StepByStepCoding.Application.Features.Submissions.Queries.GetSubmissionsList
{
    public sealed class GetSubmissionsListQuery() : CacheablePageRequest<PaginatedDTO<SubmissionListDTO>>(CacheGroups.Submissions)
    {
        public Guid? QuestionId
        {
            get; set;
        }

        public Guid? AttemptId
        {
            get; set;
        }
    }
}
