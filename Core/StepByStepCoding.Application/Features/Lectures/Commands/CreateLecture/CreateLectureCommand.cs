using StepByStepCoding.Domain.Enums;
using StepByStepCoding.Application.Utilities.Caching;

namespace StepByStepCoding.Application.Features.Lectures.Commands.CreateLecture
{
    public sealed class CreateLectureCommand() : CacheInvalidatingRequest<Guid>(CacheGroups.Academic)
    {
        public string? FileName { get; set; }

        public ProgrammingLanguage Language { get; set; } = ProgrammingLanguage.Cpp;

        public string InitialCode { get; set; } = "";

        public required Guid SectionId
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
