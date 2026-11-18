using StepByStepCoding.Application.Utilities.Caching;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Application.Features.Files.Commands.CreateLectureFile
{
    public sealed class CreateLectureFileCommand() : CacheInvalidatingRequest<Guid>(CacheGroups.Academic)
    {
        public required Guid LectureId
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
