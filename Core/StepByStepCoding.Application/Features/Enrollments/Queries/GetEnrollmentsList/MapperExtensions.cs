using StepByStepCoding.Domain.Entities;

namespace StepByStepCoding.Application.Features.Enrollments.Queries.GetEnrollmentsList
{
    internal static class MapperExtensions
    {
        public static EnrollmentListDTO ToDTO(this Enrollment entity, string studentName = "")
        {
            return new EnrollmentListDTO
            {
                Id = entity.Id,
                Version = entity.Version,
                SectionId = entity.SectionId,
                StudentId = entity.StudentId,
                StudentName = studentName,
                Status = entity.Status
            };
        }
    }
}
