using System.ComponentModel.DataAnnotations;

namespace StepByStepCoding.Application.Models.Security
{
    public class LoginDTO
    {
        [Required, EmailAddress, MaxLength(256)]
        public required string Email
        {
            get; set;
        }

        [Required, MaxLength(128)]
        public required string Password
        {
            get; set;
        }
    }
}
