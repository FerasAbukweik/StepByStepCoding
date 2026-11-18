using StepByStepCoding.Domain.Entities;

namespace StepByStepCoding.Application.Features.ExamAttempts.Queries.GetExamAttemptsList
{
    internal static class MapperExtensions
    {
        public static ExamAttemptListDTO ToDTO(this ExamAttempt entity, string studentName = "")
        {
            return new ExamAttemptListDTO
            {
                Id = entity.Id,
                Version = entity.Version,
                ExamId = entity.ExamId,
                StudentId = entity.StudentId,
                StudentName = studentName,
                StartedAt = entity.StartedAt,
                ExpiresAt = entity.ExpiresAt,
                Status = entity.Status
            };
        }
    }
}
