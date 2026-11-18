using StepByStepCoding.Application.Utilities.Caching;
using StepByStepCoding.Application.Features.Files.Queries.GetFileDetail;

namespace StepByStepCoding.Application.Features.Files.Queries.GetLectureFileByLecture
{
    public sealed class GetLectureFileByLectureQuery() : CacheableRequest<FileDetailDTO>(CacheGroups.LectureFiles)
    {
        public required Guid LectureId
        {
            get; set;
        }
    }
}
