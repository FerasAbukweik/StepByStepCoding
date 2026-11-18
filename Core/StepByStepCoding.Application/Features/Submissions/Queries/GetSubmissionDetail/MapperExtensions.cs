using StepByStepCoding.Domain.Entities;

namespace StepByStepCoding.Application.Features.Submissions.Queries.GetSubmissionDetail
{
    internal static class MapperExtensions
    {
        public static SubmissionDetailDTO ToDTO(this Submission entity)
        {
            return new SubmissionDetailDTO
            {
                Id = entity.Id,
                Version = entity.Version,
                AttemptId = entity.AttemptId,
                QuestionId = entity.QuestionId,
                SubmittedCode = entity.SubmittedCode,
                Language = entity.Language,
                SubmittedAt = entity.SubmittedAt,
                Score = entity.Score,
                Feedback = entity.Feedback,
                ReviewedBy = entity.ReviewedBy,
                ReviewedAt = entity.ReviewedAt
            };
        }
    }
}
