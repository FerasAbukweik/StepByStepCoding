using StepByStepCoding.Application.Utilities.Caching;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Application.Features.Lectures.Commands.UpdateLecture
{
    public sealed class UpdateLectureCommand() : CacheInvalidatingRequest(CacheGroups.Academic)
    {
        public override string Key() => FileId.HasValue ? $"{CacheGroups.File}:{FileId:D}" : base.Key();

        public Guid? FileId { get; set; }

        public Guid? FileVersion { get; set; }

        public string? FileName { get; set; }

        public ProgrammingLanguage Language { get; set; } = ProgrammingLanguage.Cpp;

        public string InitialCode { get; set; } = "";

        public required Guid Id
        {
            get; set;
        }

        public required Guid Version
        {
            get; set;
        }

        public required string Title
        {
            get; set;
        }

        public required DateTimeOffset ScheduledStart
        {
            get; set;
        }
    }
}
