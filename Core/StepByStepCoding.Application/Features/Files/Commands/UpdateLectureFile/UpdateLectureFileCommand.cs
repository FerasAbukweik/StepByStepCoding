using StepByStepCoding.Application.Utilities.Caching;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Application.Features.Files.Commands.UpdateLectureFile
{
    public sealed class UpdateLectureFileCommand() : CacheInvalidatingRequest(CacheGroups.Academic)
    {
        public required Guid Id
        {
            get; set;
        }

        public override string Key() => $"{CacheGroups.File}:{Id:D}";

        public required Guid Version
        {
            get; set;
        }

        public required string FileName
        {
            get; set;
        }

        public required ProgrammingLanguage Language
        {
            get; set;
        }

        public required string SavedContent
        {
            get; set;
        }

        public required long SavedRevision
        {
            get; set;
        }
    }
}
