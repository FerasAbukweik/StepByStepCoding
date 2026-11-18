using StepByStepCoding.Domain.Common;

namespace StepByStepCoding.Domain.Entities
{
    public sealed class Course : AcademicEntity
    {
        public string Name { get; private set; } = null!;

        public ICollection<Section> Sections { get; private set; } = [];

        private Course()
        {
        }

        public Course(string name)
        {
            Name = AcademicRules.Text(name, nameof(Name), 150);
        }

        public void Update(string name)
        {
            Name = AcademicRules.Text(name, nameof(Name), 150);
            MarkChanged();
        }
    }
}
