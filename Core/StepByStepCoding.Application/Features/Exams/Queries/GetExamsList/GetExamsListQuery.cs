using StepByStepCoding.Application.Utilities.Caching;
using StepByStepCoding.Application.Models.Pagination;

namespace StepByStepCoding.Application.Features.Exams.Queries.GetExamsList
{
    public sealed class GetExamsListQuery() : CacheablePageRequest<PaginatedDTO<ExamListDTO>>(CacheGroups.Exams)
    {
        public Guid? SectionId
        {
            get; set;
        }
    }
}
