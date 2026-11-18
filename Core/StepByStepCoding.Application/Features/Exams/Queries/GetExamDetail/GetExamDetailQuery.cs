using StepByStepCoding.Application.Utilities.Caching;
using StepByStepCoding.Application.Models.Pagination;

namespace StepByStepCoding.Application.Features.Exams.Queries.GetExamDetail
{
    public sealed class GetExamDetailQuery() : CacheableRequest<ExamDetailDTO>(CacheGroups.Exams)
    {
        public required Guid Id
        {
            get; set;
        }
    }
}
