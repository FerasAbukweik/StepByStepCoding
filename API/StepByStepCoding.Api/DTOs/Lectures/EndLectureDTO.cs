using System.ComponentModel.DataAnnotations;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.DTOs.Lectures
{
    public sealed class EndLectureDTO
    {
        public required Guid Version
        {
            get; set;
        }
    }
}
