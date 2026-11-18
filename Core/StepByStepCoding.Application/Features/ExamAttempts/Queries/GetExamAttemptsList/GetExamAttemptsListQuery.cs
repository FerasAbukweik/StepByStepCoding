using StepByStepCoding.Application.Utilities.Caching;
using StepByStepCoding.Application.Models.Pagination;

namespace StepByStepCoding.Application.Features.ExamAttempts.Queries.GetExamAttemptsList
{
    public sealed class GetExamAttemptsListQuery() : CacheablePageRequest<PaginatedDTO<ExamAttemptListDTO>>(CacheGroups.ExamAttempts)
    {
        public Guid? ExamId
        {
            get; set;
        }
    }
}
