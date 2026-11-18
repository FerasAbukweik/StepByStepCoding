using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Application.Models.Realtime
{
    public sealed class CodeSnapshotDTO
    {
        public required Guid LectureId
        {
            get; init;
        }
        public required Guid FileId
        {
            get; init;
        }
        public required long Revision
        {
            get; init;
        }
        public required string Code
        {
            get; init;
        }
        public required string FileName
        {
            get; init;
        }
        public required ProgrammingLanguage FileType
        {
            get; init;
        }
    }
}
