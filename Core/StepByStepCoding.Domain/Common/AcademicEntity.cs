namespace StepByStepCoding.Domain.Common
{
    public abstract class AcademicEntity : Auditable
    {
        public Guid Id { get; private set; } = Guid.CreateVersion7();

        public Guid Version { get; private set; } = Guid.NewGuid();

        public Guid OriginalVersion
        {
            get; private set;
        }

        protected void MarkChanged()
        {
            if (OriginalVersion == Guid.Empty)
                OriginalVersion = Version;

            Version = Guid.NewGuid();
        }
    }
}
