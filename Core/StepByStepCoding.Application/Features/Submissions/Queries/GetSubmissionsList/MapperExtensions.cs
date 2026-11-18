using StepByStepCoding.Domain.Entities;

namespace StepByStepCoding.Application.Features.Submissions.Queries.GetSubmissionsList
{
    internal static class MapperExtensions
    {
        public static SubmissionListDTO ToDTO(this Submission entity)
        {
            return new SubmissionListDTO
            {
                Id = entity.Id,
                Version = entity.Version,
                AttemptId = entity.AttemptId,
                QuestionId = entity.QuestionId,
                Language = entity.Language,
                SubmittedAt = entity.SubmittedAt,
                Score = entity.Score,
                ReviewedBy = entity.ReviewedBy,
                ReviewedAt = entity.ReviewedAt
            };
        }
    }
}
