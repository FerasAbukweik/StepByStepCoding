using System.ComponentModel.DataAnnotations;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.DTOs.Exams
{
    public sealed class UpdateExamDTO
    {
        public required Guid Version
        {
            get; set;
        }

        [Required, MaxLength(250)]
        public required string Title
        {
            get; set;
        }

        [MaxLength(8000)]
        public string? Instructions
        {
            get; set;
        }

        public required DateTimeOffset StartsAt
        {
            get; set;
        }

        public required DateTimeOffset Deadline
        {
            get; set;
        }

        public required int DurationMinutes
        {
            get; set;
        }
    }
}
