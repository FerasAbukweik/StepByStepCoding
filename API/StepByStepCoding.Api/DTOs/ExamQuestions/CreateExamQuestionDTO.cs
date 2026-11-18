using System.ComponentModel.DataAnnotations;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.DTOs.ExamQuestions
{
    public sealed class CreateExamQuestionDTO
    {
        public required Guid ExamId
        {
            get; set;
        }

        [Required, MaxLength(250)]
        public required string Title
        {
            get; set;
        }

        [Required, MaxLength(20000)]
        public required string Description
        {
            get; set;
        }

        [MaxLength(1024)]
        public string? ImagePath
        {
            get; set;
        }

        public required decimal MaxScore
        {
            get; set;
        }

        public required int DisplayOrder
        {
            get; set;
        }
    }
}
