using StepByStepCoding.Application.Utilities.Caching;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Application.Features.Submissions.Commands.SubmitCode
{
    public sealed class SubmitCodeCommand() : CacheInvalidatingRequest<Guid>(CacheGroups.Academic)
    {
        public required Guid AttemptId
        {
            get; set;
        }

        public required Guid QuestionId
        {
            get; set;
        }

        public required string SubmittedCode
        {
            get; set;
        }

        public required ProgrammingLanguage Language
        {
            get; set;
        }
    }
}
