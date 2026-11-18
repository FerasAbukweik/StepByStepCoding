using StepByStepCoding.Application.Utilities.Caching;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Application.Features.Sections.Commands.UpdateSection
{
    public sealed class UpdateSectionCommand() : CacheInvalidatingRequest(CacheGroups.Academic)
    {
        public required Guid Id
        {
            get; set;
        }

        public required Guid Version
        {
            get; set;
        }

        public required Guid InstructorId
        {
            get; set;
        }

        public required string Name
        {
            get; set;
        }
    }
}
