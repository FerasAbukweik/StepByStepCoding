using System.ComponentModel.DataAnnotations;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.DTOs.Courses
{
    public sealed class CreateCourseDTO
    {
        [Required, MaxLength(150)]
        public required string Name
        {
            get; set;
        }
    }
}
