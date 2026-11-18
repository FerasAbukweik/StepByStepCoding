using StepByStepCoding.Application.Utilities.Caching;
using StepByStepCoding.Application.Models.Pagination;

namespace StepByStepCoding.Application.Features.Sections.Queries.GetSectionsList
{
    public sealed class GetSectionsListQuery() : CacheablePageRequest<PaginatedDTO<SectionListDTO>>(CacheGroups.Sections)
    {
        public Guid? CourseId
        {
            get; set;
        }
    }
}
