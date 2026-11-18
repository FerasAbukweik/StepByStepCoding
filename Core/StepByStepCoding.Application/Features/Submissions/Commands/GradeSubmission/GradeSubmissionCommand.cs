using StepByStepCoding.Application.Utilities.Caching;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Application.Features.Submissions.Commands.GradeSubmission
{
    public sealed class GradeSubmissionCommand() : CacheInvalidatingRequest(CacheGroups.Academic)
    {
        public required Guid Id
        {
            get; set;
        }

        public required Guid Version
        {
            get; set;
        }

        public required decimal Score
        {
            get; set;
        }

        public required string? Feedback
        {
            get; set;
        }
    }
}
