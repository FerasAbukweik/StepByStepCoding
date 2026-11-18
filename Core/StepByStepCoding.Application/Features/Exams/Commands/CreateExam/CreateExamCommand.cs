using StepByStepCoding.Application.Utilities.Caching;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Application.Features.Exams.Commands.CreateExam
{
    public sealed class CreateExamCommand() : CacheInvalidatingRequest<Guid>(CacheGroups.Academic)
    {
        public required Guid SectionId
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
