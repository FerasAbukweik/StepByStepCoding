using StepByStepCoding.Application.Utilities.Caching;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Application.Features.ExamAttempts.Commands.FinishExamAttempt
{
    public sealed class FinishExamAttemptCommand() : CacheInvalidatingRequest(CacheGroups.Academic)
    {
        public required Guid Id
        {
            get; set;
        }

        public required Guid Version
        {
            get; set;
        }
    }
}
