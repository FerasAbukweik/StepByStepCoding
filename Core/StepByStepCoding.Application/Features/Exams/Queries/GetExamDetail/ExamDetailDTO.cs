using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Application.Features.Exams.Queries.GetExamDetail
{
    public sealed class ExamDetailDTO
    {
        public Guid InstructorId { get; init; }

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

        public required string? Instructions
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
