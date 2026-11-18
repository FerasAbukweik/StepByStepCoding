using StepByStepCoding.Domain.Common;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Domain.Entities
{
    public sealed class LectureFile : CodeFile
    {
        public Guid LectureId
        {
            get; private set;
        }

        public Lecture? Lecture
        {
            get; private set;
        }

        private LectureFile()
        {
        }

        public LectureFile(Guid lectureId, string fileName, ProgrammingLanguage language, string savedContent, long savedRevision)
            : base(fileName, language, savedContent, savedRevision)
        {
            LectureId = AcademicRules.Reference(lectureId, nameof(LectureId));
        }
    }
}
