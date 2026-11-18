using StepByStepCoding.Domain.Common;
using StepByStepCoding.Domain.Enums;

namespace StepByStepCoding.Domain.Entities
{
    public sealed class WorkspaceFile : CodeFile
    {
        public Guid UserId
        {
            get; private set;
        }

        private WorkspaceFile()
        {
        }

        public WorkspaceFile(Guid userId) : base("main.cpp", ProgrammingLanguage.Cpp, "", 0)
        {
            UserId = AcademicRules.Reference(userId, nameof(UserId));
        }
    }
}
