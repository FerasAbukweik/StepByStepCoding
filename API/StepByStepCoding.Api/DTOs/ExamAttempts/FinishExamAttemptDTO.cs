using System.ComponentModel.DataAnnotations;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.DTOs.ExamAttempts
{
    public sealed class FinishExamAttemptDTO
    {
        public required Guid Version
        {
            get; set;
        }
    }
}
