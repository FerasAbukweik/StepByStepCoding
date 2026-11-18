using StepByStepCoding.Domain.Entities;

namespace StepByStepCoding.Application.Features.Files.Queries.GetFileDetail
{
    internal static class MapperExtensions
    {
        public static FileDetailDTO ToDTO(this CodeFile entity)
        {
            return new FileDetailDTO
            {
                Id = entity.Id,
                Version = entity.Version,
                LectureId = (entity as LectureFile)?.LectureId,
                UserId = (entity as WorkspaceFile)?.UserId,
                FileName = entity.FileName,
                Language = entity.Language,
                SavedContent = entity.SavedContent,
                SavedRevision = entity.SavedRevision
            };
        }
    }
}
