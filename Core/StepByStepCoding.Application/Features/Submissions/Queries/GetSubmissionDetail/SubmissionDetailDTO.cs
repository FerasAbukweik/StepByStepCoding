using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Application.Features.Submissions.Queries.GetSubmissionDetail
{
    public sealed class SubmissionDetailDTO
    {
        public required Guid Id
        {
            get; init;
        }

        public required Guid Version
        {
            get; init;
        }

        public required Guid AttemptId
        {
            get; init;
        }

        public required Guid QuestionId
        {
            get; init;
        }

        public required string SubmittedCode
        {
            get; init;
        }

        public required ProgrammingLanguage Language
        {
            get; init;
        }

        public required DateTimeOffset SubmittedAt
        {
            get; init;
        }

        public required decimal? Score
        {
            get; init;
        }

        public required string? Feedback
        {
            get; init;
        }

        public required Guid? ReviewedBy
        {
            get; init;
        }

        public required DateTimeOffset? ReviewedAt
        {
            get; init;
        }
    }
}
