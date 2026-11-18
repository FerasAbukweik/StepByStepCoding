using System.ComponentModel.DataAnnotations;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.DTOs.Enrollments
{
    public sealed class CreateEnrollmentDTO
    {
        public required Guid SectionId
        {
            get; set;
        }

        public required Guid StudentId
        {
            get; set;
        }
    }
}
