using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Application.Features.Sections.Queries.GetSectionDetail
{
    public sealed class SectionDetailDTO
    {
        public required Guid Id
        {
            get; init;
        }

        public required Guid Version
        {
            get; init;
        }

        public required Guid CourseId
        {
            get; init;
        }

        public required Guid InstructorId
        {
            get; init;
        }

        public required string Name
        {
            get; init;
        }

        public string CourseName { get; init; } = "";

        public string InstructorName { get; init; } = "";
    }
}
