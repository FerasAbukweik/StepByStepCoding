using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Application.Features.Files.Queries.GetLectureFilesList
{
    public sealed class LectureFileListDTO
    {
        public required Guid Id
        {
            get; init;
        }

        public required Guid Version
        {
            get; init;
        }

        public required Guid LectureId
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

        public required long SavedRevision
        {
            get; init;
        }
    }
}
