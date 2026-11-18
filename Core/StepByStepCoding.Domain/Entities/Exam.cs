using StepByStepCoding.Domain.Common;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Domain.Entities
{
    public sealed class Exam : AcademicEntity
    {
        public Guid SectionId
        {
            get; private set;
        }

        public string Title { get; private set; } = null!;

        public string? Instructions
        {
            get; private set;
        }

        public DateTimeOffset StartsAt
        {
            get; private set;
        }

        public DateTimeOffset Deadline
        {
            get; private set;
        }

        public int DurationMinutes
        {
            get; private set;
        }

        public ExamStatus Status
        {
            get; private set;
        }

        public Section? Section
        {
            get; private set;
        }

        public ICollection<ExamQuestion> Questions { get; private set; } = [];

        public ICollection<ExamAttempt> Attempts { get; private set; } = [];

        private Exam()
        {
        }

        public Exam(Guid sectionId, string title, string? instructions, DateTimeOffset startsAt, DateTimeOffset deadline, int durationMinutes)
        {
            SectionId = AcademicRules.Reference(sectionId, nameof(SectionId));
            Apply(title, instructions, startsAt, deadline, durationMinutes);
            Status = ExamStatus.Draft;
        }

        public void RegisterQuestionChange()
        {
            EnsureDraft();
            MarkChanged();
        }

        public void Update(string title, string? instructions, DateTimeOffset startsAt, DateTimeOffset deadline, int durationMinutes)
        {
            EnsureDraft();
            Apply(title, instructions, startsAt, deadline, durationMinutes);
            MarkChanged();
        }

        public void EnsureDraft() => AcademicRules.Require(Status == ExamStatus.Draft, "Only draft exams and their questions can be edited.");

        public void Publish(int questionCount)
        {
            EnsureDraft();
            AcademicRules.Require(questionCount > 0, "An exam must contain at least one question before publishing.");
            Status = ExamStatus.Published;
            MarkChanged();
        }

        public void Close()
        {
            AcademicRules.Require(Status == ExamStatus.Published, "Only a published exam can be closed.");
            Status = ExamStatus.Closed;
            MarkChanged();
        }

        private void Apply(string title, string? instructions, DateTimeOffset startsAt, DateTimeOffset deadline, int durationMinutes)
        {
            AcademicRules.Require(startsAt < deadline, "The exam deadline must follow its start.");
            AcademicRules.Require(durationMinutes > 0 && durationMinutes <= 1440, "Exam duration must be between 1 and 1440 minutes.");
            Title = AcademicRules.Text(title, nameof(Title), 250);
            Instructions = AcademicRules.OptionalText(instructions, nameof(Instructions), 8000);
            StartsAt = startsAt.ToUniversalTime();
            Deadline = deadline.ToUniversalTime();
            DurationMinutes = durationMinutes;
        }
    }
}
