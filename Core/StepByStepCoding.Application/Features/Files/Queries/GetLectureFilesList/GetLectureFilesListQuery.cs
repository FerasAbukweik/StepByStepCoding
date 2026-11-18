using StepByStepCoding.Application.Utilities.Caching;
using StepByStepCoding.Application.Models.Pagination;

namespace StepByStepCoding.Application.Features.Files.Queries.GetLectureFilesList
{
    public sealed class GetLectureFilesListQuery() : CacheablePageRequest<PaginatedDTO<LectureFileListDTO>>(CacheGroups.LectureFiles)
    {
        public Guid? LectureId
        {
            get; set;
        }
    }
}
