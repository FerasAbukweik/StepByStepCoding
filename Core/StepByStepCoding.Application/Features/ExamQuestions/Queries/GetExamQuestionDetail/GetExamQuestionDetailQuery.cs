using StepByStepCoding.Application.Utilities.Caching;
using StepByStepCoding.Application.Models.Pagination;

namespace StepByStepCoding.Application.Features.ExamQuestions.Queries.GetExamQuestionDetail
{
    public sealed class GetExamQuestionDetailQuery() : CacheableRequest<ExamQuestionDetailDTO>(CacheGroups.ExamQuestions)
    {
        public required Guid Id
        {
            get; set;
        }
    }
}
