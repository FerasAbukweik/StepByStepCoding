using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Application.Features.Enrollments.Queries.GetEnrollmentsList
{
    public sealed class EnrollmentListDTO
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

        public string StudentName { get; init; } = "";

        public required Guid StudentId
        {
            get; init;
        }

        public required EnrollmentStatus Status
        {
            get; init;
        }
    }
}
