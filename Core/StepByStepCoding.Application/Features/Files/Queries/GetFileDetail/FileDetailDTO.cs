using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Application.Features.Files.Queries.GetFileDetail
{
    public sealed class FileDetailDTO
    {
        public required Guid Id
        {
            get; init;
        }

        public required Guid Version
        {
            get; init;
        }

        public required Guid? LectureId
        {
            get; init;
        }

        public Guid? UserId
        {
            get; init;
        }

        public required string FileName
        {
            get; init;
        }

        public required ProgrammingLanguage Language
        {
            get; init;
        }

        public required string SavedContent
        {
            get; init;
        }

        public required long SavedRevision
        {
            get; init;
        }
    }
}
