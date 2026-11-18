using StepByStepCoding.Application.Utilities.Caching;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Application.Features.Exams.Commands.UpdateExam
{
    public sealed class UpdateExamCommand() : CacheInvalidatingRequest(CacheGroups.Academic)
    {
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

        public required string? Instructions
        {
            get; set;
        }

        public required DateTimeOffset StartsAt
        {
            get; set;
        }

        public required DateTimeOffset Deadline
        {
            get; set;
        }

        public required int DurationMinutes
        {
            get; set;
        }
    }
}
