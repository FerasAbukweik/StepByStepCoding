using StepByStepCoding.Application.Utilities.Caching;
using StepByStepCoding.Application.Models.Pagination;

namespace StepByStepCoding.Application.Features.Lectures.Queries.GetLecturesList
{
    public sealed class GetLecturesListQuery() : CacheablePageRequest<PaginatedDTO<LectureListDTO>>(CacheGroups.Lectures)
    {
        public Guid? SectionId
        {
            get; set;
        }
    }
}
