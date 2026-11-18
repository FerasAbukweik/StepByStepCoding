using StepByStepCoding.Domain.Enums;
using StepByStepCoding.Application.Utilities.Caching;
using StepByStepCoding.Application.Models.Realtime;

namespace StepByStepCoding.Application.Features.Files.Commands.SaveChanges
{
    public sealed class SaveChangesCommand() : CacheInvalidatingRequest<CodeChangesDTO>(CacheGroups.Academic)
    {
        public required Guid FileId
        {
            get; set;
        }

        public override string Key() => $"{CacheGroups.File}:{FileId:D}";
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
            get; set;
        }
        public required long BaseRevision
        {
            get; set;
        }
        public required IReadOnlyList<CodeChangeSet> ChangeSets
        {
            get; set;
        }
    }
}
