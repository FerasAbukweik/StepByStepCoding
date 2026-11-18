using StepByStepCoding.Application.Utilities.Caching;
using StepByStepCoding.Application.Models.Realtime;

namespace StepByStepCoding.Application.Features.Files.Queries.GetCodeSnapshot
{
    public sealed class GetCodeSnapshotQuery() : CacheableRequest<CodeSnapshotDTO>(CacheGroups.Lectures)
    {
        public required Guid LectureId
        {
            get; set;
        }
    }
}
