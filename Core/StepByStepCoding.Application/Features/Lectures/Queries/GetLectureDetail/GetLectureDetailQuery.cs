using StepByStepCoding.Application.Utilities.Caching;
using StepByStepCoding.Application.Models.Pagination;

namespace StepByStepCoding.Application.Features.Lectures.Queries.GetLectureDetail
{
    public sealed class GetLectureDetailQuery() : CacheableRequest<LectureDetailDTO>(CacheGroups.Lectures)
    {
        public required Guid Id
        {
            get; set;
        }
    }
}
