using System.ComponentModel.DataAnnotations;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.DTOs.Courses
{
    public sealed class UpdateCourseDTO
    {
        public required Guid Version
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
