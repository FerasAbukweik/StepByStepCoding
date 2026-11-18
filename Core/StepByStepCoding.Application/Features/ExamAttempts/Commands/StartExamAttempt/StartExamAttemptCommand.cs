using StepByStepCoding.Application.Utilities.Caching;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Application.Features.ExamAttempts.Commands.StartExamAttempt
{
    public sealed class StartExamAttemptCommand() : CacheInvalidatingRequest<Guid>(CacheGroups.Academic)
    {
        public required Guid ExamId
        {
            get; set;
        }
    }
}
