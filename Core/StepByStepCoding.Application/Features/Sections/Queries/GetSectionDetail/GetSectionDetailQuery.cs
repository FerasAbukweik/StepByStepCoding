using StepByStepCoding.Application.Utilities.Caching;
using StepByStepCoding.Application.Models.Pagination;

namespace StepByStepCoding.Application.Features.Sections.Queries.GetSectionDetail
{
    public sealed class GetSectionDetailQuery() : CacheableRequest<SectionDetailDTO>(CacheGroups.Sections)
    {
        public required Guid Id
        {
            get; set;
        }
    }
}
