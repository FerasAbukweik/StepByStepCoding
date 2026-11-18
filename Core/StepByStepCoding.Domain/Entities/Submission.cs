using StepByStepCoding.Domain.Common;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Domain.Entities
{
    public sealed class Submission : AcademicEntity
    {
        public Guid AttemptId
        {
            get; private set;
        }

        public Guid QuestionId
        {
            get; private set;
        }

        public string SubmittedCode { get; private set; } = null!;

        public ProgrammingLanguage Language
        {
            get; private set;
        }

        public DateTimeOffset SubmittedAt
        {
            get; private set;
        }

        public decimal? Score
        {
            get; private set;
        }

        public string? Feedback
        {
            get; private set;
        }

        public Guid? ReviewedBy
        {
            get; private set;
        }

        public DateTimeOffset? ReviewedAt
        {
            get; private set;
        }

        public ExamAttempt? Attempt
        {
            get; private set;
        }

        public ExamQuestion? Question
        {
            get; private set;
        }

        private Submission()
        {
        }

        public Submission(Guid attemptId, Guid questionId, string submittedCode, ProgrammingLanguage language, DateTimeOffset submittedAt)
        {
            AcademicRules.Require(Enum.IsDefined(language), "Unsupported programming language.");
            AttemptId = AcademicRules.Reference(attemptId, nameof(AttemptId));
            QuestionId = AcademicRules.Reference(questionId, nameof(QuestionId));
            SubmittedCode = AcademicRules.Code(submittedCode);
            Language = language;
            SubmittedAt = submittedAt.ToUniversalTime();
        }

        public void Grade(decimal score, decimal maxScore, string? feedback, Guid reviewedBy, DateTimeOffset reviewedAt)
        {
            AcademicRules.Score(score, allowZero: true);
            AcademicRules.Require(score <= maxScore, "The mark cannot exceed the question's maximum score.");
            AcademicRules.Require(reviewedAt >= SubmittedAt, "A review cannot precede submission.");
            ReviewedBy = AcademicRules.Reference(reviewedBy, nameof(ReviewedBy));
            Feedback = AcademicRules.OptionalText(feedback, nameof(Feedback), 8000);
            Score = score;
            ReviewedAt = reviewedAt.ToUniversalTime();
            MarkChanged();
        }
    }
}
