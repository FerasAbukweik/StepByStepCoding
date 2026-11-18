using StepByStepCoding.Domain.Common;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Domain.Entities
{
    public sealed class ExamAttempt : AcademicEntity
    {
        public Guid ExamId
        {
            get; private set;
        }

        public Guid StudentId
        {
            get; private set;
        }

        public DateTimeOffset StartedAt
        {
            get; private set;
        }

        public DateTimeOffset ExpiresAt
        {
            get; private set;
        }

        public AttemptStatus Status
        {
            get; private set;
        }

        public Exam? Exam
        {
            get; private set;
        }

        public ICollection<Submission> Submissions { get; private set; } = [];

        private ExamAttempt()
        {
        }

        public ExamAttempt(Guid examId, Guid studentId, DateTimeOffset startedAt, DateTimeOffset expiresAt)
        {
            AcademicRules.Require(startedAt < expiresAt, "An attempt must have time remaining.");
            ExamId = AcademicRules.Reference(examId, nameof(ExamId));
            StudentId = AcademicRules.Reference(studentId, nameof(StudentId));
            StartedAt = startedAt.ToUniversalTime();
            ExpiresAt = expiresAt.ToUniversalTime();
            Status = AttemptStatus.InProgress;
        }

        public void RegisterSubmission(DateTimeOffset now)
        {
            AcademicRules.Require(Status == AttemptStatus.InProgress && now >= StartedAt && now < ExpiresAt,
                "This attempt is closed or expired.");
            MarkChanged();
        }

        public void Finish(DateTimeOffset now)
        {
            AcademicRules.Require(Status == AttemptStatus.InProgress, "This attempt has already finished.");
            Status = now >= ExpiresAt ? AttemptStatus.Expired : AttemptStatus.Submitted;
            MarkChanged();
        }
    }
}
