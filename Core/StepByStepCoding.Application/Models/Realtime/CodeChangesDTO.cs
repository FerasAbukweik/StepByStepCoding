using StepByStepCoding.Domain.Enums;
namespace StepByStepCoding.Application.Models.Realtime
{
    public sealed class CodeChangesDTO
    {
        public string? FileName
        {
            get; init;
        }

        public ProgrammingLanguage? Language
        {
            get; init;
        }

        public required Guid FileId
        {
            get; init;
        }
        public required Guid? LectureId
        {
            get; init;
        }
        public required Guid BatchId
        {
            get; init;
        }
        public required long BaseRevision
        {
            get; init;
        }
        public required long Revision
        {
            get; init;
        }
        public required IReadOnlyList<CodeChangeSet> ChangeSets
        {
            get; init;
        }
    }
}
