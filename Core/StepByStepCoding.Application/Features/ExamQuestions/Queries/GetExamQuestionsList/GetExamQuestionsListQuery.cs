using StepByStepCoding.Application.Utilities.Caching;
using StepByStepCoding.Application.Models.Pagination;

namespace StepByStepCoding.Application.Features.ExamQuestions.Queries.GetExamQuestionsList
{
    public sealed class GetExamQuestionsListQuery() : CacheablePageRequest<PaginatedDTO<ExamQuestionListDTO>>(CacheGroups.ExamQuestions)
    {
        public Guid? ExamId
        {
            get; set;
        }
    }
}
