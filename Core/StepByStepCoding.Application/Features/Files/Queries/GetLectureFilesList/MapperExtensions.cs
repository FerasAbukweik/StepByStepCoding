using StepByStepCoding.Domain.Entities;

namespace StepByStepCoding.Application.Features.Files.Queries.GetLectureFilesList
{
    internal static class MapperExtensions
    {
        public static LectureFileListDTO ToDTO(this LectureFile entity)
        {
            return new LectureFileListDTO
            {
                Id = entity.Id,
                Version = entity.Version,
                LectureId = entity.LectureId,
                FileName = entity.FileName,
                Language = entity.Language,
                SavedRevision = entity.SavedRevision
            };
        }
    }
}
