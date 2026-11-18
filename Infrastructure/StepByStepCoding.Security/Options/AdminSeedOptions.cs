using System.ComponentModel.DataAnnotations;

namespace StepByStepCoding.Security.Options
{
    public sealed class AdminSeedOptions
    {
        public const string SectionName = "AdminSeed";

        public bool Enabled
        {
            get; set;
        }

        [Required, MaxLength(250)]
        public string FullName { get; set; } = "";

        [Required, EmailAddress, MaxLength(256)]
        public string Email { get; set; } = "";

        [Required, MinLength(8), MaxLength(128)]
        public string Password { get; set; } = "";
    }
}
