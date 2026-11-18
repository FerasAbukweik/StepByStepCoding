using System.ComponentModel.DataAnnotations;

namespace StepByStepCoding.DTOs.Files
{
    public sealed class RunFileDTO
    {
        [Required(AllowEmptyStrings = true), MaxLength(100000)]
        public required string Code { get; init; }

        [Required(AllowEmptyStrings = true), MaxLength(10000)]
        public string Stdin { get; init; } = "";
    }
}
