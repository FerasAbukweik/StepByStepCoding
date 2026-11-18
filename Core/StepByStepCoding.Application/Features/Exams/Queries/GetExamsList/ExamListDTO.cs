using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Application.Features.Exams.Queries.GetExamsList
{
    public sealed class ExamListDTO
    {
        public required Guid Id
        {
            get; init;
        }

        public required Guid Version
        {
            get; init;
        }

        public required Guid SectionId
        {
            get; init;
        }

        public required string Title
        {
            get; init;
        }

        public required DateTimeOffset StartsAt
        {
            get; init;
        }

        public required DateTimeOffset Deadline
        {
            get; init;
        }

        public required int DurationMinutes
        {
            get; init;
        }

        public required ExamStatus Status
        {
            get; init;
        }
    }
}
