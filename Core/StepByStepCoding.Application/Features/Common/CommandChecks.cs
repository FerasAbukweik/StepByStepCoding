using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Domain.Common;

namespace StepByStepCoding.Application.Features.Common
{
    internal static class CommandChecks
    {
        public static void Version(AcademicEntity entity, Guid expected)
        {
            if (entity.Version != expected)
                throw new ConflictException("The record changed. Reload it before trying again.");
        }
    }
}
