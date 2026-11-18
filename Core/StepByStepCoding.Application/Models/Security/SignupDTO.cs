using StepByStepCoding.Application.Enums;
using System.ComponentModel.DataAnnotations;

namespace StepByStepCoding.Application.Models.Security
{
    public class SignupDTO
    {
        [Required, MaxLength(250)]
        public required string FullName
        {
            get; set;
        }

        [Required, EmailAddress, MaxLength(256)]
        public required string Email
        {
            get; set;
        }

        [Required, MinLength(8), MaxLength(128)]
        public required string Password
        {
            get; set;
        }

        [Required, MinLength(1)]
        public required List<UserRole> Roles
        {
            get; set;
        }
    }
}
