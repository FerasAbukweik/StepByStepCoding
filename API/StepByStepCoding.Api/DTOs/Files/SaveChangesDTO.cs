using StepByStepCoding.Domain.Enums;
using StepByStepCoding.Application.Models.Realtime;

namespace StepByStepCoding.DTOs.Files
{
    public sealed class SaveChangesDTO
    {
        public string? FileName
        {
            get; init;
        }

        public ProgrammingLanguage? Language
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
        public required IReadOnlyList<CodeChangeSet> ChangeSets
        {
            get; init;
        }
    }
}
