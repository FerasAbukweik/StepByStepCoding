using StepByStepCoding.Domain.Entities;

namespace StepByStepCoding.Application.Features.Enrollments.Queries.GetEnrollmentDetail
{
    internal static class MapperExtensions
    {
        public static EnrollmentDetailDTO ToDTO(this Enrollment entity)
        {
            return new EnrollmentDetailDTO
            {
                Id = entity.Id,
                Version = entity.Version,
                SectionId = entity.SectionId,
                StudentId = entity.StudentId,
                Status = entity.Status
            };
        }
    }
}
