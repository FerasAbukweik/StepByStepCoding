using StepByStepCoding.Application.Utilities.Caching;
using StepByStepCoding.Application.Models.Pagination;

namespace StepByStepCoding.Application.Features.ExamAttempts.Queries.GetExamAttemptDetail
{
    public sealed class GetExamAttemptDetailQuery() : CacheableRequest<ExamAttemptDetailDTO>(CacheGroups.ExamAttempts)
    {
        public required Guid Id
        {
            get; set;
        }
    }
}
