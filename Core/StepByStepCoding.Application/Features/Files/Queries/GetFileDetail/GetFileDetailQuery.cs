using StepByStepCoding.Application.Utilities.Caching;
using StepByStepCoding.Application.Models.Pagination;

namespace StepByStepCoding.Application.Features.Files.Queries.GetFileDetail
{
    public sealed class GetFileDetailQuery() : CacheableRequest<FileDetailDTO>(CacheGroups.File)
    {
        public required Guid Id
        {
            get; set;
        }

        public override string Key() => $"{CacheGroups.File}:{Id:D}";
    }
}
