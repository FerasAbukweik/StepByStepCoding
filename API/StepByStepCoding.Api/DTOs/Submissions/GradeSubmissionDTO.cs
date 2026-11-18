using System.ComponentModel.DataAnnotations;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.DTOs.Submissions
{
    public sealed class GradeSubmissionDTO
    {
        public required Guid Version
        {
            get; set;
        }

        public required decimal Score
        {
            get; set;
        }

        [MaxLength(8000)]
        public string? Feedback
        {
            get; set;
        }
    }
}
