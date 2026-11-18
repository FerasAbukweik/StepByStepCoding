using System.ComponentModel.DataAnnotations;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.DTOs.Lectures
{
    public sealed class CreateLectureDTO
    {
        [MaxLength(250)]
        public string? FileName { get; set; }

        [EnumDataType(typeof(ProgrammingLanguage))]
        public ProgrammingLanguage Language { get; set; } = ProgrammingLanguage.Cpp;

        [Required(AllowEmptyStrings = true), MaxLength(100000)]
        public string InitialCode { get; set; } = "";

        public required Guid SectionId
        {
            get; set;
        }

        [Required, MaxLength(250)]
        public required string Title
        {
            get; set;
        }

        public required DateTimeOffset ScheduledStart
        {
            get; set;
        }
    }
}
