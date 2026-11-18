namespace StepByStepCoding.Application.Models.Realtime
{
    public sealed class CodeEdit
    {
        public required int RangeOffset
        {
            get; init;
        }

        public required int RangeLength
        {
            get; init;
        }

        public required string Text
        {
            get; init;
        }
    }
}
