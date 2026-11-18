using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Application.Features.ExamAttempts.Queries.GetExamAttemptsList
{
    public sealed class ExamAttemptListDTO
    {
        public required Guid Id
        {
            get; init;
        }

        public required Guid Version
        {
            get; init;
        }

        public required Guid ExamId
        {
            get; init;
        }

        public string StudentName { get; init; } = "";

        public required Guid StudentId
        {
            get; init;
        }

        public required DateTimeOffset StartedAt
        {
            get; init;
        }

        public required DateTimeOffset ExpiresAt
        {
            get; init;
        }

        public required AttemptStatus Status
        {
            get; init;
        }
    }
}
