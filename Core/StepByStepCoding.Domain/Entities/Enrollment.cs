using StepByStepCoding.Domain.Common;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Domain.Entities
{
    public sealed class Enrollment : AcademicEntity
    {
        public Guid SectionId
        {
            get; private set;
        }

        public Guid StudentId
        {
            get; private set;
        }

        public EnrollmentStatus Status
        {
            get; private set;
        }

        public Section? Section
        {
            get; private set;
        }

        private Enrollment()
        {
        }

        public Enrollment(Guid sectionId, Guid studentId)
        {
            SectionId = AcademicRules.Reference(sectionId, nameof(SectionId));
            StudentId = AcademicRules.Reference(studentId, nameof(StudentId));
            Status = EnrollmentStatus.Active;
        }

        public void Update(bool active)
        {
            Status = active ? EnrollmentStatus.Active : EnrollmentStatus.Inactive;
            MarkChanged();
        }
    }
}
