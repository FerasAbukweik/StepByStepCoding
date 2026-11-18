using System.ComponentModel.DataAnnotations;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.DTOs.LectureFiles
{
    public sealed class CreateLectureFileDTO
    {
        public required Guid LectureId
        {
            get; set;
        }

        [Required, MaxLength(250)]
        public required string FileName
        {
            get; set;
        }

        public required ProgrammingLanguage Language
        {
            get; set;
        }

        [MaxLength(100000)]
        public required string SavedContent
        {
            get; set;
        }

        public required long SavedRevision
        {
            get; set;
        }
    }
}
