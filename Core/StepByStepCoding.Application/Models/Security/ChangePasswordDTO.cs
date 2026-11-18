using System.ComponentModel.DataAnnotations;

namespace StepByStepCoding.Application.Models.Security
{
    public sealed class ChangePasswordDTO
    {
        [Required, MaxLength(128)]
        public required string CurrentPassword
        {
            get; init;
        }

        [Required, MinLength(8), MaxLength(128)]
        public required string NewPassword
        {
            get; init;
        }
    }
}
