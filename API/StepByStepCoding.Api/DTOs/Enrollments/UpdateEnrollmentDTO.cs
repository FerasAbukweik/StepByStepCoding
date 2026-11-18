using System.ComponentModel.DataAnnotations;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.DTOs.Enrollments
{
    public sealed class UpdateEnrollmentDTO
    {
        public required Guid Version
        {
            get; set;
        }

        public required bool Active
        {
            get; set;
        }
    }
}
