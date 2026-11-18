using StepByStepCoding.Domain.Entities;

namespace StepByStepCoding.Application.Features.ExamAttempts.Queries.GetExamAttemptDetail
{
    internal static class MapperExtensions
    {
        public static ExamAttemptDetailDTO ToDTO(this ExamAttempt entity)
        {
            return new ExamAttemptDetailDTO
            {
                Id = entity.Id,
                Version = entity.Version,
                ExamId = entity.ExamId,
                StudentId = entity.StudentId,
                StartedAt = entity.StartedAt,
                ExpiresAt = entity.ExpiresAt,
                Status = entity.Status
            };
        }
    }
}
