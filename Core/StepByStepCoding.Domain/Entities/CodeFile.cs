using StepByStepCoding.Domain.Common;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Domain.Entities
{
    public abstract class CodeFile : AcademicEntity
    {
        public string FileName { get; private set; } = null!;

        public ProgrammingLanguage Language
        {
            get; private set;
        }

        public string SavedContent { get; private set; } = null!;

        public long SavedRevision
        {
            get; private set;
        }


        protected CodeFile()
        {
        }

        protected CodeFile(string fileName, ProgrammingLanguage language, string content, long revision)
        {
            Apply(fileName, language, content, revision);
        }

        public void Update(string fileName, ProgrammingLanguage language, string savedContent, long savedRevision)
        {
            AcademicRules.Require(savedRevision > SavedRevision, "The saved revision must increase.");
            Apply(fileName, language, savedContent, savedRevision);
            MarkChanged();
        }

        private void Apply(string fileName, ProgrammingLanguage language, string savedContent, long savedRevision)
        {
            AcademicRules.Require(Enum.IsDefined(language), "Unsupported programming language.");
            AcademicRules.Require(savedContent is not null && savedContent.Length <= 100000, "File content must not exceed 100000 characters.");
            AcademicRules.Require(savedRevision >= 0, "Saved revision cannot be negative.");
            FileName = AcademicRules.Text(fileName, nameof(FileName), 250);
            Language = language;
            SavedContent = savedContent!;
            SavedRevision = savedRevision;
        }
    }
}
