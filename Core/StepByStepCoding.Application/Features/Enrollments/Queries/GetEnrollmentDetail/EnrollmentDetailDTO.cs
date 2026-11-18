using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Application.Features.Enrollments.Queries.GetEnrollmentDetail
{
    public sealed class EnrollmentDetailDTO
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
