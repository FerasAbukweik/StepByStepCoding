using StepByStepCoding.Domain.Common;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Domain.Entities
{
    public sealed class Lecture : AcademicEntity
    {
        public Guid SectionId
        {
            get; private set;
        }

        public string Title { get; private set; } = null!;

        public DateTimeOffset ScheduledStart
        {
            get; private set;
        }

        public DateTimeOffset? StartedAt
        {
            get; private set;
        }

        public DateTimeOffset? EndedAt
        {
            get; private set;
        }

        public LectureStatus Status
        {
            get; private set;
        }

        public Section? Section
        {
            get; private set;
        }

        public LectureFile? File
        {
            get; private set;
        }

        private Lecture()
        {
        }

        public Lecture(Guid sectionId, string title, DateTimeOffset scheduledStart)
        {
            SectionId = AcademicRules.Reference(sectionId, nameof(SectionId));
            Title = AcademicRules.Text(title, nameof(Title), 250);
            ScheduledStart = scheduledStart.ToUniversalTime();
            Status = LectureStatus.Scheduled;
        }

        public void RegisterFileChange()
        {
            AcademicRules.Require(Status != LectureStatus.Ended, "Ended lecture files are read-only.");
            MarkChanged();
        }

        public void Update(string title, DateTimeOffset scheduledStart)
        {
            AcademicRules.Require(Status == LectureStatus.Scheduled, "Only scheduled lectures can be edited.");
            Title = AcademicRules.Text(title, nameof(Title), 250);
            ScheduledStart = scheduledStart.ToUniversalTime();
            MarkChanged();
        }

        public void Start(DateTimeOffset now)
        {
            AcademicRules.Require(Status == LectureStatus.Scheduled, "Only a scheduled lecture can be started.");
            StartedAt = now.ToUniversalTime();
            Status = LectureStatus.Live;
            MarkChanged();
        }

        public void End(DateTimeOffset now)
        {
            AcademicRules.Require(Status == LectureStatus.Live && now >= StartedAt, "Only a live lecture can be ended.");
            EndedAt = now.ToUniversalTime();
            Status = LectureStatus.Ended;
            MarkChanged();
        }
    }
}
