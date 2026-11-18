using StepByStepCoding.Application.Utilities.Caching;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Application.Features.Enrollments.Commands.UpdateEnrollment
{
    public sealed class UpdateEnrollmentCommand() : CacheInvalidatingRequest(CacheGroups.Academic)
    {
        public required Guid Id
        {
            get; set;
        }

        public required Guid Version
        {
            get; set;
        }

        public required bool Active
        {
            get; set;
        }
    }
}
