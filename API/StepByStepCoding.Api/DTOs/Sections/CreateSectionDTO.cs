using System.ComponentModel.DataAnnotations;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.DTOs.Sections
{
    public sealed class CreateSectionDTO
    {
        public required Guid CourseId
        {
            get; set;
        }

        public required Guid InstructorId
        {
            get; set;
        }

        [Required, MaxLength(150)]
        public required string Name
        {
            get; set;
        }
    }
}
