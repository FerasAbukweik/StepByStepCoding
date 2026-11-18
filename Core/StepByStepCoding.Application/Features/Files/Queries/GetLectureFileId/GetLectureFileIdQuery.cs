using StepByStepCoding.Application.Utilities.Caching;

namespace StepByStepCoding.Application.Features.Files.Queries.GetLectureFileId
{
    public sealed class GetLectureFileIdQuery() : CacheableRequest<Guid>(CacheGroups.LectureFiles)
    {
        public required Guid LectureId
        {
            get; set;
        }
    }
}
