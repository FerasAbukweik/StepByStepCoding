namespace StepByStepCoding.Application.Models.Realtime
{
    public sealed class CodeChangeSet
    {
        // Edits from one Monaco event share offsets in the same original document.
        public required IReadOnlyList<CodeEdit> Changes
        {
            get; init;
        }
    }
}
