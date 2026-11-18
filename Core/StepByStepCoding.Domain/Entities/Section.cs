using StepByStepCoding.Domain.Common;

namespace StepByStepCoding.Domain.Entities
{
    public sealed class Section : AcademicEntity
    {
        public Guid CourseId
        {
            get; private set;
        }

        public Guid InstructorId
        {
            get; private set;
        }

        public string Name { get; private set; } = null!;

        public Course? Course
        {
            get; private set;
        }

        public ICollection<Enrollment> Enrollments { get; private set; } = [];

        public ICollection<Lecture> Lectures { get; private set; } = [];

        public ICollection<Exam> Exams { get; private set; } = [];

        private Section()
        {
        }

        public Section(Guid courseId, Guid instructorId, string name)
        {
            CourseId = AcademicRules.Reference(courseId, nameof(CourseId));
            InstructorId = AcademicRules.Reference(instructorId, nameof(InstructorId));
            Name = AcademicRules.Text(name, nameof(Name), 150);
        }

        public void Update(Guid instructorId, string name)
        {
            InstructorId = AcademicRules.Reference(instructorId, nameof(InstructorId));
            Name = AcademicRules.Text(name, nameof(Name), 150);
            MarkChanged();
        }
    }
}
