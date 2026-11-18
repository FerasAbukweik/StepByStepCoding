using StepByStepCoding.Application.Utilities.Caching;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Application.Features.Sections.Commands.CreateSection
{
    public sealed class CreateSectionCommand() : CacheInvalidatingRequest<Guid>(CacheGroups.Academic)
    {
        public required Guid CourseId
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
