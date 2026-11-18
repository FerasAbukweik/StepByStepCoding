using System.ComponentModel.DataAnnotations;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.DTOs.ExamAttempts
{
    public sealed class StartExamAttemptDTO
    {
        public required Guid ExamId
        {
            get; set;
        }
    }
}
