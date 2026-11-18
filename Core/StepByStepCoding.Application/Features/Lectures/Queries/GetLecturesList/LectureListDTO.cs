using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Application.Features.Lectures.Queries.GetLecturesList
{
    public sealed class LectureListDTO
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

        public Guid? FileId
        {
            get; init;
        }

        public required string Title
        {
            get; init;
        }

        public required DateTimeOffset ScheduledStart
        {
            get; init;
        }

        public required DateTimeOffset? StartedAt
        {
            get; init;
        }

        public required DateTimeOffset? EndedAt
        {
            get; init;
        }

        public required LectureStatus Status
        {
            get; init;
        }
    }
}
