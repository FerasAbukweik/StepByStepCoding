using System.ComponentModel.DataAnnotations;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.DTOs.Exams
{
    public sealed class CloseExamDTO
    {
        public required Guid Version
        {
            get; set;
        }
    }
}
