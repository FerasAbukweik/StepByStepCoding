using StepByStepCoding.Application.Utilities.Caching;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Application.Features.Enrollments.Commands.CreateEnrollment
{
    public sealed class CreateEnrollmentCommand() : CacheInvalidatingRequest<Guid>(CacheGroups.Academic)
    {
        public required Guid SectionId
        {
            get; set;
        }

        public required Guid StudentId
        {
            get; set;
        }
    }
}
