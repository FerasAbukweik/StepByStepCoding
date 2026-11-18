using System.ComponentModel.DataAnnotations;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.DTOs.Submissions
{
    public sealed class SubmitCodeDTO
    {
        public required Guid AttemptId
        {
            get; set;
        }

        public required Guid QuestionId
        {
            get; set;
        }

        [Required, MaxLength(100000)]
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
